using ServiceStack.IO;
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
        Socket _listener;

        Socket _connectedClient;

        public event Action OnClientConnect;

        public event Action<byte[]> OnGetMessage;

        public void InitialServer(int port)
        {
            // Инициализируем сокет для прослушивания 
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            
            _listener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            _listener.Listen(1);
            
            while(true)
            {
                // Ожидаем подулючение клиента
                _listener.BeginAccept(AcceptCallback, _listener);
            }
        }

        private void AcceptCallback(IAsyncResult asyncResult)
        {
            // Завершаем прием подключения клиента
            Socket acceptedSocket = _listener.EndAccept(asyncResult);

            // Оповещаем о подключении
            OnClientConnect?.Invoke();

            // Начинаем прием другого клиента
            //_listener.BeginAccept(AcceptCallback, _listener);

            StateObject state = new StateObject();
            _connectedClient = acceptedSocket;

            state.workSocket = acceptedSocket;

            acceptedSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int countBytesRead = handler.EndReceive(ar);

            if(countBytesRead>0)
            {
                OnGetMessage?.Invoke(state.buffer);
            }
        }

        public void Send(byte[] buffer)
        {
            _connectedClient.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), _connectedClient);
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket handler = (Socket)ar.AsyncState;

            int bytesSend = handler.EndSend(ar);
        }
    }
}
