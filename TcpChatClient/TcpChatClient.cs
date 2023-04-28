using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TcpChatClient
{
    public static class TcpChatClient
    {
        private static TcpClient client;
        private static NetworkStream stream;

        public static event Action onDisconnect;
        public static event Action onConnect;
        public static event Action<string> onReceiveMessage;

        public static void Connect(string host, int port, string userName)
        {
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.Unicode.GetBytes(message);
                stream.Write(data, 0, data.Length);

                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start(); //старт потока

                onConnect?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // отправка сообщений
        public static void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        // получение сообщений
        private static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    onReceiveMessage?.Invoke(builder.ToString());
                }
                catch
                {
                    Disconnect();
                }
            }
        }

        public static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента

            onDisconnect?.Invoke();
        }
    }
}
