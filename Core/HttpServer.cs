using System.Net;
using System.Text.Json;
using CustomHttpServer.Framework.Handlers;

namespace CustomHttpServer.Core;

public class HttpServer
{
    private HttpListener _listener;
    private Settings _settings;
    private bool _isRunning;

    public void Start()
    {
        // Проверяем settings.json
        if (!File.Exists("settings.json"))
        {
            Console.WriteLine("Ошибка: settings.json не найден!");
            return;
        }

        // Читаем настройки
        string json = File.ReadAllText("settings.json");
        _settings = JsonSerializer.Deserialize<Settings>(json);

        // Проверяем search-engine.html
        string searchPath = Path.Combine("static", "search-engine.html");
        if (!File.Exists(searchPath))
        {
            Console.WriteLine($"Ошибка: файл {searchPath} не найден!");
            return;
        }

        // Создаём и запускаем сервер
        _listener = new HttpListener();
        string prefix = $"http://{_settings.Server.Host}:{_settings.Server.Port}/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();
        _isRunning = true;

        Console.WriteLine($"Сервер запущен и слушает: {prefix}");
    }

    public async Task ListenAsync()
    {
        while (_isRunning)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = ProcessRequestAsync(context); // обрабатываем в фоне
            }
            catch (HttpListenerException)
            {
                break; // сервер остановлен
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        Handler staticFileHandler = new StaticFileHandler();
        Handler controllerHandler = new ControllerHandler();

        staticFileHandler.Successor = controllerHandler;

        Console.WriteLine("Пришёл запрос");
        await staticFileHandler.HandleRequest(context);

        Console.WriteLine($"Обработан запрос: {context.Request.Url}");
    }

    public void Stop()
    {
        if (_listener != null && _listener.IsListening)
        {
            _isRunning = false;
            _listener.Stop();
            Console.WriteLine("Сервер завершил свою работу");
        }
    }
}