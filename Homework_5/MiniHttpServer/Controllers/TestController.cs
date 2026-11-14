using MiniHttpServer.Framework.Attributes;

namespace MiniHttpServer.Controllers
{
    [Controller]
    public class TestController
    {
        [Get]
        public string Index() => "Hello from controller!";

        [Get]
        public string Hello() => "Hello World!";

        [Get]
        public string Details(int id, string name) => $"ID: {id}, Name: {name}";

        [Post]
        public string Submit() => "POST request processed";
    }
}