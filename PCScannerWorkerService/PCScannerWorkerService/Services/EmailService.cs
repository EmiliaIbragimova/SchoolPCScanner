//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;

//namespace PCScannerWorkerService.Services
//{
//    public class EmailService : IEmailService
//    {
//        private readonly ILogger<EmailService> _logger;
//        private readonly IConfiguration _configuration;

//        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
//        {
//            _logger = logger;
//            _configuration = configuration;
//        }

//        public async Task SendEmailAsync(string receiver, string subject, string message)
//        {
//            var port = int.Parse(_configuration["Smtp:Port"]);
//            var credentials = new NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]);
//            var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"]);
            
//            var smtpClient = new SmtpClient(_configuration["Smtp:Host"])
//            {
//                Port = port,
//                Credentials = credentials,
//                EnableSsl = enableSsl,
//            };
//            //var smtpClient = new SmtpClient(_configuration1["Smtp:Host"])
//            //{
//            //    Port = int.Parse(_configuration1["Smtp:Port"]),
//            //    Credentials = new NetworkCredential(_configuration1["Smtp:Username"], _configuration1["Smtp:Password"]),
//            //    EnableSsl = bool.Parse(_configuration1["Smtp:EnableSsl"])
//            //};

//            var mailMessage = new MailMessage
//            {
//                From = new MailAddress(_configuration["Smtp:Username"], _configuration["Smtp:FromName"]),
//                Subject = subject,
//                Body = message,
//                IsBodyHtml = true,
//            };

//            mailMessage.To.Add(receiver);

//            try
//            {
//                await smtpClient.SendMailAsync(mailMessage);
//                _logger.LogInformation($"Email sent to {receiver}");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Failed to send email to {receiver}");
//            }
//        }
//    }
//}
