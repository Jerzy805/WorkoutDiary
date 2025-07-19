using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingDiary.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Muscle { get; set; }
        public string Weight { get; set; }
        public int Repeats { get; set; }
        public DateTime Date { get; set; }

        public Session(int id, string muscle, string weight, int repeats, DateTime date)
        {
            Id = id;
            Muscle = muscle;
            Weight = weight;
            Repeats = repeats;
            Date = date;
        }

        public Exercise TransferToExercise()
        {
            var name = Muscle;
            int sets = 1;
            int reps = 1;
            var comment = string.Empty;
            var date = Date;
            var weight = Weight;

            var total = Repeats.ToString();

            if (total.Length == 3)
            {
                reps = int.Parse(total[0].ToString());
                sets = int.Parse(total[2].ToString());
            }
            else if (total.Length == 4)
            {
                reps = int.Parse(total[0].ToString());
                sets = int.Parse(total[2].ToString());
            }
            else if (total.Length > 4)
            {
                reps = int.Parse($"{total[0].ToString()}{total[1].ToString()}");
                sets = int.Parse(total[3].ToString());
            }

            return new Exercise(name, sets, reps, weight, comment, date);
        }
    }
}
