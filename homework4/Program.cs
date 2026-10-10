using CustomHttpServer.Core;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new HttpServer();
        server.Start();

        // Запускаем прослушивание в фоне
        _ = server.ListenAsync();

        // Ждём команду stop
        Console.WriteLine("Введите 'stop' для остановки сервера:");
        while (true)
        {
            string command = Console.ReadLine();
            if (command != null && command.Trim().ToLower() == "stop")
            {
                server.Stop();
                break;
            }
        }
    }
}