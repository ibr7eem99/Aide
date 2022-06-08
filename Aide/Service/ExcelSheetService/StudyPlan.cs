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

            int rangeBase = worksheet.Dimension.End.Row / 4;
            int semesterYear = 0;

            foreach (Supuervised currentSupuervised in studentSupuervised)
            {
                if ((currentSupuervised.Mark >= 50.0 && currentSupuervised.Mark <= 100.0))
                {
                    var courseNumberAddress = x.FirstOrDefault(c => c.Text.Equals($"{currentSupuervised.CourseNumber}"));
                    if (courseNumberAddress is not null)
                    {
                        registeredAddress = worksheet.Cells[courseNumberAddress.Start.Row, (courseNumberAddress.Start.Column + 6)];
                        registeredAddress.Value = $"{currentSupuervised.Year}{currentSupuervised.Semester}";
                        switch (courseNumberAddress.Start.Row)
                        {
                            case int row when row <= rangeBase:
                                semesterYear = firstYear;
                                break;
                            case int row when row > rangeBase && row <= rangeBase * 2:
                                semesterYear = firstYear + 1;
                                break;
                            case int row when row > rangeBase * 2 && row <= rangeBase * 3:
                                semesterYear = firstYear + 2;
                                break;
                            case int row when row > rangeBase * 3:
                                semesterYear = firstYear + 3;
                                break;
                        }
                        worksheet.Cells[courseNumberAddress.Start.Row, (courseNumberAddress.Start.Column + 7)]
                        .Value = GetStatusShape(currentSupuervised, semesterYear);
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

            /*Regex reg = new Regex(@"([0-9])");
            Supuervised currentCource = null;
            bool flag = false;
            string semester = null;
            for (int row = start.Row + 1; row <= end.Row - 3 || row <= end.Row - 2; row++)
            {
                if (reg.IsMatch(worksheet.Cells[row, leftCourseNumberCol].Text))
                {
                    currentCource = GetSupuervisedInfo(studentSupuervisedList, worksheet.Cells[row, leftCourseNumberCol].Text);

                    if (currentCource is not null)
                    {
                        semester = $"{currentCource.Year}{currentCource.Semester}";
                        worksheet.Cells[row, (leftCourseNumberCol + 6)].Value = semester;
                        *//*int semesterYearRow = registeredAtAddress.FirstOrDefault(r => r.Start.Column ==
                            worksheet.Cells[row, (leftCourseNumberCol + 6)].Start.Column)
                            .Start.Row - 2;
                        string semesterYear = worksheet.Cells[semesterYearRow, (leftCourseNumberCol + 6)].Text.Split("-")[0];
                        worksheet.Cells[row, (leftCourseNumberCol + 7)].Value = GetStatusShape(currentCource, Convert.ToInt32(semesterYear));*//*
                        studentSupuervisedList.RemoveAll(s => s.CourseNumber == currentCource.CourseNumber);
                    }
                }

                if (reg.IsMatch(worksheet.Cells[row, rightCourseNumberCol].Text))
                {
                    
                    currentCource = GetSupuervisedInfo(studentSupuervisedList, worksheet.Cells[row, rightCourseNumberCol].Text);

                    if (currentCource is not null)
                    {
                        semester = $"{currentCource.Year}{currentCource.Semester}";
                        worksheet.Cells[row, (rightCourseNumberCol + 6)].Value = semester;
                        *//*int semesterYearRow = registeredAtAddress.FirstOrDefault(r => r.Start.Column ==
                            worksheet.Cells[row, (rightCourseNumberCol + 6)].Start.Column)
                            .Start.Row - 2;
                        string semesterYear = worksheet.Cells[semesterYearRow, (rightCourseNumberCol + 6)].Text.Split("-")[0];
                        worksheet.Cells[row, (rightCourseNumberCol + 7)].Value = GetStatusShape(currentCource, Convert.ToInt32(semesterYear));*//*
                        studentSupuervisedList.RemoveAll(s => s.CourseNumber == currentCource.CourseNumber);
                    }
                }
                *//*if (flag)
                {
                    flag = false;
                    year++;
                }*//*
            }*/

            /*for (int row = 5; row <= end.Row - 3 || row <= end.Row - 2; row++)
            {
                for (int col = 2; col <= end.Column; col++)
                {
                    string colText = worksheet.Cells[3, col].Text;
                    if (colText == "Course Number" || colText == "Subject Number")
                    {
                        var currentCource = studentSupuervisedList.Where(c => c.CourseNumber
                                        .ToString()
                                        .Equals(worksheet.Cells[row, col].Text));

                        if (currentCource is not null)
                        {
                            *//*if (currentCource.Mark >= 50 && currentCource.Mark <= 100)
                            {*//*
                            if (reg.IsMatch(worksheet.Cells[row, col].Text))
                            {
                                worksheet.Cells[row, col].AutoFitColumns(11);

                                worksheet.Cells[row, col + 6].Value = $"{currentCource.Year}{currentCource.Semester}";
                                studentSupuervisedList.Remove(currentCource);
                            }
                            *//*}*//*
                        }
                    }
                }
            }*/

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
                    int row = end.Row + 2 + (i + 1);
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

        private string GetStatusShape(Supuervised supuervised, int year)
        {
            string shape = "";
            if (supuervised.Year == year)
            {
                int semester = supuervised.Semester;
                if (semester == 1 || semester == 2)
                {
                    shape = "☼";
                }
                else if (semester == 3)
                {
                    shape = "►";
                }
            }
            else if (supuervised.Year > year)
            {
                shape = "►";
            }
            else if (supuervised.Year < year)
            {
                shape = "◄";
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
