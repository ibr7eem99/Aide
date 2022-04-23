using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aide.Service
{
    public class StudyPlan
    {
        /*private readonly IWebHostEnvironment _webHostEnvironment;

        public StudyPlan(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<bool> GenarateExcelSheet()
        {
            string path = @"C:\Users\ib_ra\Desktop\StudyPlans";
            if (!System.IO.Directory.Exists(path))
            {
                CreateDirectory(path);
            }

            foreach (var student in StudentsInfo)
            {
                CreateExcelSheet(path, student);

                string copyExcelSheetPath = GetLastEcelSheetInTheDirectory(path);
                FileInfo fileInfo = new FileInfo(copyExcelSheetPath);
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = GetSpecificWorkSheet(package);
                    FillStudentAdvisingPlanTemplate(worksheet, student);

                    try
                    {
                        await package.SaveAsync();
                    }
                    catch
                    {

                    }
                }
            }
            return true;
        }

        private void FillStudentAdvisingPlanTemplate(ExcelWorksheet worksheet, Student student)
        {
            int courseNumberAddress1 = 0;
            int courseNumberAddress2 = 0;
            int registeredAtAddress1 = 0;
            int registeredAtAddress2 = 0;
            *//*string yearAddress1;
            string yearAddress2;
            string semesterAddress1;
            string semesterAddress2;*//*
            ExcelCellAddress start = worksheet.Dimension.Start;
            ExcelCellAddress end = worksheet.Dimension.End;
            Regex reg = new Regex(@"([0-9])");

            for (int col = start.Column + 1; col <= end.Column; ++col)
            {
                if (courseNumberAddress1 == 0 && courseNumberAddress2 == 0)
                {
                    if (worksheet.Cells[start.Row + 2, col].Text == "Course Number")
                    {
                        courseNumberAddress1 = col;
                        courseNumberAddress2 = (end.Column / 2) + courseNumberAddress1;
                    }
                }

                if (registeredAtAddress1 == 0 && registeredAtAddress2 == 0)
                {
                    if (worksheet.Cells[start.Row + 3, col].Text == "Registered At")
                    {
                        registeredAtAddress1 = col;
                        registeredAtAddress2 = (end.Column / 2) + registeredAtAddress1;
                    }
                }
            }

            var query = from course in Courses
                        join registration in registrations on course.CourseId equals registration.CourseId
                        where registration.StudentId == student.StudentId
                        select new
                        {
                            courseId = registration.CourseId,
                            semeter = registration.Semester,
                        };

            var currentRegistration = query.ToList();

            for (int row = start.Row + 4; row <= end.Row - 3 || row <= end.Row - 2; row++)
            {
                for (int col = start.Column + 1; col <= end.Column - 3 || col <= end.Column - end.Column - 2; ++col)
                {

                    if (reg.IsMatch(worksheet.Cells[row, courseNumberAddress1].Text))
                    {
                        worksheet.Cells[row, courseNumberAddress1].AutoFitColumns(10);

                        if (query.Any())
                        {
                            var currentCource = currentRegistration.
                            FirstOrDefault(c => c.courseId.ToString().Equals(worksheet.Cells[row, courseNumberAddress1].Text));
                            if (currentCource is not null)
                            {
                                worksheet.Cells[row, registeredAtAddress1].Value = currentCource.semeter;
                                currentRegistration.Remove(currentCource);
                            }
                        }
                    }

                    if (reg.IsMatch(worksheet.Cells[row, courseNumberAddress2].Text))
                    {
                        worksheet.Cells[row, courseNumberAddress2].AutoFitColumns(10);
                        if (query.Any())
                        {
                            var currentCource = currentRegistration.
                            FirstOrDefault(c => c.courseId.ToString().Equals(worksheet.Cells[row, courseNumberAddress2].Text));
                            if (currentCource is not null)
                            {
                                worksheet.Cells[row, registeredAtAddress2].Value = currentCource.semeter;
                                currentRegistration.Remove(currentCource);
                            }
                        }
                    }
                }
            }

            if (currentRegistration.Count() > 0)
            {
                worksheet.Cells[end.Row + 2, 2].Value = "Course Number";
                worksheet.Cells[end.Row + 2, 2].AutoFitColumns(10);
                worksheet.Cells[end.Row + 2, 2].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 2, end.Row + 3, 2].Merge = true;
                worksheet.Cells[end.Row + 2, 2].Style.WrapText = true;
                worksheet.Cells[end.Row + 2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                *//*worksheet.Cells[end.Row + 2, 3].Value = "Course Title";
                worksheet.Cells[end.Row + 2, 3].AutoFitColumns();
                worksheet.Cells[end.Row + 2, 3].Style.Border.Top.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[end.Row + 2, 3].Style.Border.Right.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[end.Row + 2, 3].Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                worksheet.Cells[end.Row + 2, 3, end.Row + 3, 3].Merge = true;*//*

                worksheet.Cells[end.Row + 2, 3].Value = "Registered At";
                worksheet.Cells[end.Row + 2, 3].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                worksheet.Cells[end.Row + 2, 3, end.Row + 3, 3].Merge = true;
                worksheet.Cells[end.Row + 2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                for (int i = 0; i < currentRegistration.Count(); i++)
                {
                    worksheet.Cells[(end.Row + 2) + (i + 1), 2].Value = currentRegistration[i].courseId;
                    worksheet.Cells[(end.Row + 2) + (i + 1), 2].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[(end.Row + 2) + (i + 1), 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    *//*worksheet.Cells[end.Row + (i + 1), 3].Value = registrations[i];*//*
                    worksheet.Cells[(end.Row + 2) + (i + 1), 3].Value = currentRegistration[i].semeter;
                    worksheet.Cells[(end.Row + 2) + (i + 1), 3].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[(end.Row + 2) + (i + 1), 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }
        }

        private ExcelWorksheet GetSpecificWorkSheet(ExcelPackage package)
        {
            return package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("SE - Adv E") || w.Name.Contains("CS-Adv-E"));
        }

        private string GetLastEcelSheetInTheDirectory(string path)
        {
            return System.IO.Directory.GetFiles(path).LastOrDefault();
        }

        private void CreateExcelSheet(string path, Student student)
        {
            try
            {
                // Create Empty Excel Sheet
                System.IO.File.Create($@"{path}\{student.StudentId}.xlsx").Close();
                // Copy Existing Excel Sheet Template to the Empty Excel Sheet
                CopyExcelSheetTempleate(path, student.StudentId, student.MajorId);
            }
            catch
            {
                *//*System.IO.File.Create($@"{path}\{student.StudentId}.xlsx");*//*
            }
        }

        private void CopyExcelSheetTempleate(string path, int studentId, int majorId)
        {
            // Get Student Advising Plan File based on MajorId & StudentId
            string existingExcelSheetPath = GetStudentPalnSheetFile(studentId, majorId);
            System.IO.File.Copy(existingExcelSheetPath, $@"{path}\{studentId}.xlsx", true);
        }

        private void CreateDirectory(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                *//*ModelState.AddModelError("CreateDirectory", ex.Message);*//*
            }
        }

        *//*private bool CheckFileLength(IFormFile dataSheet)
        {
            return (dataSheet == null || dataSheet.Length == 0) ? false : true;
        }*//*

        private string GetStudentPalnSheetFile(int studentId, int majorId)
        {
            string FullFileName = $"{_webHostEnvironment.WebRootPath}\\Advising Material\\";
            switch (majorId)
            {
                case 1301:
                    FullFileName += "CS_Plans\\";
                    if (studentId.ToString().Contains("20151") || studentId.ToString().Contains("20152") || studentId.ToString().Contains("20153"))
                    {
                        FullFileName += "CS_2015-2016.xlsx";
                    }
                    else if (studentId.ToString().Contains("20161") || studentId.ToString().Contains("20162") || studentId.ToString().Contains("20163"))
                    {
                        FullFileName += "CS_2016-2017.xlsx";
                    }
                    else if (studentId.ToString().Contains("20171") || studentId.ToString().Contains("20172") || studentId.ToString().Contains("20173"))
                    {
                        FullFileName += "CS_2017-2018.xlsx";
                    }
                    else if (studentId.ToString().Contains("20181") || studentId.ToString().Contains("20182") || studentId.ToString().Contains("20183"))
                    {
                        FullFileName += "CS_2018-2019.xlsx";
                    }
                    else if (studentId.ToString().Contains("20191") || studentId.ToString().Contains("20192") || studentId.ToString().Contains("20193"))
                    {
                        FullFileName += "CS_2019-2020.xlsx";
                    }
                    break;
                case 1302:
                    FullFileName += "SE_Plans\\";
                    if (studentId.ToString().Contains("20151") || studentId.ToString().Contains("20152") || studentId.ToString().Contains("20153"))
                    {
                        FullFileName += "SE-Study Plan 2015-2016.xlsx";
                    }
                    else if (studentId.ToString().Contains("20161") || studentId.ToString().Contains("20162") || studentId.ToString().Contains("20163"))
                    {
                        FullFileName += "SE-Study Plan 2016-2017.xlsx";
                    }
                    else if (studentId.ToString().Contains("20171") || studentId.ToString().Contains("20172") || studentId.ToString().Contains("20173"))
                    {
                        FullFileName += "SE-Study Plan 2017-2018.xlsx";
                    }
                    else if (studentId.ToString().Contains("20181") || studentId.ToString().Contains("20182") || studentId.ToString().Contains("20183"))
                    {
                        FullFileName += "SE-Study Plan 2018-2019.xlsx";
                    }
                    else if (studentId.ToString().Contains("20191") || studentId.ToString().Contains("20192") || studentId.ToString().Contains("20193"))
                    {
                        FullFileName += "SE-Study Plan 2019-2020.xlsx";
                    }
                    else if (studentId.ToString().Contains("20201") || studentId.ToString().Contains("20202") || studentId.ToString().Contains("20203"))
                    {
                        FullFileName += "SE-Study Plan 2020-2021.xlsx";
                    }
                    break;
            }
            return FullFileName;
        }
    }*/
}
