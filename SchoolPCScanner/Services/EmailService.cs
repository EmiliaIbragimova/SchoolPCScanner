using System.Net.Mail;
using System.Net;
using SchoolPCScanner.Services.Interfaces;
using Humanizer;
using SchoolPCScanner.Models;
using Microsoft.Extensions.Options;

namespace SchoolPCScanner.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        //private readonly IConfiguration _configuration;
        private readonly IOptions<SmtpSettings> _smtpSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<SmtpSettings> smtpSettings)
        {
            _logger = logger;
            _smtpSettings = smtpSettings;
        }
        public async Task SendEmailAsync(string receiver, string subject, string message)
        {
            if (receiver == null)
            {
                throw new ArgumentNullException(nameof(receiver), "Ontvangeradres mag niet null zijn");
            }
            var port = _smtpSettings.Value.Port;
            var credentials = new NetworkCredential(_smtpSettings.Value.Username, _smtpSettings.Value.Password);
            var enableSsl = _smtpSettings.Value.EnableSsl;

            var smtpClient = new SmtpClient(_smtpSettings.Value.Host)
            {
                Port = port,
                Credentials = credentials,
                EnableSsl = enableSsl,
            };

            var mailMessage = new MailMessage
            {
                //From = new MailAddress(_configuration["Smtp:Username"], _configuration["Smtp:FromName"]),
                From = new MailAddress(_smtpSettings.Value.Username),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(receiver);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email sent to {receiver}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {receiver}");
            }
        }
    }
}
