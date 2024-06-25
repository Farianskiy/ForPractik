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
    /// Логика взаимодействия для ManagementPage.xaml
    /// </summary>
    public partial class ManagementPage : Page
    {
        public ManagementPage()
        {
            InitializeComponent();
        }

        private void btn_StudentManagementPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new StudentManagementPage());
        }

        private void btn_AddSpecialties(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddSpecialtiesPage());
        }

        private void btn_AddModulesPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddModulesPage());
        }

        private void btn_AddCompanyPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddCompanyPage());
        }

        private void btn_WorkingStudentsPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new WorkingStudentsPage());
        }

        private void btn_AddGroupStudentsPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddGroupStudentsPage());
        }

        private void btn_AddGroupPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddGroupPage());
        }

        private void btn_AddTeachersPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddTeachersPage());
        }

        private void btn_UpdateTeachersPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new UpdateTeachersPage());
        }

        private void btn_StudentGroupUpdatePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new StudentGroupUpdatePage());
        }
    }
}
