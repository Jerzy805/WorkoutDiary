using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainingDiary.Models;

namespace TrainingDiary.Interfaces
{
    interface IRepository
    {
        void MakeDirectory();
        Task<List<Exercise>> GetExercisesAsync();
        List<Exercise> GetExercises();
        Task SaveExercisesAsync(List<Exercise> exercises);
        Task<List<string>> GetMusclesAsync(); // muscles -> nazwy ćwiczeń
        List<string> GetMuscles(); // muscles -> nazwy ćwiczeń
        void SaveMuscles(List<string> muscles);
        Task SaveMusclesAsync(List<string> muscles);
        User? GetUser();
    }
}
