using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using NotikaIdentityEmail.Entities;
using Microsoft.Extensions.Configuration;
using NotikaIdentityEmail.Models.IdentityModels;

namespace NotikaIdentityEmail.Controllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public RegisterController(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterUserViewModel model)
        {
            Random rnd = new Random();
            int code = rnd.Next(100000, 1000000); // 6 haneli kod

            AppUser appUser = new AppUser()
            {
                Name = model.Name,
                Email = model.Email,
                UserName = model.UserName,
                SurName = model.Surname,
                ActivationCode = code
            };

            var result = await _userManager.CreateAsync(appUser, model.Password);

            if (result.Succeeded)
            {
                // E-posta mesajı oluştur
                var mimeMessage = new MimeMessage();
                var smtpFrom = _configuration.GetSection("Smtp")["From"] ?? _configuration.GetSection("Smtp")["Username"];
                var mailboxAddressFrom = new MailboxAddress("Admin", smtpFrom);
                var mailboxAddressTo = new MailboxAddress("User", model.Email);

                mimeMessage.From.Add(mailboxAddressFrom);
                mimeMessage.To.Add(mailboxAddressTo);
                mimeMessage.Subject = "Notika Identity Aktivasyon Kodu";

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = $"Merhaba {model.Name},\n\nHesabınızı doğrulamak için gerekli olan aktivasyon kodu: {code}\n\nTeşekkürler."
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                // SMTP ayarlarını config'den oku ve gönder
                var smtpSection = _configuration.GetSection("Smtp");
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
                    TempData["EmailMove"] = model.Email;
                }

                return RedirectToAction("UserActivation", "Activation");
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

                return View();
            }
        }
    }
}
