using Shadowsocks.Model;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Shadowsocks.Controller.Strategy
{
    class HighAvailabilityStrategy : IStrategy
    {
        protected ServerStatus _currentServer;
        protected Dictionary<Server, ServerStatus> _serverStatus;
        ShadowsocksController _controller;
        Random _random;

        public class ServerStatus
        {
            /// <summary>
            /// time interval needed to finish connect.
            /// default to 100s.
            /// </summary>
            public TimeSpan latency;

            /// <summary>
            /// default to time of last reloadserver.
            /// </summary>
            public DateTime lastTimeDetectLatency;

            /// <summary>
            /// last time anything received.
            /// default mintime + 100s.
            /// </summary>
            public DateTime lastRead;

            /// <summary>
            /// last time anything sent.
            /// default mintime.
            /// </summary>
            public DateTime lastWrite;

            /// <summary>
            /// connection refused or closed before anything received.
            /// default mintime.
            /// </summary>
            public DateTime lastFailure;

            public Server server;

            public double score;
        }

        public HighAvailabilityStrategy(ShadowsocksController controller)
        {
            _controller = controller;
            _random = new Random();
            _serverStatus = new Dictionary<Server, ServerStatus>();
        }

        public string Name
        {
            get { return I18N.GetString("High Availability"); }
        }

        public string ID
        {
            get { return "com.shadowsocks.strategy.ha"; }
        }

        public void ReloadServers()
        {
            // make a copy to avoid locking
            var newServerStatus = new Dictionary<Server, ServerStatus>();
            DateTime n = DateTime.Now;

            foreach (var server in _controller.GetCurrentConfiguration().configs)
            {
                var status = new ServerStatus();
                status.server = server;
                
                if (!_serverStatus.ContainsKey(server))
                {                                        
                    status.lastFailure = DateTime.MinValue;
                    status.lastRead = DateTime.MinValue.AddSeconds(100);
                    status.lastWrite = DateTime.MinValue;
                    status.latency = new TimeSpan(0, 0, 100);
                    status.lastTimeDetectLatency = n;
                    newServerStatus[server] = status;
                }
                else
                {                    
                    // use existing status
                    newServerStatus[server] = _serverStatus[server];
                }
            }            

            _serverStatus = newServerStatus;

            ChooseNewServer();
        }

        public Server GetAServer(IStrategyCallerType type, System.Net.IPEndPoint localIPEndPoint, EndPoint destEndPoint)
        {
            if (type == IStrategyCallerType.TCP)
            {
                ChooseNewServer();
            }
            if (_currentServer == null)
            {
                return null;
            }
            return _currentServer.server;
        }

        public void ChooseNewServer()
        {            
            List<ServerStatus> servers = new List<ServerStatus>(_serverStatus.Values); // shallow copy
            DateTime now = DateTime.Now;
            foreach (var status in servers)
            {
                double successReponseInterval = (status.lastRead - status.lastWrite).TotalMilliseconds;
                // (lastread-lastwrite) is also a meassure of server latency

                // all of failure, latency, (lastread - lastwrite) normalized to ms, then
                // 10* failure - latency - 1000 / (lastread - lastwrite)
                status.score = 10 * Math.Max(2, (now - status.lastFailure).TotalSeconds)
                    - status.latency.TotalSeconds / (1 + (now - status.lastTimeDetectLatency).TotalMinutes / 5)                    
                    + 1000 / Math.Max(0.1, successReponseInterval);
                //Logging.Debug(String.Format("server: {0} latency:{1} score: {2}", status.server.FriendlyName(), status.latency, status.score));
            }
            ServerStatus max = null;
            foreach (var status in servers)
            {
                if (max == null)
                {
                    max = status;
                }
                else
                {
                    if (status.score >= max.score)
                    {
                        max = status;
                    }
                }
            }
            if (max != null)
            {
                if (_currentServer == null || max.score - _currentServer.score > 200)
                {
                    _currentServer = max;
                    Logging.Info($"HA switching to server: {_currentServer.server.FriendlyName()}");
                }
                else 
                    Logging.Info("HA server not switched.");
            }
            else
                Logging.Info("HA server not switched.");
        }

        public void UpdateLatency(Model.Server server, TimeSpan latency)
        {
            Logging.Debug($"latency: {server.FriendlyName()} {latency}");

            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                status.latency = latency;
                status.lastTimeDetectLatency = DateTime.Now;
            }
        }

        public void UpdateLastRead(Model.Server server)
        {
            Logging.Debug($"last read: {server.FriendlyName()}");

            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                if(status.lastRead < status.lastWrite) // one write multiple read
                    status.lastRead = DateTime.Now;
            }
        }

        public void UpdateLastWrite(Model.Server server)
        {
            Logging.Debug($"last write: {server.FriendlyName()}");

            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                TimeSpan delta = status.lastRead - status.lastWrite;
                status.lastWrite = DateTime.Now;
                delta += delta;
                if (delta.TotalSeconds > 100) delta = new TimeSpan(0, 0, 100);
                status.lastRead += delta;
            }
        }

        public void SetFailure(Model.Server server)
        {
            Logging.Debug($"failure: {server.FriendlyName()}");

            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                status.lastFailure = DateTime.Now;
            }
        }
    }
}
