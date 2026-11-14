using System;

namespace MiniHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer();
            server.Start();

            while (true)
            {
                string? command = Console.ReadLine();
                if (command != null && command.Trim().ToLower() == "stop")
                {
                    server.Stop();
                    break;
                }
            }

            Console.WriteLine("Приложение завершило работу.");
        }
    }
}