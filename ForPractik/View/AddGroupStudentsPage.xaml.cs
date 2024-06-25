using ForPractik.ViewModel;
using ForPractik.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace ForPractik.View
{
    /// <summary>
    /// Логика взаимодействия для AddGroupStudentsPage.xaml
    /// </summary>
    public partial class AddGroupStudentsPage : Page
    {
        private readonly HttpClient _client;

        public AddGroupStudentsPage()
        {
            InitializeComponent();
            _client = new HttpClient();
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

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        private async void btn_AddGroupSave(object sender, RoutedEventArgs e)
        {
            string groupName = SecondComboBox.Text;

            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Введите имя группы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Получение токена (если он еще не был получен)
            if (string.IsNullOrEmpty(_client.Token))
            {
                string token = await _client.GetTokenAsync("username", "password"); // Замените "username" и "password" на реальные данные
                if (token == null)
                {
                    MessageBox.Show("Не удалось получить токен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _client.SetToken(token);
            }

            int? groupId = await _client.GetGroupIdAsync(groupName);

            if (groupId.HasValue)
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                List<Student> students = await _client.GetStudentsAsync(groupId.Value, currentDate);

                if (students != null && students.Any())
                {
                    // Сохранение студентов в базу данных
                    foreach (var student in students)
                    {
                        // Предположим, что student.Name содержит полное имя
                        string[] nameParts = student.Name.Split(' ');
                        string surname = nameParts.Length > 0 ? nameParts[0] : "";
                        string name = nameParts.Length > 1 ? nameParts[1] : "";
                        string patronymic = nameParts.Length > 2 ? nameParts[2] : "";

                        DatabaseContext.GetContext().AddStudentWithDependencies(surname, name, patronymic, groupName, false);
                    }

                    MessageBox.Show("Студенты успешно добавлены в базу данных.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось получить студентов или список пуст", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Не удалось найти группу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
