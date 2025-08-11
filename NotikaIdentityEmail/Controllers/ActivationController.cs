using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;

namespace NotikaIdentityEmail.Controllers
{
    public class ActivationController : Controller
    {
        private readonly EmailContext _emailContext;

        public ActivationController(EmailContext emailContext)
        {
            _emailContext = emailContext;
        }

        [HttpGet]
        public IActionResult UserActivation()
        {
            var email= TempData["EmailMove"] ; // E-posta adresini TempData'dan al
            TempData["Test1"] = email;
            return View();
        }

        [HttpPost]

        public IActionResult UserActivation(int userCodeParameter)
        {
            string email = TempData["Test1"].ToString(); // E-posta adresini TempData'dan al
            var code = _emailContext.Users.Where(y => y.Email == email)
                .Select(y => y.ActivationCode)
                .FirstOrDefault();
            if (userCodeParameter == code)
            {

                var value = _emailContext.Users.Where(x=>x.Email == email).FirstOrDefault();
                value.EmailConfirmed = true;
                _emailContext.SaveChanges();

                return RedirectToAction("UserLogin", "Login");

            }

            return View();

        }

    }
}
// yqcc wvzz dqpl kisx