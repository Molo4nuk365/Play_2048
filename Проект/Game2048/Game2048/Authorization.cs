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
using static Play2048.Games2048;
using static Play2048.User;
using static Play2048.AdminPanel;


namespace Play2048
{
    public partial class Authorization : Form
    {
        // Добавляем свойство для передачи авторизованного пользователя
        public User CurrentUser { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public Authorization()
        {
            InitializeComponent();
            IsAuthenticated = false;
            CurrentUser = null; // Инициализируем
        }



        private void Authorization_Load(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = true;
            checkBox1.Text = "Показать пароль";


        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.UseSystemPasswordChar = false;
                checkBox1.Text = "Скрыть пароль";
            }
            else
            {
                textBox2.UseSystemPasswordChar = true;
                checkBox1.Text = "Показать пароль";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            User.CreateDefaultAdmin("users.dat");

            User user = User.Authenticate("users.dat", login, password);

            if (user != null)
            {
                CurrentUser = user;
                IsAuthenticated = true;

                MessageBox.Show($"Добро пожаловать, {user.Login}!",
                    "Успешный вход", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Если пользователь - админ, открываем админ-панель
                if (user.Role == "admin")
                {
                    Form adminPanel = new AdminPanel();
                    adminPanel.Show();
                    this.Hide();
                }
                else
                {
                    // Для обычного пользователя открываем игру
                    Form game = new Games2048(user.Login);
                    game.Show();
                    this.Hide();
                }
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!",
                    "Ошибка входа", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void button2_Click(object sender, EventArgs e)
        {
            string login = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль для регистрации!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (login.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!User.UserExists("users.dat", login))
            {
                User newUser = new User(login, password, "user");
                string result = newUser.Save("users.dat");

                if (result.Contains("Сохранено"))
                {
                    CurrentUser = newUser;
                    IsAuthenticated = true;

                    MessageBox.Show($"Регистрация успешна! Добро пожаловать, {login}!",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Games2048 game = new Games2048(login);
                    game.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show($"Ошибка регистрации: {result}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show($"Пользователь с логином '{login}' уже существует!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
    



