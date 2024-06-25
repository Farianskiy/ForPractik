
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
    /// Логика взаимодействия для EnterpriseDetailsPage.xaml
    /// </summary>
    public partial class EnterpriseDetailsPage : Page
    {
        public EnterpriseDetailsPage(Requisites requisites)
        {
            InitializeComponent();
            this.DataContext = requisites;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}
