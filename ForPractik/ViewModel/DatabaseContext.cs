using ForPractik.Model;
using GSF.Console;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace ForPractik.ViewModel
{
    public partial class DatabaseContext : DbContext
    {
        private static DatabaseContext _context;

        public static DatabaseContext GetContext()
        {
            if (_context == null)
                _context = new DatabaseContext();
            return _context;
        }

        private NpgsqlConnection connection;

        public DatabaseContext()
        {
            string connectionString = "Host=127.0.0.1;Port=5432;Database=educationalpracticetri;Username=postgres;Password=122";
            connection = new NpgsqlConnection(connectionString);
        }

        public void OpenConnection()
        {
            try
            {
                connection.Open();
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message);
            }
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        public List<string> GetGroups(int courseNumber)
        {
            List<string> groups = new List<string>();

            try
            {
                // Открываем соединение с базой данных
                DatabaseContext.GetContext().OpenConnection();

                // Выполняем запрос к базе данных для получения групп выбранного курса
                string query = "SELECT groupid FROM groups WHERE groupid LIKE @CourseNumber";
                NpgsqlCommand command = new NpgsqlCommand(query, DatabaseContext.GetContext().connection);
                command.Parameters.AddWithValue("@CourseNumber", "%-" + courseNumber + "%");

                NpgsqlDataReader reader = command.ExecuteReader();

                // Читаем результат запроса
                while (reader.Read())
                {
                    string groupName = reader.GetString(0);
                    groups.Add(groupName);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении групп: " + ex.Message);
            }
            finally
            {
                // Закрываем соединение с базой данных
                DatabaseContext.GetContext().CloseConnection();
            }

            return groups;
        }




        // Другие методы для выполнения SQL-запросов и работы с базой данных.

        public DataTable SelectData(string query)
        {
            DataTable dataTable = new DataTable();

            try
            {
                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка выполнения SQL-запроса: " + ex.Message);
            }

            return dataTable;
        }

        public string GetGroupNameByStudentId(int studentId)
        {
            // Метод для получения имени группы по Id студента
            string query = $"SELECT Groups.GroupId FROM Students JOIN Groups ON Students.GroupId = Groups.Id WHERE Students.Id = {studentId}";
            DataTable result = DatabaseContext.GetContext().SelectData(query);
            return result.Rows.Count > 0 ? result.Rows[0]["GroupId"].ToString() : string.Empty;


        }

        public void AddStudentWithDependencies(string surname, string name, string patronymic, string groupName, bool worksThere)
        {
            try
            {
                OpenConnection();

                // Получение Id группы по её имени
                int groupId = GetGroupIdByGroupName(groupName);

                // Создаем SQL запрос для вставки студента
                string insertStudentQuery = "INSERT INTO public.students (surname, name, patronymic, groupid, works_there) VALUES (@Surname, @Name, @Patronymic, @GroupId, @WorksThere)";
                NpgsqlCommand insertStudentCommand = new NpgsqlCommand(insertStudentQuery, connection);
                insertStudentCommand.Parameters.AddWithValue("@Surname", surname);
                insertStudentCommand.Parameters.AddWithValue("@Name", name);
                insertStudentCommand.Parameters.AddWithValue("@Patronymic", patronymic);
                insertStudentCommand.Parameters.AddWithValue("@GroupId", groupId);
                insertStudentCommand.Parameters.AddWithValue("@WorksThere", worksThere);

                // Выполняем запрос для вставки студента
                insertStudentCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении студента: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }


        public int GetGroupIdByGroupName(string groupName)
        {
            string query = $"SELECT Id FROM Groups WHERE GroupId = '{groupName}'";
            DataTable result = SelectData(query);
            return result.Rows.Count > 0 ? (int)result.Rows[0]["Id"] : -1; // Возвращаем Id группы или -1, если не найдено
        }

        private int GetSpecializationIdByGroupId(int groupId)
        {
            string query = $"SELECT Specialization FROM Groups WHERE Id = {groupId}";
            DataTable result = SelectData(query);
            return result.Rows.Count > 0 ? (int)result.Rows[0]["Specialization"] : -1; // Возвращаем Id специализации или -1, если не найдено
        }

        private int GetModuleIdBySpecializationAndModule(int specializationId, string module)
        {
            string query = $"SELECT Id FROM Modules WHERE IdSpecialties = {specializationId} AND Module = '{module}'";
            DataTable result = SelectData(query);
            return result.Rows.Count > 0 ? (int)result.Rows[0]["Id"] : -1; // Возвращаем Id модуля или -1, если не найдено
        }

        public List<Student> GetStudents()
        {
            string query = "SELECT id, SurName, Name, Patronymic FROM Students";
            DataTable studentsTable = SelectData(query);
            List<Student> studentsList = new List<Student>();

            foreach (DataRow row in studentsTable.Rows)
            {
                Student student = new Student
                {
                    Id = Convert.ToInt32(row["id"]),
                    SurName = row["SurName"].ToString(),
                    Name = row["Name"].ToString(),
                    Patronymic = row["Patronymic"].ToString(),
                };

                studentsList.Add(student);
            }

            return studentsList;
        }


        private void ExecuteNonQuery(string query)
        {
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выполнения SQL-запроса: " + ex.Message);
            }
        }

        public void UpdateStudent(int studentId, string surname, string name, string patronymic, string groupName)
        {
            try
            {
                OpenConnection();

                // Обновление данных о студенте в таблице "Students"
                string updateStudentQuery = $"UPDATE Students SET SurName = '{surname}', Name = '{name}', Patronymic = '{patronymic}' WHERE Id = {studentId}";

                using (NpgsqlCommand cmdStudent = new NpgsqlCommand(updateStudentQuery, connection))
                {
                    cmdStudent.ExecuteNonQuery();
                }

                // Получение Id группы по её имени
                int groupId = GetGroupIdByGroupName(groupName);

                if (groupId != -1)
                {
                    // Обновление группы студента в таблице "Students"
                    string updateGroupQuery = $"UPDATE Students SET groupId = '{groupId}' WHERE Id = {studentId}";

                    using (NpgsqlCommand cmdGroup = new NpgsqlCommand(updateGroupQuery, connection))
                    {
                        cmdGroup.ExecuteNonQuery();
                    }
                    MessageBox.Show("Изменения прошли успешно.");
                }
                else
                {
                    Console.WriteLine("Группа не найдена в базе данных.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обновлении студента: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

        }

        public void DeleteStudents(List<int> studentIds)
        {
            try
            {
                OpenConnection();

                // Создаем параметры для SQL-запроса
                string deleteQuery = $"DELETE FROM Students WHERE Id = ANY(@StudentIds)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(deleteQuery, connection))
                {
                    // Добавляем параметр
                    cmd.Parameters.AddWithValue("@StudentIds", studentIds.ToArray());

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при удалении студентов: " + ex.Message);
                throw; // Вы можете выбросить исключение, чтобы обработать его в вызывающем коде
            }
            finally
            {
                CloseConnection();
            }
        }



        public void AddSpecialty(string specialization, string professionCodes, string qualification)
        {
            try
            {
                OpenConnection();

                // Проверка, не существует ли уже специальности с такой квалификацией
                string checkExistenceQuery = "SELECT COUNT(*) FROM specialties WHERE qualification = @Qualification";
                NpgsqlCommand checkExistenceCommand = new NpgsqlCommand(checkExistenceQuery, connection);
                checkExistenceCommand.Parameters.AddWithValue("@Qualification", qualification);
                int count = Convert.ToInt32(checkExistenceCommand.ExecuteScalar());

                if (count > 0)
                {
                    MessageBox.Show("Специальность с такой квалификацией уже существует.");
                    return;
                }

                // Вставка новой специальности
                string addSpecialtyQuery = "INSERT INTO specialties (specialization, profession_codes, qualification) VALUES (@Specialization, @ProfessionCodes, @Qualification)";
                NpgsqlCommand addSpecialtyCommand = new NpgsqlCommand(addSpecialtyQuery, connection);
                addSpecialtyCommand.Parameters.AddWithValue("@Specialization", specialization);
                addSpecialtyCommand.Parameters.AddWithValue("@ProfessionCodes", professionCodes);
                addSpecialtyCommand.Parameters.AddWithValue("@Qualification", qualification);
                addSpecialtyCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении специальности: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }



        public List<string> GetSpecialties()
        {
            List<string> specialtiesList = new List<string>();
            string query = "SELECT Specialization FROM Specialties";
            DataTable specialtiesTable = SelectData(query);

            foreach (DataRow row in specialtiesTable.Rows)
            {
                specialtiesList.Add(row["Specialization"].ToString());
            }

            return specialtiesList;
        }

        public List<string> GetCurators()
        {
            List<string> curatorsList = new List<string>();
            string query = "SELECT full_name FROM teachers";
            DataTable curatorsTable = SelectData(query);

            foreach (DataRow row in curatorsTable.Rows)
            {
                curatorsList.Add(row["full_name"].ToString());
            }

            return curatorsList;
        }


        public int GetSpecializationIdByName(string specializationName)
        {
            string query = $"SELECT Id FROM Specialties WHERE Specialization = '{specializationName}'";
            DataTable result = SelectData(query);
            return result.Rows.Count > 0 ? Convert.ToInt32(result.Rows[0]["Id"]) : -1; // Возвращаем Id специальности или -1, если не найдено
        }

        public int GetCuratorIdByName(string curatorName)
        {
            int curatorId = -1;
            try
            {
                OpenConnection();
                string query = $"SELECT id FROM teachers WHERE full_name = '{curatorName}'";
                DataTable result = SelectData(query);

                if (result.Rows.Count > 0)
                {
                    curatorId = Convert.ToInt32(result.Rows[0]["id"]);
                }
                else
                {
                    throw new Exception("Куратор с таким именем не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении идентификатора куратора: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return curatorId;
        }

        public void AddGroup(string groupName, int specializationId, int teacherId)
        {
            try
            {
                OpenConnection();

                // Проверка, не существует ли уже группы с таким названием и Id специальности
                string checkExistenceQuery = $"SELECT COUNT(*) FROM groups WHERE groupid = '{groupName}' AND specialization = {specializationId}";
                int count = Convert.ToInt32(SelectData(checkExistenceQuery).Rows[0][0]);

                if (count > 0)
                {
                    MessageBox.Show("Группа с таким названием и специальностью уже существует.");
                    return;
                }

                // Вставка новой группы
                string addGroupQuery = $"INSERT INTO groups (groupid, specialization, teacher_id) VALUES ('{groupName}', {specializationId}, {teacherId})";
                ExecuteNonQuery(addGroupQuery);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении группы: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }


        public void AddModule(string moduleName, int specializationId)
        {
            try
            {
                OpenConnection();

                // Проверка, не существует ли уже модуля с таким названием и Id специальности
                string checkExistenceQuery = $"SELECT COUNT(*) FROM Modules WHERE Module = '{moduleName}' AND IdSpecialties = {specializationId}";
                int count = Convert.ToInt32(SelectData(checkExistenceQuery).Rows[0][0]);

                if (count > 0)
                {
                    MessageBox.Show("Модуль с таким названием и специальностью уже существует.");
                    return;
                }

                // Вставка нового модуля
                string addModuleQuery = $"INSERT INTO Modules (IdSpecialties, Module) VALUES ({specializationId}, '{moduleName}')";
                ExecuteNonQuery(addModuleQuery);


            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении модуля: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<Student> GetStudentsByGroup(string groupName)
        {
            List<Student> studentsList = new List<Student>();

            try
            {
                // Открываем соединение с базой данных
                DatabaseContext.GetContext().OpenConnection();

                // Выполняем запрос к базе данных для получения списка студентов определенной группы
                string query = "SELECT s.id, s.SurName, s.Name, s.Patronymic " +
                               "FROM Students s INNER JOIN groups g ON s.groupid = g.id " +
                               "WHERE g.groupid = @GroupName";
                NpgsqlCommand command = new NpgsqlCommand(query, DatabaseContext.GetContext().connection);
                command.Parameters.AddWithValue("@GroupName", groupName);
                NpgsqlDataReader reader = command.ExecuteReader();

                // Читаем результат запроса
                while (reader.Read())
                {
                    Student student = new Student
                    {
                        Id = reader.GetInt32(0),
                        SurName = reader.GetString(1),
                        Name = reader.GetString(2),
                        Patronymic = reader.GetString(3),
                    };

                    studentsList.Add(student);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении списка студентов: " + ex.Message);
            }
            finally
            {
                // Закрываем соединение с базой данных
                DatabaseContext.GetContext().CloseConnection();
            }

            return studentsList;
        }



        public void AddEnterpriseWithRequisites(string companyName, string fullNameOfTheHead, string tin, string kpp, string oprn, string bic, string checkingAccount, string corporateAccount, string bank, string telephone, string address, int groupId, string jobTitle)
        {
            try
            {
                OpenConnection();

                // Создаем SQL запрос для вставки реквизитов предприятия
                string insertRequisitesQuery = "INSERT INTO public.requisites (tin, kpp, oprn, bic, checkingaccount, corporateaccount, bank, telephone, address) VALUES (@Tin, @Kpp, @Oprn, @Bic, @CheckingAccount, @CorporateAccount, @Bank, @Telephone, @Address) RETURNING id";
                NpgsqlCommand insertRequisitesCommand = new NpgsqlCommand(insertRequisitesQuery, connection);
                insertRequisitesCommand.Parameters.AddWithValue("@Tin", tin);
                insertRequisitesCommand.Parameters.AddWithValue("@Kpp", kpp);
                insertRequisitesCommand.Parameters.AddWithValue("@Oprn", oprn);
                insertRequisitesCommand.Parameters.AddWithValue("@Bic", bic);
                insertRequisitesCommand.Parameters.AddWithValue("@CheckingAccount", checkingAccount);
                insertRequisitesCommand.Parameters.AddWithValue("@CorporateAccount", corporateAccount);
                insertRequisitesCommand.Parameters.AddWithValue("@Bank", bank);
                insertRequisitesCommand.Parameters.AddWithValue("@Telephone", telephone);
                insertRequisitesCommand.Parameters.AddWithValue("@Address", address);

                // Получаем сгенерированный id из последней вставленной записи в таблице "requisites"
                int requisitesId = Convert.ToInt32(insertRequisitesCommand.ExecuteScalar());

                // Создаем SQL запрос для вставки предприятия
                string insertEnterpriseQuery = "INSERT INTO public.enterprises (companyname, fullnameofthehead, requisites, jobtitle) VALUES (@CompanyName, @FullNameOfTheHead, @Requisites, @JobTitle) RETURNING id";
                NpgsqlCommand insertEnterpriseCommand = new NpgsqlCommand(insertEnterpriseQuery, connection);
                insertEnterpriseCommand.Parameters.AddWithValue("@CompanyName", companyName);
                insertEnterpriseCommand.Parameters.AddWithValue("@FullNameOfTheHead", fullNameOfTheHead);
                insertEnterpriseCommand.Parameters.AddWithValue("@Requisites", requisitesId); // Используем полученный requisitesId
                insertEnterpriseCommand.Parameters.AddWithValue("@JobTitle", jobTitle); // Добавляем должность


                // Получаем сгенерированный id из последней вставленной записи в таблице "enterprises"
                int enterpriseId = Convert.ToInt32(insertEnterpriseCommand.ExecuteScalar());

                // Создаем SQL запрос для вставки записи в groupenterprises
                string insertGroupEnterprisesQuery = "INSERT INTO public.groupenterprises (group_id, enterprise_id) VALUES (@GroupId, @EnterpriseId)";
                NpgsqlCommand insertGroupEnterprisesCommand = new NpgsqlCommand(insertGroupEnterprisesQuery, connection);
                insertGroupEnterprisesCommand.Parameters.AddWithValue("@GroupId", groupId);
                insertGroupEnterprisesCommand.Parameters.AddWithValue("@EnterpriseId", enterpriseId);

                // Выполняем запрос для вставки записи в groupenterprises
                insertGroupEnterprisesCommand.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении предприятия, реквизитов и связи с группой: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }




        // Получаем данные студента и его место прохождение практики и руководителя 
        /*public DataTable GetStudentPracticeData(List<Student> students)
        {
            DataTable dataTable = new DataTable();

            try
            {
                OpenConnection();

                string query = @"
            SELECT
                s.surname AS student_surname,
                s.name AS student_name,
                s.patronymic AS student_patronymic,
                p.placeofpractice,
                p.headofpractice
            FROM
                students s
            JOIN
                practice p ON s.id = p.student
            WHERE
                s.id IN (" + string.Join(",", students.Select(s => s.Id)) + ")";

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, connection))
                {
                    adapter.Fill(dataTable);
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при получении данных о практике студентов: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return dataTable;
        }*/

        public List<Student> GetStudentsByGroupOpen(string groupName)
        {
            List<Student> studentsList = new List<Student>();

            try
            {
                // Выполняем запрос к базе данных для получения списка студентов определенной группы
                string query = "SELECT s.id, s.SurName, s.Name, s.Patronymic " +
                               "FROM Students s INNER JOIN groups g ON s.groupid = g.id " +
                               "WHERE g.groupid = @GroupName";
                NpgsqlCommand command = new NpgsqlCommand(query, DatabaseContext.GetContext().connection);
                command.Parameters.AddWithValue("@GroupName", groupName);
                NpgsqlDataReader reader = command.ExecuteReader();

                // Читаем результат запроса
                while (reader.Read())
                {
                    Student student = new Student
                    {
                        Id = reader.GetInt32(0),
                        SurName = reader.GetString(1),
                        Name = reader.GetString(2),
                        Patronymic = reader.GetString(3),
                    };

                    studentsList.Add(student);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении списка студентов: " + ex.Message);
            }

            return studentsList;
        }


        public int GetEnterpriseIdByPlace(string placeOfPractice)
        {
            int enterpriseId = -1; // Инициализируем ID предприятия значением по умолчанию

            try
            {
                DatabaseContext.GetContext().OpenConnection(); // Открываем соединение

                // Создаем SQL-запрос для получения ID предприятия по названию места практики
                string query = "SELECT id FROM public.enterprises WHERE companyname = @CompanyName";

                using (NpgsqlCommand command = new NpgsqlCommand(query, DatabaseContext.GetContext().connection))
                {
                    command.Parameters.AddWithValue("@CompanyName", placeOfPractice);

                    // Выполняем запрос и получаем результат
                    object result = command.ExecuteScalar();

                    // Если результат не равен null, преобразуем его в int
                    if (result != null)
                    {
                        enterpriseId = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении ID предприятия: " + ex.Message);
            }
            finally
            {
                DatabaseContext.GetContext().CloseConnection(); // Закрываем соединение
            }

            return enterpriseId;
        }



        private int InsertPracticeChecklist(DatabaseContext dbContext)
        {
            int checklistId = -1; // Инициализируем ID чеклиста значением по умолчанию

            try
            {
                // Создание SQL-запроса для вставки записи чек-листа
                string insertChecklistQuery = "INSERT INTO public.practicecheck (listokpk, questionnaire, diary, agreement, review, protection, report, grade) " +
                                               "VALUES (@Listokpk, @Questionnaire, @Diary, @Agreement, @Review, @Protection, @Report, @Grade) RETURNING id";

                // Создание команды с параметрами для чек-листа
                using (NpgsqlCommand checklistCommand = new NpgsqlCommand(insertChecklistQuery, dbContext.connection))
                {
                    // Установка значений параметров чек-листа
                    checklistCommand.Parameters.AddWithValue("@Listokpk", false);
                    checklistCommand.Parameters.AddWithValue("@Questionnaire", false);
                    checklistCommand.Parameters.AddWithValue("@Diary", false);
                    checklistCommand.Parameters.AddWithValue("@Agreement", false);
                    checklistCommand.Parameters.AddWithValue("@Review", false);
                    checklistCommand.Parameters.AddWithValue("@Protection", false);
                    checklistCommand.Parameters.AddWithValue("@Report", false);
                    checklistCommand.Parameters.AddWithValue("@Grade", DBNull.Value); // Оценка еще не установлена

                    // Выполнение запроса и получение ID чеклиста
                    checklistId = (int)checklistCommand.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записи чек-листа: " + ex.Message);
            }

            return checklistId;
        }

        public int GetModuleIdByName(string moduleName)
        {
            int moduleId = -1; // Инициализируем ID модуля значением по умолчанию

            try
            {
                // Создаем SQL-запрос для получения ID модуля по его названию
                string query = "SELECT id FROM public.modules WHERE module = @ModuleName";
                NpgsqlCommand command = new NpgsqlCommand(query, DatabaseContext.GetContext().connection);
                command.Parameters.AddWithValue("@ModuleName", moduleName);

                // Выполняем запрос и получаем результат
                object result = command.ExecuteScalar();

                // Если результат не равен null, преобразуем его в int
                if (result != null)
                {
                    moduleId = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении ID модуля: " + ex.Message);
            }

            return moduleId;
        }


        /*public void AddPracticeForAllStudents(string groupName, DateTime startOfPractice, DateTime endOfPractice, List<string> modules, string typeOfPractice, int numberOfHours)
        {
            try
            {
                DatabaseContext dbContext = DatabaseContext.GetContext();
                dbContext.OpenConnection();

                List<Student> studentsInGroup = GetStudentsByGroupOpen(groupName); // Получить список студентов в текущей группе

                foreach (var student in studentsInGroup)
                {
                    // Создаем уникальный Checklist для каждого студента
                    int checklistId = InsertPracticeChecklist(dbContext);

                    foreach (string moduleId in modules)
                    {
                        // Создание SQL-запроса для вставки записи о практике
                        string insertPracticeQuery = "INSERT INTO public.practice (startofpractice, endofpractice, nameofpractice, student, placeofpractice, headofpractice, practicecompleted, module, typeofpractice, checklist, numberofhours) " +
                                                     "VALUES (@StartOfPractice, @EndOfPractice, @NameOfPractice, @Student, @PlaceOfPractice, @HeadOfPractice, @PracticeCompleted, @Module, @TypeOfPractice, @Checklist, @NumberOfHours)";

                        // Создание команды с параметрами
                        using (NpgsqlCommand command = new NpgsqlCommand(insertPracticeQuery, dbContext.connection))
                        {
                            // Установка параметров
                            command.Parameters.AddWithValue("@StartOfPractice", startOfPractice);
                            command.Parameters.AddWithValue("@EndOfPractice", endOfPractice);
                            command.Parameters.AddWithValue("@NameOfPractice", "Практика студента " + student.SurName + " " + student.Name + " " + student.Patronymic); // Пример имени практики
                            command.Parameters.AddWithValue("@PlaceOfPractice", DBNull.Value);
                            command.Parameters.AddWithValue("@HeadOfPractice", DBNull.Value);
                            command.Parameters.AddWithValue("@PracticeCompleted", false); // Практика еще не завершена
                            command.Parameters.AddWithValue("@TypeOfPractice", typeOfPractice); // Тип практики
                            command.Parameters.AddWithValue("@Student", student.Id);
                            command.Parameters.AddWithValue("@Module", GetModuleIdByName(moduleId)); // ID модуля
                            command.Parameters.AddWithValue("@Checklist", checklistId); // Уникальный checklist для студента
                            command.Parameters.AddWithValue("@NumberOfHours", numberOfHours); // Количество часов практики

                            // Добавление записи о практике
                            command.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Записи о практике для всех студентов добавлены успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записей о практике для всех студентов: " + ex.Message);
            }
            finally
            {
                DatabaseContext.GetContext().CloseConnection();
            }
        }*/

        public string GetEnterpriseNameById(int enterpriseId)
        {
            try
            {
                DatabaseContext dbContext = DatabaseContext.GetContext();

                string query = "SELECT companyname FROM public.enterprises WHERE id = @EnterpriseId";
                using (NpgsqlCommand command = new NpgsqlCommand(query, dbContext.connection))
                {
                    command.Parameters.AddWithValue("@EnterpriseId", enterpriseId);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        // В случае если предприятие с указанным ID не найдено
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                throw new Exception("Ошибка при получении названия предприятия: " + ex.Message);
            }
        }

        public string GetHeadOfPracticeByEnterpriseId(int enterpriseId)
        {
            try
            {
                DatabaseContext dbContext = DatabaseContext.GetContext();

                string query = "SELECT fullnameofthehead FROM public.enterprises WHERE id = @EnterpriseId";
                using (NpgsqlCommand command = new NpgsqlCommand(query, dbContext.connection))
                {
                    command.Parameters.AddWithValue("@EnterpriseId", enterpriseId);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        return result.ToString();
                    }
                    else
                    {
                        // В случае если предприятие с указанным ID не найдено
                        return string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                throw new Exception("Ошибка при получении руководителя практики: " + ex.Message);
            }
        }



        public void AddPracticeForAllStudents(string groupName, DateTime startOfPractice, DateTime endOfPractice, List<string> modules, string typeOfPractice, int numberOfHours)
        {
            try
            {
                DatabaseContext dbContext = DatabaseContext.GetContext();
                dbContext.OpenConnection();

                List<Student> studentsInGroup = GetStudentsByGroupOpen(groupName); // Получить список студентов в текущей группе

                foreach (var student in studentsInGroup)
                {
                    // Создаем уникальный Checklist для каждого студента
                    int checklistId = InsertPracticeChecklist(dbContext);

                    foreach (string moduleId in modules)
                    {
                        // Получаем ID студента
                        int studentId = student.Id;

                        // Проверяем, есть ли студент в таблице studentworking
                        int enterpriseId = -1; // По умолчанию устанавливаем -1, если студента нет в таблице studentworking

                        string checkStudentWorkingQuery = "SELECT enterprise_id FROM public.studentworking WHERE student_id = @StudentId";
                        using (NpgsqlCommand command = new NpgsqlCommand(checkStudentWorkingQuery, dbContext.connection))
                        {
                            command.Parameters.AddWithValue("@StudentId", studentId);
                            object result = command.ExecuteScalar();
                            if (result != null)
                            {
                                // Если студент найден в таблице studentworking, получаем enterprise_id
                                enterpriseId = Convert.ToInt32(result);
                            }
                        }

                        // Создание SQL-запроса для вставки записи о практике
                        string insertPracticeQuery = "INSERT INTO public.practice (startofpractice, endofpractice, nameofpractice, student, placeofpractice, enterprises, headofpractice, practicecompleted, module, typeofpractice, checklist, numberofhours) " +
                                                     "VALUES (@StartOfPractice, @EndOfPractice, @NameOfPractice, @Student, @PlaceOfPractice, @Enterprises, @HeadOfPractice, @PracticeCompleted, @Module, @TypeOfPractice, @Checklist, @NumberOfHours)";

                        // Создание команды с параметрами
                        using (NpgsqlCommand command = new NpgsqlCommand(insertPracticeQuery, dbContext.connection))
                        {
                            // Установка параметров
                            command.Parameters.AddWithValue("@StartOfPractice", startOfPractice);
                            command.Parameters.AddWithValue("@EndOfPractice", endOfPractice);
                            command.Parameters.AddWithValue("@NameOfPractice", "Практика студента " + student.SurName + " " + student.Name + " " + student.Patronymic); // Пример имени практики
                            if (enterpriseId != -1)
                            {
                                // Если студент найден в таблице studentworking, используем данные из нее
                                command.Parameters.AddWithValue("@PlaceOfPractice", GetEnterpriseNameById(enterpriseId)); // Название предприятия по его ID
                                command.Parameters.AddWithValue("@Enterprises", enterpriseId); // Используем enterprise_id
                                command.Parameters.AddWithValue("@HeadOfPractice", GetHeadOfPracticeByEnterpriseId(enterpriseId)); // Руководитель практики из таблицы предприятий
                            }
                            else
                            {
                                // Если студент не найден в таблице studentworking, оставляем поля пустыми
                                command.Parameters.AddWithValue("@PlaceOfPractice", DBNull.Value);
                                command.Parameters.AddWithValue("@Enterprises", DBNull.Value);
                                command.Parameters.AddWithValue("@HeadOfPractice", DBNull.Value);
                            }
                            command.Parameters.AddWithValue("@PracticeCompleted", false); // Практика еще не завершена
                            command.Parameters.AddWithValue("@TypeOfPractice", typeOfPractice); // Тип практики
                            command.Parameters.AddWithValue("@Student", studentId);
                            command.Parameters.AddWithValue("@Module", GetModuleIdByName(moduleId)); // ID модуля
                            command.Parameters.AddWithValue("@Checklist", checklistId); // Уникальный checklist для студента
                            command.Parameters.AddWithValue("@NumberOfHours", numberOfHours); // Количество часов практики

                            // Добавление записи о практике
                            command.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Записи о практике для всех студентов добавлены успешно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записей о практике для всех студентов: " + ex.Message);
            }
            finally
            {
                DatabaseContext.GetContext().CloseConnection();
            }
        }





        public List<StudentPractik> GetPracticeDetailsByGroupName(string groupName)
        {
            List<StudentPractik> practiceDetails = new List<StudentPractik>();

            try
            {
                DatabaseContext.GetContext().OpenConnection();

                string sql = @"
                SELECT 
                    p.id AS practiceId,
                    s.id AS studentId,
                    s.surname || ' ' || s.name || ' ' || COALESCE(s.patronymic, '') AS full_name,
                    p.placeofpractice,
                    p.headofpractice,
                    p.checklist AS checklistId -- Включение ID чек-листа
                FROM 
                    public.students s
                JOIN 
                    public.practice p ON s.id = p.student
                JOIN 
                    public.groups g ON s.groupid = g.id
                WHERE 
                    g.groupid = @groupName";

                using (var cmd = new NpgsqlCommand(sql, DatabaseContext.GetContext().connection))
                {
                    cmd.Parameters.AddWithValue("groupName", groupName);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int practiceId = Convert.ToInt32(reader["practiceId"]);
                            int studentId = Convert.ToInt32(reader["studentId"]);
                            string fullName = reader["full_name"].ToString();
                            string placeOfPractice = reader["placeofpractice"].ToString();
                            string headOfPractice = reader["headofpractice"].ToString();
                            int checklistId = Convert.ToInt32(reader["checklistId"]); // Получение ID чек-листа

                            // Создаем объект StudentPractik и добавляем его в список
                            practiceDetails.Add(new StudentPractik
                            {
                                Id = practiceId,
                                StudentId = studentId,
                                Student = fullName,
                                Placeofpractice = placeOfPractice,
                                Headofpractice = headOfPractice,
                                CheckListId = checklistId // Установка ID чек-листа
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка выполнения запроса: " + ex.Message);
            }
            finally
            {
                DatabaseContext.GetContext().CloseConnection();
            }

            return practiceDetails;
        }





        public void UpdatePracticeForStudent(int studentId, string placeOfPractice, int enterprisesId, string headOfPractice)
        {
            string sql = "UPDATE public.practice SET placeofpractice = @PlaceOfPractice, enterprises = @EnterprisesId, headofpractice = @HeadOfPractice WHERE student = @StudentId";

            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    DatabaseContext.GetContext().OpenConnection(); // Открываем соединение только если оно закрыто
                }

                using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@PlaceOfPractice", placeOfPractice);
                    command.Parameters.AddWithValue("@EnterprisesId", enterprisesId);
                    command.Parameters.AddWithValue("@HeadOfPractice", headOfPractice);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Информация о практике студента успешно обновлена.");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось обновить информацию о практике студента.");
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка при выполнении SQL-запроса: " + ex.Message);
            }
            finally
            {
                DatabaseContext.GetContext().CloseConnection(); // Закрываем соединение с базой данных после выполнения запроса
            }
        }


        public ChecklistData GetChecklistData(int checklistId)
        {
            if (checklistId <= 0)
            {
                throw new ArgumentException("Неверный ID чек-листа.", nameof(checklistId));
            }

            OpenConnection();

            try
            {
                string sql = "SELECT * FROM practicecheck WHERE id = @Id";
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("Id", checklistId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ChecklistData checklistData = new ChecklistData
                            {
                                Listokpk = reader.GetBoolean(reader.GetOrdinal("listokpk")),
                                Questionnaire = reader.GetBoolean(reader.GetOrdinal("questionnaire")),
                                Diary = reader.GetBoolean(reader.GetOrdinal("diary")),
                                Agreement = reader.GetBoolean(reader.GetOrdinal("agreement")),
                                Review = reader.GetBoolean(reader.GetOrdinal("review")),
                                Protection = reader.GetBoolean(reader.GetOrdinal("protection")),
                                Report = reader.GetBoolean(reader.GetOrdinal("report"))
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("grade")))
                            {
                                checklistData.Grade = reader.GetInt32(reader.GetOrdinal("grade"));
                            }

                            return checklistData;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
        }


        public void SaveChecklistData(ChecklistData checklistData)
        {
            OpenConnection();

            try
            {
                string sql = @"
            INSERT INTO practicecheck (Id, Listokpk, Questionnaire, Diary, Agreement, Review, Protection, Report, Grade)
            VALUES (@Id, @Listokpk, @Questionnaire, @Diary, @Agreement, @Review, @Protection, @Report, @Grade)
            ON CONFLICT (Id) DO UPDATE
            SET Listokpk = EXCLUDED.Listokpk,
                Questionnaire = EXCLUDED.Questionnaire,
                Diary = EXCLUDED.Diary,
                Agreement = EXCLUDED.Agreement,
                Review = EXCLUDED.Review,
                Protection = EXCLUDED.Protection,
                Report = EXCLUDED.Report,
                Grade = EXCLUDED.Grade";

                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("Id", checklistData.Id); // Предполагается, что у вас есть свойство Id в классе ChecklistData
                    cmd.Parameters.AddWithValue("Listokpk", checklistData.Listokpk);
                    cmd.Parameters.AddWithValue("Questionnaire", checklistData.Questionnaire);
                    cmd.Parameters.AddWithValue("Diary", checklistData.Diary);
                    cmd.Parameters.AddWithValue("Agreement", checklistData.Agreement);
                    cmd.Parameters.AddWithValue("Review", checklistData.Review);
                    cmd.Parameters.AddWithValue("Protection", checklistData.Protection);
                    cmd.Parameters.AddWithValue("Report", checklistData.Report);
                    cmd.Parameters.AddWithValue("Grade", checklistData.Grade);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }

        public List<PracticeData> GetPracticeDataByGroup(string groupId)
        {
            List<PracticeData> practiceDataList = new List<PracticeData>();

            OpenConnection();

            try
            {
                string sql = @"
SELECT
    p.id,
    s.surname || ' ' || s.name || ' ' || s.patronymic AS fullname,
    p.placeofpractice,
    p.headofpractice,
    p.numberOfHours -- Включение столбца numberOfHours
FROM
    public.practice p
JOIN
    public.students s ON p.student = s.id
JOIN
    public.groups g ON s.groupid = g.id
WHERE
    g.groupid = @GroupId";


                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("GroupId", groupId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PracticeData practiceData = new PracticeData
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                FullNameStudent = reader.GetString(reader.GetOrdinal("fullname")),
                                PlaceOfPractice = reader.IsDBNull(reader.GetOrdinal("placeofpractice")) ? null : reader.GetString(reader.GetOrdinal("placeofpractice")),
                                HeadOfPractice = reader.IsDBNull(reader.GetOrdinal("headofpractice")) ? null : reader.GetString(reader.GetOrdinal("headofpractice")),
                                NumberOfHours = reader.GetInt32(reader.GetOrdinal("numberOfHours")) // Получение значения столбца numberOfHours
                            };
                            practiceDataList.Add(practiceData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return practiceDataList;
        }



        public PracticePeriod GetPracticePeriodByGroup(string groupId)
        {
            PracticePeriod practicePeriod = null;

            OpenConnection();

            try
            {
                string sql = @"
SELECT
    p.startofpractice,
    p.endofpractice
FROM
    public.practice p
JOIN
    public.students s ON p.student = s.id
JOIN
    public.groups g ON s.groupid = g.id
WHERE
    g.groupid = @GroupId
LIMIT 1";  // Ограничиваем результат одной строкой

                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("GroupId", groupId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            practicePeriod = new PracticePeriod
                            {
                                StartOfPractice = reader.GetDateTime(reader.GetOrdinal("startofpractice")),
                                EndOfPractice = reader.GetDateTime(reader.GetOrdinal("endofpractice"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return practicePeriod;
        }




        public List<PracticeModule> GetPracticeModulesByGroup(string groupId)
        {
            List<PracticeModule> practiceModules = new List<PracticeModule>();

            OpenConnection();

            try
            {
                string sql = @"
SELECT DISTINCT
    p.module AS ModuleId,
    m.module AS ModuleName
FROM
    public.practice p
JOIN
    public.students s ON p.student = s.id
JOIN
    public.groups g ON s.groupid = g.id
JOIN
    public.modules m ON p.module = m.id
WHERE
    g.groupid = @GroupId
ORDER BY p.module";  // Упорядочиваем по модулю

                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("GroupId", groupId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PracticeModule practiceModule = new PracticeModule
                            {
                                ModuleId = reader.GetInt32(reader.GetOrdinal("ModuleId")),
                                ModuleName = reader.GetString(reader.GetOrdinal("ModuleName"))
                            };
                            practiceModules.Add(practiceModule);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return practiceModules;
        }


        public List<string> GetAllModules()
        {
            List<string> modules = new List<string>();

            OpenConnection();

            try
            {
                string sql = "SELECT module FROM public.modules";

                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            modules.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return modules;
        }

        public string GetTeacherByGroupName(string groupName)
        {
            string teacherName = null;

            try
            {
                OpenConnection();

                string query = "SELECT CONCAT(SPLIT_PART(t.full_name, ' ', 1), ' ', " +
                               "LEFT(SPLIT_PART(t.full_name, ' ', 2), 1), '.', " +
                               "LEFT(SPLIT_PART(t.full_name, ' ', 3), 1), '.') AS teacher_name " +
                               "FROM groups g " +
                               "JOIN teachers t ON g.teacher_id = t.id " +
                               "WHERE g.groupid = @GroupName;";

                using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@GroupName", groupName);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            teacherName = reader.GetString(0);
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка выполнения запроса: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }

            return teacherName;
        }

        public void AddStudentWorking(int studentId, int enterpriseId)
        {
            try
            {
                OpenConnection();

                // Создаем SQL запрос для вставки записи в studentWorking
                string insertStudentWorkingQuery = "INSERT INTO public.studentWorking (student_id, enterprise_id) VALUES (@StudentId, @EnterpriseId)";
                NpgsqlCommand insertStudentWorkingCommand = new NpgsqlCommand(insertStudentWorkingQuery, connection);
                insertStudentWorkingCommand.Parameters.AddWithValue("@StudentId", studentId);
                insertStudentWorkingCommand.Parameters.AddWithValue("@EnterpriseId", enterpriseId);

                // Выполняем запрос для вставки записи в studentWorking
                insertStudentWorkingCommand.ExecuteNonQuery();

                // Обновляем поле works_there студента на true
                string updateStudentQuery = "UPDATE public.students SET works_there = true WHERE id = @StudentId";
                NpgsqlCommand updateStudentCommand = new NpgsqlCommand(updateStudentQuery, connection);
                updateStudentCommand.Parameters.AddWithValue("@StudentId", studentId);
                updateStudentCommand.ExecuteNonQuery();

                MessageBox.Show("Запись в studentWorking успешно добавлена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении записи в studentWorking: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }


        public List<GroupEnterprise> GetEnterprisesByGroupId(string groupId)
        {
            List<GroupEnterprise> groupEnterprises = new List<GroupEnterprise>();

            string query = @"SELECT ge.id, ge.group_id, ge.enterprise_id, e.companyname, e.fullnameofthehead
                     FROM public.groupenterprises ge
                     JOIN public.enterprises e ON ge.enterprise_id = e.id
                     JOIN public.groups g ON ge.group_id = g.id
                     WHERE g.groupid = @GroupId";

            DatabaseContext dbContext = DatabaseContext.GetContext();

            try
            {
                dbContext.OpenConnection();

                using (NpgsqlCommand command = new NpgsqlCommand(query, dbContext.connection))
                {
                    command.Parameters.AddWithValue("@GroupId", groupId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            groupEnterprises.Add(new GroupEnterprise
                            {
                                Id = reader.GetInt32(0),
                                GroupId = reader.GetInt32(1),
                                EnterpriseId = reader.GetInt32(2),
                                EnterpriseName = reader.GetString(3),
                                HeadOfEnterprise = reader.GetString(4)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении данных: " + ex.Message);
            }
            finally
            {
                dbContext.CloseConnection();
            }

            return groupEnterprises;
        }

        public List<StudentEnterpriseDetails> GetStudentEnterpriseDetails(string selectedGroup)
        {
            List<StudentEnterpriseDetails> details = new List<StudentEnterpriseDetails>();

            try
            {
                DatabaseContext.GetContext().OpenConnection();

                string sql = @"
SELECT 
    s.id,
    s.surname,
    s.name,
    s.patronymic,
    e.companyname,
    e.fullnameofthehead
FROM 
    public.students s
JOIN 
    public.studentworking sw ON s.id = sw.student_id
JOIN 
    public.enterprises e ON sw.enterprise_id = e.id
JOIN 
    public.groups g ON s.groupid = g.id
WHERE 
    g.groupid = @selectedGroup";

                using (var cmd = new NpgsqlCommand(sql, DatabaseContext.GetContext().connection))
                {
                    cmd.Parameters.AddWithValue("@selectedGroup", selectedGroup);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["id"]);
                            string fullName = $"{reader["surname"]} {reader["name"]} {reader["patronymic"]}";
                            string companyName = reader["companyname"].ToString();
                            string fullnameOfTheHead = reader["fullnameofthehead"].ToString();

                            details.Add(new StudentEnterpriseDetails
                            {
                                Id = id,
                                FullName = fullName,
                                CompanyName = companyName,
                                FullnameOfTheHead = fullnameOfTheHead
                            });
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка выполнения запроса: " + ex.Message);
            }
            finally
            {
                DatabaseContext.GetContext().CloseConnection();
            }

            return details;
        }

        public void DeleteStudentWorking(List<int> studentIds)
        {
            try
            {
                OpenConnection();

                // Создаем SQL-запрос для удаления записей из таблицы studentWordking
                string sql = "DELETE FROM studentworking WHERE student_id = ANY(@studentIds)";

                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    // Добавляем параметр для списка идентификаторов студентов
                    cmd.Parameters.AddWithValue("@studentIds", studentIds.ToArray());

                    // Выполняем запрос
                    cmd.ExecuteNonQuery();
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show("Ошибка выполнения запроса: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
        }















    }
}
