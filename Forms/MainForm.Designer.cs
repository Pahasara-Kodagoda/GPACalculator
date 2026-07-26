using System;
using System.Drawing;
using System.Windows.Forms;

namespace GPACalculator.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private Panel sidebar;
        private Panel header;
        private Label lblTitle;
        private Label lblStudent;
        private Button btnSem1;
        private Button btnSem2;
        private Button btnSem3;
        private Button btnSem4;
        private Button btnSem5;
        private Button btnSem6;
        private Button btnSem7;
        private Button btnSem8;
        private Button btnCalculate;
        private Button btnReport;
        private Button btnExit;
        private Button btnThemeToggle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.sidebar = new Panel();
            this.header = new Panel();
            this.lblTitle = new Label();
            this.lblStudent = new Label();
            this.btnSem1 = new Button();
            this.btnSem2 = new Button();
            this.btnSem3 = new Button();
            this.btnSem4 = new Button();
            this.btnSem5 = new Button();
            this.btnSem6 = new Button();
            this.btnSem7 = new Button();
            this.btnSem8 = new Button();
            this.btnCalculate = new Button();
            this.btnReport = new Button();
            this.btnExit = new Button();
            this.btnThemeToggle = new Button();

            // FORM PROPERTIES
            this.Text = "University GPA Calculator";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.MinimumSize = new Size(900, 600);

            // SIDEBAR PANEL
            this.sidebar.Dock = DockStyle.Left;
            this.sidebar.Width = 230;
            this.sidebar.BackColor = Color.FromArgb(30, 41, 59);

            // HEADER PANEL
            this.header.Dock = DockStyle.Top;
            this.header.Height = 80;
            this.header.BackColor = Color.White;

            // TITLE LABEL
            this.lblTitle.Text = "🎓 GPA Calculator";
            this.lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.FromArgb(37, 99, 235);
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new Point(300, 20);

            // SIDEBAR BUTTONS SETUP (MODERN LOOK)
            Button[] semesterButtons = {
                btnSem1, btnSem2, btnSem3, btnSem4,
                btnSem5, btnSem6, btnSem7, btnSem8
            };

            // Add buttons in reverse order so the first button appears visually at the top
            for (int i = semesterButtons.Length - 1; i >= 0; i--)
            {
                semesterButtons[i].Text = "  Year " + ((i / 2) + 1) + " • Sem " + ((i % 2) + 1);
                semesterButtons[i].AutoSize = false;
                semesterButtons[i].Height = 48;
                semesterButtons[i].Dock = DockStyle.Top;
                semesterButtons[i].Margin = new Padding(0);
                semesterButtons[i].FlatStyle = FlatStyle.Flat;
                semesterButtons[i].FlatAppearance.BorderSize = 0;
                semesterButtons[i].ForeColor = Color.FromArgb(203, 213, 225);
                semesterButtons[i].BackColor = Color.FromArgb(30, 41, 59);
                semesterButtons[i].Font = new Font("Segoe UI", 10, FontStyle.Bold);
                semesterButtons[i].Cursor = Cursors.Hand;
                semesterButtons[i].TextAlign = ContentAlignment.MiddleLeft;
                semesterButtons[i].Padding = new Padding(20, 0, 0, 0);

                int index = i; // Closure capture for hover effect
                semesterButtons[i].MouseEnter += (s, e) =>
                {
                    semesterButtons[index].BackColor = Color.FromArgb(51, 65, 85);
                    semesterButtons[index].ForeColor = Color.White;
                };
                semesterButtons[i].MouseLeave += (s, e) =>
                {
                    semesterButtons[index].BackColor = Color.FromArgb(30, 41, 59);
                    semesterButtons[index].ForeColor = Color.FromArgb(203, 213, 225);
                };

                semesterButtons[i].Click += new System.EventHandler(this.semesterButton_Click);
                this.sidebar.Controls.Add(semesterButtons[i]);
            }

            // DASHBOARD ACTION BUTTONS
            this.btnCalculate.Text = "Calculate GPA";
            this.btnCalculate.Location = new Point(270, 440);
            this.btnCalculate.Size = new Size(160, 45);
            this.btnCalculate.FlatStyle = FlatStyle.Flat;
            this.btnCalculate.FlatAppearance.BorderSize = 0;
            this.btnCalculate.BackColor = Color.FromArgb(34, 197, 94);
            this.btnCalculate.ForeColor = Color.White;
            this.btnCalculate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnCalculate.Cursor = Cursors.Hand;
            this.btnCalculate.Click += new System.EventHandler(this.btnCalculate_Click);

            this.btnReport.Text = "Report";
            this.btnReport.Location = new Point(450, 440);
            this.btnReport.Size = new Size(120, 45);
            this.btnReport.FlatStyle = FlatStyle.Flat;
            this.btnReport.FlatAppearance.BorderSize = 0;
            this.btnReport.BackColor = Color.FromArgb(59, 130, 246);
            this.btnReport.ForeColor = Color.White;
            this.btnReport.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnReport.Cursor = Cursors.Hand;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);

            this.btnExit.Text = "Exit";
            this.btnExit.Location = new Point(590, 440);
            this.btnExit.Size = new Size(120, 45);
            this.btnExit.FlatStyle = FlatStyle.Flat;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.BackColor = Color.FromArgb(239, 68, 68);
            this.btnExit.ForeColor = Color.White;
            this.btnExit.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            this.btnExit.Cursor = Cursors.Hand;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);

            // ADD CONTROLS TO HEADER & FORM
            this.header.Controls.Add(this.lblTitle);
            this.header.Controls.Add(this.btnThemeToggle);
            this.Controls.Add(this.header);
            this.Controls.Add(this.sidebar);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.btnReport);
            this.Controls.Add(this.btnExit);
        }
    }
}