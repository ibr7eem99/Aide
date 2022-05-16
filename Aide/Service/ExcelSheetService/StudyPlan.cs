using Aide.Data;
using oneDrive = Aide.Service.OneDriveService;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Globalization;

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

        public async Task<bool> GenarateExcelSheet(IEnumerable<Supuervised> supuerviseds, string professorName, GraphServiceClient graphServiceClient)
        {
            string advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
            DriveItem professorFolder = await _oneDriveService.GetProfessorFolder(graphServiceClient, professorName) as DriveItem;

            if (!System.IO.Directory.Exists(advisingMaterialPath))
            {
                CreateDirectory(advisingMaterialPath);
            }

            /*var results = from s in supuerviseds
                          group s by s.StudentID into g
                          select new
                          {
                              StudentID = g.Key
                          };*/

            bool isClosed = true;

            Supuervised supuervised = supuerviseds.FirstOrDefault();

            for (int i = 0; i < supuerviseds.Count(); i++)
            {
                if (supuerviseds.ElementAt(i).StudentID == supuervised.StudentID)
                {
                    if (isClosed)
                    {
                        isClosed = false;
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                        advisingMaterialPath += $@"\{textInfo.ToTitleCase(supuervised.StudentNameEn)} {supuervised.StudentID}.xlsx";
                        /*advisingMaterialPath += $@"\{textInfo.ToTitleCase(supuervised.StudentNameEn)} {supuervised.StudentID}.xlsx";*/
                        CreateExcelSheet(advisingMaterialPath, supuervised);
                        var studentSupuervised = supuerviseds.Where(s => s.StudentID == supuervised.StudentID);
                        await OpenNewExcelPackag(advisingMaterialPath, studentSupuervised);
                        DriveItem studentFolder = await _oneDriveService.GetStudentFolder(
                                                        graphServiceClient,
                                                        professorFolder.Id, $"{supuervised.StudentNameEn} {supuervised.StudentID}"
                                                        ) as DriveItem;
                        await _oneDriveService.UplodExcelSheet(graphServiceClient, studentFolder.Id, advisingMaterialPath);
                        FileInfo exclesheetfile = new FileInfo(advisingMaterialPath);
                        if (exclesheetfile.Exists)
                        {
                            exclesheetfile.Delete();
                        }
                    }
                }
                else
                {
                    isClosed = true;
                    supuervised = supuerviseds.ElementAt(i);
                    advisingMaterialPath = $@"{_webHostEnvironment.WebRootPath}\AdvisingMaterial\StudentAdvisingPlanFolder";
                }
            }

            // TODO
            return true;
            /*foreach (var supuervised in supuerviseds)
            {
                if (supuervised.StudentID == studentId)
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
                else
                {
                    studentId = supuervised.StudentID;
                }
                *//*return true;*//*
            }*/
        }


        /*
            TODO:
            remove the path parameter
        */
        private async Task<bool> OpenNewExcelPackag(string path, IEnumerable<Supuervised> studentSupuervised)
        {
            /*string copyExcelSheetPath = GetExcelSheetInTheDirectory(path);*/
            string copyExcelSheetPath = path;
            FileInfo fileInfo = new FileInfo(copyExcelSheetPath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = GetSpecificWorkSheet(package);
                FillStudentAdvisingPlanTemplate(worksheet, studentSupuervised);

                try
                {
                    await package.SaveAsync();
                }
                catch
                {

                }
            }
            return true;
        }

        private void FillStudentAdvisingPlanTemplate(
            ExcelWorksheet worksheet,
            IEnumerable<Supuervised> studentSupuervised
            )
        {
            /*int courseNumberAddress1 = 0;
            int courseNumberAddress2 = 0;
            int registeredAtAddress1 = 0;
            int registeredAtAddress2 = 0;
            string yearAddress1;
            string yearAddress2;
            string semesterAddress1;
            string semesterAddress2;*/
            ExcelCellAddress start = worksheet.Dimension.Start;
            ExcelCellAddress end = worksheet.Dimension.End;
            Regex reg = new Regex(@"([0-9])");

            /*for (int col = start.Column + 1; col <= end.Column; ++col)
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
            }*/

            /*var query = from course in Courses
                        join registration in registrations on course.CourseId equals registration.CourseId
                        where registration.StudentId == student.StudentId
                        select new
                        {
                            courseId = registration.CourseId,
                            semeter = registration.Semester,
                        };*/

            /*var currentRegistration = query.ToList();*/

            var currentStudent = studentSupuervised.ToList<Supuervised>();
            for (int row = 5; row <= end.Row - 3 || row <= end.Row - 2; row++)
            {
                for (int col = 2; col <= end.Column; col++)
                {
                    string colText = worksheet.Cells[3, col].Text;
                    if (colText == "Course Number" || colText == "Subject Number")
                        if (reg.IsMatch(worksheet.Cells[row, col].Text))
                        {
                            worksheet.Cells[row, col].AutoFitColumns(10);
                            /*if (query.Any())                            {*/
                            var currentCource = currentStudent.
                                                FirstOrDefault(c => c.CourseNumber.
                                                ToString().
                                                Equals(worksheet.Cells[row, col].Text));
                            if (currentCource is not null)
                            {
                                worksheet.Cells[row, col + 6].Value = $"{currentCource.Year}{currentCource.Semester}";
                                currentStudent.Remove(currentCource);
                            }
                            /*}*/
                        }
                    /*if (reg.IsMatch(worksheet.Cells[row, courseNumberAddress2].Text))                    {                        worksheet.Cells[row, courseNumberAddress2].AutoFitColumns(10);                        *//*if (query.Any())                        {*//*                        var currentCource = currentStudent.                        FirstOrDefault(c => c.CourseNumber.ToString().Equals(worksheet.Cells[row, courseNumberAddress2].Text));                        if (currentCource is not null)                        {                            worksheet.Cells[row, registeredAtAddress2].Value = $"{currentCource.Year}{currentCource.Semester}";                            currentStudent.Remove(currentCource);                        }                        *//*}*//*                    }*/
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
            return package.Workbook.Worksheets.FirstOrDefault(w => w.Name.Contains("SE - Adv E") || w.Name.Contains("CS-Adv-E"));
        }

        private string GetExcelSheetInTheDirectory(string path)
        {
            return System.IO.Directory.GetFiles(path).LastOrDefault();
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
            catch (Exception ex)
            {
                /*System.IO.File.Create($@"{path}\{supuervised.StudentNameEn} {supuervised.StudentID}.xlsx");*/
            }
            finally
            {

            }
        }

        private void CopyExcelSheetTempleate(string path, Supuervised supuervised)
        {
            // Get Student Advising Plan File based on MajorId & StudentId
            string existingExcelSheetPath = GetStudentPalnSheetFile(supuervised.SemesterStudyPlan, supuervised.SpecNameEn);
            System.IO.File.Copy(existingExcelSheetPath, path, true);
        }

        private void CreateDirectory(string path)
        {
            try
            {
                System.IO.Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                /*ModelState.AddModelError("CreateDirectory", ex.Message);*/
            }
        }

        private string GetStudentPalnSheetFile(int semesterStudyPlan, string majorName)
        {
            string FullFileName = $"{_webHostEnvironment.WebRootPath}\\AdvisingMaterial\\";

            switch (majorName.ToUpper())
            {
                case "COMPUTER SCIENCE":
                    FullFileName += "CS_Plans";
                    FullFileName = GetStudentPalnSheetFileName(FullFileName, semesterStudyPlan);
                    break;
                case "SOFTWARE ENGINEERING":
                    FullFileName += "SE_Plans";
                    FullFileName = GetStudentPalnSheetFileName(FullFileName, semesterStudyPlan);
                    break;
            }
            return FullFileName;
        }

        private string GetStudentPalnSheetFileName(string FullFileName, int semesterStudyPlan)
        {
            DirectoryInfo place = new DirectoryInfo(FullFileName);
            return place.GetFiles().FirstOrDefault(f => f.Name.Split("-")[0].Contains(semesterStudyPlan.ToString().Remove(semesterStudyPlan.ToString().Count() - 1))).FullName;
        }
    }


}
