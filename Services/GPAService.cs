using System;
using System.Collections.Generic;
using System.Linq;
using GPACalculator.Models;


namespace GPACalculator.Services
{

    public static partial class GPAService
    {


        public static double CalculateSemesterGPA(
            List<Course> courses)
        {

            double totalPoints = 0;

            int totalCredits = 0;



            foreach (var course in courses)
            {

                course.GradePoint =
                    GradeConverter.GetPoint(course.Grade);


                totalPoints +=
                    course.GradePoint *
                    course.Credits;


                totalCredits +=
                    course.Credits;

            }



            if (totalCredits == 0)
                return 0;



            return Math.Round(
                totalPoints / totalCredits,
                2
            );

        }




        public static double CalculateCGPA(
            List<Semester> semesters)
        {

            double totalPoints = 0;

            int totalCredits = 0;



            foreach (var semester in semesters)
            {

                foreach (var course in semester.Courses)
                {

                    course.GradePoint =
                        GradeConverter.GetPoint(
                            course.Grade
                        );


                    totalPoints +=
                        course.GradePoint *
                        course.Credits;


                    totalCredits +=
                        course.Credits;

                }

            }



            if (totalCredits == 0)
                return 0;



            return Math.Round(
                totalPoints / totalCredits,
                2
            );

        }





        public static string GetDegreeClass(
            double cgpa)
        {

            if (cgpa >= 3.70)
                return "First Class";


            else if (cgpa >= 3.30)
                return "Second Upper";


            else if (cgpa >= 3.00)
                return "Second Lower";


            else if (cgpa >= 2.00)
                return "General Degree";


            else
                return "Fail";

        }


    }

}