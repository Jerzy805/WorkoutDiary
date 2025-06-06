using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingDiary.Models
{
    public class Exercise 
    {
        public string Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public string Weight { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }

        public Exercise (string name, int sets, int reps, string weight, string comment, DateTime date)
        {
            Name = name;
            Sets = sets;
            Reps = reps;
            Comment = comment;
            Date = date;
            Weight = weight;
        }

        //public Exercise(string name, int sets, int reps, string weight, DateTime date)
        //{
        //    Name = name;
        //    Sets = sets;
        //    Reps = reps;
        //    Comment = string.Empty;
        //    Date = date; 
        //    Weight = weight;
        //}

        public string GetDate()
        {
            return Date.ToString("dd-MM-yyyy");
        }
    }
}
