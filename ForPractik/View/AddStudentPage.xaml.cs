using ForPractik.Model;
using ForPractik.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace ForPractik.View
{
    /// <summary>
    /// Логика взаимодействия для AddStudentPage.xaml
    /// </summary>
    public partial class AddStudentPage : Page
    {
        private Student _currentStudent = new Student();
        private List<Student> excelData;

        public AddStudentPage(Student selectedStudent)
        {
            InitializeComponent();

            if (selectedStudent != null)
                _currentStudent = selectedStudent;

            DataContext = _currentStudent;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddStudentSave(object sender, RoutedEventArgs e)
        {
            bool worksThere = false;    

            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentStudent.SurName))
                errors.AppendLine("Укажите фамилию студента");
            if (string.IsNullOrWhiteSpace(_currentStudent.Name))
                errors.AppendLine("Укажите имя студента");
            if (string.IsNullOrWhiteSpace(_currentStudent.Patronymic))
                errors.AppendLine("Укажите отчество студента");
            if (string.IsNullOrWhiteSpace(_currentStudent.Group))
                errors.AppendLine("Укажите группу студента");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                if (_currentStudent.Id == 0)
                    DatabaseContext.GetContext().AddStudentWithDependencies(_currentStudent.SurName, _currentStudent.Name, _currentStudent.Patronymic, _currentStudent.Group, worksThere);

                MessageBox.Show("Студент успешно добавлен!");

                Manager.MainFrame.GoBack();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void btn_AddStudentListSave(object sender, RoutedEventArgs e)
        {
            excelData = LoadExcelData();

            if (excelData == null || excelData.Count == 0)
            {
                MessageBox.Show("Нет загруженных данных из Excel файла.");
                return;
            }

            StringBuilder errors = new StringBuilder();

            foreach (var student in excelData)
            {
                if (string.IsNullOrWhiteSpace(student.SurName) ||
                    string.IsNullOrWhiteSpace(student.Name) ||
                    string.IsNullOrWhiteSpace(student.Patronymic) ||
                    string.IsNullOrWhiteSpace(student.Group))
                {
                    errors.AppendLine($"Ошибка в данных студента: {student.Name} {student.SurName}");
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                DatabaseContext.GetContext().AddStudentListWithDependencies(excelData);
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }



        private List<Student> LoadExcelData()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                return ReadDataFromExcel(openFileDialog.FileName);
            }
            return null;
        }



        private List<Student> ReadDataFromExcel(string filePath)
        {
            var students = new List<Student>();
            string groupName = string.Empty;

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Open(filePath);
            Excel._Worksheet worksheet = workbook.Sheets[1];
            Excel.Range excelRange = worksheet.UsedRange;

            try
            {
                // Предполагаем, что первая строка содержит название группы
                groupName = Convert.ToString(excelRange.Cells[1, 1].Value2);

                int rowCount = excelRange.Rows.Count;
                for (int row = 2; row <= rowCount; row++) // Начинаем со второй строки
                {
                    string fullName = Convert.ToString(excelRange.Cells[row, 1].Value2);
                    var nameParts = fullName.Split(' ');

                    if (nameParts.Length < 3)
                    {
                        // Если не удалось разделить на три части (фамилия, имя, отчество)
                        MessageBox.Show($"Неверный формат ФИО в строке {row}: {fullName}");
                        continue;
                    }

                    var student = new Student
                    {
                        SurName = nameParts[0],
                        Name = nameParts[1],
                        Patronymic = nameParts[2],
                        Group = groupName // Используем имя группы из первой строки
                    };

                    students.Add(student);
                }
            }
            finally
            {
                // Освобождаем ресурсы
                Marshal.ReleaseComObject(excelRange);
                Marshal.ReleaseComObject(worksheet);
                workbook.Close(false);
                Marshal.ReleaseComObject(workbook);
                excelApp.Quit();
                Marshal.ReleaseComObject(excelApp);
            }

            return students;
        }


    }
}
