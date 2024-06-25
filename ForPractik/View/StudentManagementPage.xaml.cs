using ForPractik.Model;
using ForPractik.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    /// Логика взаимодействия для StudentManagementPage.xaml
    /// </summary>
    public partial class StudentManagementPage : Page
    {
        public StudentManagementPage()
        {
            InitializeComponent();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddStudentPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddStudentPage(null));
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

        private void SecondComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SecondComboBox.SelectedItem != null)
            {
                string selectedGroup = SecondComboBox.SelectedItem.ToString();
                List<Student> studentList = DatabaseContext.GetContext().GetStudentsByGroup(selectedGroup);


                DGridStudent.ItemsSource = studentList;
            }
        }

        private void btn_EditStudent(object sender, RoutedEventArgs e)
        {
            Student selectedStudent = DGridStudent.SelectedItem as Student;
            if (selectedStudent != null)
            {
                // Получаем имя группы по идентификатору студента
                string groupName = DatabaseContext.GetContext().GetGroupNameByStudentId(selectedStudent.Id);

                // Добавляем имя группы к объекту Student
                selectedStudent.Group = groupName;

                // Переходим на страницу редактирования, передавая выбранного студента
                Manager.MainFrame.Navigate(new AddStudentPage(selectedStudent));
            }
            else
            {
                MessageBox.Show("Выберите студента из списка!");
            }
        }



        private void btn_DeleteStudent(object sender, RoutedEventArgs e)
        {
            var studentsForRemoving = DGridStudent.SelectedItems.Cast<Student>().Select(student => student.Id).ToList();

            if (MessageBox.Show($"Вы точно хотите удалить следующие {studentsForRemoving.Count()} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseContext.GetContext().DeleteStudents(studentsForRemoving);
                    MessageBox.Show("Данные удалены!");
                    DGridStudent.ItemsSource = DatabaseContext.GetContext().GetStudents().AsEnumerable().ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }




    }

}
