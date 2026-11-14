using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static List<IController> _controllers = new List<IController>
    {
        new HomeController(),
        new CardsController(),
        new AdminController(),
        new LoginController()
    };

    static void Main()
    {
        HttpListener server = new HttpListener();
        server.Prefixes.Add("http://+:8080/");
        server.Start();
        Console.WriteLine("Сервер Бункера запущен на http://localhost:8080/");

        Thread consoleThread = new Thread(() =>
        {
            while (true)
            {
                string input = Console.ReadLine();
                if (input != null && input.Trim().ToLower() == "стоп")
                {
                    server.Stop();
                    break;
                }
            }
        });
        consoleThread.IsBackground = true;
        consoleThread.Start();

        try
        {
            while (server.IsListening)
            {
                HttpListenerContext context = server.GetContext();
                ThreadPool.QueueUserWorkItem((state) => ProcessRequest(context));
            }
        }
        catch (HttpListenerException) { }
        catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
    }

    static void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest req = context.Request;
        HttpListenerResponse res = context.Response;
        string url = req.Url.AbsolutePath;
        string method = req.HttpMethod;

        try
        {
            if (url.StartsWith("/css/"))
            {
                string filePath = url.TrimStart('/');
                if (File.Exists(filePath))
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    res.ContentType = "text/css";
                    res.OutputStream.Write(fileBytes, 0, fileBytes.Length);
                }
                else res.StatusCode = 404;
                res.OutputStream.Close();
                return;
            }

            foreach (var controller in _controllers)
            {
                if (controller.CanHandle(url, method))
                {
                    controller.Handle(context);
                    return;
                }
            }

            res.StatusCode = 404;
            byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
            res.OutputStream.Write(buffer, 0, buffer.Length);
            res.OutputStream.Close();
        }
        catch (Exception ex)
        {
            try
            {
                res.StatusCode = 500;
                byte[] buffer = Encoding.UTF8.GetBytes("Ошибка: " + ex.Message);
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
            }
            catch { }
        }
    }
}