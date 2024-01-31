using Microsoft.AspNetCore.Identity;

namespace EmailCustomerServiceApi.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string? EmailAddress { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int EmailCount { get; set; }
        public int LastAllocated { get; set; }
        public bool Available { get; set; }
        public string? Department { get; set; }
    }
}
