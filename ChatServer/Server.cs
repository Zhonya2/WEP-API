using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    // Інформація про підключеного клієнта
    public class ClientInfo
    {
        public string Name { get; set; } = "Unknown";
        public TcpClient TcpClient { get; }
        public StreamWriter Writer { get; }

        public ClientInfo(TcpClient client, StreamWriter writer)
        {
            TcpClient = client;
            Writer = writer;
        }
    }

    public static class Server
    {
        private const int Port = 5050;
        private const string LogFile = "server.log";

        private static readonly object ClientsLock = new();
        private static readonly List<ClientInfo> Clients = new();
        private static readonly Dictionary<string, ClientInfo> ClientsByName = new();
        private static readonly object LogLock = new();

        public static async Task RunAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            Log($"Сервер запущено на порту {Port}");
            Console.WriteLine($"[INFO] Сервер запущено на порту {Port}. Очікування клієнтів...");

            while (true)
            {
                TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(tcpClient); // обробляємо клієнта асинхронно
            }
        }

        private static async Task HandleClientAsync(TcpClient tcpClient)
        {
            string clientEndPoint = tcpClient.Client.RemoteEndPoint?.ToString() ?? "невідомо";
            ClientInfo? client = null;

            try
            {
                using NetworkStream networkStream = tcpClient.GetStream();
                using StreamReader reader = new StreamReader(networkStream, Encoding.UTF8);
                StreamWriter writer = new StreamWriter(networkStream, Encoding.UTF8)
                {
                    AutoFlush = true
                };

                client = new ClientInfo(tcpClient, writer);

                // Спочатку просимо нік
                await writer.WriteLineAsync("Введіть свій нік у форматі: /nick ВашНік");

                string? line;
                while (true)
                {
                    line = await reader.ReadLineAsync();
                    if (line == null)
                    {
                        tcpClient.Close();
                        return;
                    }

                    if (line.StartsWith("/nick ", StringComparison.OrdinalIgnoreCase))
                    {
                        string nick = line.Substring(6).Trim();

                        if (string.IsNullOrWhiteSpace(nick))
                        {
                            await writer.WriteLineAsync("Нік не може бути порожнім. Спробуйте ще раз: /nick ВашНік");
                            continue;
                        }

                        lock (ClientsLock)
                        {
                            if (ClientsByName.ContainsKey(nick))
                            {
                                writer.WriteLine("Такий нік вже використовується. Перепідключіться з іншим ніком.");
                                Log($"Клієнт {clientEndPoint} спробував нік '{nick}', але він вже зайнятий.");
                                return;
                            }

                            client.Name = nick;
                            Clients.Add(client);
                            ClientsByName[nick] = client;
                        }

                        Log($"Клієнт {clientEndPoint} встановив нік '{client.Name}'");
                        Console.WriteLine($"[Підключення] {client.Name} ({clientEndPoint})");

                        await BroadcastAsync($"*** {client.Name} приєднався до чату ***", client);
                        await writer.WriteLineAsync("Вітаємо у чаті, " + client.Name + "!");
                        await writer.WriteLineAsync("Команди:");
                        await writer.WriteLineAsync("  /pm Ім'яКористувача повідомлення  - приватне повідомлення");
                        await writer.WriteLineAsync("  /exit                             - вийти з чату");
                        break;
                    }
                    else
                    {
                        await writer.WriteLineAsync("Спочатку задайте нік: /nick ВашНік");
                    }
                }

                // Основний цикл отримання повідомлень
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0)
                        continue;

                    if (line.Equals("/exit", StringComparison.OrdinalIgnoreCase))
                    {
                        await writer.WriteLineAsync("Вихід з чату...");
                        break;
                    }

                    if (line.StartsWith("/pm ", StringComparison.OrdinalIgnoreCase))
                    {
                        await HandlePrivateMessage(client, line);
                    }
                    else
                    {
                        string msg = $"{client.Name}: {line}";
                        Console.WriteLine("[MSG] " + msg);
                        Log("MSG " + msg);
                        await BroadcastAsync(msg, client);
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Помилка з клієнтом {clientEndPoint}: {ex.Message}");
            }
            finally
            {
                ClientInfo? disconnectedClient = null;

                lock (ClientsLock)
                {
                    if (client != null && Clients.Contains(client))
                    {
                        disconnectedClient = client;
                        Clients.Remove(client);

                        if (!string.IsNullOrEmpty(client.Name))
                        {
                            ClientsByName.Remove(client.Name);
                        }
                    }
                }

                tcpClient.Close();

                if (disconnectedClient != null)
                {
                    Console.WriteLine($"[Відключення] {disconnectedClient.Name} ({clientEndPoint})");
                    Log($"Відключення клієнта {disconnectedClient.Name} ({clientEndPoint})");
                    _ = BroadcastAsync($"*** {disconnectedClient.Name} вийшов з чату ***", disconnectedClient);
                }
            }
        }

        private static async Task BroadcastAsync(string message, ClientInfo? sender = null)
        {
            List<ClientInfo> snapshot;

            lock (ClientsLock)
            {
                snapshot = new List<ClientInfo>(Clients);
            }

            foreach (var client in snapshot)
            {
                if (sender != null && ReferenceEquals(client, sender))
                    continue;

                try
                {
                    await client.Writer.WriteLineAsync(message);
                }
                catch
                {
                    // Якщо не вдалося надіслати — скоріш за все клієнт відключився
                }
            }
        }

        private static async Task HandlePrivateMessage(ClientInfo sender, string line)
        {
            // Формат: /pm Ім'яКористувача повідомлення
            string rest = line.Substring(4).Trim();
            int spaceIndex = rest.IndexOf(' ');

            if (spaceIndex <= 0)
            {
                await sender.Writer.WriteLineAsync("Невірний формат /pm. Використання: /pm Ім'яКористувача повідомлення");
                return;
            }

            string targetName = rest.Substring(0, spaceIndex);
            string messageText = rest.Substring(spaceIndex + 1).Trim();

            ClientInfo? target;
            lock (ClientsLock)
            {
                ClientsByName.TryGetValue(targetName, out target);
            }

            if (target == null)
            {
                await sender.Writer.WriteLineAsync($"Користувача з ніком '{targetName}' не знайдено.");
                return;
            }

            string msgToTarget = $"[PM від {sender.Name}] {messageText}";
            string msgToSender = $"[PM до {target.Name}] {messageText}";

            try
            {
                await target.Writer.WriteLineAsync(msgToTarget);
                await sender.Writer.WriteLineAsync(msgToSender);

                Log($"PM {sender.Name} -> {target.Name}: {messageText}");
                Console.WriteLine($"[PM] {sender.Name} -> {target.Name}: {messageText}");
            }
            catch (Exception ex)
            {
                await sender.Writer.WriteLineAsync("Не вдалося відправити приватне повідомлення: " + ex.Message);
            }
        }

        private static void Log(string text)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
            lock (LogLock)
            {
                File.AppendAllText(LogFile, line + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
