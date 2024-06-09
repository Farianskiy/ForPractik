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
    /// Логика взаимодействия для AddModulesPage.xaml
    /// </summary>
    public partial class AddModulesPage : Page
    {
        public AddModulesPage()
        {
            InitializeComponent();

            ComboSpecialties.ItemsSource = DatabaseContext.GetContext().GetSpecialties().AsEnumerable().ToList();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddModulSave(object sender, RoutedEventArgs e)
        {
            try
            {
                string moduleName = txtModuleName.Text;
                string selectedSpecialty = ComboSpecialties.SelectedItem as string;

                ValidateInput(moduleName, selectedSpecialty);

                int specializationId = DatabaseContext.GetContext().GetSpecializationIdByName(selectedSpecialty);
                DatabaseContext.GetContext().AddModule(moduleName, specializationId);

                MessageBox.Show("Модуль успешно добавлен.");

                ClearInputFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

            void ValidateInput(string moduleName, string selectedSpecialty)
            {
                if (string.IsNullOrWhiteSpace(moduleName))
                    MessageBox.Show("Введите название модуля!");

                if (string.IsNullOrWhiteSpace(selectedSpecialty))
                    MessageBox.Show("Выберите специальность для модуля!");
            }

            // Очистить текстовые поля и сбросить выбор в ComboBox после успешного добавления
            void ClearInputFields()
            {
                txtModuleName.Clear();
                ComboSpecialties.SelectedIndex = -1; // Сбрасываем выбор в ComboBox
            }

        }
    }
}
