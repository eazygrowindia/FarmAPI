using System.Net;
using System.Net.Mail;

namespace FarmAPI.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;

    public SmtpEmailSender(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        // Configure from appsettings or env vars
        var host = _cfg["Smtp:Host"];
        var port = int.Parse(_cfg["Smtp:Port"] ?? "587");
        var user = _cfg["Smtp:User"];
        var pass = _cfg["Smtp:Pass"];
        var from = _cfg["Smtp:From"];

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var msg = new MailMessage(from!, to, subject, body);
        msg.IsBodyHtml = false;
        await client.SendMailAsync(msg);
    }
}
