using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Services;

namespace MiniHttpServer.Controllers
{
    [Controller]
    public class AuthController
    {
        private readonly IEmailService _emailService = new EmailService();

        [Post]
        public async Task<string> SendEmail(string name, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(message))
                return "Ошибка: Имя и сообщение обязательны.";

            try
            {
                await _emailService.SendEmailAsync("student@example.com", name, message);
                return $"Письмо успешно отправлено от {name}";
            }
            catch (Exception ex)
            {
                return $"Ошибка при отправке: {ex.Message}";
            }
        }
    }
}