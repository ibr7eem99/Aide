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
        [StringLength(20, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,20}$", ErrorMessage = "Password should contains Capital letters, small leters and numbers")]
        public string Password { get; set; }
    }
}
