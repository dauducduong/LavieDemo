using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LavieDemo
{
    class TCPServer
    {
        private TcpListener _server;
        private bool _isRunning;
        private Thread _serverThread;

        public event Action<string> ClientConnected;
        public event Action<string> DataReceived;
        public event Action<string> ErrorOccurred;

        public TCPServer(string ipAddress, int port)
        {
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            if (_isRunning)
                return;

            _isRunning = true;
            _server.Start();
            _serverThread = new Thread(ListenForClients);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _server?.Stop();
            _serverThread?.Join();
        }

        private void ListenForClients()
        {
            try
            {
                while (_isRunning)
                {
                    var client = _server.AcceptTcpClient();
                    ClientConnected?.Invoke($"Client connected: {client.Client.RemoteEndPoint}");
                    var clientThread = new Thread(HandleClientComm);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Error: {ex.Message}");
            }
        }

        private void HandleClientComm(object clientObj)
        {
            var client = (TcpClient)clientObj;
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    DataReceived?.Invoke($"Received: {receivedData}");
                    // Echo the data back to the client
                    stream.Write(buffer, 0, bytesRead);
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke($"Client communication error: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }
    }
}
