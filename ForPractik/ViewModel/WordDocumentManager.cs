using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Office.Interop.Word;
using ForPractik.Model;
using Application = Microsoft.Office.Interop.Word.Application;
using GSF;

namespace ForPractik.ViewModel
{
    internal class WordDocumentManager : IDisposable
    {
        private Application wordApp;
        private Document doc;

        public WordDocumentManager()
        {
            wordApp = new Application();
        }

        public void OpenDocument(string templatePath)
        {
            doc = wordApp.Documents.Open(templatePath);
        }

        public void SaveAndCloseDocument(string filePath)
        {
            doc.SaveAs2(filePath);
            doc.Close();
        }

        public void Dispose()
        {
            if (doc != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);

            if (wordApp != null)
            {
                wordApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }
        }

        public string GetInitialsFromFullName(string fullName)
        {
            string[] parts = fullName.Split(' ');
            string lastName = parts[0];
            string initials = "";
            for (int i = 1; i < parts.Length; i++)
            {
                initials += parts[i][0] + ".";
            }
            return $"{initials}{lastName}";
        }

        public void ReplaceTextInDocument(string placeholder, string replacement)
        {
            foreach (Range range in doc.StoryRanges)
            {
                range.Find.ClearFormatting();
                range.Find.Text = placeholder;
                range.Find.Replacement.Text = replacement;
                if (range.Find.Execute(Replace: WdReplace.wdReplaceOne))
                {
                    return;
                }
            }
            throw new Exception("Метка-заглушка не найдена: " + placeholder);
        }

        public void FillDocumentWithData(string groupName, string chairman, string departmentHead, string groupCurator, string date, List<Student> students, string special)
        {
            try
            {
                ReplaceTextInDocument("<Special>", special);
                ReplaceTextInDocument("<Gruop>", groupName);
                ReplaceTextInDocument("<Chairman>", chairman);
                ReplaceTextInDocument("<DepartmentHead>", departmentHead);
                ReplaceTextInDocument("<GroupCurator>", groupCurator);
                ReplaceTextInDocument("<Date>", date);
                ReplaceTextInDocument("<ChairmanInit>", GetInitialsFromFullName(chairman));
                ReplaceTextInDocument("<DepartmentHeadInit>", GetInitialsFromFullName(departmentHead));
                ReplaceTextInDocument("<GroupCuratorInit>", GetInitialsFromFullName(groupCurator));
                PressDeleteAfterFirstTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public void PressDeleteAfterFirstTable()
        {
            Table firstTable = doc.Tables[1];
            Range rangeAfterTable = doc.Range(firstTable.Range.End, firstTable.Range.End);
            rangeAfterTable.Select();
            doc.Application.Selection.Delete();
        }

        public void InsertStudentsIntoTable(List<Student> students)
        {
            // Предполагается, что таблица находится на первой странице, измените индекс, если это не так
            Table table = doc.Tables[2];

            // Добавляем новые строки в таблицу для каждого студента
            foreach (Student student in students)
            {
                // Добавляем новую строку в конец таблицы
                Row newRow = table.Rows.Add();

                // Далее ваш код
                newRow.Cells[1].Range.Text = (table.Rows.Count - 1).ToString(); // Заполняем первый столбец

                // Заполняем вторую ячейку ФИО студента
                newRow.Cells[2].Range.Text = $"{student.SurName} {student.Name} {student.Patronymic}";

                // Заполняем ячейки со словом "зачет" в 3, 5 и 7 столбцах
                newRow.Cells[3].Range.Text = "зачет";
                newRow.Cells[5].Range.Text = "зачет";
                newRow.Cells[7].Range.Text = "зачет";
            }

            // Удаляем первую строку, если она существует
            if (table.Rows.Count >= 2) // Проверяем, что в таблице есть хотя бы две строки
            {
                table.Rows[1].Delete(); // Удаляем первую строку
            }
        }

        public void InsertPracticeDataIntoTable(List<PracticeData> practiceDataList)
        {
            try
            {
                if (doc.Tables.Count < 1)
                {
                    MessageBox.Show("Таблица с указанным индексом не найдена.");
                    return;
                }

                Table table = doc.Tables[1];

                // Индекс для новых строк начинается со второго, чтобы не удалять заголовок
                int startRowIndex = table.Rows.Count + 1;

                foreach (PracticeData practiceData in practiceDataList)
                {
                    Row newRow = table.Rows.Add();
                    newRow.Cells[1].Range.Text = (startRowIndex++ - 1).ToString();

                    SetCellText(newRow.Cells[2], practiceData.FullNameStudent);
                    SetCellText(newRow.Cells[3], practiceData.PlaceOfPractice ?? "");
                    SetCellText(newRow.Cells[4], practiceData.HeadOfPractice ?? "");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при вставке данных в таблицу: " + ex.Message);
            }
        }

        private void SetCellText(Cell cell, string text)
        {
            cell.Range.Text = text;
            cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
        }

        public void FillDocumentListDistribution(string groupName, string special, PracticePeriod practicePeriod, List<PracticeModule> practiceModules, string date, List<PracticeData> practiceDataList, string teacherName)
        {
            try
            {
                ReplaceTextInDocument("<Special>", special);
                ReplaceTextInDocument("<Gruop>", groupName);
                ReplaceTextInDocument("<StartOfPractice>", practicePeriod.StartOfPractice.ToString("dd.MM.yyyy"));
                ReplaceTextInDocument("<EndOfPractice>", practicePeriod.EndOfPractice.ToString("dd.MM.yyyy"));

                // Объединяем имена модулей, разделяя их символом новой строки
                string moduleNames = string.Join(Environment.NewLine, practiceModules.Select(m => m.ModuleName));
                ReplaceTextInDocument("<Module>", moduleNames);

                ReplaceTextInDocument("<Date>", date);
                // Выбираем первое значение NumberOfHours из списка practiceDataList
                int numberOfHours = practiceDataList.FirstOrDefault()?.NumberOfHours ?? 0;
                ReplaceTextInDocument("<Hours>", numberOfHours.ToString());
                ReplaceTextInDocument("<Curator>", teacherName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public void FillDocumentWithStudentData(StudentEnterpriseDetails student, string headOfThePractice, string associateDirector)
        {
            try
            {
                // Код для заполнения документа данными студента
                ReplaceTextInDocument("<StudentFullName>", student.FullName);
                ReplaceTextInDocument("<CompanyName>", student.CompanyName);
                ReplaceTextInDocument("<FullnameOfTheHead>", student.FullnameOfTheHead);
                ReplaceTextInDocument("<HeadOfThePractice>", headOfThePractice);
                ReplaceTextInDocument("<AssociateDirector>", associateDirector);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }



    }
}
