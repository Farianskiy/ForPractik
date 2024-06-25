using ForPractik.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ForPractik.ViewModel
{
    internal class HttpClient
    {
        private readonly System.Net.Http.HttpClient _client;
        public string Token { get; private set; }

        public HttpClient(string token = null)
        {
            _client = new System.Net.Http.HttpClient();
        }

        // Метод для получения токена
        public async Task<string> GetTokenAsync(string username, string password)
        {
            try
            {
                var loginDto = new LoginDTO
                {
                    Username = username,
                    Password = password
                };

                string jsonString = JsonSerializer.Serialize(loginDto);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("URL_FOR_GET_TOKEN", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var tokenResponse = JsonSerializer.Deserialize<TokenResponseDTO>(responseBody);
                return tokenResponse?.Jwt;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public void SetToken(string token)
        {
            Token = token;
        }

        private void AddAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(Token))
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);
            }
        }

        // Метод для получения id группы по имени
        public async Task<int?> GetGroupIdAsync(string groupName)
        {
            try
            {
                AddAuthorizationHeader();

                var groupSearchQuery = new GroupSearchQueryDTO
                {
                    GroupName = groupName,
                    OnlyActive = true
                };

                string jsonString = JsonSerializer.Serialize(groupSearchQuery);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("URL_FOR_GROUP_SEARCH", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var group = JsonSerializer.Deserialize<GroupDTO>(responseBody);
                return group?.Id;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        // Метод для получения студентов по id группы и дате
        public async Task<List<Student>> GetStudentsAsync(int groupId, string date)
        {
            try
            {
                AddAuthorizationHeader();

                var groupHistoryQuery = new GroupHistoryQueryDTO
                {
                    Id = groupId,
                    OnDate = date
                };

                string jsonString = JsonSerializer.Serialize(groupHistoryQuery);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("URL_FOR_STUDENT_SEARCH", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                List<Student> students = DeserializeStudents(responseBody);
                return students;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public static List<Student> DeserializeStudents(string responseBody)
        {
            try
            {
                List<Student> students = JsonSerializer.Deserialize<List<Student>>(responseBody);
                return students;
            }
            catch (JsonException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }
    }

    // DTO для запроса группы
    public class GroupSearchQueryDTO
    {
        public GroupSearchQueryDTO()
        {
            GroupName = null;
        }

        [JsonRequired]
        public string GroupName { get; set; }
        public bool OnlyActive { get; set; }
    }

    // DTO для ответа с id группы
    public class GroupDTO
    {
        public int Id { get; set; }
    }

    // DTO для запроса студентов по истории группы
    public class GroupHistoryQueryDTO
    {
        public GroupHistoryQueryDTO()
        {
            OnDate = "";
        }

        [JsonRequired]
        public int Id { get; set; }
        [JsonRequired]
        public string OnDate { get; set; }
    }

    // DTO для запроса логина
    public class LoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    // DTO для ответа с токеном
    public class TokenResponseDTO
    {
        public string Jwt { get; set; }
    }
}
