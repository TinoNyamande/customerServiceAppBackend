namespace EmailCustomerServiceApi.Models
{
    public class Email
    {
        public string? Id { get; set; }
        public string? From { get; set; }
        public string? FromName { get; set; }
        public string? To { get; set; }
        public string? Body { get; set; }
        public string? FilePath { get; set; }
        public DateTime EmailDate { get; set; }
        public string? Subject { get; set; }
        public string? Status { get; set; }
        public string? AssignedTo { get; set; }
        public string? EmailUid { get; set; }
        public string? ComplainResponse { get; set; }
        public DateTime TimeAssigned { get; set; }
        public DateTime TimeResolved { get; set; }
        public decimal TimeTaken { get; set; }
    }
}
