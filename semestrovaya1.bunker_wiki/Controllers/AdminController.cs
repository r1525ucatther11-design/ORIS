using System;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;

public class AdminController : IController
{
    public bool CanHandle(string url, string method)
    {
        return url.StartsWith("/admin");
    }

    public void Handle(HttpListenerContext context)
    {
        var req = context.Request;
        var res = context.Response;
        string url = req.Url.AbsolutePath;

        var cookie = req.Cookies["user"];
        if (cookie == null || cookie.Value != "admin")
        {
            res.StatusCode = 303;
            res.RedirectLocation = "/login";
            res.OutputStream.Close();
            return;
        }

        if (url == "/admin" && req.HttpMethod == "GET")
        {
            var db = new DatabaseContext();
            var cards = db.GetAllCards();
            var categories = db.GetAllCategories();
            
            StringBuilder rows = new StringBuilder();
            foreach (var card in cards)
            {
                rows.Append("<tr>");
                rows.Append($"<td>{card.Id}</td>");
                rows.Append($"<td>{card.Title}</td>");
                rows.Append($"<td>{card.Category}</td>");
                rows.Append($"<td>");
                rows.Append($"<a href='/admin/edit?id={card.Id}' class='btn-edit'>Ред.</a> ");
                rows.Append($"<a href='/admin/delete?id={card.Id}' class='btn-delete' onclick='return confirm(\"Точно удалить?\")'>Удалить</a>");
                rows.Append($"</td>");
                rows.Append("</tr>");
            }

            var model = new { error = "", TableRows = rows.ToString(), Categories = categories };
            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Templates/admin.html", model);
            
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            res.ContentType = "text/html; charset=utf-8";
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
        else if (url == "/admin/delete" && req.HttpMethod == "GET")
        {
            if (int.TryParse(req.QueryString["id"], out int id))
            {
                new DatabaseContext().DeleteCard(id);
            }
            res.StatusCode = 303;
            res.RedirectLocation = "/admin";
            res.OutputStream.Close();
        }
        else if (url == "/admin/edit" && req.HttpMethod == "GET")
        {
            if (int.TryParse(req.QueryString["id"], out int id))
            {
                var db = new DatabaseContext();
                var card = db.GetAllCards().FirstOrDefault(c => c.Id == id);
                var categories = db.GetAllCategories();
                
                if (card != null)
                {
                    var model = new { Card = card, Categories = categories };
                    var renderer = new HtmlTemplateRenderer();
                    string html = renderer.RenderFromFile("Templates/admin_edit.html", model);
                    byte[] buffer = Encoding.UTF8.GetBytes(html);
                    res.ContentType = "text/html; charset=utf-8";
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.OutputStream.Close();
                    return;
                }
            }
            res.StatusCode = 404;
            res.OutputStream.Close();
        }
        else if (url == "/admin/update" && req.HttpMethod == "POST")
        {
            int.TryParse(req.QueryString["id"], out int id);
            string body;
            using (var reader = new StreamReader(req.InputStream, Encoding.UTF8)) body = reader.ReadToEnd();
            var form = System.Web.HttpUtility.ParseQueryString(body);

            new DatabaseContext().UpdateCard(id, form["title"], form["category"], form["description"]);
            
            res.StatusCode = 303;
            res.RedirectLocation = "/admin";
            res.OutputStream.Close();
        }
        else if (url == "/admin/create" && req.HttpMethod == "POST")
        {
            string body;
            using (var reader = new StreamReader(req.InputStream, Encoding.UTF8)) body = reader.ReadToEnd();
            var form = System.Web.HttpUtility.ParseQueryString(body);
            
            string title = form["title"];
            string category = form["category"];
            string description = form["description"];

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                var db = new DatabaseContext();
                var cards = db.GetAllCards();
                var categories = db.GetAllCategories();
                
                StringBuilder rows = new StringBuilder();
                foreach (var card in cards)
                {
                    rows.Append($"<tr><td>{card.Id}</td><td>{card.Title}</td><td>{card.Category}</td>");
                    rows.Append($"<td><a href='/admin/edit?id={card.Id}' class='btn-edit'>Ред.</a> <a href='/admin/delete?id={card.Id}' class='btn-delete'>Удалить</a></td></tr>");
                }

                var model = new { error = "Заполните все поля!", TableRows = rows.ToString(), Categories = categories };
                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Templates/admin.html", model);
                byte[] buffer = Encoding.UTF8.GetBytes(html);
                res.ContentType = "text/html; charset=utf-8";
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
            }
            else
            {
                new DatabaseContext().AddCard(title, category, description);
                res.StatusCode = 303;
                res.RedirectLocation = "/cards";
                res.OutputStream.Close();
            }
        }
    }
}