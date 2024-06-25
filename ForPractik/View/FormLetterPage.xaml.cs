using ForPractik.Model;
using ForPractik.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Логика взаимодействия для FormLetterPage.xaml
    /// </summary>
    public partial class FormLetterPage : Page
    {
        public ObservableCollection<string> Rukovoditeli { get; set; }
        public ObservableCollection<string> ZamDirectors { get; set; }

        public FormLetterPage(string student, string placeofpractice, string headofpractice, string groupName, int enterpriseId)
        {
            InitializeComponent();

            // Загрузка сохраненных элементов из настроек приложения
            ZamDirectors = new ObservableCollection<string>();
            LoadZamDirectors();

            // Привязка к полю TextBox
            cmbZamDirector.ItemsSource = ZamDirectors;

            // Загрузка сохраненных элементов из настроек приложения
            Rukovoditeli = new ObservableCollection<string>();
            LoadCustomItems();

            cmbRukovoditel.ItemsSource = Rukovoditeli;

            Student = student;
            Placeofpractice = placeofpractice;
            Headofpractice = headofpractice;
            GroupName = groupName;
            EnterpriseId = DatabaseContext.GetContext().GetFullNameOfTheHeadByEnterpriseId(enterpriseId);


            // Установите DataContext для привязки данных
            DataContext = this;
        }

        // Обработчик сохранения текста
        private void SaveZamDirectors_Click(object sender, RoutedEventArgs e)
        {
            string newItem = cmbZamDirector.Text.Trim();

            // Проверка на пустой ввод и наличие такого элемента
            if (!string.IsNullOrEmpty(newItem) && !ZamDirectors.Contains(newItem))
            {
                ZamDirectors.Add(newItem);
                cmbZamDirector.SelectedItem = newItem;

                // Сохранение элементов в настройки приложения
                SaveZamDirectors();
            }

            cmbZamDirector.Text = ""; // Очистить поле ввода после сохранения
        }

        // Загрузка сохраненных элементов из настроек приложения
        private void LoadZamDirectors()
        {
            StringCollection zamDirectors = Properties.Settings.Default.ZamDirectors;
            if (zamDirectors != null)
            {
                foreach (string item in zamDirectors)
                {
                    ZamDirectors.Add(item);
                }
            }
        }

        // Сохранение элементов в настройки приложения
        private void SaveZamDirectors()
        {
            StringCollection zamDirectors = new StringCollection();
            foreach (string item in ZamDirectors)
            {
                zamDirectors.Add(item);
            }
            Properties.Settings.Default.ZamDirectors = zamDirectors;
            Properties.Settings.Default.Save();
        }

        private void btn_Back(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }

        // Свойства для привязки данных
        public string Student { get; set; }
        public string Placeofpractice { get; set; }
        public string Headofpractice { get; set; }
        public string GroupName { get; set; }   
        public string EnterpriseId { get; set; }

        // Обработчик кнопки добавления
        private void AddRukovoditel_Click(object sender, RoutedEventArgs e)
        {
            string newItem = cmbRukovoditel.Text.Trim();

            // Проверка на пустой ввод и наличие такого элемента
            if (!string.IsNullOrEmpty(newItem) && !Rukovoditeli.Contains(newItem))
            {
                Rukovoditeli.Add(newItem);
                cmbRukovoditel.SelectedItem = newItem; // Выбрать новый элемент после добавления

                // Сохранение элемента в настройки приложения
                SaveCustomItems();
            }

            cmbRukovoditel.Text = ""; // Очистить поле ввода после добавления
        }

        // Загрузка сохраненных элементов из настроек приложения
        private void LoadCustomItems()
        {
            StringCollection customItems = Properties.Settings.Default.CustomItems;
            if (customItems != null)
            {
                foreach (string item in customItems)
                {
                    Rukovoditeli.Add(item);
                }
            }
        }

        // Сохранение элементов в настройки приложения
        private void SaveCustomItems()
        {
            StringCollection customItems = new StringCollection();
            foreach (string item in Rukovoditeli)
            {
                customItems.Add(item);
            }
            Properties.Settings.Default.CustomItems = customItems;
            Properties.Settings.Default.Save();
        }

        private void btn_Add(object sender, RoutedEventArgs e)
        {
            // Вызываем метод FillWordDocument с необходимыми параметрами
            FillWordDocument(new StudentPractik());
        }

        public void FillWordDocument(StudentPractik student)
        {
            using (WordDocumentManager docManager = new WordDocumentManager())
            {
                // Открываем шаблон документа
                string templatePath = "C:\\Users\\Farianskiy\\Desktop\\ForPractik\\ForPractik\\Resources\\LetterOfficial.docx";
                docManager.OpenDocument(templatePath);


                // Получаем данные из полей ввода
                string director = txtDirector.Text;
                string organization = txtOrganization.Text;

                string organizTwo = txtOrganiz.Text;
                string students = txtStudent.Text;

                string zamDiretor = cmbZamDirector.Text;
                string rukovoditel = cmbRukovoditel.Text;

                string head = txtHeadOrganization.Text;

                PracticePeriod practicePeriod = DatabaseContext.GetContext().GetPracticePeriodByGroup(GroupName);

                Specialty specialty = DatabaseContext.GetContext().GetSpecialtyByGroupName(GroupName);

                int course = GetCourseFromGroupName(GroupName);

                // Вставляем данные студента в документ
                docManager.FillDocumentWithStudentData(students, director, organization, practicePeriod, specialty, course, organizTwo, EnterpriseId, zamDiretor, rukovoditel, head);

                // Сохраняем заполненный документ по указанному пути
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Документ Word (*.docx)|*.docx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    docManager.SaveAndCloseDocument(saveFileDialog.FileName);
                    MessageBox.Show("Документ успешно заполнен и сохранен.", "Успех");
                }
            }
        }

        public int GetCourseFromGroupName(string groupName)
        {
            // Регулярное выражение для извлечения курса из имени группы
            var regex = new System.Text.RegularExpressions.Regex(@"(\d+)");
            var match = regex.Match(groupName);

            if (match.Success && int.TryParse(match.Value.Substring(0, 1), out int course))
            {
                return course;
            }
            else
            {
                throw new ArgumentException("Invalid group name format");
            }
        }

    }
}
