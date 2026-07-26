using System;
using System.Windows.Forms;
using GPACalculator.Services; // Namespace එකතු කරගන්න

namespace GPACalculator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ඩේටාබේස් එක සහ Tables මුලින්ම සකස් කරගැනීම
            DatabaseHelper.InitializeDatabase();

            Application.Run(new Forms.MainForm());
        }
    }
}