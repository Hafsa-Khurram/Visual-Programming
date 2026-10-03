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
    public partial class viewpayments : Form
    {
        public viewpayments()
        {
            InitializeComponent();
            Ui.StyleGrid(dataGridView1);
            SetupLists();
            Theme.Apply(this);
        }

        private void DataGridView()
        {
            try
            {
                using (var db = Db.Create())
                {
                    dataGridView1.DataSource = db.Payments.OrderByDescending(p => p.PI_ID).ToList();
                }
                Ui.Titles(dataGridView1, "PI_ID", "ID", "BookingID", "Booking ID", "PaymentAmount", "Amount (Rs)",
                    "PaymentDate", "Date", "PaymentMethod", "Method", "PaymentStatus", "Status");
                if (dataGridView1.Columns.Contains("PI_ID"))
                {
                    dataGridView1.Columns["PI_ID"].FillWeight = 40;
                    dataGridView1.Columns["PaymentAmount"].DefaultCellStyle.Format = "N0";
                    dataGridView1.Columns["PaymentDate"].DefaultCellStyle.Format = "dd MMM yyyy";
                }
                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
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
            Ui.Go(this, new payments());
        }

        private void ViewBbutton_Click(object sender, EventArgs e)
        {
            Ui.Go(this, new viewbookings());
        }

        private void ViewPbutton_Click(object sender, EventArgs e)
        {
            // Already on this screen.
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

        private void viewpayments_Load(object sender, EventArgs e)
        {
            DataGridView();
        }

        private void Clearbutton_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }

        private int? SelectedId()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                return null;
            }
            return Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["PI_ID"].Value);
        }

        private void Updatebutton_Click(object sender, EventArgs e)
        {
            int? id = SelectedId();
            if (id == null)
            {
                MessageBox.Show(this, "Please select a row first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                using (var db = Db.Create())
                {
                    Payment pyt = db.Payments.FirstOrDefault(s => s.PI_ID == id);
                    if (pyt == null)
                    {
                        DataGridView();
                        return;
                    }
                    if (!ReadForm(db, pyt))
                    {
                        return;
                    }
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Payment updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearTextBoxes();
                DataGridView();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        public void ClearTextBoxes()
        {
            Ui.ClearInputs(this);
            dataGridView1.ClearSelection();
            bookingID.Focus();
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return; // header click (sorting)
            }
            var row = dataGridView1.Rows[e.RowIndex];
            bookingID.Text = row.Cells["BookingID"].Value?.ToString() ?? string.Empty;
            PayAmount.Text = row.Cells["PaymentAmount"].Value?.ToString() ?? string.Empty;
            PaymentdateTimePicker.Value = Ui.DateOr(row.Cells["PaymentDate"].Value);
            Ui.Select(PayMthdcomboBox, row.Cells["PaymentMethod"].Value);
            Ui.Select(PaystatuscomboBox, row.Cells["PaymentStatus"].Value);
        }

        private void Delbutton_Click(object sender, EventArgs e)
        {
            int? id = SelectedId();
            if (id == null)
            {
                MessageBox.Show(this, "Please select a row first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MessageBox.Show(this, $"Do you really want to delete payment #{id}?", "Delete payment",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            try
            {
                using (var db = Db.Create())
                {
                    Payment pyt = db.Payments.FirstOrDefault(s => s.PI_ID == id);
                    if (pyt != null)
                    {
                        db.Payments.DeleteOnSubmit(pyt);
                        db.SubmitChanges();
                    }
                }
                MessageBox.Show(this, "Payment deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearTextBoxes();
                DataGridView();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }
    }
}
