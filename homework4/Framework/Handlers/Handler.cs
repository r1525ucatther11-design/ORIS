using System.Net;

namespace CustomHttpServer.Framework.Handlers
{
    abstract class Handler
    {
        public Handler Successor { get; set; }

        public abstract Task HandleRequest(HttpListenerContext context);
    }
}