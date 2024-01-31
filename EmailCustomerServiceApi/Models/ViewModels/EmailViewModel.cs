using Microsoft.AspNetCore.Mvc;

namespace EmailCustomerServiceApi.Models.ViewModels
{
    public class EmailViewModel
    {
        public string? Id { get; set; }
        public string? Subject { get;set; }
        public FileContentResult? EmailFile { get; set; }
        public string? Status { get; set; }
        public string? From { get; set; }
        public DateTime EmailDate { get; set; }
    }
}
