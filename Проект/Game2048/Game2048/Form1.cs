
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Play2048.Authorization;
using static Play2048.instruction;

namespace Play2048
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

 
        private void button1_Click(object sender, EventArgs e)
        {
            Form ifrm = new instruction();
            ifrm.Show(); // отображаем instruction
            this.Hide(); // скрываем Form1 (this - текущая форма
        }

        private void button2_Click(object sender, EventArgs e)
        {
             Form ifrm = new Authorization();
            ifrm.Show(); // отображаем Authorization
            this.Hide(); // скрываем Form1 (this - текущая форма)
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
