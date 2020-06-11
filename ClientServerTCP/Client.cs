using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientServerTCP
{
    public class Client
    {
        /// <summary>
        /// Клиент для обмена с сервером
        /// </summary>
        TcpClient _client;

        /// <summary>
        /// Событие при получении массива байт от клиента
        /// </summary>
        public event Action<byte[]> OnGetMessage;

        /// <summary>
        /// Событие подключения к серверу
        /// </summary>
        public event Action OnConnected;

        /// <summary>
        /// Событие отключения от сервера
        /// </summary>
        public event Action OnDisconnect;

        private String _host;
        private Int32 _port;
        private bool _isReadData;
        private int intervalReadData;

        public int ReadBufferSize { get; set; }
        public int IntervalReadData { get => intervalReadData; set => intervalReadData = value < 50 ? 50 : value; }

        public Client()
        {
            ReadBufferSize = 1024;
            IntervalReadData = 1000;
        }

        /// <summary>
        /// Инициализация клиента
        /// </summary>
        /// <param name="host">Хост</param>
        /// <param name="port">Порт</param>
        public void InitialClient(String host, int port)
        {
            _client = new TcpClient();

            _host = host;
            _port = port;

            // Ожидаем подключения к серверу
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

            // Оповещаем о подключении клиента к серверу
            OnConnected?.Invoke();

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

        /// <summary>
        /// Отправка данных 
        /// </summary>
        /// <param name="buffer"></param>
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
                OnDisconnect?.Invoke();
            }
        }

        public void Disconnect()
        {
            if (_client != null)
            {
                _client.Close();
            }
        }
    }
}
