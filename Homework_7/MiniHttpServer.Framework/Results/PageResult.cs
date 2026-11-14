using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.Framework.Results
{
    public class PageResult : IHttpResult
    {
        private readonly string _templatePath;
        private readonly object _model;
        private readonly IHtmlTemplateRenderer _renderer;

        public PageResult(string templatePath, object model, IHtmlTemplateRenderer renderer)
        {
            _templatePath = templatePath;
            _model = model;
            _renderer = renderer;
        }

        public async Task ExecuteAsync(HttpListenerResponse response, CancellationToken cancellationToken = default)
        {
            string fullPath = Path.Combine(AppContext.BaseDirectory, "Templates", _templatePath);
            string html = _renderer.RenderFromFile(fullPath, _model);

            var htmlResult = new HtmlResult(html);
            await htmlResult.ExecuteAsync(response, cancellationToken);
        }
    }
}