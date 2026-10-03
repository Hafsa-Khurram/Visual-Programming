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
    public partial class payments : Form
    {
        public payments()
        {
            InitializeComponent();
            SetupLists();
            Theme.Apply(this);
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
            Ui.Go(this, new bookings());
        }

        private void Paymentsbutton_Click(object sender, EventArgs e)
        {
            // Already on this screen.
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

        private static readonly string[] Statuses = { "Paid", "Pending", "Unpaid", "Failed", "Refunded" };

        private void SetupLists()
        {
            Ui.RemoveBlankItems(PayMthdcomboBox);
            PaystatuscomboBox.Items.Clear();
            PaystatuscomboBox.Items.AddRange(Statuses);
            PaystatuscomboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            bookingID.MaxLength = 9;
            PayAmount.MaxLength = 9;
            bookingID.Leave += (s, e) => SuggestAmount();
        }

        /// <summary>When a valid booking ID is typed, fill in the amount (package price x people).</summary>
        private void SuggestAmount()
        {
            if (PayAmount.Text.Trim() != "" || !int.TryParse(bookingID.Text.Trim(), out int id))
            {
                return;
            }
            try
            {
                using (var db = Db.Create())
                {
                    var booking = db.Bookings.FirstOrDefault(b => b.B_ID == id);
                    var pkg = booking == null ? null : db.Packages.FirstOrDefault(p => p.PName == booking.BPackage);
                    if (pkg != null)
                    {
                        PayAmount.Text = ((long)pkg.PPrice * booking.NoOfPeople).ToString();
                    }
                }
            }
            catch (Exception)
            {
                // Only a suggestion; errors are reported when saving.
            }
        }

        /// <summary>Checks the form and fills the payment; returns false when something is wrong.</summary>
        private bool ReadForm(TravelDBDataContext db, Payment py)
        {
            if (bookingID.Text.Trim() == "" || PayAmount.Text.Trim() == ""
                || PayMthdcomboBox.SelectedIndex == -1 || PaystatuscomboBox.SelectedIndex == -1)
            {
                MessageBox.Show(this, "Please fill in all the required fields and select valid options from the dropdown menus.",
                    "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            int id = Check.Number(bookingID.Text, "Booking ID", 1, int.MaxValue, out string idError);
            if (Check.Warn(this, idError, bookingID))
            {
                return false;
            }
            int amount = Check.Number(PayAmount.Text, "Payment amount", 1, 1000000000, out string amountError);
            if (Check.Warn(this, amountError, PayAmount))
            {
                return false;
            }
            Booking booking = db.Bookings.FirstOrDefault(b => b.B_ID == id);
            if (booking == null)
            {
                Check.Warn(this, $"There is no booking with ID {id}. Check the ID on the View Bookings screen.", bookingID);
                return false;
            }
            if (PaymentdateTimePicker.Value.Date > DateTime.Today)
            {
                Check.Warn(this, "Payment date cannot be in the future.", PaymentdateTimePicker);
                return false;
            }
            py.BookingID = id;
            py.PaymentAmount = amount;
            py.PaymentDate = PaymentdateTimePicker.Value.Date;
            py.PaymentMethod = PayMthdcomboBox.Text;
            py.PaymentStatus = PaystatuscomboBox.Text;
            return true;
        }

        private void Addbutton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var db = Db.Create())
                {
                    var py = new Payment();
                    if (!ReadForm(db, py))
                    {
                        return;
                    }
                    bool alreadyPaid = db.Payments.Any(p => p.BookingID == py.BookingID && p.PaymentStatus == "Paid");
                    if (alreadyPaid && py.PaymentStatus == "Paid" && MessageBox.Show(this,
                            $"Booking #{py.BookingID} already has a Paid payment. Add another one?", "Already paid",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                    db.Payments.InsertOnSubmit(py);
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Payment has been added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            bookingID.Focus();
        }

        private void Clearbutton_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }
    }
}
