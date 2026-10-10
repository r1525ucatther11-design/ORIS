using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Http;
using CustomHttpServer.Http.HttpResponses;

namespace CustomHttpServer.Controllers
{
    [HttpController("Dashboard")]
    public class DashboardController : ControllerBase
    {
        [Get("profile")]
        public IHttpResponseTypeResult GetProfile(int userId)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "static", "Dashboard", "profile.html");
            string html = File.ReadAllText(path);
            html = html.Replace("{{userId}}", userId.ToString());
            return Html(html);
        }

        [Get("activities")]
        public IHttpResponseTypeResult GetActivities(int userId)
        {
            var userActivities = new Dictionary<int, string[]>
            {
                [1]   = new[] { "Вход в систему — 08:15", "Поиск «C#» — 08:30", "Открыт профиль — 09:00" },
                [42]  = new[] { "Вход в систему — 09:00", "Поиск «steam» — 09:15", "Открыт профиль — 09:30" },
                [777] = new[] { "Вход — 07:00", "Поиск «js» — 07:15", "Выход — 07:30" },
                [999] = new[] { "Вход в систему — 22:00", "Поиск «ночь» — 22:30", "Выход — 23:00" },
            };

            var activities = userActivities.TryGetValue(userId, out var list)
                ? list
                : new[] { $"Для пользователя №{userId} активностей не найдено" };

            string itemsHtml = string.Join("", activities.Select(a => $"<li>{a}</li>"));

            string path = Path.Combine(AppContext.BaseDirectory, "static", "Dashboard", "activities.html");
            string html = File.ReadAllText(path)
                .Replace("{{userId}}", userId.ToString())
                .Replace("{{activities}}", itemsHtml);

            return Html(html);
        }

        [Get("json")]
        public IHttpResponseTypeResult GetJson()
        {
            return Json(new { UserId = 42, Name = "Saad", Status = "Active" });
        }
    }
}