using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Content;

namespace MiniHttpServer.Framework.Handlers
{
    public class StaticFilesHandler : RequestHandler
    {
        private readonly string _publicRoot;

        public StaticFilesHandler()
        {
            var settings = ConfigurationManager.Instance.Settings;
            _publicRoot = Path.GetFullPath(settings.PublicDirectoryPath);
        }

        public override async Task HandleAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string urlPath = Uri.UnescapeDataString(request.Url?.AbsolutePath ?? "/");

            if (urlPath == "/")
            {
                string menu = "<h1>MiniHttpServer</h1><ul>" +
                              "<li><a href=\"SearcherFinder/\">SearcherFinder</a></li>" +
                              "<li><a href=\"Satisfactory/\">Satisfactory</a></li>" +
                              "<li><a href=\"Steam/\">Steam</a></li></ul>";
                byte[] menuBuf = Encoding.UTF8.GetBytes(menu);
                response.StatusCode = 200;
                response.ContentType = "text/html; charset=utf-8";
                response.ContentLength64 = menuBuf.Length;
                response.Headers["Server"] = "MiniHttpServer/1.0";
                await response.OutputStream.WriteAsync(menuBuf, 0, menuBuf.Length);
                response.Close();
                return;
            }

            string relative = urlPath.TrimStart('/');
            if (urlPath.EndsWith("/")) relative += "index.html";

            string fullPath = Path.GetFullPath(Path.Combine(_publicRoot, relative));

            if (!fullPath.StartsWith(_publicRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                response.StatusCode = 403;
                response.Close();
                return;
            }

            if (File.Exists(fullPath))
            {
                byte[] buffer = await File.ReadAllBytesAsync(fullPath);
                string contentType = MimeTypeProvider.GetContentType(fullPath);

                response.StatusCode = 200;
                response.ContentType = contentType;
                response.ContentLength64 = buffer.Length;
                response.Headers["Server"] = "MiniHttpServer/1.0";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.Close();
            }
            else
            {
                await HandleNextAsync(context);
            }
        }
    }
}