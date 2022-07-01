using System.ComponentModel.DataAnnotations;

namespace Aide.Data
{
    public class Login
    {
        public string grand_type { get; set; }
        public string client_id { get; set; }
        public string scope { get; set; }
        [Required]
        //[EmailAddress]
        /*[StringLength(50,MinimumLength =11)]*/
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "The field Password must be at least 8 character")]
        [MaxLength(20, ErrorMessage = "The field Password must be at most 20 character")]
        public string Password { get; set; }
    }
}
