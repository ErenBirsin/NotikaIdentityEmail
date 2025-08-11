using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;

namespace NotikaIdentityEmail.ViewComponents.MessageViewComponent
{
    public class _MessageSideBarComponentPartial : ViewComponent
    {

        private readonly EmailContext _emailContext;
        private readonly UserManager<AppUser> _userManager;

        public _MessageSideBarComponentPartial(EmailContext emailContext, UserManager<AppUser> userManager)
        {
            this._emailContext = emailContext;
            this._userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            ViewBag.sendMessageCount = _emailContext.Messages.Where(x=>x.SenderEmail == user.Email).Count();
            ViewBag.receiveMessageCount = _emailContext.Messages.Where(x=>x.ReceiverEmail == user.Email).Count();
            return View();
        }
    }
}
