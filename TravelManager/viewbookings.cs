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
    public partial class viewbookings : Form
    {
        public viewbookings()
        {
            InitializeComponent();
            Ui.StyleGrid(dataGridView1);
            customerName.MaxLength = 50;
            NoOfpeople.MaxLength = 2;
            SetupStatuses();
        }

        private void DataGridView()
        {
            try
            {
                using (var db = Db.Create())
                {
                    dataGridView1.DataSource = db.Bookings.OrderByDescending(b => b.B_ID).ToList();
                }
                Ui.Titles(dataGridView1, "B_ID", "ID", "BPackage", "Package", "CustomerName", "Customer",
                    "BookingDate", "Booked On", "BookingStatus", "Status", "TravelDate", "Travel Date", "NoOfPeople", "People");
                foreach (string c in new[] { "BookingDate", "TravelDate" })
                {
                    if (dataGridView1.Columns.Contains(c))
                    {
                        dataGridView1.Columns[c].DefaultCellStyle.Format = "dd MMM yyyy";
                    }
                }
                if (dataGridView1.Columns.Contains("B_ID"))
                {
                    dataGridView1.Columns["B_ID"].FillWeight = 40;
                    dataGridView1.Columns["NoOfPeople"].FillWeight = 50;
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
            // Already on this screen.
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

        private int? SelectedId()
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                return null;
            }
            return Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["B_ID"].Value);
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
                    Booking bok = db.Bookings.FirstOrDefault(s => s.B_ID == id);
                    if (bok == null)
                    {
                        DataGridView();
                        return;
                    }
                    if (!ReadForm(bok, false))
                    {
                        return;
                    }
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Booking updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            PackageCombobox.Focus();
        }

        private void viewbookings_Load(object sender, EventArgs e)
        {
            LoadPackages();
            DataGridView();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = dataGridView1.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0)
            {
                return;
            }
            var row = dataGridView1.Rows[hit.RowIndex];
            Ui.Select(PackageCombobox, row.Cells["BPackage"].Value);
            customerName.Text = row.Cells["CustomerName"].Value?.ToString() ?? string.Empty;
            BookingdateTimePicker.Value = Ui.DateOr(row.Cells["BookingDate"].Value);
            Ui.Select(BookingStatuscomboBox, row.Cells["BookingStatus"].Value);
            TraveldateTimePicker.Value = Ui.DateOr(row.Cells["TravelDate"].Value);
            NoOfpeople.Text = row.Cells["NoOfPeople"].Value?.ToString() ?? string.Empty;
        }

        private void Clearbutton_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }

        private void Delbutton_Click(object sender, EventArgs e)
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
                    Booking bok = db.Bookings.FirstOrDefault(s => s.B_ID == id);
                    if (bok == null)
                    {
                        DataGridView();
                        return;
                    }
                    int payments = db.Payments.Count(p => p.BookingID == bok.B_ID);
                    string extra = payments > 0 ? $"\n\nIts {payments} payment record(s) will be deleted too." : "";
                    if (MessageBox.Show(this, $"Do you really want to delete booking #{bok.B_ID} ({bok.CustomerName})?{extra}",
                            "Delete booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                    db.Payments.DeleteAllOnSubmit(db.Payments.Where(p => p.BookingID == bok.B_ID));
                    db.Bookings.DeleteOnSubmit(bok);
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Booking deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
