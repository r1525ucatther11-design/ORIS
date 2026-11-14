using MailKit.Net.Smtp;
using MimeKit;

namespace MiniHttpServer.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string name, string message)
        {
            Console.WriteLine($"[ТЕСТОВОЕ ПИСЬМО]");
            Console.WriteLine($"Кому: {to}");
            Console.WriteLine($"От: {name}");
            Console.WriteLine($"Сообщение: {message}");
            
            await Task.CompletedTask;
        }
    }
}