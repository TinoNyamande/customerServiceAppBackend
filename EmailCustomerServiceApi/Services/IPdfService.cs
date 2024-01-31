namespace EmailCustomerServiceApi.Services
{
    public interface IPdfService
    {
        public byte[] GeneratePdf(string htmlContent, string fileName);
        
    }
}
