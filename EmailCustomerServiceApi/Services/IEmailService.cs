using EmailCustomerServiceApi.Models;

namespace EmailCustomerServiceApi.Services
{
    public interface IEmailService
    {
        public  Task<string> SendEmail(MailData mailData ,CancellationToken ct, string successMessage);
        
    }
}
