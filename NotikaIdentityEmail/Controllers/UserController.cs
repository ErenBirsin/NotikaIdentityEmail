using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;

namespace NotikaIdentityEmail.Controllers
{
    public class UserController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager <AppUser> _userManager;

        public UserController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> UserList()
        {
            var values = await _userManager.Users.ToListAsync();
            return View(values);
        }

        public async Task<IActionResult> DeActiveUserCount(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("UserList");
        }

        public async Task<IActionResult> ActivateUserCount(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            user.IsActive = true;
            await _userManager.UpdateAsync(user);
            return RedirectToAction("UserList");
        }

    }
}
