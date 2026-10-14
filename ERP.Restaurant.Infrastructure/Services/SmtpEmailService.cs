using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace ERP.Restaurant.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        var host = _config["Smtp:Host"] ?? "localhost";
        var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 25;
        var user = _config["Smtp:User"];
        var pass = _config["Smtp:Pass"];

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = bool.TryParse(_config["Smtp:EnableSsl"], out var ssl) && ssl
        };

        if (!string.IsNullOrEmpty(user))
            client.Credentials = new NetworkCredential(user, pass);

        var mail = new MailMessage(from: _config["Smtp:From"] ?? "noreply@example.com", to, subject, body)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mail);
    }
}
