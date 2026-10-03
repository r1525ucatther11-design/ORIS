using System.Net;
using System.Text.Json;

namespace CustomHttpServer.Core;

public class HttpServer
{
    private HttpListener _listener;
    private Settings _settings;

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
        string prefix = $"http://{_settings.Server.Host}:{_settings.Server.Port}/{_settings.Server.Path}/";
        _listener.Prefixes.Add(prefix);
        _listener.Start();

        Console.WriteLine($"Сервер запущен и слушает: {prefix}");
    }

    public async Task ListenAsync()
    {
        while (_listener.IsListening)
        {
            var context = await _listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }

    private async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        Console.WriteLine($"Пришёл запрос: {request.Url.LocalPath}");

        // Берём путь из запроса
        string path = request.Url.LocalPath;

        // Убираем /connection/ из начала пути
        string prefix = $"/{_settings.Server.Path}/";
        if (path.StartsWith(prefix))
            path = path.Substring(prefix.Length);
        else if (path == $"/{_settings.Server.Path}")
            path = "";

        // Если путь пустой — отдаём index.html
        if (string.IsNullOrEmpty(path) || path == "/")
            path = "index.html";

        // Строим путь к файлу
        string filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "static",
            path.Replace('/', Path.DirectorySeparatorChar)
        );

        // Если файла нет — 404
        if (!File.Exists(filePath))
        {
            response.StatusCode = 404;
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "static", "404.html");
        }

        // Определяем Content-Type
        string extension = Path.GetExtension(filePath);
        response.ContentType = ContentTypeHelper.Get(extension);

        // Читаем и отправляем файл
        byte[] buffer = await File.ReadAllBytesAsync(filePath);
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();

        Console.WriteLine("Запрос обработан");
    }

    public void Stop()
    {
        if (_listener != null && _listener.IsListening)
        {
            _listener.Stop();
            Console.WriteLine("Сервер завершил свою работу");
        }
    }
}