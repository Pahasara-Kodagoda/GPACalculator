using System;

namespace GPACalculator.Models
{
    public partial class Course
    {
        public string CourseCode { get; set; }

        public string CourseName { get; set; }

        public int Credits { get; set; }

        public string Grade { get; set; }

        public double GradePoint { get; set; }


        public Course()
        {

        }


        public Course(
            string code,
            string name,
            int credits,
            string grade)
        {
            CourseCode = code;
            CourseName = name;
            Credits = credits;
            Grade = grade;
        }
    }
}