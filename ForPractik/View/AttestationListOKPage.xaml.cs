using ForPractik.Model;
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
    /// Логика взаимодействия для AttestationListOKPage.xaml
    /// </summary>

    public class OKEntry
    {
        public string Group { get; set; }
        public string OK { get; set; }
        public string Description { get; set; }
        public double Hours { get; set; }
    }



    public partial class AttestationListOKPage : Page
    {
        private List<Dictionary<string, string>> excelData;

        public AttestationListOKPage()
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
                excelData = ReadDataFromExcel(openFileDialog.FileName);
                MessageBox.Show("Данные из Excel загружены успешно.");
            }
        }

        private List<Dictionary<string, string>> ReadDataFromExcel(string excelFilePath)
        {
            var data = new List<Dictionary<string, string>>();
            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook workbook = excelApp.Workbooks.Open(excelFilePath);
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            Excel.Range range = worksheet.UsedRange;

            int rowCount = range.Rows.Count;
            int colCount = range.Columns.Count;

            for (int i = 2; i <= rowCount; i++)
            {
                var entry = new Dictionary<string, string>
        {
            { "ОК", (string)(range.Cells[i, 1] as Excel.Range).Value2 },
            { "Описание", (string)(range.Cells[i, 2] as Excel.Range).Value2 }
        };
                data.Add(entry);
            }

            workbook.Close(false);
            excelApp.Quit();
            Marshal.ReleaseComObject(excelApp);

            return data;
        }



        private void FillDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            FillWordDocument();
        }

        public void FillWordDocument()
        {
            using (WordDocumentManager docManager = new WordDocumentManager())
            {
                // Открываем шаблон документа
                string templatePath = "C:\\Users\\Farianskiy\\Desktop\\ForPractik\\ForPractik\\Resources\\AttestationListOK.docx";
                docManager.OpenDocument(templatePath);

                // Получаем выбранную группу из ComboBox
                string groupName = SecondComboBox.Text;

                // Проверяем, что данные из Excel загружены
                if (excelData != null && excelData.Any())
                {
                    // Преобразуем данные из excelData в формат, который ожидает метод InsertStudentsContactIntoTable
                    List<OKEntry> okEntries = new List<OKEntry>();

                    foreach (var entry in excelData)
                    {
                        string ok = entry.ContainsKey("ОК") ? entry["ОК"] : string.Empty;
                        string description = entry.ContainsKey("Описание") ? entry["Описание"] : string.Empty;

                        okEntries.Add(new OKEntry { OK = ok, Description = description });
                    }

                    // Вставляем данные из Excel в таблицу
                    docManager.InsertOKEntriesIntoTable(okEntries, groupName);
                }
                else
                {
                    MessageBox.Show("Данные из Excel не загружены.", "Ошибка");
                    return;
                }

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


    }
}
