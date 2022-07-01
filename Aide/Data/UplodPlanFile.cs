using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Aide.Data
{
    public class UplodPlanFile
    {
        [Display(Name = "Major Name")]
        public string MajorName { get; set; }
        [Display(Name = "New Major Name")]
        public string NewMajorName { get; set; }
        [Display(Name = "Plan Year", Prompt = "Example: 2015")]
        [Required]
        [RegularExpression(@"^([\d]){4}$", ErrorMessage = "The field Plan Year must be contains 4 digit")]
        public int PlanYear { get; set; }
        [Display(Name = "Plan File")]
        [DataType(DataType.Upload)]
        [Required]
        public IFormFile PlanFile { get; set; }
        public bool IsActive { get; set; }
    }
}
