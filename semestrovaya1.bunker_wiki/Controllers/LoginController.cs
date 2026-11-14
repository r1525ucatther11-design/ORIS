using System;
using System.IO;
using System.Net;
using System.Text;

public class LoginController : IController
{
    public bool CanHandle(string url, string method)
    {
        return (url == "/login" && method == "GET") || 
               (url == "/login" && method == "POST") ||
               (url == "/logout" && method == "GET");
    }
    public void Handle(HttpListenerContext context)
    {
        var req = context.Request;
        var res = context.Response;
        string url = req.Url.AbsolutePath;

        if (url == "/login" && req.HttpMethod == "GET")
        {
            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Templates/login.html", new { error = "" });
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            res.ContentType = "text/html; charset=utf-8";
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
        else if (url == "/login" && req.HttpMethod == "POST")
        {
            string body;
            using (var reader = new StreamReader(req.InputStream, Encoding.UTF8)) 
                body = reader.ReadToEnd();
            var form = System.Web.HttpUtility.ParseQueryString(body);
            
            string login = form["login"];
            string password = form["password"];

            var db = new DatabaseContext();
            var user = db.GetUserByLogin(login);

            if (user != null && password == "admin123")
            {
                res.AppendHeader("Set-Cookie", "user=admin; Path=/; HttpOnly");
                res.StatusCode = 303;
                res.RedirectLocation = "/admin";
                res.OutputStream.Close();
            }
            else
            {
                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Templates/login.html", 
                    new { error = "Неверный логин или пароль" });
                byte[] buffer = Encoding.UTF8.GetBytes(html);
                res.ContentType = "text/html; charset=utf-8";
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
            }
        }
        else if (url == "/logout" && req.HttpMethod == "GET")
        {
            res.AppendHeader("Set-Cookie", "user=; Path=/; Max-Age=0");
            res.StatusCode = 303;
            res.RedirectLocation = "/login";
            res.OutputStream.Close();
        }
    }
}