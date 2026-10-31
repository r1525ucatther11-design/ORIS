using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

class Program
{
    static void Main()
    {
        //cd kontrolnaya, dotnet run
        HttpListener server = new HttpListener();
        server.Prefixes.Add("http://localhost:8080/");
        server.Start();
        Console.WriteLine("Сервер запущен на http://localhost:8080/");
        Console.WriteLine("Пропишите в браузере: http://localhost:8080/quizzes");

        while (true)
        {
            HttpListenerContext context = server.GetContext();
            HttpListenerRequest req = context.Request;
            HttpListenerResponse res = context.Response;

            string url = req.Url.AbsolutePath;
            string method = req.HttpMethod;

            if (url == "/quizzes" && method == "GET")
            {
                string minStr = req.QueryString["minQuestions"];
                int min = 0;
                if (minStr != null)
                {
                    if (!int.TryParse(minStr, out min) || min < 0)
                    {
                        res.StatusCode = 400;
                        byte[] err = Encoding.UTF8.GetBytes("400 Bad Request: minQuestions must be a non-negative integer");
                        res.OutputStream.Write(err, 0, err.Length);
                        res.OutputStream.Close();
                        continue;
                    }
                }

                string html = "<html><body><h1>Все тесты</h1>";
                html += "<form method='POST' action='/quizzes/create'>";
                html += "Тема: <input type='text' name='topic'>";
                html += "Вопросов: <input type='number' name='questionsCount'>";
                html += "<button type='submit'>Создать</button></form><hr>";
                html += "<ul>";

                int totalCount = 0;
                double sum = 0;

                for (int i = 0; i < Data.List.Count; i++)
                {
                    Test t = Data.List[i];
                    if (t.Count >= min)
                    {
                        string safeName = HttpUtility.HtmlEncode(t.Name);
                        html += "<li>ID: " + t.Id + " | Тема: <a href='/quizzes/details?id=" + t.Id + "'>" + safeName + "</a> | Вопросов: " + t.Count + " | Статус: " + t.Status + "</li>";
                        sum = sum + t.Count;
                        totalCount++;
                    }
                }

                html += "</ul>";
                if (totalCount > 0)
                {
                    html += "<p>Среднее количество: " + (sum / totalCount) + "</p>";
                }
                else
                {
                    html += "<p>Ничего не найдено</p>";
                }
                html += "</body></html>";

                byte[] buffer = Encoding.UTF8.GetBytes(html);
                res.StatusCode = 200;
                res.ContentType = "text/html; charset=utf-8";
                res.OutputStream.Write(buffer, 0, buffer.Length);
                res.OutputStream.Close();
            }
            
            else if (url == "/quizzes/details" && method == "GET")
            {
                int id = 0;
                int.TryParse(req.QueryString["id"], out id);

                Test found = null;
                for (int i = 0; i < Data.List.Count; i++)
                {
                    if (Data.List[i].Id == id)
                    {
                        found = Data.List[i];
                        break;
                    }
                }

                if (found == null)
                {
                    res.StatusCode = 404;
                    byte[] err = Encoding.UTF8.GetBytes("404 Not Found: quiz with this id does not exist");
                    res.OutputStream.Write(err, 0, err.Length);
                    res.OutputStream.Close();
                }
                else
                {
                    string safeName = HttpUtility.HtmlEncode(found.Name);
                    string html = "<html><body>";
                    html += "<h1>" + safeName + "</h1>";
                    html += "<p>ID: " + found.Id + "</p>";
                    html += "<p>Вопросов: " + found.Count + "</p>";
                    html += "<p>Статус: " + found.Status + "</p>";
                    html += "<a href='/quizzes'>Назад</a>";
                    html += "</body></html>";

                    byte[] buffer = Encoding.UTF8.GetBytes(html);
                    res.StatusCode = 200;
                    res.ContentType = "text/html; charset=utf-8";
                    res.OutputStream.Write(buffer, 0, buffer.Length);
                    res.OutputStream.Close();
                }
            }

            else if (url == "/quizzes/create" && method == "POST")
            {
                StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding);
                string body = reader.ReadToEnd();
                var form = HttpUtility.ParseQueryString(body);

                string topic = form["topic"];
                string countStr = form["questionsCount"];
                int count = 0;
                
                if (topic == null) topic = "";
                if (countStr != null) int.TryParse(countStr, out count);

                if (topic.Length < 3 || topic.Length > 80 || count < 5 || count > 50)
                {
                    res.StatusCode = 400;
                    byte[] err = Encoding.UTF8.GetBytes("400 Bad Request: topic must be 3-80 chars, questions must be 5-50");
                    res.OutputStream.Write(err, 0, err.Length);
                    res.OutputStream.Close();
                }
                else
                {
                    Data.Add(topic, count);
                    res.StatusCode = 303;
                    res.RedirectLocation = "/quizzes";
                    res.OutputStream.Close();
                }
            }

            else if (url == "/quizzes/update" && method == "PUT")
            {
                int id = 0;
                int.TryParse(req.QueryString["id"], out id);

                StreamReader reader = new StreamReader(req.InputStream, req.ContentEncoding);
                string body = reader.ReadToEnd();
                var form = HttpUtility.ParseQueryString(body);

                string topic = form["topic"];
                string countStr = form["questionsCount"];
                string status = form["status"];
                int count = 0;

                if (topic == null) topic = "";
                if (status == null) status = "";
                if (countStr != null) int.TryParse(countStr, out count);

                Test found = null;
                for (int i = 0; i < Data.List.Count; i++)
                {
                    if (Data.List[i].Id == id) { found = Data.List[i]; break; }
                }

                if (found == null)
                {
                    res.StatusCode = 404;
                    byte[] err = Encoding.UTF8.GetBytes("404 Not Found: quiz with this id does not exist");
                    res.OutputStream.Write(err, 0, err.Length);
                    res.OutputStream.Close();
                }
                else
                {
                    if (topic.Length < 3 || topic.Length > 80 || count < 5 || count > 50 || (status != "open" && status != "closed"))
                    {
                        res.StatusCode = 400;
                        byte[] err = Encoding.UTF8.GetBytes("400 Bad Request: invalid topic, count or status");
                        res.OutputStream.Write(err, 0, err.Length);
                        res.OutputStream.Close();
                    }
                    else
                    {
                        found.Name = topic;
                        found.Count = count;
                        found.Status = status;
                        
                        res.StatusCode = 204;
                        res.OutputStream.Close();
                    }
                }
            }
            else
            {
                res.StatusCode = 404;
                byte[] err = Encoding.UTF8.GetBytes("404 Not Found: route not found");
                res.OutputStream.Write(err, 0, err.Length);
                res.OutputStream.Close();
            }
        }
    }
}