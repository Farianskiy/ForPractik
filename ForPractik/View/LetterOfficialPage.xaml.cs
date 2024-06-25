using DeepMorphy;
using ForPractik.Model;
using ForPractik.ViewModel;
using Humanizer;
using Microsoft.Win32;
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
    /// Логика взаимодействия для LetterOfficialPage.xaml
    /// </summary>
    public partial class LetterOfficialPage : Page
    {

        public LetterOfficialPage()
        {
            InitializeComponent();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
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

        private HashSet<int> displayedStudentIds = new HashSet<int>();

        private void SecondComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SecondComboBox.SelectedItem != null)
            {
                string selectedGroup = SecondComboBox.SelectedItem.ToString();
                List<StudentPractik> studentPractikList = DatabaseContext.GetContext().GetPracticeDetailsByGroupNameOne(selectedGroup);

                List<StudentPractik> uniqueStudents = new List<StudentPractik>();

                foreach (var student in studentPractikList)
                {
                    if (!displayedStudentIds.Contains(student.StudentId)) // Changed from `StudentPractik.StudentId`
                    {
                        uniqueStudents.Add(student);
                        displayedStudentIds.Add(student.StudentId); // Changed from `student.Id`
                    }
                }

                DGridStudentPractik.ItemsSource = uniqueStudents;
            }
        }


        private void FillDocumentButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                string groupName = SecondComboBox.Text;
                // Получаем данные студента из строки DataGrid
                var selectedStudent = button.DataContext as StudentPractik;
                if (selectedStudent != null)
                {
                    // Переходим на страницу редактирования, передавая нужные данные
                    Manager.MainFrame.Navigate(new FormLetterPage(selectedStudent.Student, selectedStudent.Placeofpractice, selectedStudent.Headofpractice, groupName, selectedStudent.EnterpriseId));
                }
                else
                {
                    MessageBox.Show("Выберите студента из списка!");
                }
            }
        }




        


        

    }
}
