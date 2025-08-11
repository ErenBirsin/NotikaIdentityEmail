using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotikaIdentityEmail.Context;
using NotikaIdentityEmail.Entities;

namespace NotikaIdentityEmail.Controllers
{
    public class CommentController : Controller
    {
        private readonly EmailContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(EmailContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult UserComments()
        {
            var values = _context.Comments.Include(x => x.AppUser).ToList();
            return View(values);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult UserCommentList()
        {
            var values = _context.Comments.Include(x => x.AppUser).ToList();
            return View(values);
        }

        [HttpGet]

        public PartialViewResult CreateComment()
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment(Comment comment)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            comment.AppUserId = user.Id;
            comment.CommentDate = DateTime.Now;

            using (var client = new HttpClient())
            {
                var apiKey = "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";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                try
                {
                    var translateRequestBody = new
                    {

                        inputs = comment.CommentDetail,

                    };
                    var translateJson = JsonSerializer.Serialize(translateRequestBody);
                    var translateContent = new StringContent(translateJson, Encoding.UTF8, "application/json");

                    var translateResponse = await client.PostAsync("https://api-inference.huggingface.co/models/Helsinki-NLP/opus-mt-tr-en", translateContent);

                    var translateResponseString = await translateResponse.Content.ReadAsStringAsync();
                    string englishComment = comment.CommentDetail;
                    if (translateResponseString.TrimStart().StartsWith("["))
                    {
                        var translateDocument = JsonDocument.Parse(translateResponseString);
                        englishComment = translateDocument.RootElement[0].GetProperty("translation_text").GetString();
                    }
                    else
                    {
                        englishComment = translateResponseString;
                    }

                    var toxicRequestBody = new
                    {
                        inputs = englishComment
                    };



                    var toxcJson = JsonSerializer.Serialize(toxicRequestBody);


                    var toxicContent = new StringContent(toxcJson, Encoding.UTF8, "application/json");

                    var toxicResponse = await client.PostAsync("https://api-inference.huggingface.co/models/unitary/toxic-bert", toxicContent);
                    var toxicResponseString = await toxicResponse.Content.ReadAsStringAsync();

                    if (toxicResponseString.TrimStart().StartsWith("["))
                    {
                        var toxicDocument = JsonDocument.Parse(toxicResponseString);
                        foreach (var item in toxicDocument.RootElement[0].EnumerateArray())
                        {
                            string label = item.GetProperty("label").GetString();
                            double score = item.GetProperty("score").GetDouble();

                            if (score > 0.5)
                            {
                                comment.CommentStatus = "Toksik Yorum";
                                break;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(comment.CommentStatus))
                    {
                        comment.CommentStatus = "Yorum Onaylandı";
                    }

                }
                catch (Exception ex)
                {
                    comment.CommentStatus = "Onay Bekliyor";

                }
                _context.Comments.Add(comment);
                _context.SaveChanges();
                return RedirectToAction("UserCommentList");
            }

        }

        public IActionResult DeleteComment(int id)
        {
            var values = _context.Comments.Find(id);
            _context.Comments.Remove(values);
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }

        public IActionResult CommentStatusChangeToToxic(int id )
        {
            var values = _context.Comments.Find(id);
            values.CommentStatus = "Toksik Yorum";
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }

        public IActionResult CommentStatusChangeToPassive(int id)
        {
            var values = _context.Comments.Find(id);
            values.CommentStatus = "Yorum Kaldırıldı";
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }

        public IActionResult CommentStatusChangeToActive(int id)
        {
            var values = _context.Comments.Find(id);
            values.CommentStatus = "Yorum Onaylandı";
            _context.SaveChanges();
            return RedirectToAction("UserCommentList");
        }

    }
}
// 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