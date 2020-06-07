using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    public class Client2
    {
        TcpClient _client;

        public event Action<byte[]> OnGetMessage;

        public event Action OnConnected;

        public void InitialClient(String host, int port)
        {
            //_client = new TcpClient(host, port);

            _client = new TcpClient();

            _client.BeginConnect(host, port, new AsyncCallback(ConnectCallback), _client);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
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
                OnGetMessage(state.buffer);

                ReadData();
            }
        }

        public void SendData(byte[] buffer)
        {
            _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
