using System.ComponentModel.DataAnnotations;

namespace DevDiary.Models
{

    public class EmailSetting
    {
        [Required(ErrorMessage = "Email address is required.")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Email password is required.")]
        public string? EmailPassword { get; set; }

        [Required(ErrorMessage = "Email host is required.")]
        public string? EmailHost { get; set; }

        [Required(ErrorMessage = "Email port is required.")]
        public int EmailPort { get; set; }
    }

}
