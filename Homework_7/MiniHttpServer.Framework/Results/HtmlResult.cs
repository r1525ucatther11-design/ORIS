using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Results
{
    public class HtmlResult : IHttpResult
    {
        private readonly string _html;
        private readonly int _statusCode;

        public HtmlResult(string html, int statusCode = 200)
        {
            _html = html;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(_html);
            response.StatusCode = _statusCode;
            response.ContentType = "text/html; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }
    }
}