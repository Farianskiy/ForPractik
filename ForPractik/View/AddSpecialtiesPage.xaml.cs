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
    /// Логика взаимодействия для AddSpecialtiesPage.xaml
    /// </summary>
    public partial class AddSpecialtiesPage : Page
    {
        public AddSpecialtiesPage()
        {
            InitializeComponent();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddSpecialtySave(object sender, RoutedEventArgs e)
        {
            try
            {
                string specialtyName = txtSpecialtyName.Text;
                string specialtyCode = txtSpecialtyCode.Text;
                string specialtyQualification = txtSpecialtyQualification.Text;

                // Проверка, чтобы не добавлять пустое название специальности
                if (string.IsNullOrWhiteSpace(specialtyName))
                {
                    MessageBox.Show("Введите название специальности.");
                    return;
                }

                // Вызов метода добавления специальности в базу данных
                DatabaseContext.GetContext().AddSpecialty(specialtyName, specialtyCode, specialtyQualification);

                MessageBox.Show("Специальность успешно добавлена!");

                // Очистить текстовое поле после успешного добавления
                txtSpecialtyName.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
