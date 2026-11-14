using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Handlers
{
    public class NotFoundHandler : RequestHandler
    {
        public override async Task HandleAsync(HttpListenerContext context)
        {
            var response = context.Response;
            string message = $"Файл {context.Request.Url?.AbsolutePath} не найден.";
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.Close();
        }
    }
}