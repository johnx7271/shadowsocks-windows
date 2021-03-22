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
            // time interval between SYN and SYN+ACK
            public TimeSpan latency;
            public DateTime lastTimeDetectLatency;

            // last time anything received
            public DateTime lastRead;

            // last time anything sent
            public DateTime lastWrite;

            // connection refused or closed before anything received
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
            // a shallow copy

            foreach (var server in _controller.GetCurrentConfiguration().configs)
            {
                var status = new ServerStatus();
                status.server = server;

                if (!_serverStatus.ContainsKey(server))
                {                                        
                    status.lastFailure = DateTime.MinValue;
                    status.lastRead = DateTime.Now;
                    status.lastWrite = DateTime.Now;
                    status.latency = new TimeSpan(0, 0, 0, 0, 10);
                    status.lastTimeDetectLatency = DateTime.Now;
                    newServerStatus[server] = status;
                }
                else
                {                    
                    // use existing status
                    newServerStatus[server] = _serverStatus[server];
                }
            }
            // what about removed server?

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
                // all of failure, latency, (lastread - lastwrite) normalized to ms, then
                // 100 * failure - 10 * latency - 1 * (lastread - lastwrite)
                status.score = 100 * 1000 * Math.Min(60, (now - status.lastFailure).TotalSeconds)
                    - 10 * status.latency.TotalMilliseconds / (1 + (now - status.lastTimeDetectLatency).TotalSeconds / 300)
                    - 1000 * Math.Min(0.1, (status.lastRead - status.lastWrite).TotalSeconds); // (lastread-lastwrite) is also a meassure of server latency                                
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
                status.lastRead = DateTime.Now;
            }
        }

        public void UpdateLastWrite(Model.Server server)
        {
            Logging.Debug($"last write: {server.FriendlyName()}");

            ServerStatus status;
            if (_serverStatus.TryGetValue(server, out status))
            {
                status.lastWrite = DateTime.Now;
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
