using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using JsonConvert = SimpleJson.SimpleJson;

using Shadowsocks.Model;

namespace Shadowsocks.Controller.Strategy
{
	using Statistics = Dictionary<string, List<StatisticsRecord>>;

    internal class StatisticsStrategy : IStrategy, IDisposable
    {
        private readonly ShadowsocksController _controller;
        private Server _currentServer;

        private HighAvailabilityStrategy _agent;
        private int _servercount;

        private Statistics _filteredStatistics => 
                _controller.availabilityStatistics.FilteredStatistics??
                _controller.availabilityStatistics.RawStatistics;
        
        public StatisticsStrategy(ShadowsocksController controller)
        {
            _controller = controller;
            var servers = controller.GetCurrentConfiguration().configs;
            _servercount = servers.Count;
            var randomIndex = new Random().Next() % _servercount;
            
            _agent = new HighAvailabilityStrategy(controller);                        
        }        

        //return the score by data
        //server with highest score will be choosen
        private float? GetScore(string identifier, List<StatisticsRecord> records)
        {
            var config = _controller.StatisticsConfiguration;
            float? score = null;

            StatisticsRecord averageRecord = new StatisticsRecord();

            double? t;
            t = records.Select(record => record.AverageInboundSpeed).Average();
            averageRecord.AverageInboundSpeed = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MinInboundSpeed).Average();
            averageRecord.MinInboundSpeed = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MaxInboundSpeed).Average();
            averageRecord.MaxInboundSpeed = t == null ? null : (int?)(int)t.Value;

            t = records.Select(record => record.AverageOutboundSpeed).Average();
            averageRecord.AverageOutboundSpeed = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MinOutboundSpeed).Average();
            averageRecord.MinOutboundSpeed = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MaxOutboundSpeed).Average();
            averageRecord.MaxOutboundSpeed = t == null ? null : (int?)(int)t.Value;

            t = records.Select(record => record.AverageLatency).Average();
            averageRecord.AverageLatency = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MinLatency).Average();
            averageRecord.MinLatency = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MaxLatency).Average();
            averageRecord.MaxLatency = t == null ? null : (int?)(int)t.Value;

            t = records.Select(record => record.AverageResponse).Average();
            averageRecord.AverageResponse = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MinResponse).Average();
            averageRecord.MinResponse = t == null ? null : (int?)(int)t.Value;
            t = records.Select(record => record.MaxResponse).Average();
            averageRecord.MaxResponse = t == null ? null : (int?)(int)t.Value;

            t = records.Select(record => record.PingPassRate).Average();
            averageRecord.PingPassRate = t == null ? null : (float?)(float)t.Value;

            averageRecord.FailCount = records.Select(record => record.FailCount).Average();

            foreach (var calculation in config.Calculations)
            {
                var name = calculation.Key;
                var field = typeof (StatisticsRecord).GetField(name);
                dynamic value = field?.GetValue(averageRecord);
                var factor = calculation.Value;
                if (value == null || factor.Equals(0)) continue;
                score = score ?? 0;
                score += value * factor;
            }

            //if (score != null)
            //{
            //    Logging.Debug($"Server score: {score} {JsonConvert.SerializeObject(averageRecord/*, Formatting.Indented*/)}");
            //}
            return score;
        }

        private Server ChooseNewServer(List<Server> servers)
        {            
            try
            {
                var serversWithStatistics = (from server in servers
                    let id = server.Identifier()
                    where _filteredStatistics.ContainsKey(id)
                    let score = GetScore(id, _filteredStatistics[id])
                    where score != null
                    select new
                    {
                        server,
                        score
                    }).ToArray();

                if (serversWithStatistics.Length < 2 && _servercount >= 2 )
                {
                    LogWhenEnabled("no enough statistics data or all factors in calculations are 0");
                    return null;
                }

                var bestResult = serversWithStatistics
                    .Aggregate((server1, server2) => server1.score > server2.score ? server1 : server2);

                LogWhenEnabled($"ST switch to server: {bestResult.server.FriendlyName()}, score {bestResult.score}");
                return bestResult.server;
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                return null;
            }
        }

        private void LogWhenEnabled(string log)
        {
            if (_controller.GetCurrentStrategy()?.ID == ID) //output when enabled
            {
                Console.WriteLine(log);
            }
        }

        public string ID => "com.shadowsocks.strategy.scbs";

        public string Name => I18N.GetString("Choose by statistics");

        public Server GetAServer(IStrategyCallerType type, IPEndPoint localIPEndPoint, EndPoint destEndPoint)
        {
            if (_filteredStatistics == null || _servercount == 0)
                return _currentServer = null;
            else if (_filteredStatistics.Count < _servercount)
                return _currentServer = _agent.GetAServer(type, localIPEndPoint, destEndPoint);

            List<Server> servers = _controller.GetCurrentConfiguration().configs;
            Server t = ChooseNewServer(servers);
            if(t == null)
                t = _agent.GetAServer(type, localIPEndPoint, destEndPoint); 

            return _currentServer = t;  //current server cached for CachedInterval
        }

        public void ReloadServers()
        {
            _servercount = this._controller.GetCurrentConfiguration().configs.Count;
            _agent.ReloadServers();            
        }

        public void SetFailure(Server server)
        {            
            Statistics t = _filteredStatistics;
            if (t == null || t.Count < _servercount)
                _agent.SetFailure(server);
            else
                Logging.Debug($"failure: {server.FriendlyName()}");
        }

        public void UpdateLastRead(Server server)
        {
            Statistics t = _filteredStatistics;
            if (t == null || t.Count < _servercount)
                _agent.UpdateLastRead(server);
        }

        public void UpdateLastWrite(Server server)
        {
            Statistics t = _filteredStatistics;
            if (t == null || t.Count < _servercount)
                _agent.UpdateLastWrite(server);
        }

        public void UpdateLatency(Server server, TimeSpan latency)
        {
            Statistics t = _filteredStatistics;
            if (t == null || t.Count < _servercount)
                _agent.UpdateLatency(server, latency);
        }

        public void Dispose()
        {            
        }
    }
}
