using System;
using System.IO;
using System.Text.Json;

namespace MiniHttpServer.Framework.Configuration
{
    public sealed class ConfigurationManager
    {
        private static readonly Lazy<ConfigurationManager> _instance =
            new(() => new ConfigurationManager(), isThreadSafe: true);

        public static ConfigurationManager Instance => _instance.Value;

        public ServerSettings Settings { get; }

        private ConfigurationManager()
        {
            string json = File.ReadAllText("settings.json");
            Settings = JsonSerializer.Deserialize<ServerSettings>(json)
                ?? throw new InvalidOperationException("Не удалось загрузить настройки.");
        }
    }
}