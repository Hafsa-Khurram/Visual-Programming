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
    public partial class splash : Form
    {
        private BackgroundWorker backgroundWorker;
        public splash()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Connect to SQL Server and create the database/tables if needed,
            // while the progress bar runs.
            Exception error = null;
            var setup = Task.Run(() =>
            {
                try { Db.Initialize(); }
                catch (Exception ex) { error = ex; }
            });
            for (int i = 0; i <= 100; i++)
            {
                System.Threading.Thread.Sleep(25);
                if (i == 90)
                {
                    setup.Wait(); // hold at 90% until the database is ready
                }
                backgroundWorker.ReportProgress(i);
            }
            e.Result = error;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            label1.Text = $"Loading {e.ProgressPercentage}% ...";
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var ex = e.Result as Exception;
            if (ex != null)
            {
                MessageBox.Show(this, ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            // Loading is complete: open the welcome screen and hide the splash.
            main form2 = new main();
            form2.Show();
            this.Hide();
        }

        private void InitializeBackgroundWorker()
        {
            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            backgroundWorker.DoWork += backgroundWorker1_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted;
        }

        private void splash_Load(object sender, EventArgs e)
        {
            backgroundWorker.RunWorkerAsync();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void logopanel_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
    }
}
