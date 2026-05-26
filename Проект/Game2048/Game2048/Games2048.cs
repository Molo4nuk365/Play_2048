using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static Play2048.User;
using static Play2048.Authorization;
using static Play2048.Form1;
using static Play2048.AdminPanel;
using static Play2048.GameOverForm;

namespace Play2048
{
    public partial class Games2048 : Form
    {
        public int[,] map = new int[4, 4];
        public Label[,] labels = new Label[4, 4];
        public PictureBox[,] pics = new PictureBox[4, 4];
        private int score = 0;
        private string currentUser;
        private bool isGameOver = false;
        private const int CELL_SIZE = 70;
        private const int CELL_MARGIN = 10;
        private const int START_X = 20;
        private const int START_Y = 100;
        public Games2048(string currentUser)
        {
            InitializeComponent();
            this.currentUser = currentUser;
            this.KeyDown += new KeyEventHandler(OnKeyboardPressed);
            this.FormClosing += Game2048_FormClosing;
            this.Size = new Size(400, 500);
            CreateMap();
            GenerateNewPic();
            GenerateNewPic();
        }

        public Games2048()
        {
        }

        private void CreateMap()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    PictureBox pic = new PictureBox();
                    pic.Location = new Point(
                        START_X + (CELL_SIZE + CELL_MARGIN) * j,
                        START_Y + (CELL_SIZE + CELL_MARGIN) * i);
                    pic.Size = new Size(CELL_SIZE, CELL_SIZE);
                    pic.BackColor = Color.FromArgb(205, 193, 180);
                    this.Controls.Add(pic);
                }
            }
        }

        private void GenerateNewPic()
        {
            Random rnd = new Random();
            int a, b;

            int freeCells = 0;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (map[i, j] == 0) freeCells++;

            if (freeCells == 0) return;

            do
            {
                a = rnd.Next(0, 4);
                b = rnd.Next(0, 4);
            } while (map[a, b] != 0);

            int value = rnd.Next(0, 10) == 0 ? 4 : 2;
            map[a, b] = value;

            pics[a, b] = new PictureBox();
            labels[a, b] = new Label();
            labels[a, b].Text = value.ToString();
            labels[a, b].Size = new Size(CELL_SIZE, CELL_SIZE);
            labels[a, b].TextAlign = ContentAlignment.MiddleCenter;
            labels[a, b].Font = new Font("Segoe UI", 18, FontStyle.Bold);

            if (value <= 4)
                labels[a, b].ForeColor = Color.FromArgb(119, 110, 101);
            else
                labels[a, b].ForeColor = Color.White;

            pics[a, b].Controls.Add(labels[a, b]);
            pics[a, b].Location = new Point(
                START_X + (CELL_SIZE + CELL_MARGIN) * b,
                START_Y + (CELL_SIZE + CELL_MARGIN) * a);
            pics[a, b].Size = new Size(CELL_SIZE, CELL_SIZE);
            ChangeColor(value, a, b);
            this.Controls.Add(pics[a, b]);
            pics[a, b].BringToFront();
        }

        private void ChangeColor(int value, int k, int j)
        {
            switch (value)
            {
                case 2:
                    pics[k, j].BackColor = Color.FromArgb(238, 228, 218);
                    break;
                case 4:
                    pics[k, j].BackColor = Color.FromArgb(237, 224, 200);
                    break;
                case 8:
                    pics[k, j].BackColor = Color.FromArgb(242, 177, 121);
                    break;
                case 16:
                    pics[k, j].BackColor = Color.FromArgb(245, 149, 99);
                    break;
                case 32:
                    pics[k, j].BackColor = Color.FromArgb(246, 124, 95);
                    break;
                case 64:
                    pics[k, j].BackColor = Color.FromArgb(246, 94, 59);
                    break;
                case 128:
                    pics[k, j].BackColor = Color.FromArgb(237, 207, 114);
                    break;
                case 256:
                    pics[k, j].BackColor = Color.FromArgb(237, 204, 97);
                    break;
                case 512:
                    pics[k, j].BackColor = Color.FromArgb(237, 200, 80);
                    break;
                case 1024:
                    pics[k, j].BackColor = Color.FromArgb(237, 197, 63);
                    break;
                case 2048:
                    pics[k, j].BackColor = Color.FromArgb(237, 194, 46);
                    break;
                default:
                    pics[k, j].BackColor = Color.FromArgb(60, 58, 50);
                    break;
            }
        }

        private void OnKeyboardPressed(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;

            bool ifPicksMoved = false;

            switch (e.KeyCode)
            {
                case Keys.Right:
                    ifPicksMoved = MoveRight();
                    break;
                case Keys.Left:
                    ifPicksMoved = MoveLeft();
                    break;
                case Keys.Down:
                    ifPicksMoved = MoveDown();
                    break;
                case Keys.Up:
                    ifPicksMoved = MoveUp();
                    break;
            }

            if (ifPicksMoved)
            {
                GenerateNewPic();
                CheckGameOver();
            }
        }

        private bool MoveRight()
        {
            bool moved = false;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 2; col >= 0; col--)
                {
                    if (map[row, col] != 0)
                    {
                        for (int newCol = col + 1; newCol < 4; newCol++)
                        {
                            if (map[row, newCol] == 0)
                            {
                                moved = true;
                                MoveCell(row, col, row, newCol);
                                col = newCol;
                                break;
                            }
                            else if (map[row, newCol] == map[row, col])
                            {
                                if (labels[row, newCol].Text == labels[row, col].Text)
                                {
                                    moved = true;
                                    MergeCells(row, col, row, newCol);
                                }
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return moved;
        }

        private bool MoveLeft()
        {
            bool moved = false;
            for (int row = 0; row < 4; row++)
            {
                for (int col = 1; col < 4; col++)
                {
                    if (map[row, col] != 0)
                    {
                        for (int newCol = col - 1; newCol >= 0; newCol--)
                        {
                            if (map[row, newCol] == 0)
                            {
                                moved = true;
                                MoveCell(row, col, row, newCol);
                                col = newCol;
                                break;
                            }
                            else if (map[row, newCol] == map[row, col])
                            {
                                if (labels[row, newCol].Text == labels[row, col].Text)
                                {
                                    moved = true;
                                    MergeCells(row, col, row, newCol);
                                }
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return moved;
        }

        private bool MoveDown()
        {
            bool moved = false;
            for (int col = 0; col < 4; col++)
            {
                for (int row = 2; row >= 0; row--)
                {
                    if (map[row, col] != 0)
                    {
                        for (int newRow = row + 1; newRow < 4; newRow++)
                        {
                            if (map[newRow, col] == 0)
                            {
                                moved = true;
                                MoveCell(row, col, newRow, col);
                                row = newRow;
                                break;
                            }
                            else if (map[newRow, col] == map[row, col])
                            {
                                if (labels[newRow, col].Text == labels[row, col].Text)
                                {
                                    moved = true;
                                    MergeCells(row, col, newRow, col);
                                }
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return moved;
        }

        private bool MoveUp()
        {
            bool moved = false;
            for (int col = 0; col < 4; col++)
            {
                for (int row = 1; row < 4; row++)
                {
                    if (map[row, col] != 0)
                    {
                        for (int newRow = row - 1; newRow >= 0; newRow--)
                        {
                            if (map[newRow, col] == 0)
                            {
                                moved = true;
                                MoveCell(row, col, newRow, col);
                                row = newRow;
                                break;
                            }
                            else if (map[newRow, col] == map[row, col])
                            {
                                if (labels[newRow, col].Text == labels[row, col].Text)
                                {
                                    moved = true;
                                    MergeCells(row, col, newRow, col);
                                }
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return moved;
        }

        private void MoveCell(int fromRow, int fromCol, int toRow, int toCol)
        {
            map[toRow, toCol] = map[fromRow, fromCol];
            map[fromRow, fromCol] = 0;

            pics[toRow, toCol] = pics[fromRow, fromCol];
            pics[fromRow, fromCol] = null;

            labels[toRow, toCol] = labels[fromRow, fromCol];
            labels[fromRow, fromCol] = null;

            int xPos = START_X + (CELL_SIZE + CELL_MARGIN) * toCol;
            int yPos = START_Y + (CELL_SIZE + CELL_MARGIN) * toRow;
            pics[toRow, toCol].Location = new Point(xPos, yPos);
        }

        private void MergeCells(int fromRow, int fromCol, int toRow, int toCol)
        {
            int value1 = map[fromRow, fromCol];
            int value2 = map[toRow, toCol];
            int newValue = value1 + value2;

            score += newValue;
            label1.Text = "Очки: " + score;

            map[toRow, toCol] = newValue;
            map[fromRow, fromCol] = 0;
            labels[toRow, toCol].Text = newValue.ToString();

            if (newValue <= 4)
                labels[toRow, toCol].ForeColor = Color.FromArgb(119, 110, 101);
            else
                labels[toRow, toCol].ForeColor = Color.White;

            ChangeColor(newValue, toRow, toCol);

            this.Controls.Remove(pics[fromRow, fromCol]);
            this.Controls.Remove(labels[fromRow, fromCol]);
            pics[fromRow, fromCol] = null;
            labels[fromRow, fromCol] = null;
        }

        private void CheckGameOver()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (map[i, j] == 2048)
                    {
                        ShowGameOverForm("Поздравляем! Вы выиграли! Достигнута плитка 2048!");
                        return;
                    }
                }
            }

            bool hasMoves = false;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (map[i, j] == 0)
                    {
                        hasMoves = true;
                        break;
                    }
                }
                if (hasMoves) break;
            }

            if (!hasMoves)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (j < 3 && map[i, j] == map[i, j + 1])
                            hasMoves = true;
                        if (i < 3 && map[i, j] == map[i + 1, j])
                            hasMoves = true;
                    }
                }
            }

            if (!hasMoves)
            {
                ShowGameOverForm("Игра окончена!");
            }
        }

        private void ShowGameOverForm(string message)
        {
            isGameOver = true;

            this.KeyDown -= OnKeyboardPressed;

            MessageBox.Show($"{message}\nОчков: {score}", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            using (GameOverForm gameOverForm = new GameOverForm(score, currentUser))
            {
                var result = gameOverForm.ShowDialog();

                if (result == DialogResult.Yes)
                {
                    this.Close();
                    Games2048 newGame = new Games2048(currentUser);
                    newGame.Show();
                }
                else if (result == DialogResult.No)
                {
                    this.Close();
                    Form1 mainMenu = new Form1();
                    mainMenu.Show();
                }
                else
                {
                    this.Close();
                }
            }
        }

        private void Game2048_Load(object sender, EventArgs e)
        {

        }

        private void Game2048_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (!isGameOver && score > 0)
            {
                var result = MessageBox.Show("Сохранить текущий результат перед выходом?",
                    "Сохранение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool updated = User.UpdateHighScore("users.dat", currentUser, (uint)score);
                    if (updated)
                    {
                        MessageBox.Show($"Рекорд сохранен: {score} очков!",
                            "Сохранено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }
    }
    }

