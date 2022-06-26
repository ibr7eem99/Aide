using System.ComponentModel.DataAnnotations;

namespace Aide.Data
{
    public class StudentPlanInfo
    {
        [Required]
        [RegularExpression(@"^([\d])+$", ErrorMessage = "The value is not valid, Year should be integer number")]
        [DataType(DataType.Text)]
        public int Year { get; set; }
        [Required]
        public eSemester Semester { get; set; }
    }
}
