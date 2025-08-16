using TrainingDiary.Interfaces;
using TrainingDiary.Models;

namespace TrainingDiary;

public partial class Settings : ContentPage
{
	private readonly IRepository repository;
	private static Settings instance;
	public static User ? user;

	public Settings()
	{
		InitializeComponent();

		this.repository = new Repository();

		user = repository.GetUser()!;

		UserNullLabel.FontSize = 20 * MainPage.fontSize;

		WeightLabel.FontSize = 20 * MainPage.fontSize;

		HeightLabel.FontSize = 20 * MainPage.fontSize;

        GoalWeightLabel.FontSize = 20 * MainPage.fontSize;

		LastUpdateLabel.FontSize = 19 * MainPage.fontSize;

        ChangeStatsButton.FontSize = 24 * MainPage.fontSize;

        var controls = UserGrid.Children;

		controls.Remove(UserNullLabel);

		if (user == null)
		{
			foreach (var control in controls)
			{
				if (control is View v)
				{
					v.IsVisible = false;
				}
			}

			MuscleGrid.IsVisible = false;

			UserNullLabel.IsVisible = true;
			UserNullLabel.Text = "Configure your data in \"Home\" section";

			return;
		}

        WeightLabel.Text = $"Weight: {user.GetWeight()}";
        HeightLabel.Text = $"Height: {user.GetHeight()}";
        GoalWeightLabel.Text = $"Target weight: {user.GetGoalWeight()}";
        LastUpdateLabel.Text = $"Last update: {user.LastUpdated}";

		MainPage.isSettingsLoaded = true;

		instance = this;
    }

	public static Settings GetSettings()
	{
		return instance;
	}

	public void UpdateFontSize()
	{
		var userLabels = new List<Label>()
		{ 
			UserNullLabel,
			WeightLabel,
			GoalWeightLabel,
			HeightLabel,
			LastUpdateLabel
		};

        var muscleLabels = MuscleInputGrid.OfType<Label>();

		var labels = new List<Label>();
		labels.AddRange(userLabels);
		labels.AddRange(muscleLabels);

        foreach (var label in labels)
        {
            label.FontSize = 20 * MainPage.fontSize;
        }

		ChangeStatsButton.FontSize = 24 * MainPage.fontSize;
    }

    private void ChangeStatsButton_Clicked(object sender, EventArgs e)
    {
        var controls = UserGrid.Children;

        controls.Remove(UserNullLabel);
		//controls.Remove(LastUpdateLabel);

        foreach (var control in controls)
        {
            if (control is View v)
            {
                v.IsVisible = false;
            }
        }

		LastUpdateLabel.IsVisible = true;

		var weightEntry = new Entry();
		weightEntry.FontSize = 20 * MainPage.fontSize;
		weightEntry.Placeholder = $"Weight ({user.UsedWeightUnit})";
		weightEntry.Text = user.Weight.ToString("0.0");
		weightEntry.Keyboard = Keyboard.Numeric;

        var heightEntry = new Entry();
        heightEntry.FontSize = 20 * MainPage.fontSize;
        heightEntry.Placeholder = $"Height ({user.UsedHeightUnit})";
        heightEntry.Text = user.Height.ToString("0.0");
        heightEntry.Keyboard = Keyboard.Numeric;

        var goalWeightEntry = new Entry();
        goalWeightEntry.FontSize = 20 * MainPage.fontSize;
        goalWeightEntry.Placeholder = $"Target weight ({user.UsedWeightUnit})";
        goalWeightEntry.Text = user.GoalWeight.ToString("0.0");
        goalWeightEntry.Keyboard = Keyboard.Numeric;

		var acceptButton = new Button();
		acceptButton.FontSize = 20 * MainPage.fontSize;
		acceptButton.Text = "Save";
		acceptButton.Margin = new Thickness(30, 0);

		acceptButton.Clicked += async (sender, e) =>
		{
			if (!float.TryParse(weightEntry.Text, out var weight) || !float.TryParse(heightEntry.Text, out var height)
				|| !float.TryParse(goalWeightEntry.Text, out var goalWeight))
			{
                await DisplayAlert("Workout Diary", "Enter all data.", "Ok");
				return;
            }

			user.Weight = weight;
			user.Height = height;
			user.GoalWeight = goalWeight;
			user.LastUpdated = DateTime.UtcNow.ToString("dd/MM/yyyy");

            await repository.SaveUserAsync(user);

            await Task.Run(() =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UserGrid.Children.Remove(weightEntry);
                    UserGrid.Children.Remove(heightEntry);
                    UserGrid.Children.Remove(goalWeightEntry);
                    UserGrid.Children.Remove(acceptButton);
                });
            });

            foreach (var control in controls)
            {
                if (control is View v)
                {
                    v.IsVisible = true;
                }
            }

            LastUpdateLabel.IsVisible = true;

            WeightLabel.Text = $"Weight: {user.GetWeight()}";
            HeightLabel.Text = $"Height: {user.GetHeight()}";
            GoalWeightLabel.Text = $"Target weight: {user.GetGoalWeight()}";
			LastUpdateLabel.Text = $"Last update: {user.LastUpdated}";

            string text = "Target weight achieved! Now just maintain it.";

            var suffix = $": {Math.Abs(user.Weight - user.GoalWeight)} {user.UsedWeightUnit}";

            if (user.Weight > user.GoalWeight)
            {
                text = $"Fat to burn{suffix}";
            }

            if (user.Weight < user.GoalWeight)
            {
                text = $"Weight to gain{suffix}";
            }

            MainPage.WelcomeLabel2.Text = text;
			
			if (MainPage.isProgressTrackingLoaded)
			{
                ProgressTracking.DisplayProgress();
                ProgressTracking.Progress = await repository.GetProgressAsync();
            }
        };

		UserGrid.Add(weightEntry, 0, 0);
		UserGrid.Add(heightEntry, 1, 0);
		UserGrid.Add(goalWeightEntry, 0, 1);
		UserGrid.Add(acceptButton, 0, 2);
		UserGrid.SetColumnSpan(acceptButton, 2);

    }

    private async void EditMusclesButton_Clicked(object sender, EventArgs e)
    {
		InfoLabel.IsVisible = boxView.IsVisible = !InfoLabel.IsVisible;

		MuscleInputGrid.IsVisible = !MuscleInputGrid.IsVisible;

		MuscleInputGrid.RowDefinitions.Clear();
		MuscleInputGrid.Clear();

		var sessions = await repository.GetExercisesAsync();

        var muscleNames = await repository.GetMusclesAsync();

        var exercises = sessions.Select(s => s.Name).ToList();

		exercises = exercises.GroupBy(e => e).OrderByDescending(g => g.Count())
			.SelectMany(o => o).Distinct().ToList();

		exercises.AddRange(muscleNames);

		exercises = exercises.Distinct().ToList();

		//var tasks = new List<Task>();

		foreach (var exercise in exercises)
		{
			var count = sessions.Where(s => s.Name == exercise).Count();

			var row = exercises.IndexOf(exercise);

			MuscleInputGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

			var nameLabel = new Label();
			nameLabel.FontSize = 20 * MainPage.fontSize;
            nameLabel.Text = $"{exercise}: {count}";

			var editButton = new Button();
			editButton.FontSize = 20;
			editButton.Text = "✏️";

			var deleteButton = new Button();
			deleteButton.FontSize = 20;
			deleteButton.Text = "🗑️";

			editButton.Clicked += async (sender, e) =>
			{
				deleteButton.IsEnabled = nameLabel.IsVisible = false;

				var buttons = MuscleInputGrid.OfType<Button>();

				foreach (var button in buttons)
				{
					button.IsEnabled = false;
				}

				var nameEntry = new Entry();
				nameEntry.FontSize = 20 * MainPage.fontSize;
                nameEntry.Placeholder = "Exercise name";
				nameEntry.Text = exercise;

				var acceptButton = new Button();
				acceptButton.FontSize = 20;
				acceptButton.Text = "✔️";

				acceptButton.Clicked += async (sender, e) =>
				{
					if (nameEntry.Text.Trim() == string.Empty)
					{
						return;
					}

					var newName = nameEntry.Text.Trim();

					var currentSessions = sessions.Where(s => s.Name == exercise);

					foreach (var session in currentSessions)
					{
						session.Name = newName;
					}

					var muscles = await repository.GetMusclesAsync();

					muscles.Remove(exercise);
					muscles.Add(newName);

					await repository.SaveMusclesAsync(muscles);

					await repository.SaveExercisesAsync(sessions);

					deleteButton.IsEnabled = nameLabel.IsVisible = true;

                    foreach (var button in buttons)
                    {
                        button.IsEnabled = true;
                    }

					MuscleInputGrid.Remove(acceptButton);
					MuscleInputGrid.Remove(nameEntry);

					exercises[count] = newName;

					//exercise = newName;

					nameLabel.Text = $"{newName}: {count}";

                    if (MainPage.isDiaryLoaded)
                    {
                        var diary = Diary.GetDiary();
                        diary.UpdateMuscleSelector(muscles);
                        diary.UpdateExerciseNamePicker(muscles);
                        diary.Exercises = sessions;

                        if (diary.isByName)
                        {
                            diary.MuscleSelectorChange();
                        }
                        else
                        {
                            diary.DateSelectorChange();
                        }
                    }

                    if (MainPage.isWorkoutPlanningLoaded)
                    {
                        var workoutPlanning = WorkoutPlanning.GetWorkoutPlanning();

                        workoutPlanning.UpdateExerciseNamePicker(muscles);
                    }
                };

				await Task.Run(() =>
				{
					MainThread.BeginInvokeOnMainThread(() =>
					{
                        MuscleInputGrid.Add(nameEntry, 0, row);
                        MuscleInputGrid.SetColumnSpan(nameEntry, 3);
                        MuscleInputGrid.Add(acceptButton, 3, row);
                    });
				});
            };

			deleteButton.Clicked += async (sender, e) =>
			{
				var toDelete = await DisplayAlert("Confirm action", "This will delete all session of this exercise, " +
					"do you really want to continue?", "Yes", "Cancel");

				if (toDelete)
				{
					var sessionsToRemoveCount = sessions.Where(s => s.Name == exercise).Count();

                    if (sessionsToRemoveCount > 0)
					{
						sessions.RemoveAll(s => s.Name == exercise);
                        await repository.SaveExercisesAsync(sessions);
                    }

					var muscles = await repository.GetMusclesAsync();

					muscles.Remove(exercise);

					await repository.SaveMusclesAsync(muscles);

					// KONIECZNIE DO ZROBIENIA!!!!!!

					if (MainPage.isDiaryLoaded)
					{
						var diary = Diary.GetDiary();
						diary.UpdateMuscleSelector(muscles);
						diary.UpdateExerciseNamePicker(muscles);
						diary.Exercises = sessions;

						if (diary.isByName)
						{
							diary.MuscleSelectorChange();
						}
						else
						{
							diary.DateSelectorChange();
						}
					}

					if (MainPage.isWorkoutPlanningLoaded)
					{
						var workoutPlanning = WorkoutPlanning.GetWorkoutPlanning();

						workoutPlanning.UpdateExerciseNamePicker(muscles);
					}

					var controlsToRemove = MuscleInputGrid.OfType<IView>().Where(i => MuscleGrid.GetRow(i) == row).ToList();

					foreach (var control in controlsToRemove)
					{
						MuscleInputGrid.Children.Remove(control);
					}

                    var controlsToPushUp = MuscleInputGrid.Children.OfType<IView>().Where(i => MuscleInputGrid.GetRow(i) > row).ToList();

					if (!controlsToPushUp.Any())
						return;

					foreach (var control in controlsToPushUp)
					{
						var currentRow = MuscleInputGrid.GetRow(control);
						MuscleInputGrid.SetRow(control, currentRow - 1);
					}

                    MuscleInputGrid.RowDefinitions.RemoveAt(MuscleInputGrid.RowDefinitions.Count - 1);
                }
            };

			await Task.Run(() =>
			{
				MainThread.BeginInvokeOnMainThread(() =>
				{
                    MuscleInputGrid.Add(nameLabel, 0, row);
                    MuscleInputGrid.SetColumnSpan(nameLabel, 3);
                    MuscleInputGrid.Add(editButton, 3, row);
                    MuscleInputGrid.Add(deleteButton, 4, row);
                });
			});

			//tasks.Add(task);
        }

		//await Task.WhenAll(tasks);
    }
}