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

namespace Play2048
{
    public partial class GameOverForm : Form
    {

        private int score;
        private string currentUser;


        public GameOverForm()
        {
            InitializeComponent();
        }
        public GameOverForm(int score, string currentUser) : this()
        {
            this.score = score;
            this.currentUser = currentUser;
            this.finalScore = (uint)score;
            this.username = currentUser;

            // Отображение счета
            label1.Text = $"Результат: {score}";
        }

        private uint finalScore;
        private string username;
        private void GameOverForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Начать заново
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            // Сохраняем результат
            bool updated = User.UpdateHighScore("users.dat", username, finalScore);

            if (updated)
            {
                MessageBox.Show($"Рекорд сохранен: {finalScore} очков!",
                    "Сохранено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button2.Enabled = false;
                button2.Text = "Результат сохранен";
                button2.BackColor = Color.Gray;
            }
            else
            {
                uint currentHighScore = User.GetHighScore("users.dat", username);
                MessageBox.Show($"Текущий рекорд уже выше: {currentHighScore} очков",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button2.Enabled = false;
                button2.Text = "Рекорд не побит";
                button2.BackColor = Color.Black;
            }
        }
        

        

        private void button3_Click(object sender, EventArgs e)
        {
            Form ifrm = new Form1();
            ifrm.Show(); // отображаем Form1
            this.Hide(); // скрываем GameOverForm (this - текущая форма)
        }
    }
    
}
