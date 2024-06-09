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
    /// Логика взаимодействия для AddStudentPage.xaml
    /// </summary>
    public partial class AddStudentPage : Page
    {
        private Student _currentStudent = new Student();

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
    }
}
