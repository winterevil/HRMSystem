
using System.Net;
using System.Net.Mail;

namespace HRMSystem.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(
                "longlt4work@gmail.com",
                "cysr swpq rarr ssgv"
            ),
                EnableSsl = true
            };

            var mail = new MailMessage(
                "longlt4work@gmail.com",
                toEmail,
                subject,
                body
            );

            await smtp.SendMailAsync(mail);
        }
    }
}
