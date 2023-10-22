using System.ComponentModel.DataAnnotations;

namespace DevDiary.Models
{
    public class Tag
    {
        // primary key
        public int Id { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Name { get; set; }

        public ICollection<BlogPost> BlogsPost { get; set; } = new HashSet<BlogPost>();
    }
}

