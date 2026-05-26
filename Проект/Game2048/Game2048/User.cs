using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Play2048.Games2048;
using static Play2048.AdminPanel;
using static Play2048.Authorization;
using System.Text.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace Play2048
{
    [Serializable]
    public class User
    {
        public static List<User> Users { get; set; } = new List<User>();

        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public uint HighScore { get; set; } = 0;
        public string Role { get; set; } = "user";
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLogin { get; set; }

        public User(string login, string password, string role = "user")
        {
            Login = login;
            Password = password;
            HighScore = 0;
            Role = role;
            RegistrationDate = DateTime.Now;
            LastLogin = DateTime.Now;
        }

        public User() { }

        // ========== ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ПУТИ ==========
        private static string GetFilePath(string filename)
        {
            // Получаем путь к исполняемому файлу
            string exePath = Application.StartupPath;

            // Проверяем, что путь существует
            if (string.IsNullOrEmpty(exePath))
            {
                exePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            // Создаем полный путь к файлу
            string fullPath = Path.Combine(exePath, filename);

            // Создаем директорию, если её нет
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return fullPath;
        }

        // ========== ОСНОВНЫЕ МЕТОДЫ (БИНАРНАЯ СЕРИАЛИЗАЦИЯ) ==========

        public string Save(string filename)
        {
            try
            {
                LoadAll(filename);

                int existingIndex = Users.FindIndex(u => u.Login == this.Login);

                if (existingIndex >= 0)
                {
                    Users[existingIndex] = this;
                }
                else
                {
                    Users.Add(this);
                }

                SaveAll(filename);
                return "Сохранено!";
            }
            catch (Exception ex)
            {
                return $"Ошибка сохранения: {ex.Message}";
            }
        }

        private static void SaveAll(string filename)
        {
            try
            {
                string fullPath = GetFilePath(filename);

                using (FileStream fs = new FileStream(fullPath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fs, Users);
                }
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Нет прав на запись в файл. Запустите программу от имени администратора.");
            }
            catch (DirectoryNotFoundException)
            {
                throw new Exception("Директория не найдена.");
            }
            catch (IOException ex)
            {
                throw new Exception($"Ошибка ввода/вывода: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения файла: {ex.Message}");
            }
        }

        public static int LoadAll(string filename)
        {
            try
            {
                string fullPath = GetFilePath(filename);

                if (!File.Exists(fullPath))
                {
                    Users = new List<User>();
                    return 0;
                }

                using (FileStream fs = new FileStream(fullPath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    Users = (List<User>)formatter.Deserialize(fs);
                }

                return Users.Count;
            }
            catch (FileNotFoundException)
            {
                Users = new List<User>();
                return 0;
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Нет прав на чтение файла.");
            }
            catch (IOException ex)
            {
                throw new Exception($"Ошибка ввода/вывода: {ex.Message}");
            }
            catch (Exception)
            {
                // Если произошла ошибка при десериализации
                Users = new List<User>();

                try
                {
                    string fullPath = GetFilePath(filename);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                catch { }

                return 0;
            }
        }

        // ========== МЕТОДЫ ДЛЯ АВТОРИЗАЦИИ ==========

        public static User FindUser(string filename, string login)
        {
            try
            {
                LoadAll(filename);
                return Users.FirstOrDefault(u => u.Login == login);
            }
            catch
            {
                return null;
            }
        }

        public static User Authenticate(string filename, string login, string password)
        {
            try
            {
                LoadAll(filename);
                var user = Users.FirstOrDefault(u => u.Login == login);

                if (user != null && user.Password == password)
                {
                    user.LastLogin = DateTime.Now;
                    user.Save(filename);
                    return user;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool UserExists(string filename, string login)
        {
            try
            {
                LoadAll(filename);
                return Users.Any(u => u.Login == login);
            }
            catch
            {
                return false;
            }
        }

        public static void CreateDefaultAdmin(string filename)
        {
            try
            {
                LoadAll(filename);
                if (!Users.Any(u => u.Login == "admin"))
                {
                    User admin = new User("admin", "admin", "admin");
                    admin.Save(filename);
                }
            }
            catch
            {
                // Если ошибка, создаем новый список с админом
                Users = new List<User>();
                User admin = new User("admin", "admin", "admin");
                Users.Add(admin);
                SaveAll(filename);
            }
        }

        // ========== МЕТОДЫ ДЛЯ АДМИН-ПАНЕЛИ ==========

        public static List<User> GetAllUsers(string filename)
        {
            try
            {
                LoadAll(filename);
                return Users.ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        public static string Delete(string filename, string login)
        {
            try
            {
                if (login == "admin") return "Нельзя удалить администратора!";

                LoadAll(filename);
                int countBefore = Users.Count;
                Users.RemoveAll(u => u.Login == login);

                if (Users.Count < countBefore)
                {
                    SaveAll(filename);
                    return "Пользователь удален!";
                }
                return "Пользователь не найден!";
            }
            catch (Exception ex)
            {
                return $"Ошибка удаления: {ex.Message}";
            }
        }

        public static string ClearDatabase(string filename)
        {
            try
            {
                LoadAll(filename);

                var admins = Users.Where(u => u.Role == "admin").ToList();
                Users = admins;
                SaveAll(filename);

                return $"База очищена! Сохранено {admins.Count} администраторов.";
            }
            catch (Exception ex)
            {
                return $"Ошибка очистки базы: {ex.Message}";
            }
        }

        public static List<User> GetTop10(string filename)
        {
            try
            {
                LoadAll(filename);
                return Users
                    .OrderByDescending(u => u.HighScore)
                    .Take(10)
                    .ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        // ========== МЕТОДЫ ДЛЯ ИГРЫ ==========

        public static bool UpdateHighScore(string filename, string login, uint newScore)
        {
            try
            {
                LoadAll(filename);
                var user = Users.FirstOrDefault(u => u.Login == login);

                if (user != null && newScore > user.HighScore)
                {
                    user.HighScore = newScore;
                    user.Save(filename);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static uint GetHighScore(string filename, string login)
        {
            try
            {
                LoadAll(filename);
                var user = Users.FirstOrDefault(u => u.Login == login);
                return user?.HighScore ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsAdmin(string filename, string login)
        {
            try
            {
                LoadAll(filename);
                var user = Users.FirstOrDefault(u => u.Login == login);
                return user?.Role == "admin";
            }
            catch
            {
                return false;
            }
        }

        public static uint GetTopScore(string filename)
        {
            try
            {
                LoadAll(filename);
                if (Users.Count == 0) return 0;

                return Users.Max(u => u.HighScore);
            }
            catch
            {
                return 0;
            }
        }

        // ========== ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ ==========

        public static int GetUserCount(string filename)
        {
            try
            {
                LoadAll(filename);
                return Users.Count;
            }
            catch
            {
                return 0;
            }
        }

        public static List<User> GetRegularUsers(string filename)
        {
            try
            {
                LoadAll(filename);
                return Users.Where(u => u.Role == "user").ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        public static List<User> GetAdminUsers(string filename)
        {
            try
            {
                LoadAll(filename);
                return Users.Where(u => u.Role == "admin").ToList();
            }
            catch
            {
                return new List<User>();
            }
        }

        public static string GetUserStats(string filename, string login)
        {
            try
            {
                var user = FindUser(filename, login);
                if (user == null) return "Пользователь не найден";

                return $"Логин: {user.Login}\n" +
                       $"Роль: {user.Role}\n" +
                       $"Рекорд: {user.HighScore}\n" +
                       $"Дата регистрации: {user.RegistrationDate:dd.MM.yyyy HH:mm}\n" +
                       $"Последний вход: {user.LastLogin:dd.MM.yyyy HH:mm}";
            }
            catch (Exception ex)
            {
                return $"Ошибка получения статистики: {ex.Message}";
            }
        }
    }


}


    

        