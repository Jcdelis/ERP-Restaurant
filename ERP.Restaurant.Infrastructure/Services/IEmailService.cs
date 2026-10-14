namespace ERP.Restaurant.Infrastructure.Services;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}
