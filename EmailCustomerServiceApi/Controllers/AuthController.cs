using EmailCustomerServiceApi.Models;
using EmailCustomerServiceApi.Models.ViewModels;
using EmailCustomerServiceApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Pkcs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmailCustomerServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(SignInManager<ApplicationUser> signInManager,IEmailService emailService,IConfiguration configuration,UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailService = emailService;
            _signInManager = signInManager;

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
                //check user 
                var user = await _userManager.FindByNameAsync(loginUser.EmailAdrress);
                if (user == null)
                {
                    return BadRequest(new { message = "Username not found" });
                }
                //check password
                bool isPasswordValid = await _userManager.CheckPasswordAsync(user, loginUser.Password);
                if (!isPasswordValid)
                {
                    return BadRequest(new
                    {
                        message = "Incorrect password"
                    });
                }
                if (user != null && await _userManager.CheckPasswordAsync(user, loginUser.Password))
                {
                    //create a claims list
                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name , user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString()),
                    new Claim ("username" , user.UserName ,ClaimValueTypes.String)
                };


                    //add role to claims list
                    var userRoles = await _userManager.GetRolesAsync(user);
                    foreach (var role in userRoles)
                    {
                        authClaims.Add(new Claim("role", role, ClaimValueTypes.String));
                    }
                    try
                    {
                        //generate access token
                       var jwtToken = GetToken(authClaims);
                       return Ok(
                            new
                            {
                                token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                                //token = jwtToken,
                                expiration = jwtToken.ValidTo
                            }
                            );
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new { message = ex.ToString() });
                    }


                }
                return BadRequest("Error occured");

            }

            private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authsigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authsigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        [HttpPost(Name = "Register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel registerUser)
        {
            //check if user is null
            if (registerUser == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid object");
            }
            var userExists = await _userManager.FindByEmailAsync(registerUser.Email);
            if (userExists != null)
            {
                return BadRequest("User already exists");
            }
            var role = registerUser.Role;
            if (await _roleManager.RoleExistsAsync(role))
            {
                var highestCountUser = await _userManager.Users
                        .OrderByDescending(a => a.EmailCount)
                        .FirstOrDefaultAsync();
                ApplicationUser user = new ApplicationUser
                {
                    UserName = registerUser.Email,
                    Email = registerUser.Email,
                    FirstName = registerUser.FirstName,
                    LastName = registerUser.LastName,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    EmailCount = highestCountUser.EmailCount + 1
                };

                var result = await _userManager.CreateAsync(user, registerUser.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, role);

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { token, email = user.Email }, Request.Scheme);
                    var userEmail = new List<string> { user.Email! };
                    var emailBody = @$"<h3>Click <a href={confirmationLink}>here </a> to confirm your email</h3>";
                    var message = new MailData
                    {
                        To = user.Email,
                        Subject = "Confirmation email link",
                        ToName = user.FirstName,
                        Body = emailBody,
                        From = "tinotendanyamande0784@gmail.com",
                        FromName = "Tinotenda Nyamande"
                    };
                    var res = await _emailService.SendEmail(message, new CancellationToken(), "Confirmation email has been sent successfully");
                    return Ok(new
                    {
                        message = res
                    });


                }
                else
                {
                    return BadRequest(result);
                }
            }
            else
            {
                return BadRequest($"{role} Role does not exist");
            }

        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, "Email has been confirmed");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error");
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error");
            }
        }
        [HttpPost("AddRole")]
        public async Task<IActionResult> AddRole (string roleName)
        {
            try
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                
                return Ok(@$"Role {roleName} added successfully");
            }catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.ToString()
                });
            }
        }
        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout ()
        {
             await _signInManager.SignOutAsync();
            return Ok("Logged out successfully");
        }

    }

}
