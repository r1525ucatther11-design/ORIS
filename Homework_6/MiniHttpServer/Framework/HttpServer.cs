using System;
using System.Net;
using System.Threading.Tasks;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly string _prefix;
        private bool _isRunning;
        private RequestHandler? _firstHandler;

        public HttpServer()
        {
            var settings = ConfigurationManager.Instance.Settings;
            string pathPart = string.IsNullOrWhiteSpace(settings.Path) ? "" : settings.Path.Trim('/') + "/";
            _prefix = $"http://127.0.0.1:{settings.Port}/{pathPart}";

            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _isRunning = false;
        }

        public void SetHandler(RequestHandler handler)
        {
            _firstHandler = handler;
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;
            Console.WriteLine($"Сервер запущен: {_prefix}");
            _ = Task.Run(ListenLoop);
        }

        private async Task ListenLoop()
        {
            while (_isRunning && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    
                    if (_firstHandler != null)
                    {
                        await _firstHandler.HandleAsync(context);
                    }
                }
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _listener.Stop();
            _listener.Close();
            Console.WriteLine("Сервер остановлен.");
        }
    }
}