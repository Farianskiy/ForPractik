using ForPractik.Model;
using ForPractik.ViewModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ForPractik.View
{
    /// <summary>
    /// Логика взаимодействия для AccountingStudentsPractice.xaml
    /// </summary>
    public partial class AccountingStudentsPractice : Page
    {
        private bool isExpanded = false;
        private const double expansionWidth = 575;

        public AccountingStudentsPractice()
        {
            InitializeComponent();
        }


        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (isExpanded)
                {
                    window.Left += expansionWidth / 2;
                    window.Width -= expansionWidth;
                }
                else
                {
                    window.Left -= expansionWidth / 2;
                    window.Width += expansionWidth;
                }
                isExpanded = !isExpanded;
            }
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

        private void btn_EditStudent(object sender, RoutedEventArgs e)
        {
            StudentPractik selectedStudentPractik = DGridStudentPractik.SelectedItem as StudentPractik;
            if (selectedStudentPractik != null)
            {
                // Переходим на страницу редактирования, передавая выбранного студента
                Manager.MainFrame.Navigate(new EditpracticePage(selectedStudentPractik));
            }
            else
            {
                MessageBox.Show("Выберите студента из списка!");
            }
        }


        private HashSet<int> displayedStudentIds = new HashSet<int>();

        private void SecondComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SecondComboBox.SelectedItem != null)
            {
                string selectedGroup = SecondComboBox.SelectedItem.ToString();
                List<StudentPractik> studentPractikList = DatabaseContext.GetContext().GetPracticeDetailsByGroupName(selectedGroup);

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

            if (SecondComboBox.SelectedItem != null)
            {
                string selectedGroup = SecondComboBox.SelectedItem.ToString();
                List<GroupEnterprise> groupEnterprises = DatabaseContext.GetContext().GetEnterprisesByGroupId(selectedGroup);

                DGridPlaceOfPracticeAndGroup.ItemsSource = groupEnterprises;
            }
        }

        private void btn_Grade(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.Tag is int checklistId)
                {
                    var evaluationSheetPage = new EvaluationSheetPage(checklistId);
                    Manager.MainFrame.Navigate(evaluationSheetPage);
                }
                else
                {
                    // Если Tag не содержит int, вы можете добавить код обработки ошибки здесь
                    MessageBox.Show("Tag не содержит int.");
                }
            }
        }

        private void btn_Enterprise_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранную строку из DataGrid
            var selectedPractice = DGridStudentPractik.SelectedItem as StudentPractik;

            if (selectedPractice != null)
            {
                // Получаем название организации
                string organizationName = selectedPractice.Placeofpractice;

                // Запрашиваем реквизиты из базы данных
                Requisites requisites = DatabaseContext.GetContext().GetRequisitesByEnterpriseName(organizationName);

                // Проверяем, получили ли мы реквизиты
                if (requisites != null)
                {
                    // Создаем экземпляр страницы и передаем данные
                    EnterpriseDetailsPage detailsPage = new EnterpriseDetailsPage(requisites);
                    this.NavigationService.Navigate(detailsPage);
                }
                else
                {
                    MessageBox.Show("Реквизиты для выбранной организации не найдены.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите организацию.");
            }
        }




    }
}
