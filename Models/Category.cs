using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DevDiary.Models
{
    public class Category
    {
        // primary Key

        public int Id { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Name { get; set; }

        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Description { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        public virtual ICollection<BlogPost> BlogPosts { get; set; } = new HashSet<BlogPost>();
        public string? AppUserId { get; internal set; }
    }
}
