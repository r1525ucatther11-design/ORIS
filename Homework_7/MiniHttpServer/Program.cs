using System;
using System.Reflection;
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
            
            // Передаем текущую сборку (где лежат контроллеры) в обработчик
            var controllersHandler = new ControllersHandler(Assembly.GetExecutingAssembly());
            
            var notFoundHandler = new NotFoundHandler();

            staticFilesHandler.SetNext(controllersHandler);
            controllersHandler.SetNext(notFoundHandler);

            server.SetHandler(staticFilesHandler);
            server.Start();

            while (true)
            {
                string? cmd = Console.ReadLine();
                if (cmd?.Trim().ToLower() == "stop")
                {
                    server.Stop();
                    break;
                }
            }
        }
    }
}