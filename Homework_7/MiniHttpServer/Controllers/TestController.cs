using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Controllers
{
    [Controller]
    public class TestController : BaseController
    {
        [Get]
        public IHttpResult Index()
        {
            return Html("<h1>Hello from controller!</h1>");
        }

        [Get]
        public IHttpResult Info()
        {
            return Json(new
            {
                Id = 1,
                Message = "Hello World!",
                Success = true
            });
        }

        [Get]
        public IHttpResult Details(int id, string name)
        {
            return Json(new { ID = id, Name = name });
        }

        [Get]
        public IHttpResult RequestInfo()
        {
            return Json(new
            {
                method = Context.Request.HttpMethod,
                path = Context.Request.Url?.AbsolutePath,
                userAgent = Context.Request.Headers["User-Agent"]
            });
        }
    }
}