using Aide.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Graph;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using oneDrive = Aide.Service.OneDriveService;

namespace Aide.Service.ExcelSheetService
{
    public class StudyPlan : IStudyPlan
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private oneDrive.IOneDriveService _oneDriveService;

        public StudyPlan(IWebHostEnvironment webHostEnvironment, oneDrive.IOneDriveService oneDriveService)
        {
            _webHostEnvironment = webHostEnvironment;
            _oneDriveService = oneDriveService;
        }

        public async Task GenarateExcelSheet(IEnumerable<Supuervised> supuerviseds, string professorName, GraphServiceClient graphServiceClient)
        {
            string advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            DriveItem professorFolder = await _oneDriveService.GetProfessorFolder(graphServiceClient, professorName);

            if (!System.IO.Directory.Exists(advisingMaterialPath))
            {
                CreateNewDirectory(advisingMaterialPath);
            }

            var students = supuerviseds.GroupBy(s => s.StudentID);

            foreach (var student in students)
            {
                Supuervised supuervised = student.FirstOrDefault();
                supuervised.StudentNameEn = supuervised.StudentNameEn.Replace("'", "");
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                advisingMaterialPath += $@"\{textInfo.ToTitleCase(supuervised.StudentNameEn)} {supuervised.StudentID}.xlsx";
                CreateExcelSheet(advisingMaterialPath, supuervised);
                /*var studentSupuervised = supuerviseds.Where(s => s.StudentID == student.Key);*/
                await OpenNewExcelPackag(advisingMaterialPath, student);
                DriveItem studentFolder = await _oneDriveService.GetStudentFolder(
                                                graphServiceClient,
                                                professorFolder.Id, $"{supuervised.StudentNameEn} {supuervised.StudentID}"
                                                ) as DriveItem;

                await _oneDriveService.UplodExcelSheet(graphServiceClient, studentFolder.Id, advisingMaterialPath);

                if (System.IO.File.Exists(advisingMaterialPath))
                {
                    DeleteFile(advisingMaterialPath);
                }
                advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            }

        }

        private async Task OpenNewExcelPackag(string path, IEnumerable<Supuervised> studentSupuervised)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    try
                    {
                        ExcelWorksheet worksheet = GetSpecificWorkSheet(package);
                        FillStudentAdvisingPlanTemplate(worksheet, studentSupuervised);
                        await package.SaveAsync();
                    }
                    catch (ArgumentNullException ex)
                    {
                        throw new ArgumentNullException(ex.ParamName, "Excel Worksheet not found, Plese try again or contact with computer center");
                    }
                }
            }
            catch (LicenseException ex)
            {
                throw new Exception($"{ex.Message}, Plese contact with computer center to add EPPlus lincense");
            }
            catch
            {
                throw;
            }
        }

        private void FillStudentAdvisingPlanTemplate(
            ExcelWorksheet worksheet,
            IEnumerable<Supuervised> studentSupuervised
            )
        {
            /*int courseNumberAddress = 0;*/

            if (worksheet is null)
            {
                throw new ArgumentNullException(nameof(worksheet));
            }

            ExcelCellAddress start = worksheet.Dimension.Start;
            ExcelCellAddress end = worksheet.Dimension.End;
            Regex reg = new Regex(@"([0-9])");

            var currentStudent = studentSupuervised.ToList<Supuervised>();

            for (int row = 5; row <= end.Row - 3 || row <= end.Row - 2; row++)
            {
                for (int col = 2; col <= end.Column; col++)
                {
                    string colText = worksheet.Cells[3, col].Text;
                    if (colText == "Course Number" || colText == "Subject Number")
                    {
                        var currentCource = currentStudent.Where(c => c.CourseNumber
                                        .ToString()
                                        .Equals(worksheet.Cells[row, col].Text))
                                        .OrderBy(c => c.Mark)
                                        .LastOrDefault();

                        if (currentCource is not null)
                        {
                            if (currentCource.Mark >= 50 && currentCource.Mark <= 100)
                            {
                                if (reg.IsMatch(worksheet.Cells[row, col].Text))
                                {
                                    worksheet.Cells[row, col].AutoFitColumns(11);

                                    worksheet.Cells[row, col + 6].Value = $"{currentCource.Year}{currentCource.Semester}";
                                    currentStudent.Remove(currentCource);
                                }
                            }

                            // TODO
                            /*if (currentStudent
                                .Where(c => c.CourseNumber
                                .ToString()
                                .Equals(worksheet.Cells[row, col].Text)).Count() > 0
                                )
                            {

                            }*/
                        }
                    }
                }
            }

            if (currentStudent.Count() > 0)
            {
                worksheet.Cells[end.Row + 2, 2].Value = "Course Number";
                worksheet.Cells[end.Row + 2, 2].AutoFitColumns(10);
                worksheet.Cells[end.Row + 2, 2].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 2, end.Row + 3, 2].Merge = true;
                worksheet.Cells[end.Row + 2, 2].Style.WrapText = true;
                worksheet.Cells[end.Row + 2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[end.Row + 2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[end.Row + 2, 3].Value = "Course Title";
                worksheet.Cells[end.Row + 2, 3].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[end.Row + 2, 3].Style.Border.Right.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[end.Row + 2, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[end.Row + 2, 3, end.Row + 3, 3].Merge = true;
                worksheet.Cells[end.Row + 2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[end.Row + 2, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[end.Row + 2, 4].Value = "Registered At";
                worksheet.Cells[end.Row + 2, 4, end.Row + 3, 5].Merge = true;
                worksheet.Cells[end.Row + 2, 4].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[end.Row + 2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                for (int i = 0; i < currentStudent.Count(); i++)
                {
                    worksheet.Cells[end.Row + 2 + (i + 1), 2].Value = currentStudent[i].CourseNumber;
                    worksheet.Cells[end.Row + 2 + (i + 1), 2].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[end.Row + 2 + (i + 1), 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[end.Row + 2 + (i + 1), 3].Value = currentStudent[i].CourseNameEn;
                    worksheet.Cells[end.Row + 2 + (i + 1), 3].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[end.Row + 2 + (i + 1), 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[end.Row + 2 + (i + 1), 4].Value = $"{currentStudent[i].Year}{currentStudent[i].Semester}";
                    worksheet.Cells[end.Row + 2 + (i + 1), 4].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[end.Row + 2 + (i + 1), 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }
        }

        private ExcelWorksheet GetSpecificWorkSheet(ExcelPackage package)
        {
            if (package.Workbook.Worksheets is null)
            {
                throw new ArgumentNullException("Study Plan Excel Sheet Should have at least one sheet");
            }
            return package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("SE - Adv E") || w.Name.Contains("CS-Adv-E") || w.Name.Contains("Cyber-Adv-E"));
        }

        private void CreateExcelSheet(string path, Supuervised supuervised)
        {
            try
            {
                // Create Empty Excel Sheet
                using (FileStream fs = System.IO.File.Create(path))
                {
                    fs.Close();
                    CopyExcelSheetTempleate(path, supuervised);
                }
                // Copy Existing Excel Sheet Template to the Empty Excel Sheet
            }
            catch
            {
                DeleteFile(path);
                throw;
            }
        }

        private void CopyExcelSheetTempleate(string path, Supuervised supuervised)
        {
            // Get Student Advising Plan File based on MajorId & StudentId
            string existingExcelSheetPath = GetStudentPalnSheetFile(supuervised.SemesterStudyPlan, supuervised.SpecNameEn);
            try
            {
                System.IO.File.Copy(existingExcelSheetPath, path, true);
            }
            catch
            {
                throw;
            }
        }

        private void CreateNewDirectory(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch
            {
                throw;
            }
        }

        private void DeleteFile(string path)
        {
            try
            {
                System.IO.File.Delete(path);
            }
            catch
            {
                throw;
            }
        }

        private string GetStudentPalnSheetFile(int semesterStudyPlan, string majorName)
        {
            string FullFileName = $"{_webHostEnvironment.WebRootPath}\\AdvisingMaterial\\Majors\\";

            switch (majorName.ToUpper())
            {
                case "COMPUTER SCIENCE":
                    FullFileName += @"Computer Science\StudyPlan";
                    FullFileName = GetStudentPalnSheetFileName(FullFileName, semesterStudyPlan);
                    break;
                case "SOFTWARE ENGINEERING":
                    FullFileName += @"Software Engineer\StudyPlan";
                    FullFileName = GetStudentPalnSheetFileName(FullFileName, semesterStudyPlan);
                    break;
                case "CYBERSECURITY AND CLOUD COMPUTING":
                    FullFileName += @"Cyber Security\StudyPlan";
                    FullFileName = GetStudentPalnSheetFileName(FullFileName, semesterStudyPlan);
                    break;
            }
            return FullFileName;
        }

        private string GetStudentPalnSheetFileName(string FullFileName, int semesterStudyPlan)
        {
            try
            {
                string studyPalnYear = semesterStudyPlan.ToString()
                    .Remove(semesterStudyPlan.ToString().Count() - 1);

                return System.IO.Directory.GetFiles(FullFileName)
                       .FirstOrDefault(f => f.Split("-")[0].Contains(studyPalnYear));
            }
            catch
            {
                throw;
            }
        }
    }


}
