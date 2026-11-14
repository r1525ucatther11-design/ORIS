using System.Net;

public interface IController
{
    bool CanHandle(string url, string method);
    void Handle(HttpListenerContext context);
}