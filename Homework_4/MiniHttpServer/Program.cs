using System;
using MiniHttpServer.Framework;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer();
            
            var staticFilesHandler = new StaticFilesHandler();
            var notFoundHandler = new NotFoundHandler();

            staticFilesHandler.SetNext(notFoundHandler);
            server.SetHandler(staticFilesHandler);

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