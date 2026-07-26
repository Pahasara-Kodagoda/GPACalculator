using System;
using System.IO;
using Microsoft.Data.Sqlite; // අලුත් namespace එක

namespace GPACalculator.Services
{
    public static class DatabaseHelper
    {
        private static string dbName = "gpa_database.db";
        private static string connectionString = $"Data Source={dbName}";

        public static SqliteConnection GetConnection()
        {
            return new SqliteConnection(connectionString);
        }

        public static void InitializeDatabase()
        {
            bool fileExists = File.Exists(dbName);

            using (var connection = GetConnection())
            {
                connection.Open();

                string createProfileTable = @"
                    CREATE TABLE IF NOT EXISTS Profile (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT,
                        Degree TEXT,
                        University TEXT
                    );";

                string createCourseTable = @"
                    CREATE TABLE IF NOT EXISTS Courses (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Semester TEXT,
                        CourseCode TEXT,
                        CourseName TEXT,
                        Credits INTEGER,
                        Grade TEXT,
                        Points REAL
                    );";

                using (var cmd1 = new SqliteCommand(createProfileTable, connection))
                {
                    cmd1.ExecuteNonQuery();
                }

                using (var cmd2 = new SqliteCommand(createCourseTable, connection))
                {
                    cmd2.ExecuteNonQuery();
                }
            }
        }
    }
}