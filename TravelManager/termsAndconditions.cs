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
    public partial class termsAndconditions : Form
    {
        public termsAndconditions()
        {
            InitializeComponent();
            Theme.Apply(this);
        }

        // Shown on top of the Sign Up screen, so going back just closes it.
        private void exit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Backbutton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
