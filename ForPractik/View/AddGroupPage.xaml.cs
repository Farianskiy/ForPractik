using ForPractik.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для AddGroupPage.xaml
    /// </summary>
    public partial class AddGroupPage : Page
    {
        public AddGroupPage()
        {
            InitializeComponent();

            ComboSpecialties.ItemsSource = DatabaseContext.GetContext().GetSpecialties().AsEnumerable().ToList();
            ComboCurators.ItemsSource = DatabaseContext.GetContext().GetCurators().AsEnumerable().ToList();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddGroupSave(object sender, RoutedEventArgs e)
        {
            try
            {
                string groupName = txtGroupName.Text;
                string selectedSpecialty = ComboSpecialties.SelectedItem as string;
                string selectedCurator = ComboCurators.SelectedItem as string;

                ValidateInput(groupName, selectedSpecialty, selectedCurator);

                int specializationId = DatabaseContext.GetContext().GetSpecializationIdByName(selectedSpecialty);
                int curatorId = DatabaseContext.GetContext().GetCuratorIdByName(selectedCurator);
                DatabaseContext.GetContext().AddGroup(groupName, specializationId, curatorId);

                MessageBox.Show("Группа успешно добавлена!");

                ClearInputFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            void ValidateInput(string groupName, string selectedSpecialty, string selectedCurator)
            {
                if (string.IsNullOrWhiteSpace(groupName))
                    MessageBox.Show("Введите название группы!");

                if (string.IsNullOrWhiteSpace(selectedSpecialty))
                    MessageBox.Show("Выберите специальность для группы!");

                if (string.IsNullOrWhiteSpace(selectedCurator))
                    MessageBox.Show("Выберите куратора для группы!");
            }

            void ClearInputFields()
            {
                txtGroupName.Clear();
                ComboSpecialties.SelectedIndex = -1;
                ComboCurators.SelectedIndex = -1;
            }
        }
    }
}
