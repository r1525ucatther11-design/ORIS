using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Http;
using CustomHttpServer.Http.HttpResponses;

namespace CustomHttpServer.Controllers
{
    [HttpController("Search")]
    public class SearchController : ControllerBase
    {
        [Get("go")]
        public IHttpResponseTypeResult Go([FromQuery] string query)
        {
            query = (query ?? "").Trim().ToLower();

            Console.WriteLine($"===== Поиск: {query} =====");

            var routes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["steam"]      = "index.html",
                ["profile"]    = "Dashboard/profile.html",
                ["профиль"]    = "Dashboard/profile.html",
                ["activities"] = "Dashboard/activities.html",
                ["активности"] = "Dashboard/activities.html",
                ["login"]      = "Auth/login.html",
                ["логин"]      = "Auth/login.html",
            };

            if (!routes.TryGetValue(query, out var fileName))
            {
                return Html(
                    $"<h1>Ничего не найдено по запросу: {query}</h1>" +
                    "<p><a href='/search-engine.html'>Назад</a></p>"
                );
            }

            string path = Path.Combine(AppContext.BaseDirectory, "static",
                fileName.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(path))
            {
                return Html($"<h1>Файл {fileName} не найден</h1>");
            }

            return Html(File.ReadAllText(path));
        }
    }
}