using ForPractik.Model;
using ForPractik.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.Json;
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


        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                //DatabaseContext.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                DGridStudent.ItemsSource = DatabaseContext.GetContext().GetStudents().AsEnumerable().ToList();

            }
        }


    }

}
