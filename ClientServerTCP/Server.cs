using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    public class Server
    {
        /// <summary>
        /// Слушатель порта
        /// </summary>
        TcpListener _listener;

        /// <summary>
        /// Для обмена данными с клиентом
        /// </summary>
        TcpClient _client;

        /// <summary>
        /// Событие при подключении клиента
        /// </summary>
        public event Action OnClientConnect;

        /// <summary>
        /// Событие при получении массива байт от клиента
        /// </summary>
        public event Action<byte[]> OnGetMessage;


        public event Action OnDisconnectClient;

        /// <summary>
        /// Интревал между чтением данных в секундах, мс
        /// </summary>
        public int IntervalReadData { get => intervalReadData; set => intervalReadData = value < 50 ? 50 : value; }

        /// <summary>
        /// Размер буфера для чтения
        /// </summary>
        public int ReadBufferSize { get; set; }


        private bool _isReadData;
        private int intervalReadData;

        public Server()
        {
            ReadBufferSize = 1024;
            IntervalReadData = 1000;
        }

        /// <summary>
        /// Инициализация сервера
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void InitialServer(String host, int port)
        {
            // Создаем слушателя порта
            _listener = new TcpListener(new IPEndPoint(IPAddress.Parse(host), port));

            // Старт прослушивания портм
            _listener.Start();

            // Ожидаем подключения клиента
            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _listener);
        }

        /// <summary>
        /// Обработка подключения клиента
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptTcpClientCallback(IAsyncResult ar)
        {
            // Если слушатель порта закрыт или уничтожен
            if (_listener.Server == null || !_listener.Server.IsBound)
            {
                return;
            }

            // Запоминаем клиента
            _client = _listener.EndAcceptTcpClient(ar);

            // Оповещаем о подключении клиента
            OnClientConnect();

            // Принимаем следующее подключения клиента
            _listener.BeginAcceptTcpClient(new AsyncCallback(AcceptTcpClientCallback), _listener);

            // Запуск чтения данных
            Task.Factory.StartNew(StarReadData);
        }

        /// <summary>
        /// Проверка и чтение данных
        /// </summary>
        public void StarReadData()
        {
            _isReadData = true;

            // Пока влючено чтение данных
            while (_isReadData)
            {
                if (_client.Client != null)
                {
                    // Если клиент подключен и есть данные для чтения
                    if (_client.Connected && _client.Available > 0)
                    {
                        // Производим чтение
                        ReadData();
                    }
                }

                // Ожидаем 
                Thread.Sleep(IntervalReadData);
            }
        }

        /// <summary>
        /// Считываем данные и оповещаем 
        /// </summary>
        public void ReadData()
        {
            // Если нет связи с клиентом выходим
            if (_client == null || !_client.Connected)
            {
                return;
            }

            byte[] buffer = new byte[ReadBufferSize];

            // Асинхронно считываем данные
            var read = _client.GetStream()?.ReadAsync(buffer, 0, buffer.Length);

            // Если данные есть, то оповещаем
            if (read.Result > 0)
            {
                OnGetMessage(buffer);
            }
        }

        // Отправка данных 
        public void SendData(byte[] buffer)
        {
            // Если нет связи с клиентом выходим
            if (_client == null || !_client.Connected)
            {
                return;
            }

            try
            {
                // Отправляем данные асинхронно
                _client.GetStream().WriteAsync(buffer, 0, buffer.Length);
            }
            catch (IOException exception)
            {
                // Оповещаем об отключеннии пользователя
                OnDisconnectClient?.Invoke();
            }
        }

        public void ShutdownServer()
        {
            _isReadData = false;

            if (_listener != null)
            {
                _listener.Stop();
            }

            if (_client != null)
            {
                _client.Close();
            }

        }
    }
}
