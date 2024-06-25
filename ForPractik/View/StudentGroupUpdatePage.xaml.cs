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
    /// Логика взаимодействия для StudentGroupUpdatePage.xaml
    /// </summary>
    public partial class StudentGroupUpdatePage : Page
    {
        public StudentGroupUpdatePage()
        {
            InitializeComponent();
        }

        private void btn_EditStudent(object sender, RoutedEventArgs e)
        {
           DatabaseContext.GetContext().UpdateAndDeleteGroupsAndStudents();
           MessageBox.Show("Успешно!");
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}
