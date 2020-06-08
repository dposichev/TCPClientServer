using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    public class Server
    {
        TcpListener _listener;
        TcpClient _client;

        public event Action OnClientConnect;

        public event Action<byte[]> OnGetMessage;

        private bool _isReadData;

        public void InitialServer(int port)
        {
            // Создаем слушателя порта
            _listener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

            _listener.Start();

            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _listener);
        }

        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            if(_listener.Server == null || !_listener.Server.IsBound)
            {
                return;
            }

            _client = _listener.EndAcceptTcpClient(ar);

            OnClientConnect();

            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _listener);

            StarReadData();
        }

        public void StarReadData()
        {
            _isReadData = true;

            while(_isReadData)
            {
                if (_client.Client != null)
                {
                    if (_client.Connected && _client.Available > 0)
                    {
                        ReadData();
                    }
                }
            }
        }

        public void ReadData()
        {
            StateObject state = new StateObject();

            if(!_client.Connected)
            {
                return;
            }

            var read = _client.GetStream()?.ReadAsync(state.buffer, 0, StateObject.BufferSize);

            if (read.Result > 0)
            {
                OnGetMessage(state.buffer);
            }
        }

        public void SendData(byte[] buffer)
        {
            _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
        }

        public void ShutdownServer()
        {
            _isReadData = false;

            _listener.Stop();
            _client.Close();
            
        }
    }
}
