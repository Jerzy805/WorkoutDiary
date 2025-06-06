using Newtonsoft.Json;

using TrainingDiary.Models;

namespace TrainingDiary.Interfaces
{
    class Repository : IRepository
    {
        private readonly static string folderPath = "/storage/emulated/0/Documents/SelfProgressApp/JSON";
        private readonly static string exercisesFilePath = $"{folderPath}/exercises.json";
        private readonly static string musclesFilePath = $"{folderPath}/muscles.json";
        private readonly static string userFilePath = $"{folderPath}/user.json";

        public void MakeDirectory()
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
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

            return new List<Exercise>().OrderByDescending(e => e.Date).ToList(); // placeholder
        }

        public List<Exercise> GetExercises()
        {
            MakeDirectory();

            if (File.Exists(exercisesFilePath))
            {
                var data = File.ReadAllText(exercisesFilePath);
                return JsonConvert.DeserializeObject<List<Exercise>>(data)!;
            }

            return new List<Exercise>().OrderByDescending(e => e.Date).ThenByDescending(e => e.Name).ToList(); // placeholder
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
                return JsonConvert.DeserializeObject<List<string>>(data)!;
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

            SaveMuscles(list);

            return list;
        } // muscles -> nazwy ćwiczeń

        public List<string> GetMuscles()
        {
            MakeDirectory();

            if (File.Exists(musclesFilePath))
            {
                var data = File.ReadAllText(musclesFilePath);
                return JsonConvert.DeserializeObject<List<string>>(data)!;
            }

            var list = new List<string>();

            list.Add("Bench Press");
            list.Add("L. Raises");
            list.Add("Dips");
            list.Add("S. Press");

            SaveMuscles(list);

            return list;
        } // muscles -> nazwy ćwiczeń

        public void SaveMuscles(List<string> muscles)
        {
            var data = JsonConvert.SerializeObject(muscles);
            File.WriteAllText(musclesFilePath, data);
        }

        public async Task SaveMusclesAsync(List<string> muscles)
        {
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
    }
}
