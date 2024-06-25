using ForPractik.View;
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
using System.Windows.Threading;

namespace ForPractik
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Manager.MainFrame = MainFrame;

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void btn_ManagementPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ManagementPage());
        }

        private void btn_AgreementPage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AgreementPage());
        }

        private void btn_PracticePage(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new PracticePage());
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
