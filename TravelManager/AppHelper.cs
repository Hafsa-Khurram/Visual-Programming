using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TravelManager
{
    /// <summary>
    /// Finds the SQL Server, creates the TravelDB database and its tables when they
    /// are missing, and hands out data contexts that use the working connection.
    /// </summary>
    internal static class Db
    {
        private static string connectionString;

        /// <summary>Servers that are tried in order when the one in App.config is not reachable.</summary>
        private static readonly string[] FallbackServers =
        {
            ".", @".\SQLEXPRESS", @"(localdb)\MSSQLLocalDB", Environment.MachineName
        };

        public static TravelDBDataContext Create()
        {
            if (connectionString == null)
            {
                Initialize();
            }
            return new TravelDBDataContext(connectionString);
        }

        /// <summary>Connects to SQL Server and makes sure the database and tables exist.</summary>
        public static void Initialize()
        {
            string configured = ConfigurationManager.ConnectionStrings["TravelManager.Properties.Settings.TravelDBConnectionString"]?.ConnectionString
                                ?? Properties.Settings.Default.TravelDBConnectionString;
            var candidates = new[] { configured }.Concat(FallbackServers.Select(server =>
            {
                var b = new SqlConnectionStringBuilder(configured) { DataSource = server };
                return b.ConnectionString;
            }));

            Exception last = null;
            foreach (string candidate in candidates.Distinct())
            {
                try
                {
                    var builder = new SqlConnectionStringBuilder(candidate) { ConnectTimeout = 5 };
                    string database = string.IsNullOrEmpty(builder.InitialCatalog) ? "TravelDB" : builder.InitialCatalog;
                    var master = new SqlConnectionStringBuilder(builder.ConnectionString) { InitialCatalog = "master" };
                    using (var con = new SqlConnection(master.ConnectionString))
                    {
                        con.Open();
                        Execute(con, $"IF DB_ID(N'{database}') IS NULL CREATE DATABASE [{database}];");
                    }
                    builder.InitialCatalog = database;
                    using (var con = new SqlConnection(builder.ConnectionString))
                    {
                        con.Open();
                        CreateTables(con);
                    }
                    connectionString = builder.ConnectionString;
                    return;
                }
                catch (SqlException ex)
                {
                    last = ex;
                }
            }
            throw new InvalidOperationException(
                "Could not connect to SQL Server.\n\nMake sure SQL Server (or SQL Server Express) is installed and running, " +
                "or put your server name in App.config (Data Source=...).", last);
        }

        private static void Execute(SqlConnection con, string sql)
        {
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private static void CreateTables(SqlConnection con)
        {
            Execute(con, @"
IF OBJECT_ID(N'dbo.Signup', N'U') IS NULL
CREATE TABLE dbo.Signup (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Address NVARCHAR(255) NOT NULL,
    ContactNo NVARCHAR(15) NOT NULL,
    Password NVARCHAR(255) NOT NULL);

IF OBJECT_ID(N'dbo.Package', N'U') IS NULL
CREATE TABLE dbo.Package (
    PID INT IDENTITY(1,1) PRIMARY KEY,
    PName NVARCHAR(255) NOT NULL,
    PDescription NVARCHAR(255) NOT NULL,
    PPrice INT NOT NULL CHECK (PPrice > 0),
    PDuration NVARCHAR(255) NOT NULL,
    PDestination NVARCHAR(255));

IF OBJECT_ID(N'dbo.Booking', N'U') IS NULL
CREATE TABLE dbo.Booking (
    B_ID INT IDENTITY(1,1) PRIMARY KEY,
    BPackage NVARCHAR(255) NOT NULL,
    CustomerName NVARCHAR(255) NOT NULL,
    BookingDate DATETIME NOT NULL DEFAULT GETDATE(),
    BookingStatus NVARCHAR(255) NOT NULL,
    TravelDate DATETIME NOT NULL,
    NoOfPeople INT NOT NULL CHECK (NoOfPeople > 0));

IF OBJECT_ID(N'dbo.Payment', N'U') IS NULL
CREATE TABLE dbo.Payment (
    PI_ID INT IDENTITY(1,1) PRIMARY KEY,
    BookingID INT NOT NULL REFERENCES dbo.Booking(B_ID) ON DELETE CASCADE,
    PaymentAmount INT NOT NULL CHECK (PaymentAmount > 0),
    PaymentDate DATE NOT NULL DEFAULT GETDATE(),
    PaymentMethod NVARCHAR(255) NOT NULL,
    PaymentStatus NVARCHAR(255) NOT NULL);

IF NOT EXISTS (SELECT 1 FROM dbo.Package)
INSERT INTO dbo.Package (PName, PDescription, PPrice, PDuration, PDestination) VALUES
 (N'Beach Getaway', N'Sun, sea and relaxing beach resorts', 150000, N'7 Days', N'Dubai, United Arab Emirates'),
 (N'Adventure Tours', N'Hiking, camping and mountain views', 85000, N'10 Days', N'Hunza, Pakistan'),
 (N'Romantic Escapes', N'Candle-light dinners and city walks', 250000, N'7 Days', N'Paris, France'),
 (N'Family Vacation', N'Fun-filled trip for the whole family', 60000, N'3 Days', N'Murree, Pakistan'),
 (N'City Break', N'Historic sights, food and shopping', 180000, N'7 Days', N'Istanbul, Turkey');");
        }

        /// <summary>Shows a friendly message for database problems instead of crashing.</summary>
        public static void ShowError(IWin32Window owner, Exception ex)
        {
            var sql = ex as SqlException;
            string message = ex is InvalidOperationException && ex.InnerException is SqlException
                ? ex.Message
                : sql != null && (sql.Number == 2627 || sql.Number == 2601)
                    ? "This record already exists (duplicate value)."
                    : "Something went wrong while talking to the database:\n\n" + ex.Message;
            MessageBox.Show(owner, message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>The user who is logged in.</summary>
    internal static class Session
    {
        public static string UserName { get; set; }
    }

    /// <summary>Password hashing (SHA-256), so passwords are not stored as plain text.</summary>
    internal static class Security
    {
        public static string Hash(string password)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes("TravelManager:" + password));
                return Convert.ToBase64String(bytes);
            }
        }
    }

    /// <summary>Input checks shared by all forms. Each returns an error message, or null when the value is valid.</summary>
    internal static class Check
    {
        public static bool Warn(IWin32Window owner, string message, Control focus = null)
        {
            if (message == null)
            {
                return false;
            }
            MessageBox.Show(owner, message, "Please check your input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            focus?.Focus();
            return true;
        }

        public static string Name(string value, string field)
        {
            value = value.Trim();
            if (value.Length < 3 || value.Length > 50)
            {
                return field + " must be 3 to 50 characters long.";
            }
            return Regex.IsMatch(value, @"^[A-Za-z][A-Za-z .'-]*$") ? null : field + " can contain letters and spaces only.";
        }

        public static string Email(string value)
        {
            return Regex.IsMatch(value.Trim(), @"^[\w.+-]+@[\w-]+(\.[\w-]+)+$") && value.Trim().Length <= 100
                ? null : "Please enter a valid email address, e.g. name@gmail.com.";
        }

        public static string Phone(string value)
        {
            string digits = value.Trim().Replace("-", "").Replace(" ", "");
            return Regex.IsMatch(digits, @"^\+?\d{10,13}$") ? null : "Please enter a valid contact number, e.g. 03001234567.";
        }

        public static string Password(string value)
        {
            return value.Length >= 6 ? null : "Password must be at least 6 characters long.";
        }

        /// <summary>Parses a whole number between min and max; returns the error message through the out parameter.</summary>
        public static int Number(string text, string field, int min, int max, out string error)
        {
            string clean = text.Trim().Replace(",", "");
            if (!int.TryParse(clean, out int value) || value < min || value > max)
            {
                error = $"{field} must be a whole number from {min:N0} to {max:N0}.";
                return 0;
            }
            error = null;
            return value;
        }
    }

    /// <summary>Navigation and small UI helpers shared by all forms.</summary>
    internal static class Ui
    {
        public static readonly Color Navy = Color.FromArgb(48, 56, 79);
        public static readonly Color NavyLight = Color.FromArgb(222, 226, 238);

        /// <summary>Opens the next screen and closes the current one.</summary>
        public static void Go(Form from, Form to)
        {
            to.StartPosition = FormStartPosition.Manual;
            to.Location = from.Location;
            to.Show();
            from.Close();
        }

        /// <summary>Asks to log out; if confirmed, closes every screen and returns to Login.</summary>
        public static void Logout(Form current)
        {
            using (var dialog = new logout())
            {
                if (dialog.ShowDialog(current) != DialogResult.OK)
                {
                    return;
                }
            }
            Session.UserName = null;
            var login = new login { StartPosition = FormStartPosition.Manual, Location = current.Location };
            login.Show();
            foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
            {
                if (f != login && f.Visible && !(f is splash))
                {
                    f.Close();
                }
            }
        }

        /// <summary>Clears every input on the form, including those inside panels.</summary>
        public static void ClearInputs(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBoxBase)
                {
                    ((TextBoxBase)c).Clear();
                }
                else if (c is ComboBox)
                {
                    ((ComboBox)c).SelectedIndex = -1;
                    ((ComboBox)c).Text = "";
                }
                else if (c is DateTimePicker)
                {
                    ((DateTimePicker)c).Value = DateTime.Today;
                }
                else if (c is CheckBox)
                {
                    ((CheckBox)c).Checked = false;
                }
                if (c.HasChildren)
                {
                    ClearInputs(c);
                }
            }
        }

        /// <summary>Removes the blank entries between the items of a drop-down list.</summary>
        public static void RemoveBlankItems(ComboBox box)
        {
            for (int i = box.Items.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(box.Items[i]?.ToString()))
                {
                    box.Items.RemoveAt(i);
                }
            }
            box.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>Selects an item by text; adds it first if it is not in the list (e.g. an old value).</summary>
        public static void Select(ComboBox box, object value)
        {
            string text = value?.ToString() ?? "";
            if (text.Length == 0)
            {
                box.SelectedIndex = -1;
                return;
            }
            int index = box.FindStringExact(text);
            if (index < 0)
            {
                index = box.Items.Add(text);
            }
            box.SelectedIndex = index;
        }

        /// <summary>Makes a grid read-only, one whole row selectable, with navy headers.</summary>
        public static void StyleGrid(DataGridView grid)
        {
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Navy;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 34;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(90, 104, 145);
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(243, 245, 250);
            grid.RowTemplate.Height = 28;
            grid.GridColor = Color.FromArgb(225, 228, 236);
        }

        /// <summary>A date from a grid cell, or today when the cell is empty.</summary>
        public static DateTime DateOr(object value)
        {
            return value is DateTime ? (DateTime)value : DateTime.Today;
        }

        /// <summary>Gives the columns of a grid friendly titles: pairs of (property name, title).</summary>
        public static void Titles(DataGridView grid, params string[] pairs)
        {
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                if (grid.Columns.Contains(pairs[i]))
                {
                    grid.Columns[pairs[i]].HeaderText = pairs[i + 1];
                }
            }
        }
    }
}
