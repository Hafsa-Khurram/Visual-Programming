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
    public partial class packages : Form
    {
        public packages()
        {
            InitializeComponent();
            Ui.StyleGrid(dataGridView1);
            Ui.RemoveBlankItems(Duration);
            Ui.RemoveBlankItems(Destination);
            PackageName.MaxLength = 255;
            price.MaxLength = 9;
            Description.MaxLength = 255;
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
            // Already on this screen.
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

        // ---------- Packages ----------

        /// <summary>Checks the form and fills the package; returns false when something is wrong.</summary>
        private bool ReadForm(Package pkg)
        {
            if (PackageName.Text.Trim() == "" || Description.Text.Trim() == "" || price.Text.Trim() == ""
                || Duration.SelectedIndex == -1 || Destination.SelectedIndex == -1)
            {
                MessageBox.Show(this, "All fields should be filled!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            int value = Check.Number(price.Text, "Price", 1, 100000000, out string error);
            if (Check.Warn(this, error, price))
            {
                return false;
            }
            pkg.PName = PackageName.Text.Trim();
            pkg.PDescription = Description.Text.Trim();
            pkg.PPrice = value;
            pkg.PDuration = Duration.SelectedItem.ToString();
            pkg.PDestination = Destination.SelectedItem.ToString();
            return true;
        }

        private int? SelectedId()
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.Index < 0)
            {
                return null;
            }
            return Convert.ToInt32(dataGridView1.CurrentRow.Cells["PID"].Value);
        }

        private void Addbutton_Click(object sender, EventArgs e)
        {
            var pkg = new Package();
            if (!ReadForm(pkg))
            {
                return;
            }
            try
            {
                using (var db = Db.Create())
                {
                    if (db.Packages.Any(p => p.PName == pkg.PName))
                    {
                        Check.Warn(this, "A package with this name already exists.", PackageName);
                        return;
                    }
                    db.Packages.InsertOnSubmit(pkg);
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Package has been added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearTextBoxes();
                BindGridView();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        private void packages_Load(object sender, EventArgs e)
        {
            BindGridView();
        }

        private void BindGridView()
        {
            try
            {
                using (var db = Db.Create())
                {
                    dataGridView1.DataSource = db.Packages.OrderBy(p => p.PID).ToList();
                }
                Ui.Titles(dataGridView1, "PID", "ID", "PName", "Package", "PDescription", "Description",
                    "PPrice", "Price (Rs)", "PDuration", "Duration", "PDestination", "Destination");
                if (dataGridView1.Columns.Contains("PID"))
                {
                    dataGridView1.Columns["PID"].FillWeight = 35;
                    dataGridView1.Columns["PPrice"].DefaultCellStyle.Format = "N0";
                }
                dataGridView1.ClearSelection();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }

        private void ClearTextBoxes()
        {
            Ui.ClearInputs(this);
            dataGridView1.ClearSelection();
            PackageName.Focus();
        }

        private void Clearbutton_Click(object sender, EventArgs e)
        {
            ClearTextBoxes();
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            // Fill the form from the clicked row (header and empty area are ignored).
            var hit = dataGridView1.HitTest(e.X, e.Y);
            if (hit.RowIndex < 0)
            {
                return;
            }
            var row = dataGridView1.Rows[hit.RowIndex];
            PackageName.Text = row.Cells["PName"].Value?.ToString();
            Description.Text = row.Cells["PDescription"].Value?.ToString();
            price.Text = row.Cells["PPrice"].Value?.ToString();
            Ui.Select(Duration, row.Cells["PDuration"].Value);
            Ui.Select(Destination, row.Cells["PDestination"].Value);
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
                    Package pkg = db.Packages.FirstOrDefault(s => s.PID == id);
                    if (pkg == null)
                    {
                        MessageBox.Show(this, "This package no longer exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        BindGridView();
                        return;
                    }
                    string oldName = pkg.PName;
                    if (!ReadForm(pkg))
                    {
                        return;
                    }
                    if (db.Packages.Any(p => p.PName == pkg.PName && p.PID != pkg.PID))
                    {
                        Check.Warn(this, "Another package already has this name.", PackageName);
                        return;
                    }
                    // Keep bookings linked to the renamed package.
                    if (oldName != pkg.PName)
                    {
                        foreach (var b in db.Bookings.Where(b => b.BPackage == oldName))
                        {
                            b.BPackage = pkg.PName;
                        }
                    }
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Package has been updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearTextBoxes();
                BindGridView();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
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
                    Package pkg = db.Packages.FirstOrDefault(s => s.PID == id);
                    if (pkg == null)
                    {
                        BindGridView();
                        return;
                    }
                    int booked = db.Bookings.Count(b => b.BPackage == pkg.PName && b.BookingStatus != "Cancelled");
                    if (booked > 0)
                    {
                        MessageBox.Show(this, $"\"{pkg.PName}\" has {booked} active booking(s), so it cannot be deleted.\n" +
                            "Cancel or delete those bookings first.", "Package in use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (MessageBox.Show(this, $"Are you sure you want to delete \"{pkg.PName}\"?", "Delete package",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                    db.Packages.DeleteOnSubmit(pkg);
                    db.SubmitChanges();
                }
                MessageBox.Show(this, "Package has been deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearTextBoxes();
                BindGridView();
            }
            catch (Exception ex)
            {
                Db.ShowError(this, ex);
            }
        }
    }
}
