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
    /// Логика взаимодействия для PracticePage.xaml
    /// </summary>
    public partial class PracticePage : Page
    {
        public PracticePage()
        {
            InitializeComponent();
        }

        private void OpeningPracticePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new OpeningPracticePage());
        }

        private void AccountingStudentsPractice(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AccountingStudentsPractice());
        }

        private void ClosingPracticePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ClosingPracticePage());
        }
    }
}
