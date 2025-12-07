using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public static class Client
    {
        private const int Port = 5050;

        public static async Task RunAsync()
        {
            Console.Write("Введіть IP сервера (наприклад 127.0.0.1): ");
            string? host = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(host))
            {
                host = "127.0.0.1";
            }

            try
            {
                using TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(host, Port);
                Console.WriteLine("Підключено до сервера.");

                using NetworkStream networkStream = tcpClient.GetStream();
                using StreamReader reader = new StreamReader(networkStream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(networkStream, Encoding.UTF8)
                {
                    AutoFlush = true
                };

                // Потік на отримання повідомлень від сервера
                _ = Task.Run(async () =>
                {
                    try
                    {
                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            Console.WriteLine(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Помилка при отриманні повідомлень: " + ex.Message);
                    }

                    Console.WriteLine("З'єднання з сервером розірвано.");
                });

                // Ввід ніку
                Console.Write("Введіть ваш нік: ");
                string? nick = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(nick))
                {
                    nick = "User" + new Random().Next(1000, 9999);
                    Console.WriteLine("Порожній нік, встановлено: " + nick);
                }

                await writer.WriteLineAsync("/nick " + nick);

                Console.WriteLine("Тепер можете писати повідомлення.");
                Console.WriteLine("Приватне повідомлення: /pm Ім'яКористувача текст");
                Console.WriteLine("Вихід з чату: /exit");

                // Основний цикл відправки
                while (true)
                {
                    string? message = Console.ReadLine();
                    if (message == null)
                        continue;

                    await writer.WriteLineAsync(message);

                    if (message.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Не вдалося підключитися до сервера: " + ex.Message);
            }

            Console.WriteLine("Клієнт завершив роботу. Натисніть Enter для виходу.");
            Console.ReadLine();
        }
    }
}
