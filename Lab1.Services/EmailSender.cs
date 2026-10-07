using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Lab1.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IConfiguration configuration,
        ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string subject, string message)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]!);
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderName = _configuration["EmailSettings:SenderName"];
        var recipientEmail = _configuration["EmailSettings:RecipientEmail"];
        var username = _configuration["EmailSettings:Username"];
        var password = _configuration["EmailSettings:Password"];

        var emailMessage = new MimeMessage();

        emailMessage.From.Add(
            new MailboxAddress(senderName, senderEmail));

        emailMessage.To.Add(
            MailboxAddress.Parse(recipientEmail));

        emailMessage.Subject = subject;

        emailMessage.Body = new TextPart("plain")
        {
            Text = message
        };

        using var smtpClient = new SmtpClient();

        try
        {
            await smtpClient.ConnectAsync(
                smtpHost,
                smtpPort,
                SecureSocketOptions.StartTls);

            await smtpClient.AuthenticateAsync(
                username,
                password);

            await smtpClient.SendAsync(emailMessage);

            _logger.LogInformation(
                "Email sent successfully | Recipient: {Recipient} | Subject: {Subject}",
                recipientEmail,
                subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email | Recipient: {Recipient} | Subject: {Subject}",
                recipientEmail,
                subject);

            throw;
        }
        finally
        {
            if (smtpClient.IsConnected)
            {
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}