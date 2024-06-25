using ForPractik.ViewModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace ForPractik.View
{
    /// <summary>
    /// Логика взаимодействия для ProgramEducationalPracticePage.xaml
    /// </summary>
    public partial class ProgramEducationalPracticePage : System.Windows.Controls.Page
    {
        private List<OKEntry> okEntries;

        public ProgramEducationalPracticePage()
        {
            InitializeComponent();
        }
        private void btn_Bacl(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void FirstComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранный элемент из первого ComboBox
            ComboBoxItem selectedItem = (ComboBoxItem)FirstComboBox.SelectedItem;

            if (selectedItem != null)
            {
                // Получаем текст выбранного элемента
                string selectedItemText = selectedItem.Content.ToString();
                // Извлекаем числовое значение из строки
                string courseNumberString = Regex.Match(selectedItemText, @"\d+").Value;
                if (!string.IsNullOrEmpty(courseNumberString))
                {
                    if (int.TryParse(courseNumberString, out int courseNumber))
                    {
                        // courseNumber успешно преобразован в int
                        // Теперь вы можете использовать courseNumber как специализацию
                        List<string> groups = DatabaseContext.GetContext().GetGroups(courseNumber);
                        // Очищаем второй ComboBox перед добавлением новых элементов
                        SecondComboBox.Items.Clear();
                        // Добавляем полученные группы во второй ComboBox
                        foreach (string group in groups)
                        {
                            SecondComboBox.Items.Add(group);
                        }
                    }
                    else
                    {
                        // Невозможно преобразовать courseNumberString в int
                        MessageBox.Show("Ошибка: Невозможно преобразовать номер курса в число.");
                    }
                }
                else
                {
                    // Строка не содержит числовое значение курса
                    MessageBox.Show("Ошибка: Неверный формат выбранного элемента.");
                }
            }
        }

        private void LoadExcelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
            if (openFileDialog.ShowDialog() == true)
            {
                okEntries = ReadDataFromExcel(openFileDialog.FileName);
                MessageBox.Show("Данные из Excel загружены успешно.");
            }
        }

        private List<OKEntry> ReadDataFromExcel(string excelFilePath)
        {
            var data = new List<OKEntry>();
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Open(excelFilePath);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            Excel.Range range = worksheet.UsedRange;

            int rowCount = range.Rows.Count;
            int colCount = range.Columns.Count;

            for (int i = 2; i <= rowCount; i++)
            {
                var okCell = range.Cells[i, 1] as Excel.Range;
                var descriptionCell = range.Cells[i, 2] as Excel.Range;
                var hoursCell = range.Cells[i, 3] as Excel.Range;

                string ok = okCell.Value2 != null ? okCell.Value2.ToString().Trim() : string.Empty;
                string description = descriptionCell.Value2 != null ? descriptionCell.Value2.ToString().Trim() : string.Empty;
                double hours = 0;

                if (hoursCell.Value2 != null)
                {
                    double.TryParse(hoursCell.Value2.ToString(), out hours);
                }

                // Проверка пустых строк
                if (string.IsNullOrEmpty(ok) && string.IsNullOrEmpty(description) && hours == 0)
                {
                    continue;
                }

                if (descriptionCell.MergeCells && hoursCell.MergeCells && descriptionCell.MergeArea.Address == hoursCell.MergeArea.Address)
                {
                    var entry = new OKEntry
                    {
                        Group = descriptionCell.Value2 != null ? descriptionCell.Value2.ToString() : string.Empty,
                        OK = ok,
                        Description = string.Empty,
                        Hours = 0
                    };
                    data.Add(entry);
                }
                else
                {
                    var entry = new OKEntry
                    {
                        Group = string.Empty,
                        OK = ok,
                        Description = description,
                        Hours = hours
                    };
                    data.Add(entry);
                }
            }

            workbook.Close(false);
            excelApp.Quit();
            Marshal.ReleaseComObject(excelApp);

            return data;
        }





        public void FillWordDocument(List<OKEntry> okEntries)
        {
            using (WordDocumentManager docManager = new WordDocumentManager())
            {
                // Открываем шаблон документа
                string templatePath = "C:\\Users\\Farianskiy\\Desktop\\ForPractik\\ForPractik\\Resources\\ProgramEducationalPractice.docx";
                docManager.OpenDocument(templatePath);

                // Получаем выбранную группу из ComboBox
                string groupName = SecondComboBox.Text;

                string predsedatel = ChairmanTextBox.Text;

                string zamestitel = DeputyDirectorTextBox.Text;

                string teachers = TeachersTextBox.Text;

                // Вставляем данные из Excel в таблицу Word
                docManager.InsertOKEntriesIntoTableProgram(okEntries, groupName, predsedatel, zamestitel, teachers);

                // Сохраняем заполненный документ по указанному пути
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Документ Word (*.docx)|*.docx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    docManager.SaveAndCloseDocument(saveFileDialog.FileName);
                    MessageBox.Show("Документ успешно заполнен и сохранен.", "Успех");
                }
            }
        }

        private void FillDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            FillWordDocument(okEntries);
        }
    }
}
