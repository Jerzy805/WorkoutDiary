using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingDiary.Models
{
    public class Workout
    {
        public string Name { get; set; }
        public List<Exercise> Exercises { get; set; }
        public DateTime Date { get; set; }

        public Workout (string name, List<Exercise> exercises, DateTime date)
        {
            Name = name;
            Exercises = exercises;
            Date = date;
        }
    }
}
