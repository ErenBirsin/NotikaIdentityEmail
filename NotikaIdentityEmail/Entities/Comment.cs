using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NotikaIdentityEmail.Entities
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string CommentDetail { get; set; }
        public DateTime CommentDate { get; set; }
        public string CommentStatus { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }

}
