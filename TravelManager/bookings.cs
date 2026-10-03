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
    public partial class bookings : Form
    {
        public bookings()
        {
            InitializeComponent();
            customerName.MaxLength = 50;
            NoOfpeople.MaxLength = 2;
            SetupStatuses();
            Theme.Apply(this);
        }

        private void bookings_Shown(object sender, EventArgs e)
        {
            LoadPackages();
            if (PackageCombobox.Items.Count == 0)
            {
                MessageBox.Show(this, "There are no packages yet. Please add a package first.", "No packages",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            bookings_Shown(this, e);
        }

        // ---------- Side menu ----------

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Homebutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new home());
        }

        private void Packagesbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new packages());
        }

        private void Bookingsbutton_Click(object sender, EventArgs e)
        {
            // Already on this screen.
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

        private static readonly string[] Statuses = { "Pending", "Confirmed", "Cancelled", "Completed" };

        /// <summary>Package list comes from the Packages table, so new packages appear here.</summary>
        private void LoadPackages()
        {
            try
            {
                using (var db = Db.Create())
                {
                    PackageCombobox.Items.Clear();
                    foreach (string name in db.Packages.OrderBy(p => p.PName).Select(p => p.PName))
                    {
                        PackageCombobox.Items.Add(name);
                    }
                }
                PackageCombobox.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        private void SetupStatuses()
        {
            BookingStatuscomboBox.Items.Clear();
            BookingStatuscomboBox.Items.AddRange(Statuses);
            BookingStatuscomboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>Checks the form and fills the booking; returns false when something is wrong.</summary>
        private bool ReadForm(Booking bk, bool isNew)
        {
            if (PackageCombobox.SelectedIndex == -1 || customerName.Text.Trim() == ""
                || BookingStatuscomboBox.SelectedIndex == -1 || NoOfpeople.Text.Trim() == "")
            {
                MessageBox.Show(this, "Please fill in all the required fields and select valid options from the dropdown menus.",
                    "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            int people = Check.Number(NoOfpeople.Text, "No of People", 1, 50, out string error);
            if (Check.Warn(this, Check.Name(customerName.Text, "Customer name"), customerName)
                || Check.Warn(this, error, NoOfpeople))
            {
                return false;
            }
            if (TraveldateTimePicker.Value.Date < BookingdateTimePicker.Value.Date)
            {
                Check.Warn(this, "Travel date cannot be before the booking date.", TraveldateTimePicker);
                return false;
            }
            if (isNew && TraveldateTimePicker.Value.Date < DateTime.Today)
            {
                Check.Warn(this, "Travel date cannot be in the past.", TraveldateTimePicker);
                return false;
            }
            bk.BPackage = PackageCombobox.Text;
            bk.CustomerName = customerName.Text.Trim();
            bk.BookingDate = BookingdateTimePicker.Value.Date;
            bk.BookingStatus = BookingStatuscomboBox.Text;
            bk.TravelDate = TraveldateTimePicker.Value.Date;
            bk.NoOfPeople = people;
            return true;
        }

        private void Addbutton_Click(object sender, EventArgs e)
        {
            var bk = new Booking();
            if (!ReadForm(bk, true))
            {
                return;
            }
            try
            {
                int id;
                using (var db = Db.Create())
                {
                    db.Bookings.InsertOnSubmit(bk);
                    db.SubmitChanges();
                    id = bk.B_ID;
                }
                MessageBox.Show(this, $"Booking has been added successfully.\n\nBooking ID: {id}\n(Use this ID on the Payments screen.)",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearTextBoxes();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        public void ClearTextBoxes()
        {
            Ui.ClearInputs(this);
            PackageCombobox.Focus();
        }

        private void Clearbutton_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }
    }
}
