using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    public class Server2
    {
        TcpListener _listener;
        TcpClient _client;

        public event Action OnClientConnect;

        public event Action<byte[]> OnGetMessage;

        public void InitialServer(int port)
        {
            // Создаем слушателя порта
            _listener = new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

            _listener.Start();

            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _listener);
        }

        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            _client = _listener.EndAcceptTcpClient(ar);

            OnClientConnect();

            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _listener);

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
