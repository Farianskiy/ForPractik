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
    /// Логика взаимодействия для AddTeachersPage.xaml
    /// </summary>
    public partial class AddTeachersPage : Page
    {
        public AddTeachersPage()
        {
            InitializeComponent();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void btn_AddGroupSave(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedCurator = txtGroupName.Text;

                ValidateInput(selectedCurator);

                DatabaseContext.GetContext().AddTeacher(selectedCurator);

                MessageBox.Show("Куратор успешно добавлена!");

                ClearInputFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }

            void ValidateInput(string selectedCurator)
            {
                if (string.IsNullOrWhiteSpace(selectedCurator))
                    MessageBox.Show("Укажите ФИО куратора!");
            }

            void ClearInputFields()
            {
                txtGroupName.Clear();
            }
        }

        private void btn_DeleteTeacher(object sender, RoutedEventArgs e)
        {
            string selectedCurator = txtGroupName.Text;

            DatabaseContext.GetContext().DeleteTeacherByFullName(selectedCurator);

            txtGroupName.Clear();
        }
    }
}
