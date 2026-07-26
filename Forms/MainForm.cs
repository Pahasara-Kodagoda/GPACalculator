using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using GPA_Calculator.Forms;
using Microsoft.Data.Sqlite;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace GPACalculator.Forms
{
    public class TableFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            if (File.Exists(fontPath))
            {
                return File.ReadAllBytes(fontPath);
            }
            return null;
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo("Arial#");
        }
    }

    public partial class MainForm : Form
    {
        private bool isDarkMode = false;
        private float currentCGPA = 0.00f;

        private System.Windows.Forms.Timer sidebarTimer;
        private bool isSidebarExpanded = true;
        private Button btnMenuToggle;

        private Panel pnlMainContent;
        private Panel pnlProfile;
        private Panel pnlCircularGPA;
        private Panel pnlCardGPA;
        private Panel pnlCardCredits;
        private Panel pnlCardClass;
        private Panel pnlChartCard;

        private System.Windows.Forms.Label lblCurrentGPA;
        private System.Windows.Forms.Label lblTotalCredits;
        private System.Windows.Forms.Label lblDegreeClass;
        private System.Windows.Forms.Label lblCopyright;

        private string studentName = "Your Name";
        private string studentDegree = "Software Engineering";
        private string universityName = "University Name";

        private System.Windows.Forms.Label lblNameVal;
        private System.Windows.Forms.Label lblDegreeVal;
        private System.Windows.Forms.Label lblUniVal;

        public MainForm()
        {
            InitializeComponent();

            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new TableFontResolver();
            }

            this.MinimumSize = new Size(950, 650);

            LoadProfileFromDatabase();
            UpdateDashboardStatistics();

            if (lblStudent != null) lblStudent.Visible = false;

            sidebarTimer = new System.Windows.Forms.Timer();
            sidebarTimer.Interval = 10;
            sidebarTimer.Tick += SidebarTimer_Tick;

            this.Resize += new EventHandler(MainForm_Resize);

            SetupMenuToggleButton();
            SetupThemeButton();
            BuildModernDashboard();
            AddButtonHoverEffects();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            UpdateDashboardStatistics();
            if (pnlChartCard != null) pnlChartCard.Invalidate();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized) return;

            int formWidth = this.ClientSize.Width;
            int formHeight = this.ClientSize.Height;

            if (sidebar != null) sidebar.Height = formHeight;
            if (header != null) header.Width = formWidth - sidebar.Width;

            if (pnlMainContent != null)
            {
                pnlMainContent.Size = new Size(formWidth - sidebar.Width, formHeight - header.Height);
                pnlMainContent.Location = new Point(sidebar.Width, header.Height);

                int contentWidth = pnlMainContent.Width;
                int leftMargin = 30;
                int rightMargin = 30;
                int availableWidth = contentWidth - leftMargin - rightMargin;

                if (pnlCircularGPA != null)
                {
                    pnlCircularGPA.Location = new Point(contentWidth - rightMargin - pnlCircularGPA.Width, 20);
                }
                if (pnlProfile != null && pnlCircularGPA != null)
                {
                    pnlProfile.Width = pnlCircularGPA.Left - leftMargin - 20;
                }

                int cardSpacing = 15;
                int totalCardWidth = availableWidth - (cardSpacing * 2);
                int singleCardWidth = totalCardWidth / 3;

                if (pnlCardGPA != null)
                {
                    pnlCardGPA.Size = new Size(singleCardWidth, 105);
                    pnlCardGPA.Location = new Point(leftMargin, 190);
                }
                if (pnlCardCredits != null)
                {
                    pnlCardCredits.Size = new Size(singleCardWidth, 105);
                    pnlCardCredits.Location = new Point(leftMargin + singleCardWidth + cardSpacing, 190);
                }
                if (pnlCardClass != null)
                {
                    pnlCardClass.Size = new Size(singleCardWidth, 105);
                    pnlCardClass.Location = new Point(leftMargin + (singleCardWidth * 2) + (cardSpacing * 2), 190);
                }

                if (pnlChartCard != null)
                {
                    pnlChartCard.Width = availableWidth;
                    pnlChartCard.Location = new Point(leftMargin, 315);
                }

                if (pnlChartCard != null)
                {
                    int actionButtonsY = pnlChartCard.Bottom + 20;
                    if (btnCalculate != null) btnCalculate.Location = new Point(leftMargin, actionButtonsY);
                    if (btnReport != null) btnReport.Location = new Point(leftMargin + 175, actionButtonsY);
                    if (btnExit != null) btnExit.Location = new Point(leftMargin + 315, actionButtonsY);

                    if (lblCopyright != null)
                    {
                        int centerX = (contentWidth / 2) - (lblCopyright.Width / 2);
                        int footerY = btnCalculate != null ? btnCalculate.Bottom + 25 : actionButtonsY + 60;
                        lblCopyright.Location = new Point(centerX, footerY);
                    }
                }
            }

            RepositionHeaderElements();
        }

        private void RepositionHeaderElements()
        {
            if (header == null) return;

            if (lblTitle != null)
            {
                int headerCenterX = (header.Width / 2) - (lblTitle.Width / 2);
                int headerCenterY = (header.Height / 2) - (lblTitle.Height / 2);
                lblTitle.Location = new Point(headerCenterX, headerCenterY);
            }

            if (btnMenuToggle != null && sidebar != null)
            {
                btnMenuToggle.Location = new Point(sidebar.Width + 10, 15);
            }
        }

        private void LoadProfileFromDatabase()
        {
            try
            {
                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Name, Degree, University FROM Profile LIMIT 1;";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                studentName = reader["Name"]?.ToString() ?? "Your Name";
                                studentDegree = reader["Degree"]?.ToString() ?? "Software Engineering";
                                universityName = reader["University"]?.ToString() ?? "University Name";
                            }
                            else
                            {
                                InsertDefaultProfile(connection);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InsertDefaultProfile(SqliteConnection connection)
        {
            string insertQuery = "INSERT INTO Profile (Name, Degree, University) VALUES (@name, @degree, @uni);";
            using (var cmd = new SqliteCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@name", studentName);
                cmd.Parameters.AddWithValue("@degree", studentDegree);
                cmd.Parameters.AddWithValue("@uni", universityName);
                cmd.ExecuteNonQuery();
            }
        }

        private void SaveProfileToDatabase(string name, string degree, string uni)
        {
            try
            {
                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "UPDATE Profile SET Name = @name, Degree = @degree, University = @uni WHERE Id = 1;";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@degree", degree);
                        cmd.Parameters.AddWithValue("@uni", uni);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            InsertDefaultProfile(connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving profile: " + ex.Message);
            }
        }

        private void UpdateDashboardStatistics()
        {
            double totalPointsEarned = 0;
            int totalCreditsEarned = 0;

            try
            {
                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Credits, Points FROM Courses;";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int credits = Convert.ToInt32(reader["Credits"]);
                                double points = Convert.ToDouble(reader["Points"]);

                                totalPointsEarned += (points * credits);
                                totalCreditsEarned += credits;
                            }
                        }
                    }
                }

                if (totalCreditsEarned > 0)
                {
                    currentCGPA = (float)(totalPointsEarned / totalCreditsEarned);
                }
                else
                {
                    currentCGPA = 0.00f;
                }

                if (lblCurrentGPA != null) lblCurrentGPA.Text = currentCGPA.ToString("0.00");
                if (lblTotalCredits != null) lblTotalCredits.Text = totalCreditsEarned.ToString();

                // Degree Class Display Fix
                if (lblDegreeClass != null)
                {
                    if (totalCreditsEarned == 0)
                    {
                        lblDegreeClass.Text = "N/A";
                    }
                    else if (currentCGPA >= 3.70f)
                    {
                        lblDegreeClass.Text = "First Class";
                    }
                    else if (currentCGPA >= 3.30f)
                    {
                        lblDegreeClass.Text = "Second Upper";
                    }
                    else if (currentCGPA >= 3.00f)
                    {
                        lblDegreeClass.Text = "Second Lower";
                    }
                    else if (currentCGPA >= 2.00f)
                    {
                        lblDegreeClass.Text = "General Pass";
                    }
                    else
                    {
                        lblDegreeClass.Text = "Re-sit Required";
                    }
                }

                if (pnlCircularGPA != null) pnlCircularGPA.Invalidate();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SetupMenuToggleButton()
        {
            btnMenuToggle = new Button();
            btnMenuToggle.Text = "☰";
            btnMenuToggle.Font = new System.Drawing.Font("Segoe UI", 16, FontStyle.Bold);
            btnMenuToggle.Size = new Size(50, 50);
            btnMenuToggle.FlatStyle = FlatStyle.Flat;
            btnMenuToggle.FlatAppearance.BorderSize = 0;
            btnMenuToggle.BackColor = Color.Transparent;
            btnMenuToggle.ForeColor = Color.FromArgb(37, 99, 235);
            btnMenuToggle.Cursor = Cursors.Hand;
            btnMenuToggle.Click += (s, e) => sidebarTimer.Start();

            header.Controls.Add(btnMenuToggle);
        }

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            if (isSidebarExpanded)
            {
                sidebar.Width -= 15;
                if (sidebar.Width <= 60)
                {
                    sidebar.Width = 60;
                    isSidebarExpanded = false;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar.Width += 15;
                if (sidebar.Width >= 230)
                {
                    sidebar.Width = 230;
                    isSidebarExpanded = true;
                    sidebarTimer.Stop();
                }
            }

            MainForm_Resize(null, null);
        }

        private void BuildModernDashboard()
        {
            pnlMainContent = new Panel();
            pnlMainContent.AutoScroll = true;
            pnlMainContent.Location = new Point(sidebar.Width, header.Height);
            pnlMainContent.Size = new Size(this.ClientSize.Width - sidebar.Width, this.ClientSize.Height - header.Height);
            this.Controls.Add(pnlMainContent);

            pnlProfile = new Panel();
            pnlProfile.Size = new Size(400, 150);
            pnlProfile.Location = new Point(30, 20);

            System.Windows.Forms.Label lblProfileTitle = new System.Windows.Forms.Label() { Text = "👤 Student Profile", Font = new System.Drawing.Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(15, 12), AutoSize = true, ForeColor = Color.FromArgb(37, 99, 235), BackColor = Color.Transparent };

            lblNameVal = new System.Windows.Forms.Label() { Text = "Name : " + studentName, Font = new System.Drawing.Font("Segoe UI", 9.5f), Location = new Point(15, 45), AutoSize = true, BackColor = Color.Transparent };
            lblDegreeVal = new System.Windows.Forms.Label() { Text = "Degree : " + studentDegree, Font = new System.Drawing.Font("Segoe UI", 9.5f), Location = new Point(15, 72), AutoSize = true, BackColor = Color.Transparent };
            lblUniVal = new System.Windows.Forms.Label() { Text = "University : " + universityName, Font = new System.Drawing.Font("Segoe UI", 9.5f), Location = new Point(15, 99), AutoSize = true, BackColor = Color.Transparent };

            Button btnEditProfile = new Button();
            btnEditProfile.Text = "✏️ Edit Profile";
            btnEditProfile.Size = new Size(100, 26);
            btnEditProfile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEditProfile.Location = new Point(285, 12);
            btnEditProfile.FlatStyle = FlatStyle.Flat;
            btnEditProfile.FlatAppearance.BorderSize = 0;
            btnEditProfile.BackColor = Color.FromArgb(239, 246, 255);
            btnEditProfile.ForeColor = Color.FromArgb(37, 99, 235);
            btnEditProfile.Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnEditProfile.Cursor = Cursors.Hand;
            btnEditProfile.Click += BtnEditProfile_Click;

            pnlProfile.Controls.Add(lblProfileTitle);
            pnlProfile.Controls.Add(lblNameVal);
            pnlProfile.Controls.Add(lblDegreeVal);
            pnlProfile.Controls.Add(lblUniVal);
            pnlProfile.Controls.Add(btnEditProfile);
            pnlProfile.Paint += GlassPanel_Paint;
            pnlMainContent.Controls.Add(pnlProfile);

            pnlCircularGPA = new Panel();
            pnlCircularGPA.Size = new Size(150, 150);
            pnlCircularGPA.Location = new Point(450, 20);
            pnlCircularGPA.Paint += PnlCircularGPA_Paint;
            pnlMainContent.Controls.Add(pnlCircularGPA);

            pnlCardGPA = CreateCard("Current GPA", "0.00", 30, 190, Color.FromArgb(37, 99, 235), 180, out lblCurrentGPA);
            pnlCardCredits = CreateCard("Total Credits", "0", 225, 190, Color.FromArgb(34, 197, 94), 180, out lblTotalCredits);
            pnlCardClass = CreateCard("Degree Class", "N/A", 420, 190, Color.FromArgb(245, 158, 11), 220, out lblDegreeClass);

            pnlMainContent.Controls.Add(pnlCardGPA);
            pnlMainContent.Controls.Add(pnlCardCredits);
            pnlMainContent.Controls.Add(pnlCardClass);

            pnlChartCard = new Panel();
            pnlChartCard.Size = new Size(610, 190);
            pnlChartCard.Location = new Point(30, 315);
            pnlChartCard.Paint += PnlChartCard_Paint;
            pnlChartCard.BackColor = Color.White;
            pnlMainContent.Controls.Add(pnlChartCard);

            btnCalculate.Parent = pnlMainContent;
            btnCalculate.Location = new Point(30, 520);
            btnCalculate.BackColor = Color.FromArgb(34, 197, 94);

            btnReport.Parent = pnlMainContent;
            btnReport.Location = new Point(205, 520);
            btnReport.BackColor = Color.FromArgb(59, 130, 246);

            btnExit.Parent = pnlMainContent;
            btnExit.Location = new Point(345, 520);
            btnExit.BackColor = Color.FromArgb(239, 68, 68);

            lblCopyright = new System.Windows.Forms.Label();
            lblCopyright.Text = "Copyright © 2026 Pahasara Kodagoda. All Rights Reserved.";
            lblCopyright.Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Regular);
            lblCopyright.ForeColor = Color.DimGray;
            lblCopyright.AutoSize = true;
            lblCopyright.BackColor = Color.Transparent;
            pnlMainContent.Controls.Add(lblCopyright);

            UpdateDashboardStatistics();
            MainForm_Resize(null, null);
        }

        private void BtnEditProfile_Click(object sender, EventArgs e)
        {
            ProfileForm profileForm = new ProfileForm(studentName, studentDegree, universityName);
            if (profileForm.ShowDialog() == DialogResult.OK)
            {
                studentName = profileForm.StudentName;
                studentDegree = profileForm.StudentDegree;
                universityName = profileForm.UniversityName;

                SaveProfileToDatabase(studentName, studentDegree, universityName);

                lblNameVal.Text = "Name : " + studentName;
                lblDegreeVal.Text = "Degree : " + studentDegree;
                lblUniVal.Text = "University : " + universityName;
            }
        }

        private Panel CreateCard(string title, string value, int x, int y, Color accentColor, int width, out System.Windows.Forms.Label valueLabel)
        {
            Panel card = new Panel();
            card.Size = new Size(width, 105);
            card.Location = new Point(x, y);

            Panel topBorder = new Panel() { Height = 4, Dock = DockStyle.Top, BackColor = accentColor };
            System.Windows.Forms.Label lblTitle = new System.Windows.Forms.Label() { Text = title, Font = new System.Drawing.Font("Segoe UI", 9.5f), ForeColor = Color.Gray, Location = new Point(12, 18), AutoSize = true, BackColor = Color.Transparent };

            float fontSize = (title == "Degree Class") ? 15f : 22f;
            valueLabel = new System.Windows.Forms.Label() { Text = value, Font = new System.Drawing.Font("Segoe UI", fontSize, FontStyle.Bold), ForeColor = Color.Black, Location = new Point(10, 45), AutoSize = true, BackColor = Color.Transparent };

            card.Controls.Add(topBorder);
            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);
            card.Paint += GlassPanel_Paint;

            return card;
        }

        private void GlassPanel_Paint(object sender, PaintEventArgs e)
        {
            Panel p = sender as Panel;
            Graphics g = e.Graphics;

            Color glassColor = isDarkMode ? Color.FromArgb(100, 40, 40, 40) : Color.FromArgb(150, 245, 247, 250);
            using (SolidBrush brush = new SolidBrush(glassColor))
            {
                g.FillRectangle(brush, p.ClientRectangle);
            }

            Color borderColor = isDarkMode ? Color.FromArgb(50, 255, 255, 255) : Color.FromArgb(100, 200, 200, 200);
            using (Pen borderPen = new Pen(borderColor, 1))
            {
                g.DrawRectangle(borderPen, 0, 0, p.Width - 1, p.Height - 1);
            }
        }

        private void PnlChartCard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Panel p = sender as Panel;
            g.Clear(isDarkMode ? Color.FromArgb(30, 30, 30) : Color.White);

            using (System.Drawing.Font titleFont = new System.Drawing.Font("Segoe UI", 10.5f, FontStyle.Bold))
            {
                g.DrawString("📊 Semester-wise GPA Analytics", titleFont, isDarkMode ? Brushes.White : Brushes.DarkBlue, new PointF(15, 10));
            }

            string[] shortSemesters = {
                "Y1S1", "Y1S2", "Y2S1", "Y2S2",
                "Y3S1", "Y3S2", "Y4S1", "Y4S2"
            };

            string[] dbSearchPatterns = {
                "%1%1%", "%1%2%", "%2%1%", "%2%2%",
                "%3%1%", "%3%2%", "%4%1%", "%4%2%"
            };

            float startX = 35;
            float baselineY = 155;
            float barWidth = 34;

            float totalAvailableChartWidth = p.Width - startX - 50;
            float spacing = (totalAvailableChartWidth - (barWidth * 8)) / 7;
            if (spacing < 15) spacing = 15;

            float maxHeight = 90;

            g.DrawLine(Pens.Gray, startX, baselineY, p.Width - 30, baselineY);

            try
            {
                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();

                    for (int i = 0; i < dbSearchPatterns.Length; i++)
                    {
                        string query = "SELECT Credits, Points FROM Courses WHERE Semester LIKE @semester;";
                        double totalPoints = 0;
                        int totalCredits = 0;

                        using (var cmd = new SqliteCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@semester", dbSearchPatterns[i]);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int credits = Convert.ToInt32(reader["Credits"]);
                                    double points = Convert.ToDouble(reader["Points"]);

                                    totalPoints += (points * credits);
                                    totalCredits += credits;
                                }
                            }
                        }

                        double semGPA = totalCredits > 0 ? (totalPoints / totalCredits) : 0.0;
                        float barHeight = (float)((semGPA / 4.0) * maxHeight);

                        float x = startX + 10 + (i * (barWidth + spacing));
                        float y = baselineY - barHeight;

                        if (semGPA > 0)
                        {
                            RectangleF barRect = new RectangleF(x, y, barWidth, barHeight);
                            using (LinearGradientBrush brush = new LinearGradientBrush(barRect, Color.FromArgb(59, 130, 246), Color.FromArgb(37, 99, 235), 90f))
                            {
                                g.FillRectangle(brush, barRect);
                            }
                        }

                        using (System.Drawing.Font valFont = new System.Drawing.Font("Segoe UI", 8, FontStyle.Bold))
                        {
                            string gpaStr = semGPA > 0 ? semGPA.ToString("0.00") : "0.00";
                            SizeF sz = g.MeasureString(gpaStr, valFont);
                            g.DrawString(gpaStr, valFont, isDarkMode ? Brushes.LightGray : Brushes.DarkSlateGray, x + (barWidth / 2) - (sz.Width / 2), y - 15);
                        }

                        using (System.Drawing.Font lblFont = new System.Drawing.Font("Segoe UI", 8F))
                        {
                            g.DrawString(shortSemesters[i], lblFont, isDarkMode ? Brushes.LightGray : Brushes.DimGray, x + 1, baselineY + 5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Chart Draw Error: " + ex.Message);
            }
        }

        private void PnlCircularGPA_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(10, 10, 130, 130);
            int thickness = 12;

            Color bgColor = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(228, 228, 231);
            using (Pen bgPen = new Pen(bgColor, thickness)) g.DrawArc(bgPen, rect, 0, 360);

            float sweepAngle = (currentCGPA / 4.0f) * 360f;
            using (Pen progressPen = new Pen(Color.FromArgb(37, 99, 235), thickness))
            {
                progressPen.StartCap = LineCap.Round;
                progressPen.EndCap = LineCap.Round;
                g.DrawArc(progressPen, rect, -90, sweepAngle);
            }

            float centerX = rect.X + (rect.Width / 2f);
            float centerY = rect.Y + (rect.Height / 2f);

            string gpaText = currentCGPA.ToString("0.00");
            System.Drawing.Font gpaFont = new System.Drawing.Font("Segoe UI", 22, FontStyle.Bold);
            Brush textBrush = new SolidBrush(isDarkMode ? Color.White : Color.Black);
            SizeF textSize = g.MeasureString(gpaText, gpaFont);
            g.DrawString(gpaText, gpaFont, textBrush, centerX - (textSize.Width / 2f), centerY - (textSize.Height / 2f) - 5);

            System.Drawing.Font maxFont = new System.Drawing.Font("Segoe UI", 9.5f, FontStyle.Bold);
            Brush maxBrush = new SolidBrush(Color.Gray);
            string maxText = "/ 4.00";
            SizeF maxTextSize = g.MeasureString(maxText, maxFont);
            g.DrawString(maxText, maxFont, maxBrush, centerX - (maxTextSize.Width / 2f), centerY + 14);
        }

        private void AddButtonHoverEffects()
        {
            btnCalculate.MouseEnter += (s, e) => btnCalculate.BackColor = Color.FromArgb(22, 163, 74);
            btnCalculate.MouseLeave += (s, e) => btnCalculate.BackColor = Color.FromArgb(34, 197, 94);

            btnReport.MouseEnter += (s, e) => btnReport.BackColor = Color.FromArgb(37, 99, 235);
            btnReport.MouseLeave += (s, e) => btnReport.BackColor = Color.FromArgb(59, 130, 246);

            btnExit.MouseEnter += (s, e) => btnExit.BackColor = Color.FromArgb(220, 38, 38);
            btnExit.MouseLeave += (s, e) => btnExit.BackColor = Color.FromArgb(239, 68, 68);
        }

        private void SetupThemeButton()
        {
            if (btnThemeToggle != null)
            {
                btnThemeToggle.Text = "🌙 Dark Mode";
                btnThemeToggle.Font = new System.Drawing.Font("Segoe UI", 10, FontStyle.Bold);
                btnThemeToggle.Size = new Size(150, 40);
                btnThemeToggle.Location = new Point(header.Width - 170, 20);
                btnThemeToggle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                btnThemeToggle.FlatStyle = FlatStyle.Flat;
                btnThemeToggle.FlatAppearance.BorderSize = 0;
                btnThemeToggle.BackColor = Color.FromArgb(240, 240, 245);
                btnThemeToggle.ForeColor = Color.FromArgb(50, 50, 50);
                btnThemeToggle.Cursor = Cursors.Hand;

                btnThemeToggle.Click -= btnThemeToggle_Click;
                btnThemeToggle.Click += btnThemeToggle_Click;

                btnThemeToggle.BringToFront();
            }
        }

        private void btnThemeToggle_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                this.BackColor = Color.FromArgb(18, 18, 18);
                if (pnlMainContent != null) pnlMainContent.BackColor = Color.FromArgb(18, 18, 18);
                header.BackColor = Color.FromArgb(25, 25, 25);

                if (btnThemeToggle != null)
                {
                    btnThemeToggle.Text = "☀️ Light Mode";
                    btnThemeToggle.BackColor = Color.FromArgb(60, 60, 60);
                    btnThemeToggle.ForeColor = Color.White;
                }
                if (btnMenuToggle != null) btnMenuToggle.ForeColor = Color.LightGray;
                if (lblCopyright != null) lblCopyright.ForeColor = Color.DarkGray;

                foreach (Control c in pnlProfile.Controls) if (c is System.Windows.Forms.Label) c.ForeColor = Color.LightGray;
                foreach (Control c in pnlCardGPA.Controls) if (c is System.Windows.Forms.Label && c.Font.Size >= 15) c.ForeColor = Color.White;
                foreach (Control c in pnlCardCredits.Controls) if (c is System.Windows.Forms.Label && c.Font.Size >= 15) c.ForeColor = Color.White;
                foreach (Control c in pnlCardClass.Controls) if (c is System.Windows.Forms.Label && c.Font.Size >= 15) c.ForeColor = Color.White;

                if (pnlChartCard != null) pnlChartCard.BackColor = Color.FromArgb(30, 30, 30);
            }
            else
            {
                this.BackColor = Color.FromArgb(245, 247, 250);
                if (pnlMainContent != null) pnlMainContent.BackColor = Color.FromArgb(245, 247, 250);
                header.BackColor = Color.White;

                if (btnThemeToggle != null)
                {
                    btnThemeToggle.Text = "🌙 Dark Mode";
                    btnThemeToggle.BackColor = Color.White;
                    btnThemeToggle.ForeColor = Color.FromArgb(50, 50, 50);
                }
                if (btnMenuToggle != null) btnMenuToggle.ForeColor = Color.FromArgb(37, 99, 235);
                if (lblCopyright != null) lblCopyright.ForeColor = Color.DimGray;

                foreach (Control c in pnlProfile.Controls) if (c is System.Windows.Forms.Label) c.ForeColor = Color.Black;
                foreach (Control c in pnlCardGPA.Controls) if (c is System.Windows.Forms.Label && c.Font.Size >= 15) c.ForeColor = Color.Black;
                foreach (Control c in pnlCardCredits.Controls) if (c is System.Windows.Forms.Label && c.Font.Size >= 15) c.ForeColor = Color.Black;
                foreach (Control c in pnlCardClass.Controls) if (c is System.Windows.Forms.Label && c.Font.Size >= 15) c.ForeColor = Color.Black;

                if (pnlChartCard != null) pnlChartCard.BackColor = Color.White;
            }

            pnlProfile.Invalidate();
            pnlCardGPA.Invalidate();
            pnlCardCredits.Invalidate();
            pnlCardClass.Invalidate();
            if (pnlCircularGPA != null) pnlCircularGPA.Invalidate();
            if (pnlChartCard != null) pnlChartCard.Invalidate();
        }

        private void semesterButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                SemesterForm semForm = new SemesterForm();
                semForm.SemesterName = btn.Text;
                semForm.ShowDialog();
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            UpdateDashboardStatistics();
            if (pnlChartCard != null) pnlChartCard.Invalidate();
            MessageBox.Show("Dashboard and Analytics updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            try
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Student GPA Report";

                PdfPage page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                XFont titleFont = new XFont("Arial", 20);
                XFont headerFont = new XFont("Arial", 12);
                XFont normalFont = new XFont("Arial", 10);

                int y = 40;

                string logo = @"Assets\logo.png";

                if (File.Exists(logo))
                {
                    XImage img = XImage.FromFile(logo);
                    gfx.DrawImage(img, 40, 25, 60, 60);
                }

                gfx.DrawString(universityName, titleFont, XBrushes.DarkBlue, new XPoint(110, 50));
                gfx.DrawString("OFFICIAL GPA REPORT", headerFont, XBrushes.Gray, new XPoint(110, 70));

                y = 110;
                gfx.DrawLine(XPens.DarkGray, 40, y, 550, y);
                y += 30;

                gfx.DrawString("Student Name :", headerFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(studentName, normalFont, XBrushes.Black, new XPoint(170, y));

                y += 25;
                gfx.DrawString("Degree :", headerFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(studentDegree, normalFont, XBrushes.Black, new XPoint(170, y));

                y += 25;
                gfx.DrawString("University :", headerFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(universityName, normalFont, XBrushes.Black, new XPoint(170, y));

                y += 40;

                double totalPoints = 0;
                int totalCredits = 0;

                using (var con = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string sql = "SELECT Credits, Points FROM Courses";

                    using (var cmd = new SqliteCommand(sql, con))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            int c = Convert.ToInt32(rd["Credits"]);
                            double p = Convert.ToDouble(rd["Points"]);

                            totalCredits += c;
                            totalPoints += (c * p);
                        }
                    }
                }

                double cgpa = totalCredits == 0 ? 0 : totalPoints / totalCredits;

                string degreeClass = "N/A";
                if (totalCredits > 0)
                {
                    if (cgpa >= 3.70) degreeClass = "First Class";
                    else if (cgpa >= 3.30) degreeClass = "Second Upper";
                    else if (cgpa >= 3.00) degreeClass = "Second Lower";
                    else if (cgpa >= 2.00) degreeClass = "General Pass";
                    else degreeClass = "Re-sit Required";
                }

                gfx.DrawString("CGPA", headerFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(cgpa.ToString("0.00"), normalFont, XBrushes.DarkBlue, new XPoint(170, y));

                y += 25;
                gfx.DrawString("Credits", headerFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(totalCredits.ToString(), normalFont, XBrushes.DarkBlue, new XPoint(170, y));

                y += 25;
                gfx.DrawString("Degree Class", headerFont, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(degreeClass, normalFont, XBrushes.DarkBlue, new XPoint(170, y));

                y += 45;

                gfx.DrawRectangle(XPens.Black, 40, y, 500, 25);

                gfx.DrawString("Semester", headerFont, XBrushes.Black, new XPoint(50, y + 17));
                gfx.DrawString("Credits", headerFont, XBrushes.Black, new XPoint(220, y + 17));
                gfx.DrawString("GPA", headerFont, XBrushes.Black, new XPoint(360, y + 17));

                y += 25;

                using (var con = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string sql = @"SELECT Semester,
                                  SUM(Credits) Credits,
                                  ROUND(SUM(Credits*Points)/SUM(Credits),2) GPA
                                  FROM Courses
                                  GROUP BY Semester";

                    using (var cmd = new SqliteCommand(sql, con))
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            gfx.DrawRectangle(XPens.Gray, 40, y, 500, 22);

                            gfx.DrawString(rd["Semester"].ToString(), normalFont, XBrushes.Black, new XPoint(50, y + 15));
                            gfx.DrawString(rd["Credits"].ToString(), normalFont, XBrushes.Black, new XPoint(230, y + 15));
                            gfx.DrawString(rd["GPA"].ToString(), normalFont, XBrushes.Black, new XPoint(370, y + 15));

                            y += 22;
                        }
                    }
                }

                gfx.DrawString("Generated : " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), normalFont, XBrushes.Gray, new XPoint(40, 800));

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PDF Files|*.pdf";
                sfd.FileName = "Student_GPA_Report.pdf";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    document.Save(sfd.FileName);
                    document.Close();

                    MessageBox.Show("PDF Generated Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}