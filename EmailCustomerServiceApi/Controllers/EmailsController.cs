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
using MailKit.Net.Smtp;
using MimeKit;

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
                    await client.AuthenticateAsync("tnyamande165@gmail.com", "pgkavdapcktuavaq", ct);

                    var inbox = client.Inbox;
                    inbox.Open(FolderAccess.ReadOnly);

                    var uids = inbox.Search(SearchQuery.All);
                    List<Email> allEmails = new List<Email>();
                    foreach (var uid in uids)
                    {
                        var summary = inbox.Fetch(new[] { uid }, MessageSummaryItems.UniqueId).FirstOrDefault();
                        var id = summary.UniqueId;
                        var res =   IsProccessed(uid.ToString()).Result;
                        if(res)
                        {
                            var message = inbox.GetMessage(uid);
                            Email email = new Email
                            {
                                Id = Guid.NewGuid().ToString(),
                                Body = message.HtmlBody,
                                Subject = message.Subject,
                                From = message.From[0].ToString(),
                                EmailDate = DateTime.Now,
                                Status = "NEW",
                                EmailUid = id.ToString(),
                                TimeAssigned = DateTime.Now,
                                TimeResolved = DateTime.Now,
                                TimeTaken = 0,
                                FromName = message.From[1].ToString(),
                      
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
        public async Task<IActionResult> Resolve(ReplyMesssage replyMesssage)
        {

            try
            {
               
                var email = await _context.Emails.FirstOrDefaultAsync(a => a.Id == replyMesssage.CaseId);
                if (email == null)
                {
                    return BadRequest(new
                    {
                        message = @$"Email with id ${replyMesssage.CaseId} not found"
                    });
                }
                email.ComplainResponse = replyMesssage.Res;
                email.TimeResolved = DateTime.Now;
                email.TimeTaken = email.TimeAssigned.Subtract(DateTime.Now).Minutes;
                email.Status = "REPLY_READY";
                await _context.SaveChangesAsync();
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
        [HttpPost("SendReply")]
        public async Task<IActionResult> SendReply()
        {
            var emails = await _context.Emails.Where(a => a.Status == "REPLY_READY").ToListAsync();
            if (emails == null)
            {
                return BadRequest(new
                {
                    message = "No emaills ready for sending"
                });
            }
            try
            {


                foreach (var email in emails)
                {
                    var replyMessage = new MimeMessage();
                    replyMessage.InReplyTo = email.EmailUid;
                    replyMessage.References.Add(email.EmailUid);
                    replyMessage.Subject = "RE :" +email.Subject;
                    replyMessage.To.Add(new MailboxAddress(email.FromName, email.From));
                    var bodyBuilder = new BodyBuilder();
                    bodyBuilder.TextBody = email.ComplainResponse;
                    replyMessage.Body = bodyBuilder.ToMessageBody();
                    replyMessage.From.Add(new MailboxAddress("Tino", "tnyamande165@gmail.com"));
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, false);

                        // Use the appropriate authentication method
                        // client.Authenticate("your.", "your_password");
                        await client.AuthenticateAsync("tnyamande165@gmail.com", "pgkavdapcktuavaq", new CancellationToken());

                        await client.SendAsync(replyMessage);

                        await client.DisconnectAsync(true);
                    }

                }
                return Ok("Reply sent");
            }catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.ToString()
                });
            }

        }
        [HttpGet("GetEscalatedEmails")]
        public async Task<IActionResult> GetEscalatedEmails()
        {

            try
            {

                var emails = await _context.Emails
                                   .Where(a => a.Status == "ESCALATED")
                                   .ToListAsync();
                if (emails == null)
                {
                    return BadRequest(new
                    {
                        message = "You do not have any escalated emails"
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
        [HttpGet("Escalate")]
        public async Task<IActionResult> Escalate()
        {

            try
            {

                var emails = await _context.Emails
                                   .Where(a => a.Status == "ALLOCATED")
                                   .ToListAsync();
                if (emails == null)
                {
                    return BadRequest(new
                    {
                        message = "You do not have any outstanding emails"
                    });
                }
                foreach (var email in emails)
                {
                    var hours = email.TimeAssigned.Subtract(DateTime.Now).Hours;
                    if (hours > 3)
                    {
                        email.Status = "ESCALATED";
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok("Done");
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }


        }
        [HttpGet("OutstandingAdmin")]
        public async Task<IActionResult> OutstandingAdmin()
        {
            var emails = await _context.Emails.Where(a => a.Status == "ALLOCATED").OrderByDescending(a => a.TimeAssigned).ToListAsync();
            if (emails == null)
            {
                return BadRequest(new
                {
                    message = "No data found"
                });
            }
            return Ok(emails);
        }

        [HttpGet("Archive")]
        public async Task<IActionResult> Archive()
        {
            var emails = await _context.Emails.Where(a => a.Status == "ALLOCATED").OrderByDescending(a => a.TimeAssigned).ToListAsync();
            if (emails == null)
            {
                return BadRequest(new
                {
                    message = "No data found"
                });
            }
            return Ok(emails);
        }
    }
}

