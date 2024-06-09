using ForPractik.Model;
using ForPractik.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для EditpracticePage.xaml
    /// </summary>
    public partial class EditpracticePage : Page
    {
        private StudentPractik _currentStudentPractik = new StudentPractik();

        public EditpracticePage(StudentPractik selectedStudentPractik)
        {
            InitializeComponent();

            if (selectedStudentPractik != null)
                _currentStudentPractik = selectedStudentPractik;

            DataContext = _currentStudentPractik;
        }
        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddStudentSave(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentStudentPractik.Student))
                errors.AppendLine("Укажите ФИО студента");
            if (string.IsNullOrWhiteSpace(_currentStudentPractik.Placeofpractice))
                errors.AppendLine("Укажите место прохождения практики студента");
            if (string.IsNullOrWhiteSpace(_currentStudentPractik.Headofpractice))
                errors.AppendLine("Укажите руководителя практики студента");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {

                int enterpriseId = DatabaseContext.GetContext().GetEnterpriseIdByPlace(_currentStudentPractik.Placeofpractice);

                if (enterpriseId == -1)
                {
                    MessageBox.Show("Место практики не найдено!");
                    txtPlaceofpractice.Clear();
                    txtHeadofpractice.Clear();
                    return; // Предотвращаем продолжение выполнения кода после ошибки
                }

                DatabaseContext.GetContext().UpdatePracticeForStudent(_currentStudentPractik.StudentId, _currentStudentPractik.Placeofpractice, enterpriseId, _currentStudentPractik.Headofpractice);

                if (chkWorksThere.IsChecked == true)
                {
                    DatabaseContext.GetContext().AddStudentWorking(_currentStudentPractik.StudentId, enterpriseId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
