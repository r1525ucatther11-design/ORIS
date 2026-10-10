namespace CustomHttpServer.Core;

public class Settings
{
    public ServerSettings Server { get; set; } = new();
}

public class ServerSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public string Port { get; set; } = "8888";
    public string Path { get; set; } = "";
}