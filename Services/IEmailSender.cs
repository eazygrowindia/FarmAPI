namespace FarmAPI.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }
}