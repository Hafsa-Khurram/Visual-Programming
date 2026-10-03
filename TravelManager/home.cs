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
    public partial class home : Form
    {
        public home()
        {
            InitializeComponent();
            Theme.Apply(this);
        }

        // ---------- Side menu ----------

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Homebutton_Click(object sender, EventArgs e)
        {
            // Already on this screen.
        }

        private void Packagesbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new packages());
        }

        private void Bookingsbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new bookings());
        }

        private void Paymentsbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new payments());
        }

        private void ViewBbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new viewbookings());
        }

        private void ViewPbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new viewpayments());
        }

        private void Logoutbutton_Click(object sender, EventArgs e)
        {
            Ui.Logout(this);
        }

        private void home_Load(object sender, EventArgs e)
        {
            if (Session.UserName != null)
            {
                var welcome = new Label
                {
                    Text = "Welcome, " + Session.UserName,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Ui.Navy,
                    Padding = new Padding(10, 5, 10, 5),
                };
                Controls.Add(welcome);
                welcome.Location = new Point(Packagesbutton.Left, Packagesbutton.Top - welcome.PreferredHeight - 14);
                welcome.BringToFront();
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
    }
}
