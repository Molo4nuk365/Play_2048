using Game2048;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;

namespace Play2048
{
    public partial class AdminPanel : Form
    {
        private const string UsersFileName = "users.dat";
        public AdminPanel()
        {
            InitializeComponent();
            SetupForm();
            CreateDefaultAdmin();
            LoadUsers();
        }
        private void SetupForm()
        {
            this.Text = "Администратор";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Настройка DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.MultiSelect = false;

            // Обработка Enter для поиска
            textBox1.KeyDown += (s, args) =>
            {
                if (args.KeyCode == Keys.Enter)
                {
                    button4_Click(s, args);
                    args.Handled = true;
                    args.SuppressKeyPress = true;
                }
            };
        }

        private void CreateDefaultAdmin()
        {
            User.CreateDefaultAdmin(UsersFileName);
        }

        private void LoadUsers()
        {
            try
            {
                // Отладочный вывод в консоль
                DebugLog("=== НАЧАЛО ЗАГРУЗКИ ПОЛЬЗОВАТЕЛЕЙ ===");

                dataGridView1.Rows.Clear();

                // Проверяем существование файла
                if (!System.IO.File.Exists(UsersFileName))
                {
                    DebugLog($"Файл {UsersFileName} не существует!");
                    MessageBox.Show($"Файл {UsersFileName} не найден. Создан новый.",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    SaveEmptyFile();
                }

                // Загружаем пользователей
                int userCount = User.LoadAll(UsersFileName);
                DebugLog($"Метод LoadAll вернул: {userCount}");

                // Проверяем список пользователей
                if (User.Users == null)
                {
                    DebugLog("User.Users IS NULL!");
                    MessageBox.Show("Ошибка: список пользователей равен null", "Ошибка");
                    return;
                }

                DebugLog($"User.Users.Count = {User.Users.Count}");

                // Добавляем колонки если их нет
                if (dataGridView1.Columns.Count == 0)
                {
                    dataGridView1.Columns.Add("Login", "Логин");
                    dataGridView1.Columns.Add("Role", "Роль");
                    dataGridView1.Columns.Add("HighScore", "Рекорд");
                    dataGridView1.Columns.Add("RegistrationDate", "Дата регистрации");
                    dataGridView1.Columns.Add("LastLogin", "Последний вход");
                }

                // Если пользователей нет
                if (User.Users.Count == 0)
                {
                    DebugLog("Нет пользователей в базе");
                    dataGridView1.Rows.Add("Нет пользователей", "", "", "", "");
                    return;
                }

                // Сортируем: сначала админы
                var sortedUsers = User.Users
                    .OrderByDescending(u => u.Role == "admin")
                    .ThenBy(u => u.Login)
                    .ToList();

                DebugLog($"Отсортировано пользователей: {sortedUsers.Count}");

                // Добавляем каждого пользователя
                foreach (var user in sortedUsers)
                {
                    DebugLog($"Добавляем пользователя: {user.Login}, роль: {user.Role}, рекорд: {user.HighScore}");

                    int rowIndex = dataGridView1.Rows.Add(
                        user.Login ?? "N/A",
                        user.Role ?? "user",
                        user.HighScore.ToString(),
                        user.RegistrationDate.ToString("dd.MM.yyyy"),
                        user.LastLogin.ToString("dd.MM.yyyy HH:mm")
                    );

                    // Подсвечиваем админов
                    if (user.Role == "admin")
                    {
                        dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                        dataGridView1.Rows[rowIndex].DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    }
                }

                DebugLog($"Добавлено строк в DataGridView: {dataGridView1.Rows.Count}");
                DebugLog("=== ЗАВЕРШЕНИЕ ЗАГРУЗКИ ПОЛЬЗОВАТЕЛЕЙ ===");
            }
            catch (Exception ex)
            {
                DebugLog($"ОШИБКА В LoadUsers: {ex.Message}");
                DebugLog($"StackTrace: {ex.StackTrace}");

                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveEmptyFile()
        {
            try
            {
                // Создаем пустой JSON файл
                System.IO.File.WriteAllText(UsersFileName, "[]");
                DebugLog($"Создан пустой файл: {UsersFileName}");
            }
            catch (Exception ex)
            {
                DebugLog($"Ошибка создания файла: {ex.Message}");
            }
        }

        private void DebugLog(string message)
        {
            // Вывод в консоль Visual Studio
            Console.WriteLine($"[AdminPanel] {DateTime.Now:HH:mm:ss} - {message}");

            // Также можно выводить в TextBox на форме для отладки
            // if (textBoxDebug != null) textBoxDebug.AppendText(message + Environment.NewLine);
        }



        private void AdminPanel_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DebugLog("Нажата кнопка: Обновить список");
            LoadUsers();
            MessageBox.Show("Список пользователей обновлен",
            "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Нажата кнопка Удалить");

            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите пользователя для удаления",
                    "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedRow = dataGridView1.SelectedRows[0];
            var loginCell = selectedRow.Cells["Login"];

            if (loginCell.Value == null || string.IsNullOrWhiteSpace(loginCell.Value.ToString()))
            {
                MessageBox.Show("Неверные данные пользователя",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string login = loginCell.Value.ToString();
            Console.WriteLine($"Пытаемся удалить пользователя: {login}");

            if (login.ToLower() == "admin")
            {
                MessageBox.Show("Нельзя удалить администратора по умолчанию!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{login}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Console.WriteLine($"Вызван User.Delete для {login}");
                string deleteResult = User.Delete(UsersFileName, login);
                Console.WriteLine($"Результат удаления: {deleteResult}");

                if (deleteResult.Contains("удален"))
                {
                    MessageBox.Show(deleteResult, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsers();
                }
                else
                {
                    MessageBox.Show(deleteResult, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    
        

        private void button4_Click(object sender, EventArgs e)
        {
            DebugLog("Нажата кнопка: Поиск");

            string searchText = textBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Введите логин для поиска",
                    "Поиск", MessageBoxButtons.OK, MessageBoxIcon.Information);
                textBox1.Focus();
                return;
            }

            DebugLog($"Ищем пользователя: {searchText}");

            // Показываем все строки сначала
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                    row.Visible = true;
            }

            bool found = false;
            DataGridViewRow foundRow = null;

            // Ищем пользователя
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow || row.Cells["Login"].Value == null)
                    continue;

                string login = row.Cells["Login"].Value.ToString();

                if (login.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    foundRow = row;
                    DebugLog($"Найден пользователь: {login}");
                    break;
                }
            }

            if (found && foundRow != null)
            {
                // Выделяем найденного пользователя
                foundRow.Selected = true;
                dataGridView1.FirstDisplayedScrollingRowIndex = foundRow.Index;

                // Скрываем остальных
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow && row != foundRow)
                    {
                        row.Visible = false;
                    }
                }

                // Показываем информацию
                var user = User.FindUser(UsersFileName, searchText);
                if (user != null)
                {
                    string info = $"Найден пользователь:\n\n" +
                                 $"Логин: {user.Login}\n" +
                                 $"Роль: {user.Role}\n" +
                                 $"Рекорд: {user.HighScore}\n" +
                                 $"Дата регистрации: {user.RegistrationDate:dd.MM.yyyy HH:mm}\n" +
                                 $"Последний вход: {user.LastLogin:dd.MM.yyyy HH:mm}";

                    MessageBox.Show(info, "Результат поиска",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                DebugLog($"Пользователь '{searchText}' не найден");
                MessageBox.Show($"Пользователь с логином '{searchText}' не найден.",
                    "Результат поиска", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Показываем всех пользователей обратно
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
            }
        }



        private void button3_Click(object sender, EventArgs e)
        {
            Form ifrm = new Form1();
            ifrm.Show();
            this.Hide();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                var row = dataGridView1.Rows[e.RowIndex];

                // Пропускаем строку "Нет пользователей"
                if (row.Cells[0].Value?.ToString() == "Нет пользователей")
                    return;

                var loginCell = row.Cells["Login"];

                if (loginCell.Value != null)
                {
                    string login = loginCell.Value.ToString();
                    var user = User.FindUser(UsersFileName, login);

                    if (user != null)
                    {
                        string info = $"Информация о пользователе:\n\n" +
                                     $"Логин: {user.Login}\n" +
                                     $"Роль: {user.Role}\n" +
                                     $"Рекорд: {user.HighScore}\n" +
                                     $"Дата регистрации: {user.RegistrationDate:dd.MM.yyyy HH:mm}\n" +
                                     $"Последний вход: {user.LastLogin:dd.MM.yyyy HH:mm}";

                        MessageBox.Show(info, "Детальная информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                // Показываем всех пользователей при очистке поиска
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                        row.Visible = true;
                }
            }
        }

        private void AdminPanel_Load_1(object sender, EventArgs e)
        {

        }
    }
}
    
        