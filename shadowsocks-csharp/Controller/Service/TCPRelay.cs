﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Shadowsocks.Encryption;
using Shadowsocks.Model;
using Shadowsocks.Controller.Strategy;
using System.Timers;

namespace Shadowsocks.Controller
{

    class TCPRelay : Listener.Service
    {
        private ShadowsocksController _controller;
        private DateTime _lastSweepTime;

        public ISet<Handler> Handlers
        {
            get; set;
        }

        public bool LogTraffic
        {
            get
            {
                return Configuration.Load().logNetTraffic;
            }
        }

        public TCPRelay(ShadowsocksController controller)
        {
            this._controller = controller;
            this.Handlers = new HashSet<Handler>();
            this._lastSweepTime = DateTime.Now;
        }

        public bool Handle(byte[] firstPacket, int length, Socket socket, object state)
        {
            if (socket.ProtocolType != ProtocolType.Tcp)
            {
                return false;
            }
            if (this.LogTraffic)
            {
                Logging.LogNetTraffic("***Socks handshake with local first round.");
                Logging.LogNetTraffic(firstPacket);
            }

            byte socksver = firstPacket[0];
            if (length < 2 || socksver != 5 && socksver != 4) //ie 9 and less only support socks4
            {
                return false;
            }
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            Handler handler = new Handler(); // each connection served by an individual handler.
            handler.connection = socket;
            handler.controller = _controller;
            handler.relay = this;

            handler.Start(firstPacket, length);
            IList<Handler> handlersToClose = new List<Handler>();
            lock (this.Handlers)
            {
                this.Handlers.Add(handler);
                Logging.Debug($"connections: {Handlers.Count}");
                DateTime now = DateTime.Now;
                if (now - _lastSweepTime > TimeSpan.FromSeconds(1))
                {
                    _lastSweepTime = now;
                    foreach (Handler handler1 in this.Handlers)
                    {
                        if (now - handler1.lastActivity > TimeSpan.FromSeconds(900))
                        {
                            handlersToClose.Add(handler1);
                        }
                    }
                }
            }
            foreach (Handler handler1 in handlersToClose)
            {
                Logging.Debug("Closing timed out connection");
                handler1.Close();
            }
        return true;
        }
    }

    class Handler
    {
        //public Encryptor encryptor;
        public IEncryptor encryptor;
        public Server server;
        // Client  socket.
        public Socket remote;
        public Socket connection;
        public ShadowsocksController controller;
        public TCPRelay relay;

        public DateTime lastActivity;

        private int retryCount = 0;
        private bool connected;

        private byte command;
        private byte[] _firstPacket;
        private int _firstPacketLength;
        // Size of receive buffer.
        public const int RecvSize = 8192;
        public const int RecvReserveSize = IVEncryptor.ONETIMEAUTH_BYTES + IVEncryptor.AUTH_BYTES; // reserve for one-time auth
        public const int BufferSize = RecvSize + RecvReserveSize + 32;

        private int totalRead = 0;
        private int totalWrite = 0;

        // remote receive buffer
        private byte[] remoteRecvBuffer = new byte[BufferSize];
        // remote send buffer
        private byte[] remoteSendBuffer = new byte[BufferSize];
        // connection receive buffer
        private byte[] connetionRecvBuffer = new byte[BufferSize];
        // connection send buffer
        private byte[] connetionSendBuffer = new byte[BufferSize];
        // Received data string.

        private bool connectionShutdown = false;
        private bool remoteShutdown = false;
        private bool closed = false;

        private object encryptionLock = new object();

        private DateTime _startConnectTime;

        public void CreateRemote()
        {
            Server server = controller.GetAServer(IStrategyCallerType.TCP, (IPEndPoint)connection.RemoteEndPoint);
            if (server == null || server.server == "")
            {
                throw new ArgumentException("No server configured");
            }
            this.encryptor = EncryptorFactory.GetEncryptor(server.method, server.password, server.one_time_auth, false);
            this.server = server;
        }

        public void Start(byte[] firstPacket, int length)
        {
            this._firstPacket = firstPacket;
            this._firstPacketLength = length;
            this.HandshakeReceive();
            this.lastActivity = DateTime.Now;
        }

        private void CheckClose()
        {
            if (connectionShutdown && remoteShutdown)
            {
                this.Close();
            }
        }

        public void Close()
        {
            lock (relay.Handlers)
            {
                Logging.Debug($"connections: {relay.Handlers.Count}");
                relay.Handlers.Remove(this);
            }
            lock (this)
            {
                if (closed)
                {
                    return;
                }
                closed = true;
            }
            if (connection != null)
            {
                try
                {
                    connection.Shutdown(SocketShutdown.Both);
                    connection.Close();
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                }
            }
            if (remote != null)
            {
                try
                {
                    remote.Shutdown(SocketShutdown.Both);
                    remote.Close();
                }
                catch (Exception e)
                {
                    Logging.LogUsefulException(e);
                }
            }
            lock (encryptionLock) // need lock?
            {             
                if (encryptor != null)
                {
                    ((IDisposable)encryptor).Dispose();
                }
                
            }
        }

        // socks4 has only one handshake round, socks5 two
        private void HandshakeReceive()
        {
            if (closed)
            {
                return;
            }
            try
            {
                int bytesRead = _firstPacketLength;

                if (bytesRead > 1)
                {
                    byte ver = _firstPacket[0];
                    byte[] response;

                    if (ver == 4)
                    {
                        response = new byte[] { 0, 0x5a, 0,0, 0,0, 0,0 }; // 5a= grant
                        // socks 4 is now handshake completed.                        
                        connection.BeginSend(response, 0, response.Length, 0, new AsyncCallback(ResponseCallback), null);
                    }
                    else if (ver == 5)
                    {
                        response = new byte[] { ver, 0 }; // chose auth method: _firstPacket[2], or auth success: 0
                        connection.BeginSend(response, 0, response.Length, 0, new AsyncCallback(HandshakeSendCallback), null);
                    }
                    else
                    { // dont know what to reply
                        // response = new byte[] { 0, 0x5b }; // reject socks 4
                        Close();
                    }
                }
                else
                {
                    Close();
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void HandshakeSendCallback(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                // socks5 handshake completed.
                connection.EndSend(ar);
                connection.BeginReceive(connetionRecvBuffer, 0, connetionRecvBuffer.Length, 0,
                    new AsyncCallback(handshakeReceive2Callback), null);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        // Socks5 connection request received, check now
        private void handshakeReceive2Callback(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                int bytesRead = connection.EndReceive(ar);

                if (this.relay.LogTraffic)
                {
                    Logging.LogNetTraffic("***Socks handshake with local second round.");
                    Logging.LogNetTraffic(connetionRecvBuffer);
                }

                // +----+-----+-------+------+----------+----------+
                // |VER | CMD |  RSV  | ATYP | DST.ADDR | DST.PORT |
                // +----+-----+-------+------+----------+----------+
                // | 1  |  1  | X'00' |  1   | Variable |    2     |
                // +----+-----+-------+------+----------+----------+
                // CMD: 1 connect, 2 bind, 3 udp
                // note the dst might be a dns name or ipv6
                // TODO validate

                if (bytesRead >= 3)
                {
                    // response format:
                    // ver, status (0=ok), reserved, adrType(1=ipv4), (4 or 16 bytes, or dns name), port(2bytes)

                    command = connetionRecvBuffer[1];
                    if (command == 1)
                    {
                        byte[] response = { 5, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
                        connection.BeginSend(response, 0, response.Length, 0, new AsyncCallback(ResponseCallback), null);
                    }
                    else if (command == 3)
                    {
                        HandleUDPAssociate();
                    }
                }
                else
                {
                    Console.WriteLine("failed to recv data in handshakeReceive2Callback");
                    Close();
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void HandleUDPAssociate()
        {
            IPEndPoint endPoint = (IPEndPoint)connection.LocalEndPoint;
            byte[] address = endPoint.Address.GetAddressBytes();
            int port = endPoint.Port;
            byte[] response = new byte[4 + address.Length + 2];
            response[0] = 5;
            // response format:
            // ver, status (0=ok), reserved, adrType(1=ipv4), (4 or 16 bytes, or dns name), port(2bytes)
            if (endPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                response[3] = 1;
            }
            else if (endPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                response[3] = 4;
            }
            address.CopyTo(response, 4);
            response[response.Length - 1] = (byte)(port & 0xFF);
            response[response.Length - 2] = (byte)((port >> 8) & 0xFF);
            connection.BeginSend(response, 0, response.Length, 0, new AsyncCallback(ReadAll), true);
        }

        // not completed yet?
        private void ReadAll(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                if (ar.AsyncState != null)
                {
                    connection.EndSend(ar);
                    connection.BeginReceive(connetionRecvBuffer, 0, RecvSize, 0,
                        new AsyncCallback(ReadAll), null);
                }
                else
                {
                    int bytesRead = connection.EndReceive(ar);
                    if (bytesRead > 0)
                    {
                        connection.BeginReceive(connetionRecvBuffer, 0, RecvSize, 0,
                            new AsyncCallback(ReadAll), null);
                    }
                    else
                    {
                        this.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        // Completed negociating with local on socks
        private void ResponseCallback(IAsyncResult ar)
        {
            try
            {
                connection.EndSend(ar);

                StartConnect();
            }

            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private class ServerTimer : Timer
        {
            public Server Server;

            public ServerTimer(int p) :base(p)
            {
            }
        }

        private void StartConnect()
        {
            try
            {
                CreateRemote();

                // TODO async resolving
                IPAddress ipAddress;
                bool parsed = IPAddress.TryParse(server.server, out ipAddress);
                if (!parsed)
                {
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(server.server);
                    ipAddress = ipHostInfo.AddressList[0];
                }
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, server.server_port);

                remote = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);
                remote.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);

                _startConnectTime = DateTime.Now;
                ServerTimer connectTimer = new ServerTimer(3000);
                connectTimer.AutoReset = false;
                connectTimer.Elapsed += connectTimer_Elapsed;
                connectTimer.Enabled = true;
                connectTimer.Server = server;

                connected = false;
                // Completed negociating with local on socks, and now Connect to the remote socks endpoint.
                remote.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), connectTimer);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void connectTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (connected)
            {
                return;
            }
            Server server = ((ServerTimer)sender).Server;
            IStrategy strategy = controller.GetCurrentStrategy();
            if (strategy != null)
            {
                strategy.SetFailure(server);
            }
            Console.WriteLine(String.Format("{0} timed out", server.FriendlyName()));

            // endConnect first?
            remote.Close();
            RetryConnect();
        }

        private void RetryConnect()
        {
            if (retryCount < 4)
            {
                Logging.Debug("Connection failed, retrying");
                StartConnect();
                retryCount++;
            }
            else
            {
                this.Close();
            }
        }

        // completed sock negociating with local and also connected with the remote socks server now.
        private void ConnectCallback(IAsyncResult ar)
        {
            Server server = null;
            if (closed)
            {
                return;
            }
            try
            {
                ServerTimer timer = (ServerTimer)ar.AsyncState;
                server = timer.Server;
                timer.Elapsed -= connectTimer_Elapsed;
                timer.Enabled = false;
                timer.Dispose();

                // Complete the connection.
                remote.EndConnect(ar);

                connected = true;

                if (this.relay.LogTraffic)
                    Logging.LogNetTraffic("***Connected with remote.");

                //Console.WriteLine("Socket connected to {0}",
                //    remote.RemoteEndPoint.ToString());

                var latency = DateTime.Now - _startConnectTime;
                IStrategy strategy = controller.GetCurrentStrategy();
                if (strategy != null)
                {
                    strategy.UpdateLatency(server, latency);
                }

                StartPipe();
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                if (server != null)
                {
                    IStrategy strategy = controller.GetCurrentStrategy();
                    if (strategy != null)
                    {
                        strategy.SetFailure(server);
                    }
                }
                Logging.LogUsefulException(e);
                RetryConnect();
            }
        }

        private void StartPipe()
        {
            if (closed)
            {
                return;
            }
            try
            {
                remote.BeginReceive(remoteRecvBuffer, 0, RecvSize, 0,
                    new AsyncCallback(PipeRemoteReceiveCallback), null);
                connection.BeginReceive(connetionRecvBuffer, 0, RecvSize, 0,
                    new AsyncCallback(PipeConnectionReceiveCallback), null);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void PipeRemoteReceiveCallback(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                int bytesRead = remote.EndReceive(ar);
                totalRead += bytesRead;

                if (bytesRead > 0)
                {
                    this.lastActivity = DateTime.Now;
                    int bytesToSend;
                    lock (encryptionLock) // need lock?
                    {
                        if (closed)
                        {
                            return;
                        }
                        encryptor.Decrypt(remoteRecvBuffer, bytesRead, remoteSendBuffer, out bytesToSend);
                    }
                    connection.BeginSend(remoteSendBuffer, 0, bytesToSend, 0, new AsyncCallback(PipeConnectionSendCallback), null);

                    if (this.relay.LogTraffic)
                    {
                        Logging.LogNetTraffic("***Pipe raw and decrypted msg from remote, raw and decrypted:");
                        Logging.LogNetTraffic(remoteRecvBuffer);
                        Logging.LogNetTraffic(remoteSendBuffer);
                    }

                    IStrategy strategy = controller.GetCurrentStrategy();
                    if (strategy != null)
                    {
                        strategy.UpdateLastRead(this.server);
                    }
                }
                else
                {
                    //Console.WriteLine("bytesRead: " + bytesRead.ToString());
                    connection.Shutdown(SocketShutdown.Send);
                    connectionShutdown = true;
                    CheckClose();

                    if (totalRead == 0)
                    {
                        // closed before anything received, reports as failure
                        // disable this feature
                        // controller.GetCurrentStrategy().SetFailure(this.server);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void PipeConnectionReceiveCallback(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                int bytesRead = connection.EndReceive(ar);
                totalWrite += bytesRead;

                if (bytesRead > 0)
                {
                    int bytesToSend;
                    lock (encryptionLock)
                    {
                        if (closed)
                        {
                            return;
                        }
                        encryptor.Encrypt(connetionRecvBuffer, bytesRead, connetionSendBuffer, out bytesToSend);
                    }
                    remote.BeginSend(connetionSendBuffer, 0, bytesToSend, 0, new AsyncCallback(PipeRemoteSendCallback), null);

                    if (this.relay.LogTraffic)
                    {
                        Logging.LogNetTraffic("***Pipe raw and encrypted msg from client:");
                        Logging.LogNetTraffic(connetionRecvBuffer);
                        Logging.LogNetTraffic(connetionSendBuffer);
                    }

                    IStrategy strategy = controller.GetCurrentStrategy();
                    if (strategy != null)
                    {
                        strategy.UpdateLastWrite(this.server);
                    }
                }
                else
                {
                    remote.Shutdown(SocketShutdown.Send);
                    remoteShutdown = true;
                    CheckClose();
                }
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void PipeRemoteSendCallback(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                remote.EndSend(ar);
                connection.BeginReceive(this.connetionRecvBuffer, 0, RecvSize, 0,
                    new AsyncCallback(PipeConnectionReceiveCallback), null);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }

        private void PipeConnectionSendCallback(IAsyncResult ar)
        {
            if (closed)
            {
                return;
            }
            try
            {
                connection.EndSend(ar);
                remote.BeginReceive(this.remoteRecvBuffer, 0, RecvSize, 0,
                    new AsyncCallback(PipeRemoteReceiveCallback), null);
            }
            catch (Exception e)
            {
                Logging.LogUsefulException(e);
                this.Close();
            }
        }
    }
}
