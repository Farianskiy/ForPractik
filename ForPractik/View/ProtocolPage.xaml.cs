using ForPractik.Model;
using ForPractik.ViewModel;
using GSF;
using Microsoft.Win32;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ForPractik.View
{
    /// <summary>
    /// Логика взаимодействия для ProtocolPage.xaml
    /// </summary>
    public partial class ProtocolPage : Page
    {
        public ProtocolPage()
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


        private void FillDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем экземпляр менеджера документов Word
            using (WordDocumentManager docManager = new WordDocumentManager())
            {
                // Открываем шаблон документа
                string templatePath = "C:\\Users\\Farianskiy\\Desktop\\ForPractik\\ForPractik\\Resources\\ProtocolTB.docx";
                docManager.OpenDocument(templatePath);

                // Получаем выбранную группу из ComboBox
                string groupName = SecondComboBox.Text;

                // Получаем список студентов для выбранной группы из базы данных
                List<Student> students = DatabaseContext.GetContext().GetStudentsByGroup(groupName);

                docManager.InsertStudentsIntoTable(students);

                // Заполняем шаблон документа данными
                string special = DatabaseContext.GetContext().GetSpecializationByGroupName(groupName);
                string chairman = ChairmanTextBox.Text;
                string departmentHead = DepartmentHeadTextBox.Text;
                string groupCurator = DatabaseContext.GetContext().GetTeacherByGroupName(groupName);
                string date = DateTextBox.Text;
                docManager.FillDocumentWithData(groupName, chairman, departmentHead, groupCurator, date, students, special);

                // Сохраняем заполненный документ по указанному пути
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Документ Word (*.docx)|*.docx";
                if (saveFileDialog.ShowDialog() == true)
                {
                    docManager.SaveAndCloseDocument(saveFileDialog.FileName);
                    MessageBox.Show("Документ успешно заполнен и сохранен.", "Успех");

                    // Очищаем поля выборов
                    FirstComboBox.SelectedIndex = -1;
                    SecondComboBox.SelectedIndex = -1; // Сброс выбранной группы
                    ChairmanTextBox.Text = ""; // Очистка текста
                    DepartmentHeadTextBox.Text = ""; // Очистка текста
                    DateTextBox.Text = ""; // Очистка текста
                }
            }
        }








    }
}
