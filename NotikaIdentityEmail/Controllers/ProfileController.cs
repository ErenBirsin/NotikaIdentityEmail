using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Entities;
using NotikaIdentityEmail.Models.IdentityModels;

namespace NotikaIdentityEmail.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> userManager;

        public ProfileController(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var values = await userManager.FindByNameAsync(User.Identity.Name);
            UserEditViewModel userEditViewModel = new UserEditViewModel();
            userEditViewModel.Name = values.Name;
            userEditViewModel.Surname = values.SurName;
            userEditViewModel.PhoneNumber = values.PhoneNumber;
            userEditViewModel.ImageUrl= values.ImageUrl;
            userEditViewModel.City = values.City;
            userEditViewModel.UserName = values.UserName;
            userEditViewModel.Email = values.Email;
            return View(userEditViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditViewModel model)
        {
            if(model.Password == model.PasswordConfirm)
            {
                var user = await userManager.FindByNameAsync(User.Identity.Name);
                user.Name = model.Name;
                user.SurName = model.Surname;
                user.PhoneNumber = model.PhoneNumber;
                user.City = model.City;
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.ImageUrl = model.ImageUrl;
                user.PasswordHash = userManager.PasswordHasher.HashPassword(user, model.Password);
                await userManager.UpdateAsync(user);
            }
            return View();
        }
    }
}
