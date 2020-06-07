using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    class Client
    {
        Socket _client;

        public void InitialClient(String host, int port)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            
            _client.BeginConnect(new IPEndPoint(IPAddress.Parse(host), port), new AsyncCallback(ConnectCallback), _client);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;

            client.EndConnect(ar);
        }
    }
}
