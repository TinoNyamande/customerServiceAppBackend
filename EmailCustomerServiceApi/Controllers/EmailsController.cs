using EmailCustomerServiceApi.Data;
using EmailCustomerServiceApi.Models;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using EmailCustomerServiceApi.Services;
using EmailCustomerServiceApi.Models.ViewModels;

namespace EmailCustomerServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly PdfService _pdfservice;
        public EmailsController(PdfService pdfService, IEmailService emailService,AppDbContext context,UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _pdfservice = pdfService;
        }
        [HttpPost("ReadInbox")]
        public async Task<IActionResult> ReadInbox()
        {
            try
            {
                using (var client = new ImapClient())
                {
                    await client.ConnectAsync("imap.gmail.com", 993, true);

                    // Use ConfigureAwait(false) to avoid capturing the context
                    CancellationToken ct = new CancellationToken();
                    await client.AuthenticateAsync("tinotendanyamande0784@gmail.com", "eyispyoigblikwwc", ct);

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    var uids = inbox.Search(SearchQuery.All);
                    List<Email> allEmails = new List<Email>();
                    foreach (var uid in uids)
                    {
                        var res =   IsProccessed(uid.ToString()).Result;
                        if(res)
                        {
                            var message = inbox.GetMessage(uid);
                            Email email = new Email
                            {
                                Id = Guid.NewGuid().ToString(),
                                Body = message.HtmlBody,
                                Subject = message.Subject,
                                From = message.From.ToString(),
                                EmailDate = DateTime.Now,
                                Status = "NEW",
                                EmailUid = uid.ToString()
                            };

                            await _context.AddAsync(email);
                            await _context.SaveChangesAsync();
                            allEmails.Add(email);
                        }

                    }

                    await client.DisconnectAsync(true);
                    return Ok(allEmails);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }
        private async Task<bool> IsProccessed(string uid)
        {
            var email = await _context.Emails.FirstOrDefaultAsync(a => a.EmailUid == uid);
            if (email == null)
            {
                return true;
            }else
            {
                return false;
            }
        }
        [HttpPost("Allocate")]
        public async Task<IActionResult> Allocate ()
        {
            var newEmail = await _context.Emails.FirstOrDefaultAsync(a => a.Status == "NEW");
            if (newEmail == null)
            {
                return BadRequest("No emails");
            }
            var allUsers = await _userManager.Users.ToListAsync();
            var total = allUsers.Count;
            int nextAllocated;
            var lastCountUser = await _userManager.Users.FirstOrDefaultAsync(a => a.LastAllocated == 1);

            if (lastCountUser.EmailCount >= total)
            {
                nextAllocated = 1;
            }else
            {
                nextAllocated = lastCountUser.EmailCount + 1;
            }
            var newAllocate = await _userManager.Users.FirstOrDefaultAsync(a => a.EmailCount == nextAllocated);

            newEmail.AssignedTo = newAllocate.Id;
            newEmail.Status = "ALLOCATED";
            await _context.SaveChangesAsync();
            lastCountUser.LastAllocated = 0;
            newAllocate.LastAllocated = 1;
            await _userManager.UpdateAsync(lastCountUser);
            await _userManager.UpdateAsync(newAllocate);
          

            return Ok("Updated");

        }
        [HttpGet("GetMyEmails")]
        public async Task<IActionResult> GetMyEmails(string email)
        {
            
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        message = "You do not have any outstanding emails"
                    });
                }
                var emails = await _context.Emails.Where(a => a.AssignedTo == user.Id)
                                   .Where(a => a.Status == "ALLOCATED")
                                   .ToListAsync();
                if (emails == null)
                {
                    return BadRequest(new
                    {
                        message = "You do not have any outstanding emails"
                    });
                }
                return Ok(emails);
            }catch (Exception ex) {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }


        }
        [HttpGet("GetCompletedEmails")]
        public async Task<IActionResult> GetCompletedEmails(string email)
        {

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest(new
                    {
                        message = "You do not have any outstanding emails"
                    });
                }
                var emails = await _context.Emails.Where(a => a.AssignedTo == user.Id)
                                   .Where(a => a.Status == "RESOLVED")
                                   .ToListAsync();
                if (emails == null)
                {
                    return BadRequest(new
                    {
                        message = "You do not have any outstanding emails"
                    });
                }
                return Ok(emails);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }


        }
        [HttpPost("Resolve")]
        public async Task<IActionResult> Resolve(string caseId , string res)
        {

            try
            {
               
                var email = await _context.Emails.FirstOrDefaultAsync(a => a.Id == caseId);
                if (email == null)
                {
                    return BadRequest(new
                    {
                        message = @$"Email with id ${caseId} not found"
                    });
                }
                MailData mailData = new MailData
                {
                    To = email.From,
                    ToName = email.From,
                    From = "tinotendanyamande0784@gmail.com",
                    FromName = "Tinotenda Nyamande",
                    Body = res,
                    Subject = email.Subject
                };
                CancellationToken ct = new CancellationToken();
                await _emailService.SendEmail(mailData, ct,"Complain email sent successfully");
                email.ComplainResponse = res;
                email.Status = "RESOLVED";
                return Ok("Response sent to customer");
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }


        }
        [HttpGet("EmailDetails")]
        public async Task<IActionResult> EmailDetails(string Id)
        {
            var email = await _context.Emails.FirstOrDefaultAsync(a => a.Id == Id);
            if (email == null)
            {
                return BadRequest(new
                {
                    message = "Not found"
                });
            }
       
            return Ok(email);
        }

    }
}

