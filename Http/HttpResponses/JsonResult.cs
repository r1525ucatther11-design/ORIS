using System.Text;
using System.Text.Json;

namespace CustomHttpServer.Http.HttpResponses
{
    public class JsonResult : IHttpResponseTypeResult
    {
        private readonly object _data;

        public JsonResult(object data)
        {
            _data = data;
        }

        public byte[] Execute()
        {
            string json = JsonSerializer.Serialize(_data);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}