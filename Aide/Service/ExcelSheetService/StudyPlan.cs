using Aide.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Graph;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public async Task GenarateExcelSheet(IEnumerable<Supuervised> supuerviseds, string professorName)
        {
            string advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            DriveItem professorFolder = await _oneDriveService.GetProfessorFolder(professorName);

            if (!System.IO.Directory.Exists(advisingMaterialPath))
            {
                CreateNewDirectory(advisingMaterialPath);
            }

            var students = supuerviseds.GroupBy(s => s.StudentID);

            foreach (var student in students)
            {
                Supuervised supuervised = student.FirstOrDefault();
                supuervised.StudentNameEn = ToLowerLetter(supuervised.StudentNameEn).Replace("'", "");
                advisingMaterialPath += $@"\{supuervised.StudentNameEn}{supuervised.StudentID}.xlsx";
                CreateExcelSheet(advisingMaterialPath, supuervised);
                await OpenNewExcelPackag(advisingMaterialPath, student);
                DriveItem studentFolder = await _oneDriveService.GetStudentFolder(
                                                professorFolder.Id, $"{supuervised.StudentNameEn}{supuervised.StudentID}"
                                                ) as DriveItem;

                await _oneDriveService.UplodExcelSheet(studentFolder.Id, advisingMaterialPath);

                if (System.IO.File.Exists(advisingMaterialPath))
                {
                    DeleteFile(advisingMaterialPath);
                }
                advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            }
        }

        private string ToLowerLetter(string studentName)
        {
            string[] strList = studentName.Split(" ");
            string lowerNameLetter = "";

            foreach (var name in strList)
            {
                lowerNameLetter += name.ElementAt(0) + name.Substring(1).ToLower() + " ";
            }
            return lowerNameLetter;
        }

        private async Task OpenNewExcelPackag(string path, IEnumerable<Supuervised> studentSupuervised)
        {
            try
            {
                /*Stopwatch sw = new Stopwatch();
                sw.Start();*/
                using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
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
                /*sw.Stop();
                Console.WriteLine($"Milliseconds = {sw.ElapsedMilliseconds}");*/
                /*throw new Exception();*/
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
            if (worksheet is null)
            {
                throw new ArgumentNullException(nameof(worksheet));
            }

            IEnumerable<ExcelRangeBase> registeredAtAddress = worksheet.Cells.Where(c => c.Text.Equals("Registered At"));
            int firstYear = Convert.ToInt32(studentSupuervised.FirstOrDefault().StudentID.ToString().Substring(0, 4));
            int year = firstYear;
            for (int i = 0; i < registeredAtAddress.Count(); i++)
            {
                ExcelRangeBase element = registeredAtAddress.ElementAt(i);
                worksheet.Cells[element.Start.Row - 2, element.Start.Column].Value = $"{year}-{year + 1}";
                if ((i + 1) % 2 == 1)
                {
                    worksheet.Cells[element.Start.Row - 2, element.Start.Column + 1].Value = "First";
                }
                else if ((i + 1) % 2 == 0)
                {
                    worksheet.Cells[element.Start.Row - 2, element.Start.Column + 1].Value = "Second";
                    year++;
                }
            }

            List<Supuervised> studentSupuervisedList = new List<Supuervised>();
            ExcelRange registeredAddress = null;
            int leftCourseNumberCol = worksheet.Cells.FirstOrDefault(c => c.Text.Equals("Course Number") || c.Text.Equals("Subject Number")).Start.Column;
            int rightCourseNumberCol = worksheet.Cells.LastOrDefault(c => c.Text.Equals("Course Number") || c.Text.Equals("Subject Number")).Start.Column;
            Regex reg = new Regex(@"^([\d])+$");

            var x = worksheet.Cells
                .Where(c => (c.Start.Column == leftCourseNumberCol || c.Start.Column == rightCourseNumberCol) && reg.IsMatch(c.Text))
                .ToList();

            int rowRangeBase = worksheet.Dimension.End.Row / 4;
            int colRangeBase = 18 / 2;
            foreach (Supuervised currentSupuervised in studentSupuervised)
            {
                if ((currentSupuervised.Mark >= 50.0 && currentSupuervised.Mark <= 100.0))
                {
                    ExcelRangeBase courseNumberAddress = x.FirstOrDefault(c => c.Text.Equals($"{currentSupuervised.CourseNumber}"));
                    if (courseNumberAddress is not null)
                    {
                        int courseNumberRow = courseNumberAddress.Start.Row;
                        registeredAddress = worksheet.Cells[courseNumberRow, (courseNumberAddress.Start.Column + 6)];
                        registeredAddress.Value = $"{currentSupuervised.Year}{currentSupuervised.Semester}";
                        worksheet.Cells[courseNumberRow, (courseNumberAddress.Start.Column + 7)]
                        .Value = GetStatusShape(
                            currentSupuervised,
                            GetCourseYearBasedOnStudyPlan(courseNumberRow, rowRangeBase, firstYear),
                            GetCourseSemesterBasedOnStudyPlan(courseNumberAddress, colRangeBase)
                            );
                    }
                    else
                    {
                        studentSupuervisedList.Add(currentSupuervised);
                    }
                }
                else
                {
                    continue;
                }
            }

            if (studentSupuervisedList.Count() > 0)
            {
                ExcelCellAddress start = worksheet.Dimension.Start;
                ExcelCellAddress end = worksheet.Dimension.End;

                worksheet.Cells[end.Row + 2, 2].Value = "Course Number";
                worksheet.Cells[end.Row + 2, 2].AutoFitColumns(10);
                worksheet.Cells[end.Row + 2, 2].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 2, end.Row + 3, 2].Merge = true;
                worksheet.Cells[end.Row + 2, 2].Style.WrapText = true;
                worksheet.Cells[end.Row + 2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[end.Row + 2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[end.Row + 2, 3].Value = "Course Title";
                worksheet.Cells[end.Row + 2, 3].AutoFitColumns(35);
                worksheet.Cells[end.Row + 2, 3].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 3, end.Row + 3, 3].Merge = true;
                worksheet.Cells[end.Row + 2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[end.Row + 2, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells[end.Row + 2, 4].Value = "Registered At";
                worksheet.Cells[end.Row + 2, 4, end.Row + 3, 5].Merge = true;
                worksheet.Cells[end.Row + 2, 4].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[end.Row + 2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                for (int i = 0; i < studentSupuervisedList.Count(); i++)
                {
                    int row = (end.Row + 3) + (i + 1);
                    worksheet.Cells[row, 2].Value = studentSupuervisedList[i].CourseNumber;
                    worksheet.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 3].Value = studentSupuervisedList[i].CourseNameEn;
                    worksheet.Cells[row, 3].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 4].Value = $"{studentSupuervisedList[i].Year}{studentSupuervisedList[i].Semester}";
                    worksheet.Cells[row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }
        }

        private Supuervised GetSupuervisedInfo(List<Supuervised> supuerviseds, string courseNumber)
        {
            return supuerviseds.Where(c => c.CourseNumber
                    .ToString()
                    .Equals(courseNumber))
                    .OrderByDescending(s => s.Mark).LastOrDefault();
        }

        private int GetCourseYearBasedOnStudyPlan(int courseNumberAddress, int rowRangeBase, int firstYear)
        {
            int semesterYear = 0;
            switch (courseNumberAddress)
            {
                case int row when row <= rowRangeBase:
                    semesterYear = firstYear;
                    break;
                case int row when row > rowRangeBase && row <= rowRangeBase * 2:
                    semesterYear = firstYear + 1;
                    break;
                case int row when row > rowRangeBase * 2 && row <= rowRangeBase * 3:
                    semesterYear = firstYear + 2;
                    break;
                case int row when row > rowRangeBase * 3:
                    semesterYear = firstYear + 3;
                    break;
            }
            return semesterYear;
        }

        private int GetCourseSemesterBasedOnStudyPlan(ExcelAddressBase courseAddress, int colRangeBase)
        {
            int semester = 0;
            if (courseAddress.Start.Column < colRangeBase)
            {
                semester = 1;
            }
            else if (courseAddress.Start.Column > colRangeBase)
            {
                semester = 2;
            }
            return semester;
        }

        private string GetStatusShape(Supuervised supuervised, int yearBase, int semesterBase)
        {
            string shape = string.Empty;
            if (supuervised.Year == yearBase)
            {
                int semester = supuervised.Semester;
                if (semester == semesterBase)
                {
                    shape = "☼";
                }
                else if (semester < semesterBase)
                {
                    shape = "◄";
                }
                else if (semester == 3 || semester > semesterBase)
                {
                    shape = "►";
                }
            }
            else if (supuervised.Year > yearBase)
            {
                shape = "◄";
            }
            else if (supuervised.Year < yearBase)
            {
                shape = "►";
            }
            return shape;
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
                    // Copy Existing Excel Sheet Template to the Empty Excel Sheet
                    CopyExcelSheetTempleate(path, supuervised);
                }
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
