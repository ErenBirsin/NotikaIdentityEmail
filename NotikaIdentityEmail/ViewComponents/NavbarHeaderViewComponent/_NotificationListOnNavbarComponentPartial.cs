using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;

namespace NotikaIdentityEmail.ViewComponents.NavbarHeaderViewComponent
{
    public class _NotificationListOnNavbarComponentPartial : ViewComponent
    {
        private readonly EmailContext _context;

        public _NotificationListOnNavbarComponentPartial(EmailContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var values = _context.Notifications.ToList();
            return View(values);
        }
    }
}
