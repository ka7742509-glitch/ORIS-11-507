using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace MiniHttpServer.Services;

public sealed class EmailService : IEmailService
{
    private static readonly Lazy<SmtpSettings> Settings = new(LoadSettings);

    public async Task<bool> SendAsync(string demoUser, string message)
    {
        SmtpSettings settings = Settings.Value;

        if (string.IsNullOrWhiteSpace(settings.Host) ||
            string.IsNullOrWhiteSpace(settings.FromAddress) ||
            string.IsNullOrWhiteSpace(settings.ToAddress))
        {
            Console.WriteLine("SMTP не настроен: заполните smtpsettings.json.");
            return false;
        }

        try
        {
            using var mail = new MailMessage();
            mail.From = new MailAddress(settings.FromAddress);
            mail.To.Add(settings.ToAddress);
            mail.Subject = "ORIS. ДЗ_5: учебная форма";
            mail.Body = $"Тестовая форма со страницы Rockstar.\n\nПользователь: {demoUser}\nСообщение: {message}\n\nОтправлено: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            mail.IsBodyHtml = false;

            using var client = new SmtpClient(settings.Host, settings.Port);
            client.EnableSsl = settings.UseSsl;
            client.Credentials = new NetworkCredential(settings.FromAddress, settings.FromPassword);

            await client.SendMailAsync(mail);

            Console.WriteLine($"Письмо отправлено на {settings.ToAddress}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки письма: {ex.Message}");
            return false;
        }
    }

    private static SmtpSettings LoadSettings()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "smtpsettings.json");

        if (!File.Exists(path))
        {
            Console.WriteLine("Файл smtpsettings.json не найден. Отправка писем отключена.");
            return new SmtpSettings();
        }

        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<SmtpSettings>(json) ?? new SmtpSettings();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка чтения smtpsettings.json: {ex.Message}");
            return new SmtpSettings();
        }
    }
}