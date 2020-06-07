using System.Net.Sockets;

namespace ClientServerTCP
{
    public class StateObject
    { 
        public Socket workSocket = null;
        
        public const int BufferSize = 1024;
         
        public byte[] buffer = new byte[BufferSize];
    }
}
