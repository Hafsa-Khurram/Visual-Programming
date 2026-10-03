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
    public partial class signup : Form
    {
        public signup()
        {
            InitializeComponent();
            PasswordtextBox.PasswordChar = '*';
            NametextBox.MaxLength = 50;
            EmailtextBox.MaxLength = 100;
            AddresstextBox.MaxLength = 255;
            ContacttextBox.MaxLength = 15;
            AcceptButton = Signupbutton;
        }

        private void Signupbutton_Click(object sender, EventArgs e)
        {
            if (NametextBox.Text.Trim() == "" || EmailtextBox.Text.Trim() == "" || AddresstextBox.Text.Trim() == ""
                || ContacttextBox.Text.Trim() == "" || PasswordtextBox.Text == "")
            {
                MessageBox.Show(this, "Please fill in all the fields!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Check.Warn(this, Check.Name(NametextBox.Text, "User name"), NametextBox)
                || Check.Warn(this, Check.Email(EmailtextBox.Text), EmailtextBox)
                || Check.Warn(this, AddresstextBox.Text.Trim().Length < 5 ? "Please enter your full address." : null, AddresstextBox)
                || Check.Warn(this, Check.Phone(ContacttextBox.Text), ContacttextBox)
                || Check.Warn(this, Check.Password(PasswordtextBox.Text), PasswordtextBox)
                || Check.Warn(this, TermscheckBox.Checked ? null : "Please accept the Terms and Conditions.", TermscheckBox))
            {
                return;
            }

            string name = NametextBox.Text.Trim();
            string email = EmailtextBox.Text.Trim().ToLowerInvariant();
            try
            {
                using (var db = Db.Create())
                {
                    if (db.Signups.Any(r => r.Name == name))
                    {
                        Check.Warn(this, "User name already exists! Please use a different user name.", NametextBox);
                        return;
                    }
                    if (db.Signups.Any(r => r.Email == email))
                    {
                        Check.Warn(this, "An account with this email already exists.", EmailtextBox);
                        return;
                    }
                    db.Signups.InsertOnSubmit(new Signup
                    {
                        Name = name,
                        Email = email,
                        Address = AddresstextBox.Text.Trim(),
                        ContactNo = ContacttextBox.Text.Trim(),
                        Password = Security.Hash(PasswordtextBox.Text),
                    });
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Registration successful! You can log in now.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Ui.Go(this, new login());
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        public void ClearTextBoxes()
        {
            Ui.ClearInputs(this);
            NametextBox.Focus();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void TermslinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var tc = new termsAndconditions())
            {
                tc.StartPosition = FormStartPosition.CenterParent;
                tc.ShowDialog(this);
            }
        }

        private void Clearbutton_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }
    }
}
