using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    public class Client
    {
        TcpClient _client;

        public event Action<byte[]> OnGetMessage;

        public event Action OnConnected;

        private String _host;
        private Int32 _port;

        public void InitialClient(String host, int port)
        {
            _client = new TcpClient();

            _host = host;
            _port = port;

            _client.BeginConnect(host, port, new AsyncCallback(ConnectCallback), _client);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            if (!_client.Connected)
            {
                _client.Close();

                InitialClient(_host, _port);

                return;
            }

            _client.EndConnect(ar);

            OnConnected?.Invoke();

            ReadData();
        }

        public void ReadData()
        {
            StateObject state = new StateObject();

            var read = _client.GetStream().ReadAsync(state.buffer, 0, StateObject.BufferSize);

            if (read.Result > 0)
            {
                OnGetMessage?.Invoke(state.buffer);

                ReadData();
            }
        }

        public void SendData(byte[] buffer)
        {
            _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
        }


    }
}
