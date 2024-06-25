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
    /// Логика взаимодействия для AgreementPage.xaml
    /// </summary>
    public partial class AgreementPage : Page
    {
        public AgreementPage()
        {
            InitializeComponent();
        }

        private void btn_ProtocolPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProtocolPage());
        }

        private void btn_ListDistributionPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ListDistributionPage());
        }

        private void LetterOfficialPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new LetterOfficialPage());
        }

        private void btn_ListContactPhonePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ListContactPhonePage());
        }

        private void AttestationListOKPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AttestationListOKPage());
        }

        private void AttestationListPKPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AttestationListPKPage());
        }

        private void ProgramIndustrialPracticePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProgramIndustrialPracticePage());
        }

        private void ProgramEducationalPracticePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProgramEducationalPracticePage());
        }
    }
}
