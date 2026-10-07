namespace Lab1.Services;

public interface IEmailSender
{
    Task SendEmailAsync(string subject, string message);
}