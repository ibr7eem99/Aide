using System.ComponentModel.DataAnnotations;

namespace Aide.Models.ViewModels
{
    public class StudentPlanInfo
    {
        /*[Required]
        [MaxLength(255)]
        [DataType(DataType.Text)]
        [Display(Name = "Destination Path", Prompt = @"D:\StudyPlans")]
        public string DestinationFolderPath { get; set; }*/
        [Required]
        [DataType(DataType.Text)]
        public int Year { get; set; }
        public eSemester Semester { get; set; }
    }
}