
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using NotikaIdentityEmail.Entities;
using Microsoft.Extensions.Configuration;
using NotikaIdentityEmail.Models.ForgetPasswordModels;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace NotikaIdentityEmail.Controllers
{
    public class PasswordChangeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public PasswordChangeController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel forgetPasswordViewModel)
        {
           var user = await _userManager.FindByEmailAsync(forgetPasswordViewModel.Email);
            string passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var passwordResetTokenLink = Url.Action("ResetPassword", "PasswordChange", new
            {
                userId = user.Id,
                token = passwordResetToken
            }, HttpContext.Request.Scheme);

            MimeMessage mimeMessage = new MimeMessage();
            var smtpSection = _configuration.GetSection("Smtp");
            var fromAddress = smtpSection["From"] ?? smtpSection["Username"];

            MailboxAddress mailboxAddressFrom = new MailboxAddress("Notika Admin", fromAddress);
            mimeMessage.From.Add(mailboxAddressFrom);
            MailboxAddress mailboxAddressTo = new MailboxAddress("User", forgetPasswordViewModel.Email);
            mimeMessage.To.Add(mailboxAddressTo);


            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = passwordResetTokenLink;
            mimeMessage.Body = bodyBuilder.ToMessageBody();


            mimeMessage.Subject = "Şifre Değişiklik Talebi";
            var host = smtpSection["Host"];
            var portStr = smtpSection["Port"];
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            if (!int.TryParse(portStr, out var port)) port = 587;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                if (!string.IsNullOrWhiteSpace(username))
                {
                    await client.AuthenticateAsync(username, password);
                }
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId,string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)

        {
            var userId = TempData["userId"];
            var token = TempData["token"];

            if(userId == null || token == null)
            {
                ViewBag.v = "Hata Oluştu";
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var result = await _userManager.ResetPasswordAsync(user, token.ToString(), resetPasswordViewModel.Password);
            if(result.Succeeded)
            {
                return RedirectToAction("UserLogin", "Login");
            }
            return View();


        }
    }
}
