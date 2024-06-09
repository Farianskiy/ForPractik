using ForPractik.Model;
using ForPractik.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для AddCompanyPage.xaml
    /// </summary>
    public partial class AddCompanyPage : Page
    {
        private Company _currentCompany = new Company();
        public AddCompanyPage()
        {
            InitializeComponent();

            DataContext = _currentCompany;
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private void FirstComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Получаем выбранный элемент из первого ComboBox
            ComboBoxItem selectedItem = (ComboBoxItem)FirstComboBox.SelectedItem;

            if (selectedItem != null)
            {
                // Получаем текст выбранного элемента
                string selectedItemText = selectedItem.Content.ToString();
                // Извлекаем числовое значение из строки
                string courseNumberString = Regex.Match(selectedItemText, @"\d+").Value;
                if (!string.IsNullOrEmpty(courseNumberString))
                {
                    if (int.TryParse(courseNumberString, out int courseNumber))
                    {
                        // courseNumber успешно преобразован в int
                        // Теперь вы можете использовать courseNumber как специализацию
                        List<string> groups = DatabaseContext.GetContext().GetGroups(courseNumber);
                        // Очищаем второй ComboBox перед добавлением новых элементов
                        SecondComboBox.Items.Clear();
                        // Добавляем полученные группы во второй ComboBox
                        foreach (string group in groups)
                        {
                            SecondComboBox.Items.Add(group);
                        }
                    }
                    else
                    {
                        // Невозможно преобразовать courseNumberString в int
                        MessageBox.Show("Ошибка: Невозможно преобразовать номер курса в число.");
                    }
                }
                else
                {
                    // Строка не содержит числовое значение курса
                    MessageBox.Show("Ошибка: Неверный формат выбранного элемента.");
                }
            }
        }

        private void btn_AddCompanySave(object sender, RoutedEventArgs e)
        {

            // Получаем выбранную группу из ComboBox
            string groupName = SecondComboBox.Text;
            int groupname = DatabaseContext.GetContext().GetGroupIdByGroupName(groupName);


            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentCompany.CompanyName))
                errors.AppendLine("Укажите название предприятия");
            if (string.IsNullOrWhiteSpace(_currentCompany.FullNameOfTheHead))
                errors.AppendLine("Укажите название руководителя");
            if (string.IsNullOrWhiteSpace(_currentCompany.JobTitle))
                errors.AppendLine("Укажите должность руководителя");
            if (string.IsNullOrWhiteSpace(_currentCompany.Tin))
                errors.AppendLine("Укажите название ИНН");
            if (string.IsNullOrWhiteSpace(_currentCompany.Kpp))
                errors.AppendLine("Укажите название КПП");
            if (string.IsNullOrWhiteSpace(_currentCompany.Oprn))
                errors.AppendLine("Укажите название ОГРН");
            if (string.IsNullOrWhiteSpace(_currentCompany.Bic))
                errors.AppendLine("Укажите название БИК");
            if (string.IsNullOrWhiteSpace(_currentCompany.CheckingAccount))
                errors.AppendLine("Укажите название расчетный счет");
            if (string.IsNullOrWhiteSpace(_currentCompany.CorporateAccount))
                errors.AppendLine("Укажите название Корпоративный счет");
            if (string.IsNullOrWhiteSpace(_currentCompany.Bank))
                errors.AppendLine("Укажите название банк");
            if (string.IsNullOrWhiteSpace(_currentCompany.Telephone))
                errors.AppendLine("Укажите название телефон");
            if (string.IsNullOrWhiteSpace(_currentCompany.Address))
                errors.AppendLine("Укажите название адрес");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                if (_currentCompany.Id == 0)
                    DatabaseContext.GetContext().AddEnterpriseWithRequisites(_currentCompany.CompanyName, _currentCompany.FullNameOfTheHead, _currentCompany.Tin, _currentCompany.Kpp, _currentCompany.Oprn,
                                                                                _currentCompany.Bic, _currentCompany.CheckingAccount, _currentCompany.CorporateAccount, _currentCompany.Bank,
                                                                                _currentCompany.Telephone, _currentCompany.Address, groupname, _currentCompany.JobTitle);

                MessageBox.Show("Предприятие успешно добавлен!");


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
    }
}
