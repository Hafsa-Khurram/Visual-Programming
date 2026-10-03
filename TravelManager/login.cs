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
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
            PasswordtextBox.PasswordChar = '*';
            NametextBox.MaxLength = 50;
            AcceptButton = loginbutton; // Enter key logs in
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loginbutton_Click(object sender, EventArgs e)
        {
            string name = NametextBox.Text.Trim();
            string password = PasswordtextBox.Text;
            if (name == "" || password == "")
            {
                Check.Warn(this, "Please enter your name and password.", name == "" ? (Control)NametextBox : PasswordtextBox);
                return;
            }

            try
            {
                using (var db = Db.Create())
                {
                    var user = db.Signups.FirstOrDefault(r => r.Name == name);
                    string hash = Security.Hash(password);
                    bool ok = user != null && (user.Password == hash || user.Password == password);
                    if (ok && user.Password == password)
                    {
                        user.Password = hash; // upgrade an old plain-text password
                        db.SubmitChanges();
                    }

                    if (ok)
                    {
                        Session.UserName = user.Name;
                        MessageBox.Show(this, $"Welcome, {user.Name}!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Ui.Go(this, new home());
                    }
                    else
                    {
                        MessageBox.Show(this, "Invalid username or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        PasswordtextBox.Clear();
                        PasswordtextBox.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        private void NoAccbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new signup());
        }

        private void ShowPswdcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            PasswordtextBox.PasswordChar = ShowPswdcheckBox.Checked ? '\0' : '*';
        }
    }
}
