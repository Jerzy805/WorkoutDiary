using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingDiary.Models
{
    public class User
    {
        public string Name { get; set; }
        public int Height { get; set; }
        public float Weight { get; set; }
        public float GoalWeight { get; set; }

        public User (string name, int height, float weight, float goalWeight)
        {
            Name = name;
            Height = height;
            Weight = weight;
            GoalWeight = goalWeight;
        }

        public float GetBmi()
        {
            float bmi = 0;

            if (Height != 0)
            {
                bmi = Weight / Height;
                bmi /= Height;
            }

            return bmi;
        }
    }
}
