using System;
using System.Collections.Generic;

namespace GPACalculator.Models
{
    public class Student
    {

        public string StudentName { get; set; }


        public string StudentID { get; set; }


        public string Degree { get; set; }



        public List<Semester> Semesters { get; set; }



        public Student()
        {
            Semesters = new List<Semester>();


            for (int i = 1; i <= 8; i++)
            {
                Semesters.Add(
                    new Semester(
                        "Semester " + i
                    )
                );
            }
        }


    }
}