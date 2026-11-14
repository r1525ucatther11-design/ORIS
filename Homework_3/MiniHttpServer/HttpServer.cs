using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly ServerSettings _settings;
        private readonly string _publicRoot;
        private readonly string _prefix;
        private bool _isRunning;

        public HttpServer()
        {
            _settings = ConfigurationManager.Instance.Settings;

            _publicRoot = Path.GetFullPath(_settings.PublicDirectoryPath);

            string pathPart = string.IsNullOrWhiteSpace(_settings.Path)
                ? ""
                : _settings.Path.Trim('/') + "/";

            _prefix = $"http://127.0.0.1:{_settings.Port}/{pathPart}";

            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _isRunning = false;
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
            Console.WriteLine($"Сервер запущен: {_prefix}");
            Console.WriteLine($"Каталог статики: {_publicRoot}");
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
                catch (HttpListenerException) { break; }
                catch (ObjectDisposedException) { break; }
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
                string absolutePath = request.Url?.AbsolutePath ?? "/";
                string urlPath = Uri.UnescapeDataString(absolutePath);

                string pathPart = string.IsNullOrWhiteSpace(_settings.Path)
                    ? ""
                    : "/" + _settings.Path.Trim('/');

                if (!string.IsNullOrEmpty(pathPart) &&
                    urlPath.StartsWith(pathPart, StringComparison.OrdinalIgnoreCase))
                {
                    urlPath = urlPath.Substring(pathPart.Length);
                }

                if (string.IsNullOrEmpty(urlPath))
                {
                    urlPath = "/";
                }

                if (urlPath == "/")
                {
                    ServeText(response, 200,
                        "<!DOCTYPE html><html lang=\"ru\"><head><meta charset=\"utf-8\">" +
                        "<title>MiniHttpServer</title></head><body>" +
                        "<h1>MiniHttpServer</h1><ul>" +
                        "<li><a href=\"SearcherFinder/\">SearcherFinder</a></li>" +
                        "<li><a href=\"Satisfactory/\">Satisfactory</a></li>" +
                        "<li><a href=\"Steam/\">Steam</a></li>" +
                        "</ul></body></html>");
                    return;
                }

                string relative = urlPath.TrimStart('/');

                if (urlPath.EndsWith("/"))
                {
                    relative += "index.html";
                }

                string fullPath = Path.GetFullPath(Path.Combine(_publicRoot, relative));

                if (!fullPath.StartsWith(_publicRoot + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fullPath, _publicRoot, StringComparison.OrdinalIgnoreCase))
                {
                    WriteNotFound(response, $"Недопустимый путь: {urlPath}");
                    return;
                }

                if (!File.Exists(fullPath))
                {
                    WriteNotFound(response, $"Файл {urlPath} не найден.");
                    return;
                }

                string contentType = GetContentType(fullPath);
                ServeFile(response, fullPath, contentType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при формировании ответа: {ex.Message}");
                try { response.StatusCode = 500; response.Close(); } catch { }
            }
            finally
            {
                Console.WriteLine($"Обработан запрос: {request.HttpMethod} {request.Url.AbsolutePath}");
            }
        }

        private static string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();

            return ext switch
            {
                ".html" or ".htm" => "text/html; charset=utf-8",
                ".css"            => "text/css; charset=utf-8",
                ".js"             => "text/javascript; charset=utf-8",
                ".json"           => "application/json; charset=utf-8",
                ".png"            => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".svg"            => "image/svg+xml",
                ".gif"            => "image/gif",
                ".ico"            => "image/x-icon",
                ".woff"           => "font/woff",
                ".woff2"          => "font/woff2",
                ".pdf"            => "application/pdf",
                ".txt"            => "text/plain; charset=utf-8",
                _                 => "application/octet-stream",
            };
        }

        private void ServeFile(HttpListenerResponse response, string fullPath, string contentType)
        {
            byte[] buffer = File.ReadAllBytes(fullPath);
            response.StatusCode = 200;
            response.ContentType = contentType;
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private void ServeText(HttpListenerResponse response, int status, string html)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.StatusCode = status;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private void WriteNotFound(HttpListenerResponse response, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";
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