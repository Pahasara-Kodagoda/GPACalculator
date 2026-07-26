using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace GPACalculator.Forms
{
    public partial class SemesterForm : Form
    {
        public string SemesterName { get; set; } = "Year 1 • Sem 1";

        private DataGridView dgvCourses;
        private TextBox txtCourseCode;
        private TextBox txtCourseName;
        private NumericUpDown numCredits;
        private ComboBox cmbGrade;
        private Button btnAddCourse;
        private Button btnDeleteCourse;

        private Panel pnlGpaSummary;
        private Label lblGpaTitle;
        private Label lblSemesterGPA;

        public SemesterForm()
        {
            InitializeComponent();

            // Constructor එකෙන්ම UI එක Build කරලා Load කරමු
            this.Load += (s, e) =>
            {
                this.Text = "Manage Courses - " + SemesterName;
                BuildUI();
                LoadCoursesFromDatabase();
                CalculateAndDisplaySemesterGPA();
            };
        }

        private void BuildUI()
        {
            this.Size = new Size(750, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            this.Controls.Clear(); // හිස් නොවී පෙනීම සඳහා Clean කිරීම

            Label lblHeader = new Label();
            lblHeader.Text = "📚 " + SemesterName + " Courses";
            lblHeader.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblHeader.ForeColor = Color.FromArgb(37, 99, 235);
            lblHeader.Location = new Point(20, 15);
            lblHeader.AutoSize = true;
            this.Controls.Add(lblHeader);

            dgvCourses = new DataGridView();
            dgvCourses.Location = new Point(20, 55);
            dgvCourses.Size = new Size(700, 260);
            dgvCourses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCourses.BackgroundColor = Color.White;
            dgvCourses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvCourses.MultiSelect = false;
            dgvCourses.ReadOnly = true;
            dgvCourses.AllowUserToAddRows = false;
            dgvCourses.RowHeadersVisible = false;
            this.Controls.Add(dgvCourses);

            Label lblCode = new Label() { Text = "Course Code:", Location = new Point(20, 332), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtCourseCode = new TextBox() { Location = new Point(20, 352), Size = new Size(110, 25), Font = new Font("Segoe UI", 9.5f) };

            Label lblName = new Label() { Text = "Course Name:", Location = new Point(140, 332), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtCourseName = new TextBox() { Location = new Point(140, 352), Size = new Size(200, 25), Font = new Font("Segoe UI", 9.5f) };

            Label lblCred = new Label() { Text = "Credits:", Location = new Point(350, 332), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            numCredits = new NumericUpDown() { Location = new Point(350, 352), Size = new Size(70, 25), Minimum = 1, Maximum = 10, Value = 3, Font = new Font("Segoe UI", 9.5f) };

            Label lblGrade = new Label() { Text = "Grade:", Location = new Point(430, 332), AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            cmbGrade = new ComboBox() { Location = new Point(430, 352), Size = new Size(80, 25), DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
            cmbGrade.Items.AddRange(new object[] { "A+", "A", "A-", "B+", "B", "B-", "C+", "C", "C-", "D+", "D", "E" });
            cmbGrade.SelectedIndex = 0;

            btnAddCourse = new Button();
            btnAddCourse.Text = "➕ Add";
            btnAddCourse.Location = new Point(520, 350);
            btnAddCourse.Size = new Size(90, 30);
            btnAddCourse.BackColor = Color.FromArgb(34, 197, 94);
            btnAddCourse.ForeColor = Color.White;
            btnAddCourse.FlatStyle = FlatStyle.Flat;
            btnAddCourse.FlatAppearance.BorderSize = 0;
            btnAddCourse.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnAddCourse.Cursor = Cursors.Hand;
            btnAddCourse.Click += BtnAddCourse_Click;

            btnDeleteCourse = new Button();
            btnDeleteCourse.Text = "🗑️ Delete";
            btnDeleteCourse.Location = new Point(620, 350);
            btnDeleteCourse.Size = new Size(100, 30);
            btnDeleteCourse.BackColor = Color.FromArgb(239, 68, 68);
            btnDeleteCourse.ForeColor = Color.White;
            btnDeleteCourse.FlatStyle = FlatStyle.Flat;
            btnDeleteCourse.FlatAppearance.BorderSize = 0;
            btnDeleteCourse.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnDeleteCourse.Cursor = Cursors.Hand;
            btnDeleteCourse.Click += BtnDeleteCourse_Click;

            this.Controls.Add(lblCode);
            this.Controls.Add(txtCourseCode);
            this.Controls.Add(lblName);
            this.Controls.Add(txtCourseName);
            this.Controls.Add(lblCred);
            this.Controls.Add(numCredits);
            this.Controls.Add(lblGrade);
            this.Controls.Add(cmbGrade);
            this.Controls.Add(btnAddCourse);
            this.Controls.Add(btnDeleteCourse);

            // Highlighted Semester GPA Card
            pnlGpaSummary = new Panel();
            pnlGpaSummary.Size = new Size(700, 65);
            pnlGpaSummary.Location = new Point(20, 420);
            pnlGpaSummary.BackColor = Color.FromArgb(239, 246, 255);
            pnlGpaSummary.BorderStyle = BorderStyle.FixedSingle;

            lblGpaTitle = new Label();
            lblGpaTitle.Text = "SEMESTER GPA:";
            lblGpaTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblGpaTitle.ForeColor = Color.FromArgb(71, 85, 105);
            lblGpaTitle.Location = new Point(20, 20);
            lblGpaTitle.AutoSize = true;

            lblSemesterGPA = new Label();
            lblSemesterGPA.Text = "0.00 / 4.00";
            lblSemesterGPA.Font = new Font("Segoe UI", 22, FontStyle.Bold);
            lblSemesterGPA.ForeColor = Color.FromArgb(34, 197, 94);
            lblSemesterGPA.Location = new Point(190, 10);
            lblSemesterGPA.AutoSize = true;

            pnlGpaSummary.Controls.Add(lblGpaTitle);
            pnlGpaSummary.Controls.Add(lblSemesterGPA);
            this.Controls.Add(pnlGpaSummary);
        }

        private double GetGradePoints(string grade)
        {
            switch (grade)
            {
                case "A+": case "A": return 4.00;
                case "A-": return 3.70;
                case "B+": return 3.30;
                case "B": return 3.00;
                case "B-": return 2.70;
                case "C+": return 2.30;
                case "C": return 2.00;
                case "C-": return 1.70;
                case "D+": return 1.30;
                case "D": return 1.00;
                default: return 0.00;
            }
        }

        private void LoadCoursesFromDatabase()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Id", typeof(int));
                dt.Columns.Add("CourseCode", typeof(string));
                dt.Columns.Add("CourseName", typeof(string));
                dt.Columns.Add("Credits", typeof(int));
                dt.Columns.Add("Grade", typeof(string));
                dt.Columns.Add("Points", typeof(double));

                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string query = "SELECT Id, CourseCode, CourseName, Credits, Grade, Points FROM Courses WHERE Semester = @semester;";
                    using (var cmd = new SqliteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@semester", SemesterName);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                dt.Rows.Add(
                                    reader["Id"],
                                    reader["CourseCode"],
                                    reader["CourseName"],
                                    reader["Credits"],
                                    reader["Grade"],
                                    reader["Points"]
                                );
                            }
                        }
                    }
                }
                dgvCourses.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading courses: " + ex.Message);
            }
        }

        private void CalculateAndDisplaySemesterGPA()
        {
            double totalPoints = 0;
            int totalCredits = 0;

            try
            {
                using (var con = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    con.Open();
                    string sql = "SELECT Credits, Points FROM Courses WHERE Semester = @sem;";

                    using (var cmd = new SqliteCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@sem", SemesterName);
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
                }

                double semGPA = totalCredits == 0 ? 0.00 : totalPoints / totalCredits;

                if (lblSemesterGPA != null)
                {
                    lblSemesterGPA.Text = semGPA.ToString("0.00") + " / 4.00";

                    if (semGPA >= 3.70)
                        lblSemesterGPA.ForeColor = Color.FromArgb(34, 197, 94);
                    else if (semGPA >= 3.00)
                        lblSemesterGPA.ForeColor = Color.FromArgb(37, 99, 235);
                    else if (semGPA >= 2.00)
                        lblSemesterGPA.ForeColor = Color.FromArgb(245, 158, 11);
                    else
                        lblSemesterGPA.ForeColor = Color.FromArgb(239, 68, 68);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GPA Calc Error: " + ex.Message);
            }
        }

        private void BtnAddCourse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourseCode.Text) || string.IsNullOrWhiteSpace(txtCourseName.Text))
            {
                MessageBox.Show("Please enter Course Code and Course Name.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string code = txtCourseCode.Text.Trim();
            string name = txtCourseName.Text.Trim();
            int credits = (int)numCredits.Value;
            string grade = cmbGrade.SelectedItem.ToString();
            double points = GetGradePoints(grade);

            try
            {
                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO Courses (Semester, CourseCode, CourseName, Credits, Grade, Points) VALUES (@sem, @code, @name, @cred, @grade, @pts);";
                    using (var cmd = new SqliteCommand(insertQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@sem", SemesterName);
                        cmd.Parameters.AddWithValue("@code", code);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@cred", credits);
                        cmd.Parameters.AddWithValue("@grade", grade);
                        cmd.Parameters.AddWithValue("@pts", points);

                        cmd.ExecuteNonQuery();
                    }
                }

                txtCourseCode.Clear();
                txtCourseName.Clear();
                numCredits.Value = 3;
                cmbGrade.SelectedIndex = 0;

                LoadCoursesFromDatabase();
                CalculateAndDisplaySemesterGPA();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding course: " + ex.Message);
            }
        }

        private void BtnDeleteCourse_Click(object sender, EventArgs e)
        {
            if (dgvCourses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a course to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(dgvCourses.SelectedRows[0].Cells["Id"].Value);

            try
            {
                using (var connection = GPACalculator.Services.DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM Courses WHERE Id = @id;";
                    using (var cmd = new SqliteCommand(deleteQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                LoadCoursesFromDatabase();
                CalculateAndDisplaySemesterGPA();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting course: " + ex.Message);
            }
        }
    }
}