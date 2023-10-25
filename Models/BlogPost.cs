using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DevDiary.Models
{
    public class BlogPost
    {
        private DateTime _created;
        private DateTime? _updated;

        // Primary Key
        public int Id { get; set; }

        // Foreign Key
        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters.", MinimumLength = 2)]
        public string? Title { get; set; } // "An Interesting Blog Post?!

        [StringLength(600, ErrorMessage = "The {0} must be at most {1} characters.")]
        public string? Abstract { get; set; }

        [Required]
        public string? Content { get; set; }


        public DateTime Created
        {
            get
            {
                return _created;
            }
            set
            {
                _created = value.ToUniversalTime();
            }
        }

        public DateTime? Updated
        {
            get => _updated;

            set
            {
                if (value.HasValue)
                {
                    _updated = value.Value.ToUniversalTime();
                }
                else
                {
                    _updated = null;
                }
            }
        }

        [Required]
        public string? Slug { get; set; } // "an-interesting-blog-post

        [Display(Name = "Published")]
        public bool IsPublished { get; set; }

        [Display(Name = "Deleted")]
        public bool IsDeleted { get; set; }


        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public byte[]? ImageData { get; set; }

        public string? ImageType { get; set; }

        // Navigation Properties

        public int CategoryId { get; set; }

        [JsonIgnore]
        public virtual Category? Category { get; set; }
        [JsonIgnore]
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

        [JsonIgnore]
        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();

        [JsonIgnore]
        public virtual ICollection<BlogLike> Likes { get; set; } = new HashSet<BlogLike>();

    }
}
