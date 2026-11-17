namespace MiniHttpServer.Services;

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 465;
    public bool UseSsl { get; set; } = true;
    public string FromAddress { get; set; } = "";
    public string FromPassword { get; set; } = "";
    public string ToAddress { get; set; } = "";
}