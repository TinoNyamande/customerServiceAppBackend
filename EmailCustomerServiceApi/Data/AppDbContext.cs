using EmailCustomerServiceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmailCustomerServiceApi.Data
{
 
    public class AppDbContext(DbContextOptions<AppDbContext> options): IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Email> Emails { get; set; }

    }
}
