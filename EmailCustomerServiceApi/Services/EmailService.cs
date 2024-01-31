using EmailCustomerServiceApi.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace EmailCustomerServiceApi.Services
{
    public class EmailService : IEmailService
    {
        public async Task<string> SendEmail(MailData mailData, CancellationToken ct,string successMessage)
        {
            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    var message = new MimeMessage();
                    message.To.Add(new MailboxAddress(mailData.ToName, mailData.To));
                    message.From.Add(new MailboxAddress(mailData.FromName, mailData.From));
                    message.Subject = mailData.Subject;
                    var body = new BodyBuilder();
                    body.HtmlBody = mailData.Body;
                    message.Body = body.ToMessageBody();


                    await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, ct);
                    await smtpClient.AuthenticateAsync("tinotendanyamande0784@gmail.com", "eyispyoigblikwwc", ct);
                    await smtpClient.SendAsync(message, ct);
                    return successMessage;

                }
            }catch (Exception ex)
            {
                return ex.ToString();
            }

        }
    }
}
