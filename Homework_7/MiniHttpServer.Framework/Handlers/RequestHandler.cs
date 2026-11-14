using System.Net;
using System.Threading.Tasks;

namespace MiniHttpServer.Framework.Handlers
{
    public abstract class RequestHandler
    {
        protected RequestHandler? Next { get; private set; }

        public void SetNext(RequestHandler next)
        {
            Next = next;
        }

        public abstract Task HandleAsync(HttpListenerContext context);

        protected async Task HandleNextAsync(HttpListenerContext context)
        {
            if (Next != null)
            {
                await Next.HandleAsync(context);
            }
        }
    }
}