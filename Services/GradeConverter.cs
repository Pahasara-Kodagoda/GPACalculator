using System.Collections.Generic;

namespace GPACalculator.Services
{

    public static class GradeConverter
    {

        private static readonly Dictionary<string, double> grades =
            new()
            {
                {"A+",4.00},
                {"A",4.00},
                {"A-",3.70},

                {"B+",3.30},
                {"B",3.00},
                {"B-",2.70},

                {"C+",2.30},
                {"C",2.00},
                {"C-",1.70},

                {"D+",1.30},
                {"D",1.00},

                {"E",0.00}
            };



        public static double GetPoint(string grade)
        {

            if (grades.ContainsKey(grade))
            {
                return grades[grade];
            }


            return 0.00;

        }



        public static string[] GetGrades()
        {
            return new string[]
            {
                "A+",
                "A",
                "A-",
                "B+",
                "B",
                "B-",
                "C+",
                "C",
                "C-",
                "D+",
                "D",
                "E"
            };
        }

    }

}