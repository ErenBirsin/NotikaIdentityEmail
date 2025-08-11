using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace NotikaIdentityEmail.Controllers
{
    public class CategoryController : Controller
    {
        private readonly EmailContext _context;

        public CategoryController(EmailContext context)
        {
            _context = context;
        }

        
        public IActionResult CategoryList()
        {
            var token = Request.Cookies["jwtToken"];
            if(string.IsNullOrEmpty(token))
            {
                TempData["error"] = "Giriş Yapmalısınız...";
                return RedirectToAction("UserLogin", "Login");
            }

            JwtSecurityToken jwt;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                jwt = handler.ReadJwtToken(token);
            }
            catch (Exception)
            {
                TempData["error"] = "Token Geçersiz";
                return RedirectToAction("UserLogin", "Login");

            }

            var city =jwt.Claims.FirstOrDefault(c => c.Type == "city")?.Value;
            if (city != "İstanbul")
            {
                return Forbid();
            }

            List<Category> categories = new List<Category>();

            if (_context.Categories != null)
            {
                categories = _context.Categories.ToList();
            }

            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            category.CategoryStatus = true;
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("CategoryList", "Category");
        }

        public IActionResult DeleteCategory(int id)
        {
            var value = _context.Categories.Find(id);
            if (value != null)
            {
                _context.Categories.Remove(value);
                _context.SaveChanges();
            }
            return RedirectToAction("CategoryList");
        }

        [HttpGet]
        public IActionResult UpdateCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return RedirectToAction("CategoryList");
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            _context.SaveChanges();
            return RedirectToAction("CategoryList");
        }
    }
}