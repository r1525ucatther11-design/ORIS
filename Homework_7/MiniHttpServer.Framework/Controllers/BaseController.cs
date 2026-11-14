using System.Net;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.HtmlTemplating;

namespace MiniHttpServer.Framework.Controllers
{
    public abstract class BaseController
    {
        protected HttpListenerContext Context { get; private set; } = null!;

        public void SetContext(HttpListenerContext context) => Context = context;

        protected IHttpResult Html(string html, int statusCode = 200) => new HtmlResult(html, statusCode);
        protected IHttpResult Json(object data, int statusCode = 200) => new JsonResult(data, statusCode);

        protected IHttpResult Page(string templatePath, object model)
        {
            var renderer = new HtmlTemplateRenderer();
            return new PageResult(templatePath, model, renderer);
        }
    }
}