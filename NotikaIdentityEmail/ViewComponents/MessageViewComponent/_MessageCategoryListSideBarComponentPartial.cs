using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;

namespace NotikaIdentityEmail.ViewComponents.MessageViewComponent
{
    public class _MessageCategoryListSideBarComponentPartial : ViewComponent
    {

        private readonly EmailContext _context;

        public _MessageCategoryListSideBarComponentPartial(EmailContext context)
        {
            _context = context;
        }
        
        public IViewComponentResult Invoke()
        {
            var values = _context.Categories.ToList();
            return View(values);
        }
    }
}
