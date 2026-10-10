using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Http;
using CustomHttpServer.Http.HttpResponses;

namespace CustomHttpServer.Controllers
{
    [HttpController("auth")]
    internal class AuthController : ControllerBase
    {
        [Get("login")]
        public IHttpResponseTypeResult Login()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "static", "Auth", "login.html");
            return Html(File.ReadAllText(path));
        }

        [Post("login")]
        public IHttpResponseTypeResult Login(string login, string password)
        {
            Console.WriteLine("===== POST /auth/login =====");
            Console.WriteLine($"Логин:  {login}");
            Console.WriteLine($"Пароль: {password}");
            Console.WriteLine("============================");

            return Html(
                $"<h1>Привет, {login}!</h1>" +
                "<p><a href='/search-engine.html'>Назад на главную</a></p>"
            );
        }
    }
}