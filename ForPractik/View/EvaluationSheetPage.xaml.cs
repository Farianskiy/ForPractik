using ForPractik.Model;
using ForPractik.ViewModel;
using Npgsql;
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
    /// Логика взаимодействия для EvaluationSheetPage.xaml
    /// </summary>
    public partial class EvaluationSheetPage : Page
    {
        private int checklistId;

        public EvaluationSheetPage(int checklistId)
        {
            InitializeComponent();
            this.checklistId = checklistId;
            LoadChecklistData(checklistId);
        }

        private void LoadChecklistData(int checklistId)
        {
            try
            {
                var checklistData = DatabaseContext.GetContext().GetChecklistData(checklistId);

                if (checklistData != null)
                {
                    // Создаем массив с комбо-боксами
                    ComboBox[] comboBoxes = { listokpkComboBox, questionnaireComboBox, diaryComboBox, agreementComboBox, reviewComboBox, protectionComboBox, reportComboBox };

                    // Для каждого комбо-бокса добавляем элементы "Да" и "Нет"
                    foreach (ComboBox comboBox in comboBoxes)
                    {
                        if (!comboBox.Items.Contains("Да"))
                            comboBox.Items.Add("Да");
                        if (!comboBox.Items.Contains("Нет"))
                            comboBox.Items.Add("Нет");
                    }

                    // Устанавливаем выбранные значения в комбо-боксы и текст в текстовое поле
                    listokpkComboBox.SelectedItem = checklistData.Listokpk ? "Да" : "Нет";
                    questionnaireComboBox.SelectedItem = checklistData.Questionnaire ? "Да" : "Нет";
                    diaryComboBox.SelectedItem = checklistData.Diary ? "Да" : "Нет";
                    agreementComboBox.SelectedItem = checklistData.Agreement ? "Да" : "Нет";
                    reviewComboBox.SelectedItem = checklistData.Review ? "Да" : "Нет";
                    protectionComboBox.SelectedItem = checklistData.Protection ? "Да" : "Нет";
                    reportComboBox.SelectedItem = checklistData.Report ? "Да" : "Нет";
                    gradeTextBox.Text = checklistData.Grade != null ? checklistData.Grade.ToString() : "";

                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку и/или выводим сообщение пользователю
                Console.WriteLine("Ошибка при загрузке данных чек-листа: " + ex.Message);
                throw;
            }
        }



        private void btn_SaveGrade(object sender, RoutedEventArgs e)
        {
            try
            {
                var checklistData = new ChecklistData
                {
                    Id = checklistId,
                    Listokpk = listokpkComboBox.SelectedIndex == 0, // 0 для "Да", 1 для "Нет"
                    Questionnaire = questionnaireComboBox.SelectedIndex == 0,
                    Diary = diaryComboBox.SelectedIndex == 0,
                    Agreement = agreementComboBox.SelectedIndex == 0,
                    Review = reviewComboBox.SelectedIndex == 0,
                    Protection = protectionComboBox.SelectedIndex == 0,
                    Report = reportComboBox.SelectedIndex == 0,
                    Grade = int.Parse(gradeTextBox.Text)
                };

                DatabaseContext.GetContext().SaveChecklistData(checklistData);
                MessageBox.Show("Данные успешно сохранены!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
        }




        private void btn_Back(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
