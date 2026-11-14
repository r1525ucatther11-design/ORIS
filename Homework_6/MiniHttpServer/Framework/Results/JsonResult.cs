using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Results
{
    public class JsonResult : IHttpResult
    {
        private readonly object _data;
        private readonly int _statusCode;

        public JsonResult(object data, int statusCode = 200)
        {
            _data = data;
            _statusCode = statusCode;
        }

        public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
        {
            byte[] buffer = JsonSerializer.SerializeToUtf8Bytes(_data);
            response.StatusCode = _statusCode;
            response.ContentType = "application/json; charset=utf-8";
            response.ContentLength64 = buffer.Length;
            response.Headers["Server"] = "MiniHttpServer/1.0";

            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }
    }
}