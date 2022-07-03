using Aide.Attribute;
using Aide.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Graph;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
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

        private IEnumerable<Supuervised> StudentCourseList { get; set; }

        public StudyPlan(IWebHostEnvironment webHostEnvironment, oneDrive.IOneDriveService oneDriveService)
        {
            _webHostEnvironment = webHostEnvironment;
            _oneDriveService = oneDriveService;
        }

        public async Task PlanGenerator(IEnumerable<Supuervised> supuerviseds, string professorName)
        {
            string advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            DriveItem professorFolder = await _oneDriveService.GetProfessorFolder(professorName);

            if (!System.IO.Directory.Exists(advisingMaterialPath))
            {
                AdvicingMatelrialFolderMangment.CreateNewDirectory(advisingMaterialPath);
            }
            else
            {
                if (System.IO.Directory.GetFiles(advisingMaterialPath).Any())
                {
                    foreach (string file in System.IO.Directory.GetFiles(advisingMaterialPath))
                    {
                        AdvicingMatelrialFolderMangment.DeleteFile(file);
                    }
                }
            }

            var students = supuerviseds.GroupBy(s => s.StudentID);

            foreach (var student in students)
            {
                StudentCourseList = student;
                Supuervised supuervised = student.FirstOrDefault();
                supuervised.StudentNameEn = ToLowerLetter(supuervised.StudentNameEn).Replace("'", "");
                advisingMaterialPath += $@"\{supuervised.StudentNameEn}{supuervised.StudentID}.xlsx";
                CreateExcelSheet(advisingMaterialPath, supuervised);
                await OpenNewExcelPackag(advisingMaterialPath);
                DriveItem studentFolder = await _oneDriveService.GetStudentFolder(
                                                professorFolder.Id, $"{supuervised.StudentNameEn}{supuervised.StudentID}"
                                                );

                await _oneDriveService.UplodExcelSheet(studentFolder.Id, advisingMaterialPath);
                AdvicingMatelrialFolderMangment.DeleteFile(advisingMaterialPath);
                advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            }
        }

        // convert first litter from student name to Lower Letter.
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

        // path: is a path of study plan sheet file inside wwwroot folder.
        private async Task OpenNewExcelPackag(string path)
        {
            try
            {
                using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
                {
                    try
                    {
                        ExcelWorksheet worksheet = GetSpecificWorkSheet(package);
                        FillStudentAdvisingPlanTemplate(worksheet, StudentCourseList);
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
        /*
         * Summary:
         *      Fill Registered At, Status value in side study plan file
         *  Parameters:
         *      worksheet: is a sheet that will fill it.
         *      studentSupuervised: all the subject that the student was take it.
         */
        private void FillStudentAdvisingPlanTemplate(
            ExcelWorksheet worksheet,
            IEnumerable<Supuervised> studentSupuervised
            )
        {
            if (worksheet is null)
            {
                throw new ArgumentNullException(nameof(worksheet));
            }

            var cell = worksheet.Cells["M1"].Value = $"{studentSupuervised.FirstOrDefault().StudentID} {studentSupuervised.FirstOrDefault().StudentNameEn}";

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

            // list that sholud cintains all the subjects which not include in the sheet.
            List<Supuervised> studentSupuervisedList = new List<Supuervised>();
            ExcelRange registeredAddress = null;
            int leftCourseNumberCol = worksheet.Cells.FirstOrDefault(c => c.Text.Equals("Course Number") || c.Text.Equals("Subject Number")).Start.Column;
            int rightCourseNumberCol = worksheet.Cells.LastOrDefault(c => c.Text.Equals("Course Number") || c.Text.Equals("Subject Number")).Start.Column;
            Regex reg = new Regex(@"^([\d])+$");

            var x = worksheet.Cells
                .Where(c => (c.Start.Column == leftCourseNumberCol || c.Start.Column == rightCourseNumberCol) && reg.IsMatch(c.Text))
                .ToList();

            // Dimension.End.Row: is a number of the row inside the sheet
            // 4 it means 4 year.
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
                            GetActualCourseYearBasedOnStudyPlan(courseNumberRow, rowRangeBase, firstYear),
                            GetActualCourseSemesterBasedOnStudyPlan(courseNumberAddress, colRangeBase)
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

                // Create new table to display all the subjects which not include in the sheet.
                worksheet.Cells[end.Row + 2, 2].Value = "Course Number";
                worksheet.Cells[end.Row + 2, 2].Style.WrapText = true;
                worksheet.Cells[end.Row + 2, 2, end.Row + 3, 2].Merge = true;

                worksheet.Cells[end.Row + 2, 3].Value = "Course Title";
                worksheet.Cells[end.Row + 2, 3].AutoFitColumns(35);
                worksheet.Cells[end.Row + 2, 3, end.Row + 3, 3].Merge = true;
                worksheet.Cells[end.Row + 2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[end.Row + 2, 4].Value = "Registered At";
                worksheet.Cells[end.Row + 2, 4, end.Row + 3, 5].Merge = true;

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

        /*
         * Summary:
         *      Get the actual course year that sholud the student take the course in.
         * Parameters:
         *      courseNumberAddress = the row value where the course id is.
         *      rowRangeBase = tatal number of row inside the sheet / 4.
         *      firstYear = first 4 digit of student university number.
        */
        private int GetActualCourseYearBasedOnStudyPlan(int courseNumberAddress, int rowRangeBase, int firstYear)
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
        // Get the actual course semester that sholud the student take the course in.
        private int GetActualCourseSemesterBasedOnStudyPlan(ExcelAddressBase courseAddress, int colRangeBase)
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

        // Identifies a course statuse shape based on actual course year and actual course semester
        // yearBase: the actual course year.
        // semesterBase: the actual course semester.
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
                shape = "►";
            }
            else if (supuervised.Year < yearBase)
            {
                shape = "◄";
            }
            return shape;
        }

        // Get the advising sheet form the student excelsheet.
        private ExcelWorksheet GetSpecificWorkSheet(ExcelPackage package)
        {
            if (package.Workbook.Worksheets is null)
            {
                throw new ArgumentNullException("Study Plan Excel Sheet Should have at least one sheet");
            }
            return package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("SE - Adv E") || w.Name.Contains("CS-Adv-E") || w.Name.Contains("Cyber-Adv-E"));
        }

        // Create empty excelsheet file then make a copy of study plan teamplate.
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
                AdvicingMatelrialFolderMangment.DeleteFile(path);
                throw;
            }
        }


        private void CopyExcelSheetTempleate(string path, Supuervised supuervised)
        {
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

        // Get Student Advising Plan File based on Major name & Student Study Plan semester.
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

        // Get Student Paln Sheet File Name from StudyPlan folder
        // FullFileName: StudyPlan folder path that inside in wwwroot folder.
        // Student Study Plan semester.
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
