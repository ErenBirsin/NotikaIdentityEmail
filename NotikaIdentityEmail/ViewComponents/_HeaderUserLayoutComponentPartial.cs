using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;

namespace NotikaIdentityEmail.ViewComponents
{
    public class _HeaderUserLayoutComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public _HeaderUserLayoutComponentPartial(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userValue = await _userManager.FindByNameAsync(User.Identity.Name);
            var userMail = userValue.Email;
            var userEmailCount = _context.Messages.Where(x=>x.ReceiverEmail == userMail).Count();
            ViewBag.userEmailCount = userEmailCount;
            ViewBag.notificationCount = _context.Notifications.Count();
            return View();
        }
    }
}
