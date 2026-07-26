using System;
using System.Collections.Generic;

namespace GPACalculator.Models
{
    public partial class Semester
    {

        public string SemesterName { get; set; }


        public List<Course> Courses { get; set; }


        public double GPA { get; set; }



        public Semester()
        {
            Courses = new List<Course>();
        }



        public Semester(string name)
        {
            SemesterName = name;

            Courses = new List<Course>();
        }

    }
}