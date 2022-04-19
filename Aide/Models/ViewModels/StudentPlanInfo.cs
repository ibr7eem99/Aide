using System.ComponentModel.DataAnnotations;

namespace Aide.Models.ViewModels
{
    public class StudentPlanInfo
    {
        [Required]
        [MaxLength(255)]
        [DataType(DataType.Text)]
        [Display(Name = "Destination Path", Prompt = @"D:\StudyPlans")]
        public string DestinationFolderPath { get; set; }
        [Required]
        [Display(Name = "Current Semester")]
        [DataType(DataType.Text)]
        public int CurrentSemester { get; set; }
    }
}