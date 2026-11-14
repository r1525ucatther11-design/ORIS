using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MiniHttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private bool _isRunning;
        private int _port;
        private string _path;

        public HttpServer()
        {
            LoadSettings();
            _listener = new HttpListener();
            string prefix = $"http://127.0.0.1:{_port}/{_path.Trim('/')}/";
            _listener.Prefixes.Add(prefix);
            _isRunning = false;
        }

        private void LoadSettings()
        {
            try
            {
                string jsonString = File.ReadAllText("settings.json");
                var settings = JsonSerializer.Deserialize<Settings>(jsonString)
                    ?? throw new InvalidOperationException("Не удалось прочитать настройки сервера.");
                _port = settings.Port;
                _path = string.IsNullOrWhiteSpace(settings.Path) ? "connection" : settings.Path;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения settings.json: {ex.Message}. Используются значения по умолчанию.");
                _port = 8888;
                _path = "connection";
            }
        }

        public void Start()
        {
            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось запустить сервер: {ex.Message}");
                return;
            }

            _isRunning = true;
            Console.WriteLine($"Сервер запущен: http://127.0.0.1:{_port}/{_path.Trim('/')}/");
            Console.WriteLine("Введите 'stop' для остановки сервера.");

            _ = Task.Run(ListenLoop);
        }

        private async Task ListenLoop()
        {
            while (_isRunning && _listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    ProcessRequest(context);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            Console.WriteLine($"Получен запрос: {request.HttpMethod} {request.Url.AbsolutePath}");

            try
            {
                string path = request.Url.AbsolutePath;
                string root = $"/{_path.Trim('/')}/";

                if (path == "/" || path == root || path == root.TrimEnd('/'))
                {
                    ServeFile(response, "search-engine.html", "text/html; charset=utf-8");
                }
                else
                {
                    string fileName = path.TrimStart('/');
                    if (File.Exists(fileName))
                    {
                        string contentType = fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
                            ? "text/html; charset=utf-8"
                            : "application/octet-stream";
                        ServeFile(response, fileName, contentType);
                    }
                    else
                    {
                        WriteNotFound(response, $"Файл {path} не найден.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при формировании ответа: {ex.Message}");
                try { response.StatusCode = 500; response.Close(); } catch { }
            }

            Console.WriteLine($"Обработан запрос: {request.HttpMethod} {request.Url.AbsolutePath}");
        }

        private void ServeFile(HttpListenerResponse response, string fileName, string contentType)
        {
            if (!File.Exists(fileName))
            {
                WriteNotFound(response, $"Файл {fileName} не найден.");
                return;
            }

            byte[] buffer = File.ReadAllBytes(fileName);
            response.StatusCode = 200;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private void WriteNotFound(HttpListenerResponse response, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при остановке: {ex.Message}");
            }

            Console.WriteLine("Сервер остановлен.");
        }
    }
}