using System;
using System.Net;
using System.Text;

public class HomeController : IController
{
    public bool CanHandle(string url, string method)
    {
        return method == "GET" && (url == "/" || url == "/rules");
    }

    public void Handle(HttpListenerContext context)
    {
        var res = context.Response;
        var renderer = new HtmlTemplateRenderer();
        string file = context.Request.Url.AbsolutePath == "/" ? "Templates/index.html" : "Templates/rules.html";
        
        string html = renderer.RenderFromFile(file, new { });
        byte[] buffer = Encoding.UTF8.GetBytes(html);
        res.ContentType = "text/html; charset=utf-8";
        res.OutputStream.Write(buffer, 0, buffer.Length);
        res.OutputStream.Close();
    }
}