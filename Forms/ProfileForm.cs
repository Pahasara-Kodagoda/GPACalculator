using System;
using System.Drawing;
using System.Windows.Forms;

namespace GPA_Calculator.Forms
{
    public partial class ProfileForm : Form
    {
        public string StudentName { get; private set; }
        public string StudentDegree { get; private set; }
        public string UniversityName { get; private set; }

        private TextBox txtNameBox;
        private TextBox txtDegreeBox;
        private ComboBox cmbUniBox;
        private Button btnSaveProfile;
        private Button btnCancelProfile;

        public ProfileForm(string currentName, string currentDegree, string currentUni)
        {
            //InitializeComponent(); // Designer conflict මඟහරින්න මේක Comment කරලා පහත UI එක පාවිච්චි කරන්න පුළුවන්

            // Manual UI Initialization to avoid missing designer controls
            this.Text = "Edit Student Profile";
            this.Size = new Size(420, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Name Label & TextBox
            Label lblName = new Label() { Text = "Student Name:", Location = new Point(30, 30), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtNameBox = new TextBox() { Text = currentName, Location = new Point(30, 55), Size = new Size(340, 25), Font = new Font("Segoe UI", 10) };

            // Degree Label & TextBox
            Label lblDegree = new Label() { Text = "Degree Program:", Location = new Point(30, 95), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtDegreeBox = new TextBox() { Text = currentDegree, Location = new Point(30, 120), Size = new Size(340, 25), Font = new Font("Segoe UI", 10) };

            // University Label & ComboBox
            Label lblUni = new Label() { Text = "University / Institute:", Location = new Point(30, 160), AutoSize = true, Font = new Font("Segoe UI", 10) };
            cmbUniBox = new ComboBox() { Location = new Point(30, 185), Size = new Size(340, 25), Font = new Font("Segoe UI", 10), DropDownStyle = ComboBoxStyle.DropDown };

            LoadUniversities(currentUni);

            // Save Button
            btnSaveProfile = new Button() { Text = "Save", Location = new Point(190, 230), Size = new Size(90, 35), BackColor = Color.FromArgb(34, 197, 94), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSaveProfile.Click += BtnSaveProfile_Click;

            // Cancel Button
            btnCancelProfile = new Button() { Text = "Cancel", Location = new Point(290, 230), Size = new Size(80, 35), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancelProfile.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Add controls to form
            this.Controls.Add(lblName);
            this.Controls.Add(txtNameBox);
            this.Controls.Add(lblDegree);
            this.Controls.Add(txtDegreeBox);
            this.Controls.Add(lblUni);
            this.Controls.Add(cmbUniBox);
            this.Controls.Add(btnSaveProfile);
            this.Controls.Add(btnCancelProfile);
        }

        private void LoadUniversities(string currentUni)
        {
            cmbUniBox.Items.Clear();
            cmbUniBox.Items.AddRange(new string[] {
                "RUSL - Rajarata University of Sri Lanka",
                "UOM - University of Moratuwa",
                "UOC - University of Colombo",
                "UOP - University of Peradeniya",
                "UOJ - University of Jaffna",
                "UOR - University of Ruhuna",
                "SJP - University of Sri Jayewardenepura",
                "KDU - General Sir John Kotelawala Defence University",
                "NSBM Green University",
                "SLIIT - Sri Lanka Institute of Information Technology",
                "IIT - Informatics Institute of Technology",
                "Other"
            });

            if (cmbUniBox.Items.Contains(currentUni))
            {
                cmbUniBox.SelectedItem = currentUni;
            }
            else
            {
                cmbUniBox.Text = currentUni;
            }
        }

        private void BtnSaveProfile_Click(object sender, EventArgs e)
        {
            StudentName = txtNameBox.Text.Trim();
            StudentDegree = txtDegreeBox.Text.Trim();
            UniversityName = cmbUniBox.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}