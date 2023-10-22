using System.ComponentModel.DataAnnotations;

namespace DevDiary.Models
{
    public class Favorites
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; }


        [Required]
        public string? AppUserId { get; set; }

        public virtual BlogPost? BlogPost { get; set; }
        public virtual AppUser? AppUser { get; set; }
    }
}
