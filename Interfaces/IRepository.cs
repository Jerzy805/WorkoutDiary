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
        Task<List<Exercise>> GetExercisesAsync();
        List<Exercise> GetExercises();
        Task SaveExercisesAsync(List<Exercise> exercises);
        Task<List<string>> GetMusclesAsync(); // muscles -> nazwy ćwiczeń
        List<string> GetMuscles(); // muscles -> nazwy ćwiczeń
        void SaveMuscles(List<string> muscles);
        Task SaveMusclesAsync(List<string> muscles);
        User? GetUser();
        void SaveUser(User user);
        Task SaveUserAsync(User user);
        List<Workout> GetWorkouts();
        Task<List<Workout>> GetWorkoutsAsync();
        void SaveWorkouts(List<Workout> workouts);
        Task SaveWorkoutsAsync(List<Workout> workouts);
        List<string> GetWorkoutNames();
        Task<List<string>> GetWorkoutNamesAsync();
        double GetFontSize();
        Task<double> GetFontSizeAsync();
        void SaveFontSize(double fontSize);
        Task SaveFontSizeAsync(double fontSize);
        List<User> GetProgress();
        Task<List<User>> GetProgressAsync();
        void SaveProgress(List<User> progress);
        Task SaveProgressAsync(List<User> progress);
        bool GetClosureInfo();
        Task SaveClosureInfo(bool info);
    }
}
