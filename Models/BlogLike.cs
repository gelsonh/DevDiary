using System.ComponentModel.DataAnnotations;

namespace DevDiary.Models
{
    public class BlogLike
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }
        public bool IsLiked { get; set; }

        [Required]
        public string? AppUserId { get; set; }

        public virtual BlogPost? BlogPost { get; set; }
        public virtual AppUser? AppUser { get; set; }
    }
}
