using System.Text;

namespace CustomHttpServer.Http.HttpResponses
{
    public class HtmlResult : IHttpResponseTypeResult
    {
        private readonly string _html;

        public HtmlResult(string html)
        {
            _html = html;
        }

        public byte[] Execute()
        {
            return Encoding.UTF8.GetBytes(_html);
        }
    }
}