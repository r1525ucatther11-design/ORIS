using System.Net;
using CustomHttpServer.Http.HttpResponses;

namespace CustomHttpServer.Http
{
    public abstract class ControllerBase
    {
        protected HttpListenerContext Context { get; private set; }

        internal void SetContext(HttpListenerContext context)
        {
            Context = context;
        }

        protected IHttpResponseTypeResult Html(string html)
        {
            return new HtmlResult(html);
        }

        protected IHttpResponseTypeResult Json(object obj)
        {
            return new JsonResult(obj);
        }
    }
}