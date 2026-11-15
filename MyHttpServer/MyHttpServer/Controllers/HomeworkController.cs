using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Controllers;

[Controller("homework")]
public class HomeworkController
{

    private const string SmtpHost = "smtp.yandex.ru";
    private const int SmtpPort = 465; 
    private const string SenderEmail = "";
    private const string SenderPassword = ""; 

    [Get("send")]
    public void Send(string to)
    {
        Console.WriteLine($"[Homework] Запрос на отправку письма на адрес: {to}");

        if (string.IsNullOrWhiteSpace(to))
        {
            Console.WriteLine("[Homework] Ошибка: параметр 'to' пуст.");
            return;
        }

        try
        {
            string archivePath = Path.Combine(AppContext.BaseDirectory, "static", "homework.zip");

            if (!File.Exists(archivePath))
            {
                Console.WriteLine($"[Homework] Ошибка: Архив не найден по пути: {archivePath}");
                Console.WriteLine("[Homework] Пожалуйста, создайте архив проекта и положите его в static/homework.zip");
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Homework", SenderEmail));
            message.To.Add(new MailboxAddress("", to));
            
            message.Subject = $"Домашнее задание: {DateTime.Now:yyyy-MM-dd HH:mm}";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = @"
11-507 Кузьмин Всеволод Александрович
";
            bodyBuilder.Attachments.Add(archivePath);

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(SmtpHost, SmtpPort, SecureSocketOptions.SslOnConnect); 
                client.Authenticate(SenderEmail, SenderPassword);
                client.Send(message);
                client.Disconnect(true);
            }

            Console.WriteLine($"[Homework] Успешно отправлено письмо на {to} с файлом ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Homework] Критическая ошибка при отправке: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}