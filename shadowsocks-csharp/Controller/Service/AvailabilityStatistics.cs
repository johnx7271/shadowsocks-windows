using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SimpleJson;
using Shadowsocks.Model;
using Shadowsocks.Util;

namespace Shadowsocks.Controller
{
    using Statistics = Dictionary<string, List<StatisticsRecord>>;

    public sealed class AvailabilityStatistics : IDisposable
    {
        public const string DateTimePattern = "yyyy-MM-dd HH:mm:ss";
        private const string StatisticsFilesName = "shadowsocks.availability.json";
        public static string AvailabilityStatisticsFile;
        //static constructor to initialize every public static fields before refereced
        static AvailabilityStatistics()
        {
            AvailabilityStatisticsFile = Utils.GetTempPath(StatisticsFilesName);
        }

        //arguments for ICMP tests
        private int Repeat => Config.RepeatTimesNum;
        public const int TimeoutMilliseconds = 500;

        // data for inner circle 
        //records cache for current server in {_monitorInterval} period
        private readonly ConcurrentDictionary<string, List<int>> _latencyRecords = new ConcurrentDictionary<string, List<int>>();        
        //speed in KiB/s
        private readonly ConcurrentDictionary<string, List<int>> _inboundSpeedRecords = new ConcurrentDictionary<string, List<int>>();
        private readonly ConcurrentDictionary<string, List<int>> _outboundSpeedRecords = new ConcurrentDictionary<string, List<int>>();
        private readonly ConcurrentDictionary<string, InOutBoundRecord> _inOutBoundRecords = new ConcurrentDictionary<string, InOutBoundRecord>();
        private readonly ConcurrentDictionary<string, int> _failCountRecords = new ConcurrentDictionary<string, int>();

        private class InOutBoundRecord
        {
            private long _inbound;
            //private long _lastInbound;
            private long _outbound;
            //private long _lastOutbound;

            public void UpdateInbound(long delta)
            {
                Interlocked.Add(ref _inbound, delta);
            }

            public void UpdateOutbound(long delta)
            {
                Interlocked.Add(ref _outbound, delta);
            }

            /// <summary>
            /// get the accumulated result and reset the inner counter.
            /// </summary>            
            public void GetDelta(out long inboundResult, out long outboundResult)
            {
                //var i = Interlocked.Read(ref _inbound);
                //var il = Interlocked.Exchange(ref _lastInbound, i);
                //inboundDelta = i - il;                
                inboundResult = Interlocked.Exchange(ref _inbound, 0);

                //    var o = Interlocked.Read(ref _outbound);
                //    var ol = Interlocked.Exchange(ref _lastOutbound, o);
                //    outboundDelta = o - ol;
                outboundResult = Interlocked.Exchange(ref _outbound, 0);
            }
        }

        // for the outer circle
        private readonly TimeSpan _delayBeforeStart = TimeSpan.FromSeconds(1);
        private readonly TimeSpan _retryInterval = TimeSpan.FromMinutes(2);
        private Timer _recorder; // ping and aggregate raw speed data and save to RawStatistics and filter to FilteredStatistics

        /// <summary>
        /// outer circle interval for _recorder      
        /// if there are many servers, then use a larger number
        /// </summary>
        private TimeSpan RecordingInterval => TimeSpan.FromMinutes(Config.DataCollectionMinutes);

        // the inner circle timer, for raw speed collecting. 
        private Timer _speedMonior;
        /// <summary>
        /// 1 seconde
        /// </summary>
        private readonly TimeSpan _monitorInterval = TimeSpan.FromSeconds(1);

        private ShadowsocksController _controller;
        private StatisticsStrategyConfiguration Config => _controller.StatisticsConfiguration;

        
        public static AvailabilityStatistics Instance { get; } = new AvailabilityStatistics();

        /// <summary>
        /// records in this are non-empty.
        /// </summary>
        public Statistics RawStatistics { get; private set; }

        /// <summary>
        /// from RawStatistics, with outdated record removed.
        /// </summary>
        public Statistics FilteredStatistics { get; private set; }

        private AvailabilityStatistics()
        {
            RawStatistics = new Statistics();
        }

        /// <summary>
        /// start the timer to meassure and populate RawStatistics
        /// RawStatistics is intially loaed.
        /// during each run of the timer, RawStatistics is saved and filtered.
        /// cached data for inner circle is reset.
        /// </summary>
        /// <param name="controller"></param>
        internal void UpdateConfiguration(ShadowsocksController controller)
        {
            _controller = controller;
            Reset();
            try
            {
                if (Config.StatisticsEnabled)
                {
                    StartTimerWithoutState(ref _recorder, Run, RecordingInterval);
                    LoadRawStatistics(); // why after the previous one?
                                         // since Run is a delayed timer proc, so this will probably run b4 Run 

                    StartTimerWithoutState(ref _speedMonior, UpdateSpeed, _monitorInterval);
                }
                else
                {
                    _recorder?.Dispose();
                    _speedMonior?.Dispose();
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        private void StartTimerWithoutState(ref Timer timer, TimerCallback callback, TimeSpan interval)
        {
            if (timer?.Change(_delayBeforeStart, interval) == null)
            {
                timer = new Timer(callback, null, _delayBeforeStart, interval);
            }
        }

        /// <summary>
        /// the timer proc for speed test.
        /// this is the inner circle with shorter period.
        /// for each server,  
        /// use raw data from _inOutBoundRecords (see UpdateInboundCounter, UpdateOutboundCounter)
        /// to append a new record for the server in _inboundSpeedRecords and _outboundSpeedRecords
        /// </summary>        
        private void UpdateSpeed(object _)
        {
            foreach (var kv in _inOutBoundRecords)
            {
                var id = kv.Key;
                var record = kv.Value;

                long inboundDelta, outboundDelta;

                record.GetDelta(out inboundDelta, out outboundDelta);

                var inboundSpeed = GetSpeedInKiBPerSecond(inboundDelta, _monitorInterval.TotalSeconds);
                var outboundSpeed = GetSpeedInKiBPerSecond(outboundDelta, _monitorInterval.TotalSeconds);

                var inR = _inboundSpeedRecords.GetOrAdd(id, (k) => new List<int>());
                var outR = _outboundSpeedRecords.GetOrAdd(id, (k) => new List<int>());

                inR.Add(inboundSpeed);
                outR.Add(outboundSpeed);

                Logging.Debug(
                    $"{id}: current/max inbound {inboundSpeed}/{inR.Max()} KiB/s, current/max outbound {outboundSpeed}/{outR.Max()} KiB/s");
            }
        }

        private void Reset()
        {
            _inboundSpeedRecords.Clear();
            _outboundSpeedRecords.Clear();
            _latencyRecords.Clear();
        }

        /// <summary>
        /// the timer proc for outer circle, also do ping test.
        /// call low level UpdateRecords
        /// and reset
        /// </summary>
        private void Run(object _)
        {
            UpdateRecords();
            Reset();
        }

        /// <summary>
        /// the low level timer proc for aggregating raw speed data and also ping test.
        /// for each server, 
        /// inspeed, outspeed, latency data are aggregated to a StatisticsRecord for the server.
        /// if (ping), use MyPing (async) to measure response and PingPassRate,         
        /// when each ping completed, the record is updated to RawStatistics
        /// when last ping is done, RawStatistics is Saved.
        /// </summary>
        private void UpdateRecords()
        {
            UpdateRecordsState state = new UpdateRecordsState();
            state.counter = _controller.GetCurrentConfiguration().configs.Count;
            // state is shared among servers, holds the number of servers remains to be pinged.
            int recordingInterval = (int)RecordingInterval.TotalSeconds;

            foreach (var server in _controller.GetCurrentConfiguration().configs)
            {
                var id = server.Identifier();
                int failedCount;
                List<int> inboundSpeedRecords = null;
                List<int> outboundSpeedRecords = null;
                List<int> latencyRecords = null;
                _inboundSpeedRecords.TryGetValue(id, out inboundSpeedRecords);
                _outboundSpeedRecords.TryGetValue(id, out outboundSpeedRecords);
                _latencyRecords.TryGetValue(id, out latencyRecords);                
                StatisticsRecord record = new StatisticsRecord(id, inboundSpeedRecords, outboundSpeedRecords, latencyRecords);

                if (_failCountRecords.TryGetValue(id, out failedCount)) 
                    record.FailCount = (float)failedCount / recordingInterval;

                if (Config.Ping)
                {
                    MyPing ping = new MyPing(server, Repeat);
                    ping.Completed += ping_Completed;
                    ping.Start(new PingState { state = state, record = record });
                }
                else if (!record.IsEmptyData())
                {
                    AppendRecord(id, record);
                }
            }

            if (!Config.Ping)
            {
                Save();
                FilterRawStatistics();
            }
        }

        /// <summary>
        /// append the ping result to RawStatistics
        /// if the last server to be pinged, save to file and filter out outdated record if Config.ByHourOfDay
        /// </summary>
        private void ping_Completed(object sender, MyPing.CompletedEventArgs e)
        {
            PingState pingState = (PingState)e.UserState;
            UpdateRecordsState state = pingState.state;
            Server server = e.Server;
            StatisticsRecord record = pingState.record;
            record.SetResponse(e.RoundtripTime);
            if (!record.IsEmptyData())
            {
                AppendRecord(server.Identifier(), record);
            }
            Logging.Debug($"Ping {server.FriendlyName()} {e.RoundtripTime.Count} roundtrip times, {(100 - record.PingPassRate * 100)}% packet loss, min {record.MinResponse} ms, max {record.MaxResponse} ms, avg {record.AverageResponse} ms");
            if (Interlocked.Decrement(ref state.counter) == 0)  // last server pinged?
            {
                Save();
                FilterRawStatistics();
            }
        }

        /// <summary>
        /// append the record to the list of statisticsrecords at RawStatistics[server],
        /// if the list is null, add a empty list before appending.
        /// </summary>
        private void AppendRecord(string serverIdentifier, StatisticsRecord record)
        {
            try
            {
                List<StatisticsRecord> records;
                lock (RawStatistics)
                {
                    if (!RawStatistics.TryGetValue(serverIdentifier, out records))
                    {
                        records = new List<StatisticsRecord>();
                        RawStatistics[serverIdentifier] = records;
                    }
                }
                records.Add(record);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        /// <summary>
        /// serialize RawStatistics to AvailabilityStatisticsFile
        /// </summary>
        private void Save()
        {
            Logging.Debug($"save statistics to {AvailabilityStatisticsFile}");
            if (RawStatistics.Count == 0)
            {
                return;
            }
            try
            {
                string content;
#if DEBUG
                content = SimpleJson.SimpleJson.SerializeObject(RawStatistics/*, Formatting.Indented*/);
#else
                content = SimpleJson.SimpleJson.SerializeObject(RawStatistics/*, , Formatting.None*/);
#endif
				File.WriteAllText(AvailabilityStatisticsFile, content);
            }
            catch (IOException e)
            {
                Logging.LogUsefulException(e);
            }
        }

        private bool IsValidRecord(StatisticsRecord record)
        {
            if (Config.ByHourOfDay)
            {
                return (DateTime.Now - record.Timestamp).TotalSeconds <= 3600;
            }
            return true;
        }

        /// <summary>
        /// remove data older.
        /// and Save
        /// </summary>        
        public void CleanseRawStatistics(int recentDays2Keep)
        {
            // remove statistics older than 7 day.
            Dictionary<string, List<StatisticsRecord>> sta = RawStatistics;
            Dictionary<string, List<StatisticsRecord>> newsta = new Dictionary<string, List<StatisticsRecord>>();
            DateTime n = DateTime.Now;

            lock (sta)
                foreach (var kv in sta)
                {
                    List<StatisticsRecord> list = new List<StatisticsRecord>(kv.Value.Count);
                    foreach (var rec in kv.Value)
                    {
                        if ((n - rec.Timestamp).TotalSeconds < recentDays2Keep * 86400)
                            list.Add(rec);
                    }
                    newsta[kv.Key] = list;
                }
            RawStatistics = RawStatistics;
            Save();
        }

        /// <summary>
        /// remove records that are outdated.
        /// in statistic config, if ByHourOfDay and record's timestamp hour is not current hour, then removed.
        /// </summary>
        private void FilterRawStatistics()
        {
            try
            {
                Logging.Debug("filter raw statistics");
                
                if (FilteredStatistics == null)
                {
                    FilteredStatistics = new Statistics();
                }

                Statistics newOne = new Statistics();
                foreach (var serverAndRecords in RawStatistics)
                {
                    var server = serverAndRecords.Key;
                    var filteredRecords = serverAndRecords.Value.FindAll(IsValidRecord);                    

                    if(filteredRecords.Count > 0) // all the records are not empty and the list is not empty.
                        newOne[server] = filteredRecords;
                }
                FilteredStatistics = newOne;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
            }
        }

        /// <summary>
        /// deserialize RawStatistics from AvailabilityStatisticsFile
        /// </summary>
        private void LoadRawStatistics()
        {
            try
            {
                var path = AvailabilityStatisticsFile;
                Logging.Debug($"loading statistics from {path}");
                if (!File.Exists(path))
                {
                    using (File.Create(path))
                    {
                        //do nothing
                    }
                }
                var content = File.ReadAllText(path);
                Statistics tmp1 = SimpleJson.SimpleJson.DeserializeObject<Statistics>(content) ?? RawStatistics;                
                List<Server> servers = _controller.GetCurrentConfiguration().configs;
                Statistics tmp2 = new Statistics(servers.Count);

                // remove unused servers
                foreach (Server server in servers)
                {
                    string serverid = server.Identifier();
                    List<StatisticsRecord> t = tmp1.ContainsKey(serverid) ?
                        tmp1[serverid] : null;
                    if (t != null)
                        if (!tmp2.ContainsKey(serverid))
                            tmp2.Add(serverid, t);
                        //else
                        //    tmp2[serverid] = t;
                }
                RawStatistics = tmp2;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                Console.WriteLine($"failed to load statistics; try to reload {_retryInterval.TotalMinutes} minutes later");
                _recorder.Change(_retryInterval, RecordingInterval);
            }
        }

        private static int GetSpeedInKiBPerSecond(long bytes, double seconds)
        {
            var result = (int)(bytes / seconds) / 1024;
            return result;
        }

        public void Dispose()
        {
            _recorder.Dispose();
            _speedMonior.Dispose();
        }

        /// <summary>
        /// Append latency to _latencyRecords for server in _latencyRecords
        /// </summary>        
        public void UpdateLatency(Server server, int latency)
        {            
            _latencyRecords.AddOrUpdate(server.Identifier(), (k) =>            
            {
                List<int> records = new List<int>();
                records.Add(latency);
                return records;
            }, (k, v) =>
            {
                v.Add(latency);
                return v;
            });
        }

        /// <summary>
        /// AddOrUpdate the Inbound of the InOutBoundRecord for server in _inOutBoundRecords
        /// </summary>        
        public void UpdateInboundCounter(Server server, long n)
        {
            _inOutBoundRecords.AddOrUpdate(server.Identifier(), (k) =>
            {
                var r = new InOutBoundRecord();
                r.UpdateInbound(n);

                return r;
            }, (k, v) =>
            {
                v.UpdateInbound(n);
                return v;
            });
        }

        /// <summary>
        /// AddOrUpdate the Outbound of the InOutBoundRecord for server in _inOutBoundRecords
        /// </summary>
        public void UpdateOutboundCounter(Server server, long n)
        {
            _inOutBoundRecords.AddOrUpdate(server.Identifier(), (k) =>
            {
                var r = new InOutBoundRecord();
                r.UpdateOutbound(n);

                return r;
            }, (k, v) =>
            {
                v.UpdateOutbound(n);
                return v;
            });
        }

        public void UpdateFail(Server server)
        {
            _failCountRecords.AddOrUpdate(server.Identifier(), (k) =>
            {
                return 1;
            }, (k, v) =>
            {                
                return v + 1;
            });
        }

        class UpdateRecordsState
        {
            /// <summary>
            /// initialised to the number of servers in the configs
            /// </summary>
            public int counter;
        }

        class PingState
        {
            public UpdateRecordsState state;
            public StatisticsRecord record;
        }

        /// <summary>
        /// used to measure the round trip time, 
        /// start an async task to do ping test, repeat this task (repeat) times,
        /// </summary>
        class MyPing
        {
            //arguments for ICMP tests
            public const int TimeoutMilliseconds = 500;

            public EventHandler<CompletedEventArgs> Completed;
            private Server server;

            private int repeat;
            private IPAddress ip;
            private Ping ping;
            private List<int?> RoundtripTime;

            public MyPing(Server server, int repeat)
            {
                this.server = server;
                this.repeat = repeat;
                RoundtripTime = new List<int?>(repeat);
                ping = new Ping();
                ping.PingCompleted += Ping_PingCompleted;
            }

            public void Start(object userstate)
            {
                if (server.server == "")
                {
                    FireCompleted(new Exception("Invalid Server"), userstate);
                    return;
                }
                new Task(() => ICMPTest(0, userstate)).Start();
            }

            private void ICMPTest(int delay, object userstate)
            {
                try
                {
                    Logging.Debug($"Ping {server.FriendlyName()}");
                    if (ip == null)
                    {
                        ip = Dns.GetHostAddresses(server.server)
                                .First(
                                    ip =>
                                        ip.AddressFamily == AddressFamily.InterNetwork ||
                                        ip.AddressFamily == AddressFamily.InterNetworkV6);
                    }
                    repeat--;
                    if (delay > 0)
                        Thread.Sleep(delay);
                    ping.SendAsync(ip, TimeoutMilliseconds, userstate);
                }
                catch (Exception e)
                {
                    Logging.Error($"An exception occured while eveluating {server.FriendlyName()}");
                    Logging.LogUsefulException(e);
                    FireCompleted(e, userstate);
                }
            }

            private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
            {
                try
                {
                    if (e.Reply.Status == IPStatus.Success)
                    {
                        Logging.Debug($"Ping {server.FriendlyName()} {e.Reply.RoundtripTime} ms");
                        RoundtripTime.Add((int?)e.Reply.RoundtripTime);
                    }
                    else
                    {
                        Logging.Debug($"Ping {server.FriendlyName()} timeout");
                        RoundtripTime.Add(null);
                    }
                    TestNext(e.UserState);
                }
                catch (Exception ex)
                {
                    Logging.Error($"An exception occured while eveluating {server.FriendlyName()}");
                    Logging.LogUsefulException(ex);
                    FireCompleted(ex, e.UserState);
                }
            }

            private void TestNext(object userstate)
            {
                if (repeat > 0)
                {
                    //Do ICMPTest in a random frequency
                    int delay = TimeoutMilliseconds + new Random().Next() % TimeoutMilliseconds;
                    new Task(() => ICMPTest(delay, userstate)).Start();
                }
                else
                {
                    FireCompleted(null, userstate);
                }
            }

            private void FireCompleted(Exception error, object userstate)
            {
                Completed?.Invoke(this, new CompletedEventArgs
                {
                    Error = error,
                    Server = server,
                    RoundtripTime = RoundtripTime,
                    UserState = userstate
                });
            }

            public class CompletedEventArgs : EventArgs
            {
                public Exception Error;
                public Server Server;
                public List<int?> RoundtripTime;
                public object UserState;
            }
        }

    }
}
