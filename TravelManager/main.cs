using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TravelManager
{
    public partial class main : Form
    {
        public main()
        {
            InitializeComponent();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void login_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new login());
        }

        private void signup_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new signup());
        }

        private void main_Load(object sender, EventArgs e)
        {
        }

        private void exit_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
