using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingDiary.Models
{
    public class MuscleDTO
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public MuscleDTO(string name, int count)
        {
            Name = name;
            Count = count;
        }
    }
}
