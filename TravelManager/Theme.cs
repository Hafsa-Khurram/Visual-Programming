using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TravelManager
{
    /// <summary>
    /// Gives every screen the same modern look at run time: clean fonts, white cards
    /// behind forms and tables, coloured rounded buttons with hover effects and a
    /// highlighted item in the side menu. The layouts made in the designer stay the same.
    /// </summary>
    internal static class Theme
    {
        public static readonly Color Navy = Color.FromArgb(48, 56, 79);
        public static readonly Color NavyDark = Color.FromArgb(36, 42, 61);
        public static readonly Color NavyHover = Color.FromArgb(66, 77, 108);
        public static readonly Color Gold = Color.FromArgb(232, 168, 56);
        public static readonly Color Page = Color.FromArgb(240, 243, 249);
        public static readonly Color Ink = Color.FromArgb(37, 44, 63);
        public static readonly Color Muted = Color.FromArgb(104, 114, 138);
        public static readonly Color Line = Color.FromArgb(214, 220, 232);
        public static readonly Color Blue = Color.FromArgb(52, 101, 196);
        public static readonly Color Red = Color.FromArgb(206, 64, 64);
        public static readonly Color Grey = Color.FromArgb(226, 230, 239);

        private const string Font = "Segoe UI";

        /// <summary>Call at the end of a form's constructor.</summary>
        public static void Apply(Form form)
        {
            form.SuspendLayout();
            Control sidebar = form.Controls.Find("dashboardPanel1", true).FirstOrDefault();

            foreach (Control c in All(form))
            {
                StyleControl(c, sidebar);
            }

            if (sidebar != null)
            {
                StyleWorkspace(form, sidebar);
            }
            else if (form.BackColor == SystemColors.Control)
            {
                // Small dialogs (logout, terms): white with a navy frame.
                form.BackColor = Color.White;
                form.Padding = new Padding(2);
                form.Paint += (s, e) =>
                {
                    using (var pen = new Pen(Navy, 4))
                    {
                        e.Graphics.DrawRectangle(pen, 0, 0, form.ClientSize.Width, form.ClientSize.Height);
                    }
                };
            }
            form.ResumeLayout(true);
        }

        private static IEnumerable<Control> All(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                yield return c;
                foreach (Control child in All(c))
                {
                    yield return child;
                }
            }
        }

        private static Font SegoeLike(Font old, float? size = null, FontStyle? style = null)
        {
            return new Font(Font, size ?? old.Size, style ?? old.Style & ~FontStyle.Underline);
        }

        private static void StyleControl(Control c, Control sidebar)
        {
            string family = c.Font.OriginalFontName ?? c.Font.Name;
            bool oldFont = family == "Times New Roman" || family == "Microsoft Sans Serif";

            if (c is Button)
            {
                StyleButton((Button)c, sidebar != null && sidebar.Contains(c));
            }
            else if (c is TextBox || c is RichTextBox)
            {
                var t = (TextBoxBase)c;
                t.BorderStyle = BorderStyle.FixedSingle;
                t.Font = new Font(Font, t.ReadOnly ? 10.5F : 11F, t.ReadOnly ? FontStyle.Bold : FontStyle.Regular);
                t.ForeColor = Ink;
            }
            else if (c is ComboBox)
            {
                var cb = (ComboBox)c;
                cb.Font = new Font(Font, 10.5F);
                cb.ForeColor = Ink;
            }
            else if (c is DateTimePicker)
            {
                var d = (DateTimePicker)c;
                d.Font = new Font(Font, 10F);
                d.Format = DateTimePickerFormat.Custom;
                d.CustomFormat = "dd MMMM yyyy";
                d.CalendarTitleBackColor = Navy;
                d.CalendarTitleForeColor = Color.White;
                d.CalendarMonthBackground = Color.White;
            }
            else if (c is CheckBox)
            {
                c.Font = new Font(Font, 9F);
                c.Cursor = Cursors.Hand;
            }
            else if (c is LinkLabel)
            {
                var link = (LinkLabel)c;
                link.Font = new Font(Font, 9F, FontStyle.Bold);
                link.LinkColor = Blue;
                link.ActiveLinkColor = Gold;
                link.LinkBehavior = LinkBehavior.HoverUnderline;
            }
            else if (c is Label && oldFont)
            {
                float size = c.Font.Size >= 11F && c.Font.Size < 16F ? 11.5F : c.Font.Size;
                c.Font = SegoeLike(c.Font, size);
                if (c.ForeColor == Color.Black || c.ForeColor.IsSystemColor)
                {
                    c.ForeColor = Ink;
                }
            }
            else if (c is ProgressBar)
            {
                c.ForeColor = Gold;
            }
        }

        // ---------- Buttons ----------

        private static void StyleButton(Button b, bool inSidebar)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.Cursor = Cursors.Hand;
            b.UseVisualStyleBackColor = false;

            if (b.Name == "exit")
            {
                b.Font = new Font(Font, 11F, FontStyle.Bold);
                b.ForeColor = Color.White;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = Red;
                b.FlatAppearance.MouseDownBackColor = Color.FromArgb(170, 45, 45);
                return;
            }

            if (inSidebar)
            {
                b.BackColor = Navy;
                b.ForeColor = Color.FromArgb(225, 230, 242);
                b.Font = new Font(Font, 11.5F, FontStyle.Bold);
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = NavyHover;
                b.FlatAppearance.MouseDownBackColor = NavyDark;
                b.TextAlign = ContentAlignment.MiddleLeft;
                b.Padding = new Padding(22, 0, 0, 0);
                b.Text = b.Text.Trim();
                return;
            }

            string text = b.Text.Trim().ToLowerInvariant();
            Color back = Navy, hover = NavyHover, fore = Color.White;
            if (text.StartsWith("update"))
            {
                back = Blue;
                hover = Color.FromArgb(72, 124, 222);
            }
            else if (text.StartsWith("delete"))
            {
                back = Red;
                hover = Color.FromArgb(226, 88, 88);
            }
            else if (text.StartsWith("clear") || text.StartsWith("cancel") || text.StartsWith("back"))
            {
                back = Grey;
                hover = Color.FromArgb(210, 216, 229);
                fore = Ink;
            }
            else if (b.BackColor == Color.White)
            {
                // White buttons on the navy welcome screen.
                back = Color.White;
                hover = Gold;
                fore = Navy;
            }

            b.BackColor = back;
            b.ForeColor = fore;
            b.Font = new Font(Font, Math.Max(10F, Math.Min(b.Font.Size, 12F)), FontStyle.Bold);
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = hover;
            b.FlatAppearance.MouseDownBackColor = back;
            Round(b, 8);
        }

        /// <summary>Rounds the corners of a control (keeps them rounded when it is resized).</summary>
        public static void Round(Control c, int radius)
        {
            Action apply = () =>
            {
                if (c.Width <= 0 || c.Height <= 0)
                {
                    return;
                }
                using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, c.Width, c.Height), radius))
                {
                    c.Region = new Region(path);
                }
            };
            apply();
            c.Resize += (s, e) => apply();
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ---------- Pages with the side menu ----------

        private static void StyleWorkspace(Form form, Control sidebar)
        {
            form.BackColor = Page;

            // Side menu: logo area and the button of the current page.
            sidebar.BackColor = Navy;
            Control logo = sidebar.Controls.Find("logopanel", false).FirstOrDefault();
            if (logo != null)
            {
                logo.BackColor = NavyDark;
                foreach (Control l in logo.Controls)
                {
                    l.BackColor = Color.Transparent;
                }
            }
            string current = CurrentMenuButton(form);
            Control active = current == null ? null : sidebar.Controls.Find(current, true).FirstOrDefault();
            if (active is Button)
            {
                var b = (Button)active;
                b.BackColor = NavyHover;
                b.FlatAppearance.MouseOverBackColor = NavyHover;
                b.ForeColor = Color.White;
                var marker = new Panel { BackColor = Gold, Width = 5, Height = b.Height, Location = new Point(b.Left, b.Top) };
                sidebar.Controls.Add(marker);
                marker.BringToFront();
            }

            // Page title: modern font with a gold line under it.
            Label title = form.Controls.OfType<Label>()
                .Where(l => !sidebar.Contains(l))
                .OrderByDescending(l => l.Font.Size)
                .FirstOrDefault();
            if (title != null && title.Font.Size >= 16)
            {
                int center = title.Left + title.Width / 2;
                title.Font = new Font(Font, 22F, FontStyle.Bold);
                title.ForeColor = Navy;
                title.AutoSize = true;
                title.Left = center - title.PreferredWidth / 2;
                var bar = new Panel
                {
                    BackColor = Gold,
                    Size = new Size(64, 4),
                    Location = new Point(center - 32, title.Top + title.PreferredHeight + 2),
                };
                form.Controls.Add(bar);
            }

            // White card behind the input fields and buttons, and another behind the table.
            Control grid = form.Controls.Find("dataGridView1", false).FirstOrDefault();
            Control bottom = form.Controls.Find("bottompanel", false).FirstOrDefault();
            Control exit = form.Controls.Find("exit", false).FirstOrDefault();
            var inputs = form.Controls.Cast<Control>()
                .Where(c => c != sidebar && c != grid && c != bottom && c != exit && c != title && !(c is Panel))
                .ToList();
            int minTop = title == null ? 0 : title.Top + title.PreferredHeight + 14;
            Rectangle inputArea = inputs.Count > 0
                ? inputs.Select(c => c.Bounds).Aggregate(Rectangle.Union)
                : Rectangle.Empty;

            if (grid != null)
            {
                var dgv = (DataGridView)grid;
                dgv.BorderStyle = BorderStyle.None;
                // Leave room for the input card above the table.
                int needTop = inputs.Count > 0 ? inputArea.Bottom + 8 + 12 + 10 : 0;
                if (grid.Top < needTop)
                {
                    int shift = needTop - grid.Top;
                    grid.Top += shift;
                    grid.Height = Math.Max(120, grid.Height - shift);
                }
                Card(form, new List<Control> { grid }, 10, int.MaxValue, 0);
            }
            if (inputs.Count > 0)
            {
                int maxBottom = grid == null ? int.MaxValue : grid.Top - 10 - 12;
                Card(form, inputs, 18, Math.Max(maxBottom, inputArea.Bottom + 8), minTop);
            }
        }

        /// <summary>Moves the given controls into a white rounded card that fits around them.</summary>
        private static Panel Card(Form form, List<Control> controls, int padding, int maxBottom, int minTop)
        {
            Rectangle bounds = controls.Select(c => c.Bounds).Aggregate(Rectangle.Union);
            bounds.Inflate(padding, padding);
            if (bounds.Top < minTop)
            {
                bounds.Height -= minTop - bounds.Top;
                bounds.Y = minTop;
            }
            if (bounds.Bottom > maxBottom)
            {
                bounds.Height = maxBottom - bounds.Top;
            }
            var card = new Panel
            {
                BackColor = Color.White,
                Bounds = bounds,
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(Line))
                using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            };
            form.Controls.Add(card);
            foreach (Control c in controls)
            {
                Point p = c.Location;
                form.Controls.Remove(c);
                c.Location = new Point(p.X - bounds.X, p.Y - bounds.Y);
                card.Controls.Add(c);
                if (c is Label)
                {
                    c.BackColor = Color.Transparent;
                }
            }
            Round(card, 12);
            card.SendToBack();
            return card;
        }

        private static string CurrentMenuButton(Form form)
        {
            switch (form.GetType().Name)
            {
                case "home": return "Homebutton";
                case "packages": return "Packagesbutton";
                case "bookings": return "Bookingsbutton";
                case "payments": return "Paymentsbutton";
                case "viewbookings": return "ViewBbutton";
                case "viewpayments": return "ViewPbutton";
                default: return null;
            }
        }
    }
}
