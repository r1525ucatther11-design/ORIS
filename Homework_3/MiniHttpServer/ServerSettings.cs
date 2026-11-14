namespace MiniHttpServer
{
    public class ServerSettings
    {
        public int Port { get; set; } = 8888;
        public string Path { get; set; } = string.Empty;
        public string PublicDirectoryPath { get; set; } = "static";
    }
}