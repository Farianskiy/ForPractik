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
    /// Логика взаимодействия для UpdateTeachersPage.xaml
    /// </summary>
    public partial class UpdateTeachersPage : Page
    {
        public UpdateTeachersPage()
        {
            InitializeComponent();
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
                string groupName = SecondComboBox.Text;
                string selectedCurator = ComboCurators.Text;

                int curatorId = DatabaseContext.GetContext().GetCuratorIdByName(selectedCurator);

                int groupId = DatabaseContext.GetContext().GetGroupIdByGroupName(groupName);

                ValidateInput(selectedCurator);

                DatabaseContext.GetContext().UpdateGroupTeacher(groupId, curatorId);

                MessageBox.Show("Куратор успешно изменен!");

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
               
            }
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

    }


}
