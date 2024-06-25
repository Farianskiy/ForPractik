using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Office.Interop.Word;
using ForPractik.Model;
using Application = Microsoft.Office.Interop.Word.Application;
using ForPractik.View;
using System.Text.RegularExpressions;

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
                ReplaceTextInDocument("<GroupCurator>", groupCurator);
                ReplaceTextInDocument("<Date>", date);
                ReplaceTextInDocument("<ChairmanInit>", GetSurnameAndInitials(chairman));
                ReplaceTextInDocument("<DepartmentHeadInit>", GetSurnameAndInitials(departmentHead));
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

        public void InsertStudentsContactIntoTable(List<Student> students)
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

                // Добавляем новые строки в таблицу для каждого студента
                foreach (Student student in students)
                {
                    // Добавляем новую строку в конец таблицы
                    Row newRow = table.Rows.Add();
                    newRow.Cells[1].Range.Text = (startRowIndex++ - 1).ToString();

                    // Устанавливаем автоматическое управление высотой строки
                    newRow.HeightRule = WdRowHeightRule.wdRowHeightAuto;

                    // Заполняем вторую ячейку ФИО студента
                    SetCellText(newRow.Cells[2], $"{student.SurName} {student.Name} {student.Patronymic}");


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при вставке данных в таблицу: " + ex.Message);
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

                    // Устанавливаем автоматическое управление высотой строки
                    newRow.HeightRule = WdRowHeightRule.wdRowHeightAuto;

                    SetCellText(newRow.Cells[2], practiceData.FullNameStudent);
                    SetCellText(newRow.Cells[3], practiceData.PlaceOfPractice ?? "");
                    SetCellText(newRow.Cells[4], GetSurnameAndInitials(practiceData.HeadOfPractice ?? ""));
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
            // Настраиваем перенос текста, чтобы он не исчезал, если превышает ширину ячейки
            cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
            cell.Range.ParagraphFormat.LineSpacingRule = WdLineSpacing.wdLineSpaceSingle;
            cell.Range.ParagraphFormat.SpaceAfter = 0;
        }

        // Вспомогательный метод для извлечения фамилии и инициалов
        private string GetSurnameAndInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;

            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return string.Empty;

            var surname = parts[0];
            var initials = parts.Length > 1 ? string.Join("", parts.Skip(1).Select(p => p[0] + ".")) : string.Empty;

            return $"{surname} {initials}";
        }

        public void FillDocumentListDistribution(string groupName, string special, PracticePeriod practicePeriod, List<PracticeModule> practiceModules, string date, List<PracticeData> practiceDataList, string teacherName)
        {
            try
            {
                ReplaceTextInDocument("<Special>", special);
                ReplaceTextInDocument("<Gruop>", groupName);
                ReplaceTextInDocument("<StartOfPractice>", practicePeriod.StartOfPractice.ToString("dd.MM.yyyy"));
                ReplaceTextInDocument("<EndOfPractice>", practicePeriod.EndOfPractice.ToString("dd.MM.yyyy"));

                if(practiceModules != null)
                {
                    // Объединяем имена модулей, разделяя их символом новой строки
                    string moduleNames = string.Join(Environment.NewLine, practiceModules.Select(m => m.ModuleName));
                    ReplaceTextInDocument("<Module>", moduleNames);
                }

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

        static string ExtractNameAndPatronymic(string fullName)
        {
            // Регулярное выражение для извлечения имени и отчества
            Regex regex = new Regex(@"^\S+\s+(\S+\s+\S+)$");
            Match match = regex.Match(fullName);
            if (match.Success)
            {
                // Возвращаем только имя и отчество
                return match.Groups[1].Value;
            }
            else
            {
                // Если не удалось найти совпадение, возвращаем пустую строку или обрабатываем ошибку
                return string.Empty;
            }
        }


        public void FillDocumentWithStudentData(string students, string director, string organization, PracticePeriod practicePeriod, Specialty specialty, int course, string organizTwo, string enterpriseId, string zamDiretor, string rukovoditel, string head)
        {
            try
            {
                // Код для заполнения документа данными студента

                
                ReplaceTextInDocument("<HeadOfThePractice>", director);
                ReplaceTextInDocument("<AssociateDirector>", organization);
                ReplaceTextInDocument("<Head>", head);

                ReplaceTextInDocument("<EnterpriseId>", ExtractNameAndPatronymic(enterpriseId));

                ReplaceTextInDocument("<CompanyName>", organizTwo);
                ReplaceTextInDocument("<Course>", course.ToString());
                ReplaceTextInDocument("<Student>", students);

                ReplaceTextInDocument("<Specialization>", specialty.Specialization);
                ReplaceTextInDocument("<ProfessionCodes>", specialty.ProfessionCodes ?? string.Empty);
                ReplaceTextInDocument("<Qualification>", specialty.Qualification ?? string.Empty);

                ReplaceTextInDocument("<StartOfPractice>", practicePeriod.StartOfPractice.ToString("dd.MM.yyyy"));
                ReplaceTextInDocument("<EndOfPractice>", practicePeriod.EndOfPractice.ToString("dd.MM.yyyy"));

                ReplaceTextInDocument("<HeadOfThePracticeOne>", rukovoditel);
                ReplaceTextInDocument("<AssociateDirectorOne>", zamDiretor);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        

        public void InsertOKEntriesIntoTable(List<OKEntry> okEntries, string groupName)
        {
            try
            {
                ReplaceTextInDocument("<Group>", groupName);

                // Предполагается, что таблица одна и она первая в документе
                Table table = doc.Tables[1];

                // Добавляем строки с данными из okEntries
                foreach (var entry in okEntries)
                {
                    Row row = table.Rows.Add();
                    row.Cells[1].Range.Text = $"{entry.OK}.{entry.Description}";
                    row.Cells[2].Range.Text = string.Empty; // Оставляем вторую ячейку пустой
                }

                // Удаляем первую строку таблицы
                table.Rows[1].Delete();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        public void InsertOKEntriesIntoTablePK(List<OKEntry> okEntries, string groupName)
        {
            try
            {
                // Заменяем текст "<Group>" в документе на groupName
                ReplaceTextInDocument("<Group>", groupName);

                // Получаем таблицу из документа
                Table table = doc.Tables[1];

                string previousGroup = null;

                // Перебираем записи и вставляем их в таблицу
                foreach (var entry in okEntries)
                {
                    if (entry.Group != previousGroup)
                    {
                        // Добавляем строку для заголовка группы
                        Row groupRow = table.Rows.Add();
                        Cell mergedCell = groupRow.Cells[1];
                        mergedCell.Merge(groupRow.Cells[2]); // Объединяем две ячейки
                        mergedCell.Range.Text = entry.Group; // Добавляем текст в объединенную ячейку
                        previousGroup = entry.Group;
                    }

                    // Добавляем строку для ОК и Описания
                    Row row = table.Rows.Add();
                    row.Cells[1].Range.Text = $"{entry.OK}. {entry.Description}";
                    if (row.Cells.Count < 2)
                    {
                        row.Cells[1].Split(1, 2);
                        row.Cells[1].Width = 431.5f;
                        row.Cells[2].Width = 70f;
                    }
                    row.Cells[2].Range.Text = string.Empty; // Оставляем вторую ячейку пустой
                }

                // Удаляем первые две строки таблицы, если есть строки
                if (table.Rows.Count > 2)
                {
                    table.Rows[1].Delete();
                    table.Rows[1].Delete();
                }
            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке с более подробной информацией
                MessageBox.Show($"Произошла ошибка при вставке записей: {ex.Message}");
            }
        }

        




        public void InsertOKEntriesIntoTableProgram(List<OKEntry> okEntries, string groupName, string predsedatel, string zamestitel, string teachers)
        {
            try
            {
                // Заменяем текст "<Group>" в документе на groupName
                ReplaceTextInDocument("<Group>", groupName);

                // Заменяем текст "<Group>" в документе на groupName
                ReplaceTextInDocument("<Predsedatel>", predsedatel);

                // Заменяем текст "<Group>" в документе на groupName
                ReplaceTextInDocument("<Zamestitel>", zamestitel);

                DateTime date = DateTime.Now;
                // Получаем только текущий год
                int year = date.Year;
                // Заменяем текст "<Date>" в документе на текущий год
                ReplaceTextInDocument("<Date>", year.ToString());
                ReplaceTextInDocument("<Date>", year.ToString());

                ReplaceTextInDocument("<Teachers>", teachers);

                // Получаем таблицу из документа
                Table table = doc.Tables[2];

                string previousGroup = null;
                int totalHours = 0; // Переменная для хранения суммы Hours

                // Перебираем записи и вставляем их в таблицу
                foreach (var entry in okEntries)
                {
                    if (!string.IsNullOrEmpty(entry.Group) && entry.Group != previousGroup)
                    {
                        // Добавляем строку для заголовка группы
                        Row groupRow = table.Rows.Add();
                        groupRow.Range.Font.Bold = 0;
                        Cell mergedCell = groupRow.Cells[2];
                        mergedCell.Merge(groupRow.Cells[3]); // Объединяем второй и третий столбцы
                        groupRow.Cells[1].Range.Text = entry.OK; // Вставляем ОК в первую ячейку
                        mergedCell.Range.Text = entry.Group; // Добавляем текст группы в объединенную ячейку

                        // Устанавливаем высоту строки
                        groupRow.Height = 15;

                        // Установка жирного шрифта для всей строки
                        groupRow.Range.Font.Bold = 1;

                        previousGroup = entry.Group;
                    }
                    else
                    {
                        // Добавляем строку для ОК, Описания и Объема часов
                        Row row = table.Rows.Add();
                        row.Range.Font.Bold = 0;
                        // Устанавливаем высоту строки
                        row.Height = 10;
                        row.Cells[1].Range.Text = entry.OK;

                        // Проверка на пустое значение ОК
                        if (!string.IsNullOrEmpty(entry.OK))
                        {
                            // Проверка на числовое значение ОК
                            int okValue;
                            bool isNumeric = int.TryParse(entry.OK, out okValue);

                            // Проверка и установка жирного шрифта, если ОК не является числом от 1 до 99
                            if (!isNumeric || okValue < 1 || okValue > 99)
                            {
                                row.Range.Font.Bold = 1;

                                totalHours += (int)entry.Hours;

                            }
                        }

                        // Проверка и разъединение ячеек, если их меньше трех
                        if (row.Cells.Count < 3)
                        {
                            row.Cells[1].Split(1, 3);
                        }

                        // Убедиться, что нет лишних столбцов
                        while (row.Cells.Count > 3)
                        {
                            row.Cells[4].Delete();
                        }

                        // Установить ширину ячеек после разъединения
                        row.Cells[1].Width = 70.5F;
                        row.Cells[2].Width = 405f;
                        row.Cells[3].Width = 56.5f;

                        SetCellText(row.Cells[2], entry.Description);
                        row.Cells[3].Range.Text = entry.Hours > 0 ? entry.Hours.ToString() : string.Empty;
                    }
                }

                // После завершения цикла заменяем текст "<Hours>" на общее количество часов
                ReplaceTextInDocument("<Hours>", totalHours.ToString());

            }
            catch (Exception ex)
            {
                // Выводим сообщение об ошибке с более подробной информацией
                MessageBox.Show($"Произошла ошибка при вставке записей: {ex.Message}");
            }
        }

















    }
}
