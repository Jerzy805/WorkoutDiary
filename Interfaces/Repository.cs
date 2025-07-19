using Newtonsoft.Json;
using TrainingDiary.Models;

namespace TrainingDiary.Interfaces
{
    class Repository : IRepository
    {
        private static readonly string JsonDirectoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal),
            "SelfProgressApp",
            "JSON"
            );

        private readonly string exercisesFilePath = GetJsonFilePath("exercises.json");
        private static readonly string musclesFilePath = GetJsonFilePath("muscles.json");
        private static readonly string userFilePath = GetJsonFilePath("user.json");
        private static readonly string workoutsFilePath = GetJsonFilePath("workouts.json");
        private static readonly string progressFilePath = GetJsonFilePath("progress.json");
        private static readonly string fontSizeFilePath = GetJsonFilePath("fontSize.json");
        private static readonly string closureInfoFilePath = GetJsonFilePath("closureInfo.json");

        private static string GetJsonFilePath(string fileName)
        {
            return Path.Combine(JsonDirectoryPath, fileName);
        }

        private void MakeDirectory()
        {
            var directoryPath = Path.GetDirectoryName(fontSizeFilePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public async Task<List<Exercise>> GetExercisesAsync()
        {
            MakeDirectory();

            if (File.Exists(exercisesFilePath))
            {
                var data = await File.ReadAllTextAsync(exercisesFilePath);
                return JsonConvert.DeserializeObject<List<Exercise>>(data)!;
            }

            return new List<Exercise>();
        }

        public List<Exercise> GetExercises()
        {
            MakeDirectory();

            if (File.Exists(exercisesFilePath))
            {
                var data = File.ReadAllText(exercisesFilePath);
                return JsonConvert.DeserializeObject<List<Exercise>>(data)!;
            }

            return new List<Exercise>();
        }

        public async Task SaveExercisesAsync(List<Exercise> exercises)
        {
            var data = JsonConvert.SerializeObject(exercises);
            MakeDirectory();
            await File.WriteAllTextAsync(exercisesFilePath, data);
            // techniczne zapisanie danych bez informacji na interfejsie

            var newMuscles = exercises.Select(e => e.Name).Distinct().ToList();
            var muscles = await GetMusclesAsync();
            muscles.AddRange(newMuscles);
            muscles = muscles.Distinct().ToList();
            await SaveMusclesAsync(muscles);
        }

        public async Task<List<string>> GetMusclesAsync()
        {
            MakeDirectory();

            if (File.Exists(musclesFilePath))
            {
                var data = await File.ReadAllTextAsync(musclesFilePath);
                var muscles = JsonConvert.DeserializeObject<List<string>>(data)!;
                muscles.Sort();

                return muscles;
            }

            var list = new List<string>();

            list.Add("Bench Press");
            list.Add("L. Raises");
            list.Add("Dips");
            list.Add("S. Press");
            list.Add("Biceps");
            list.Add("Pullups");
            list.Add("Pulldowns");

            list = list.Distinct().ToList();

            list.Sort();

            SaveMuscles(list);

            return list;
        } // muscles -> nazwy ćwiczeń

        public List<string> GetMuscles()
        {
            MakeDirectory();

            if (File.Exists(musclesFilePath))
            {
                var data = File.ReadAllText(musclesFilePath);
                var muscles = JsonConvert.DeserializeObject<List<string>>(data)!;
                muscles.Sort();

                return muscles;
            }

            var list = new List<string>();

            list.Add("Bench Press");
            list.Add("L. Raises");
            list.Add("Dips");
            list.Add("S. Press");

            list.Sort();

            SaveMuscles(list);

            return list;
        } // muscles -> nazwy ćwiczeń

        public async void SaveMuscles(List<string> muscles)
        {
            //var exercises = await GetExercisesAsync();

            //if (exercises.Count != 0)
            //{
            //    foreach (var muscle in muscles)
            //    {
            //        var exercisesWithMuscle = exercises.Where(e => e.Name == muscle);
                    
            //        if (exercisesWithMuscle.Count() == 0)
            //        {
            //            muscles.Remove(muscle);
            //        }
            //    }
            //}

            var data = JsonConvert.SerializeObject(muscles);
            File.WriteAllText(musclesFilePath, data);
        }

        public async Task SaveMusclesAsync(List<string> muscles)
        {
            //var exercises = await GetExercisesAsync();

            //if (exercises.Count != 0)
            //{
            //    foreach (var muscle in muscles)
            //    {
            //        var exercisesWithMuscle = exercises.Where(e => e.Name == muscle);

            //        if (exercisesWithMuscle.Count() == 0)
            //        {
            //            muscles.Remove(muscle);
            //        }
            //    }
            //}

            var data = JsonConvert.SerializeObject(muscles);
            await File.WriteAllTextAsync(musclesFilePath, data);
        }

        public User? GetUser()
        {
            MakeDirectory();

            if (File.Exists(userFilePath))
            {
                var data = File.ReadAllText(userFilePath);
                return JsonConvert.DeserializeObject<User>(data);
            }

            return null;
        }

        public void SaveUser(User user)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(user);
            File.WriteAllText(userFilePath, data);

            var progress = GetProgress();

            progress.Add(user);

            SaveProgress(progress);
        }

        public async Task SaveUserAsync(User user)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(user);
            await File.WriteAllTextAsync(userFilePath, data);

            var progress = await GetProgressAsync();

            progress.Add(user);

            await SaveProgressAsync(progress);
        }

        public List<Workout> GetWorkouts()
        {
            MakeDirectory();

            if (File.Exists(workoutsFilePath))
            {
                var data = File.ReadAllText(workoutsFilePath);
                return JsonConvert.DeserializeObject<List<Workout>>(data)!;
            }

            return [];
        }

        public async Task<List<Workout>> GetWorkoutsAsync()
        {
            MakeDirectory();

            if (File.Exists(workoutsFilePath))
            {
                var data = await File.ReadAllTextAsync(workoutsFilePath);
                return JsonConvert.DeserializeObject<List<Workout>>(data)!;
            }

            return [];
        }

        public void SaveWorkouts(List<Workout> workouts)
        {
            //foreach (var workout in workouts)
            //{
            //    workout.Exercises = workout.Exercises.OrderByDescending(w => w.Name).ToList();
            //}

            // ewentualnie dodać obsługę samodzielnego ustalania kolejności treningu

            var data = JsonConvert.SerializeObject(workouts);
            File.WriteAllText(workoutsFilePath, data);
        }

        public async Task SaveWorkoutsAsync(List<Workout> workouts)
        {
            //foreach (var workout in workouts)
            //{
            //    workout.Exercises = workout.Exercises.OrderByDescending(w => w.Name).ToList();
            //}

            var data = JsonConvert.SerializeObject(workouts);
            await File.WriteAllTextAsync(workoutsFilePath, data);
        }

        public List<string> GetWorkoutNames()
        {
            return GetWorkouts().Select(w => w.Name).ToList();
        }

        public async Task<List<string>> GetWorkoutNamesAsync()
        {
            var workouts = await GetWorkoutsAsync();
            return workouts.Select(w => w.Name).ToList();
        }

        public double GetFontSize()
        {
            MakeDirectory();

            if (File.Exists(fontSizeFilePath))
            {
                var data = File.ReadAllText(fontSizeFilePath);
                return JsonConvert.DeserializeObject<double>(data);
            }

            SaveFontSize(1);

            return 1;
        }

        public async Task<double> GetFontSizeAsync()
        {
            MakeDirectory();

            if (File.Exists(fontSizeFilePath))
            {
                var data = await File.ReadAllTextAsync(fontSizeFilePath);
                return JsonConvert.DeserializeObject<double>(data);
            }

            await SaveFontSizeAsync(1);

            return 1;
        }

        public void SaveFontSize(double fontSize)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(fontSize);
            File.WriteAllText(fontSizeFilePath, data);
        }

        public async Task SaveFontSizeAsync(double fontSize)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(fontSize);
            await File.WriteAllTextAsync(fontSizeFilePath, data);
        }

        public List<User> GetProgress()
        {
            MakeDirectory();

            if (File.Exists(progressFilePath))
            {
                var data = File.ReadAllText(progressFilePath);
                return JsonConvert.DeserializeObject<List<User>>(data)!.OrderByDescending(e => e.LastUpdated).ToList();
            }

            return new();
        }

        public async Task<List<User>> GetProgressAsync()
        {
            MakeDirectory();

            if (File.Exists(progressFilePath))
            {
                var data = await File.ReadAllTextAsync(progressFilePath);
                return JsonConvert.DeserializeObject<List<User>>(data)!.OrderByDescending(e => e.LastUpdated).ToList();
            }

            return new();
        }

        public void SaveProgress(List<User> progress)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(progress);
            File.WriteAllText(progressFilePath, data);
        }

        public async Task SaveProgressAsync(List<User> progress)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(progress);
            await File.WriteAllTextAsync(progressFilePath, data);
        }

        public bool GetClosureInfo()
        {
            MakeDirectory();

            if (File.Exists(closureInfoFilePath))
            {
                var data = File.ReadAllText(closureInfoFilePath);
                return JsonConvert.DeserializeObject<bool>(data);
            }

            return false;
        }

        public async Task SaveClosureInfo(bool info)
        {
            MakeDirectory();

            var data = JsonConvert.SerializeObject(info);
            await File.WriteAllTextAsync(closureInfoFilePath, data);
        }
    }
}