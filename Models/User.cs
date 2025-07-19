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
        public float Height { get; set; }
        public float Weight { get; set; }
        public float GoalWeight { get; set; }
        public string UsedHeightUnit { get; set; }
        public string UsedWeightUnit { get; set; }
        public string LastUpdated = DateTime.UtcNow.ToString("dd/MM/yyyy");

        public User (string name, float height, float weight, float goalWeight, string usedHeightUnit, string usedWeightUnit)
        {
            Name = name;
            Height = height;
            Weight = weight;
            GoalWeight = goalWeight;
            UsedHeightUnit = usedHeightUnit;
            UsedWeightUnit = usedWeightUnit;
        }

        public float GetBmi()
        {
            float bmi = 0;

            if (Height != 0)
            {
                bmi = Weight / Height;
                bmi /= Height;
            }

            bmi *= 10000;

            return float.Parse(bmi.ToString("0.0"));
        }

        public string GetHeight()
        {
            return $"{Height.ToString("0.0")} {UsedHeightUnit}";
        }

        public string GetWeight()
        {
            return $"{Weight.ToString("0.0")} {UsedWeightUnit}";
        }

        public string GetGoalWeight()
        {
            return $"{GoalWeight.ToString("0.0")} {UsedWeightUnit}";
        }
    }
}
