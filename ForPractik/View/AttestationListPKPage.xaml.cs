using DocumentFormat.OpenXml.Spreadsheet;
using ForPractik.ViewModel;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;

namespace ForPractik.View
{
    /// <summary>
    /// Логика взаимодействия для AttestationListPKPage.xaml
    /// </summary>
    public partial class AttestationListPKPage : System.Windows.Controls.Page
    {
        private List<OKEntry> okEntries;

        public AttestationListPKPage()
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

            string currentGroup = null;

            for (int i = 1; i <= rowCount; i++)
            {
                // Проверяем, если ячейка объединена и она в первом столбце, то это заголовок группы
                var cell = range.Cells[i, 1] as Excel.Range;
                if (cell.MergeCells)
                {
                    currentGroup = cell.MergeArea.Cells[1, 1].Value2;
                    continue;
                }

                // Если это не заголовок группы, то читаем подчиненные значения
                var entry = new OKEntry
                {
                    Group = currentGroup,
                    OK = (string)(range.Cells[i, 1] as Excel.Range).Value2,
                    Description = (string)(range.Cells[i, 2] as Excel.Range).Value2
                };
                data.Add(entry);
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
                string templatePath = "C:\\Users\\Farianskiy\\Desktop\\ForPractik\\ForPractik\\Resources\\AttestationListPK.docx";
                docManager.OpenDocument(templatePath);

                // Получаем выбранную группу из ComboBox
                string groupName = SecondComboBox.Text;

                // Вставляем данные из Excel в таблицу Word
                docManager.InsertOKEntriesIntoTablePK(okEntries, groupName);

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
