using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;
using NotikaIdentityEmail.Models.JwtModels;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NotikaIdentityEmail.Controllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly EmailContext _emailContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettingsModel _jwtSettingsModel;

        public LoginController(SignInManager<AppUser> signInManager, EmailContext emailContext, UserManager<AppUser> userManager, IOptions<JwtSettingsModel> jwtSettingsModel)
        {
            _signInManager = signInManager;
            _emailContext = emailContext;
            _userManager = userManager;
            _jwtSettingsModel = jwtSettingsModel.Value;
        }

        [HttpGet]
        public IActionResult UserLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(UserLoginViewModel model)
        {
            
            var value = _emailContext.Users.FirstOrDefault(x => x.UserName == model.Username);

            SimpleUserViewModel simpleUserViewModel = new SimpleUserViewModel()
            {
                City = value.City,
                Email = value.Email,
                Id = value.Id,
                Name = value.Name,
                Surname = value.SurName,
                UserName = value.UserName
            };

            if (value == null)
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                return View(model);
            }

            if (!value.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Lütfen e-posta adresinizi doğrulayın.");
                return View(model);
            }

            if (!value.IsActive )
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı Pasif Durumda, Giriş Yapamaz!");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, true, true);
            if (result.Succeeded)
            {
                var token = GenerateJwtToken(simpleUserViewModel);

                Response.Cookies.Append("jwtToken",token,new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, 
                    SameSite = SameSiteMode.Strict, 
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettingsModel.ExpirationInMinutes)
                });

                return RedirectToAction("EditProfile", "Profile");
            }
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        public string GenerateJwtToken(SimpleUserViewModel simpleUserViewModel)
        {
            var claim = new[]
            {
                new Claim("name",simpleUserViewModel.Name),
                new Claim("surname",simpleUserViewModel.Surname),
                new Claim("city",simpleUserViewModel.City),
                new Claim("username",simpleUserViewModel.UserName),
                new Claim(ClaimTypes.NameIdentifier, simpleUserViewModel.Id),
                new Claim(ClaimTypes.Email, simpleUserViewModel.Email),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettingsModel.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: _jwtSettingsModel.Issuer,
                audience: _jwtSettingsModel.Audience,
                claims: claim,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettingsModel.ExpirationInMinutes),
                signingCredentials: creds
            );
            //simpleUserViewModel.Token = new JwtSecurityTokenHandler().WriteToken(token);
            //return View(simpleUserViewModel);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet]
        public IActionResult LoginWithGoogle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUri = Url.Action("ExternalLoginCallback", "Login", new { ReturnUrl = returnUrl }, Request.Scheme);
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUri);

            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallBack(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"External Provider Error: {remoteError}");
                return RedirectToAction("UserLogin");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("UserLogin");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = new AppUser()
                {
                    UserName = email,
                    Email = email,
                    Name = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? "Google",
                    SurName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? "User",
                    EmailConfirmed = true
                };

                var identityResult = await _userManager.CreateAsync(user);
                if (identityResult.Succeeded)
                {
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in identityResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("UserLogin");
            }
        }




    }
}