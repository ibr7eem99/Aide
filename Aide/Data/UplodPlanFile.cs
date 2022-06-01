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
        [Display(Name = "Plan Semester")]
        public int PlanSemester { get; set; }
        [Display(Name = "Plan File")]
        [DataType(DataType.Upload)]
        public IFormFile PlanFile { get; set; }
        public bool IsActive { get; set; }
    }
}
