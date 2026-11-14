using System;
using System.IO;

namespace MiniHttpServer.Framework.Content
{
    public static class MimeTypeProvider
    {
        public static string GetContentType(string fileName)
        {
            string ext = Path.GetExtension(fileName).ToLowerInvariant();

            return ext switch
            {
                ".html" or ".htm" => "text/html; charset=utf-8",
                ".css"            => "text/css; charset=utf-8",
                ".js"             => "text/javascript; charset=utf-8",
                ".json"           => "application/json; charset=utf-8",
                ".png"            => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".svg"            => "image/svg+xml",
                ".gif"            => "image/gif",
                ".ico"            => "image/x-icon",
                ".woff"           => "font/woff",
                ".woff2"          => "font/woff2",
                ".pdf"            => "application/pdf",
                ".txt"            => "text/plain; charset=utf-8",
                _                 => "application/octet-stream",
            };
        }
    }
}