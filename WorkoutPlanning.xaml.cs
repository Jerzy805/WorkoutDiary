using System.Threading.Tasks;
using TrainingDiary.Interfaces;
using TrainingDiary.Models;

namespace TrainingDiary;

public partial class WorkoutPlanning : ContentPage
{
	private readonly IRepository repository;
	private List<Workout> Workouts;
	private List<Exercise> currentExercises;
	private static bool isCustom;
	private static bool isEnabled;
    private static bool isInScrollingMode;
    private static bool isPressed;
    public static Grid DynamicGrid;

	public WorkoutPlanning()
	{
        InitializeComponent();

		this.repository = new Repository();

		Workouts = repository.GetWorkouts();

		currentExercises = new ();

		ExerciseNamePicker.ItemsSource = repository.GetMuscles();

        WorkoutNamePicker.ItemsSource = repository.GetWorkoutNames();

		ExerciseNameEntry.Text = string.Empty;
		SetsEntry.Text = RepsEntry.Text = WeightEntry.Text = CommentEntry.Text = string.Empty;

		isCustom = false;
		isEnabled = true;
        isInScrollingMode = false;
        isPressed = false;

		WorkoutNameEntry.Text = string.Empty;

        ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled =
                CustomSwitch.IsEnabled = SetsEntry.IsEnabled = RepsEntry.IsEnabled =
                WeightEntry.IsEnabled = CommentEntry.IsEnabled = false;

        DynamicGrid = new Grid
        {
            ColumnSpacing = 10,
            RowSpacing = 15
        };

        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        stackLayout.Children.Add(DynamicGrid);

        MainPage.isWorkoutPlanningLoaded = true;
    }

    public static void UpdateFontSize()
    {
        var labels = DynamicGrid.OfType<Label>();

        foreach (var label in labels)
        {
            label.FontSize = 20 * MainPage.fontSize;
        }
    }

    private async void GetReadyButton_Clicked(object sender, EventArgs e)
    {
		if (isEnabled)
		{
			if (string.IsNullOrEmpty(WorkoutNameEntry.Text))
			{
				await DisplayAlert("Workout Diary", "Enter workout name.", "Ok");
				return;
			}

			isEnabled = false;
			GetReadyButton.Text = "✏️";

			WorkoutNameEntry.IsEnabled = false;
            isPressed = true;

            ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled =
                CustomSwitch.IsEnabled = SetsEntry.IsEnabled = RepsEntry.IsEnabled =
                WeightEntry.IsEnabled = CommentEntry.IsEnabled = true;

            var name = WorkoutNameEntry.Text.Trim();

			var workoutWithTheSameName = Workouts.Find(w => w.Name.ToLower() == name.ToLower());

			if (workoutWithTheSameName != null)
			{
				workoutWithTheSameName.Date = WorkoutDatePicker.Date;

				foreach (var exercise in workoutWithTheSameName.Exercises)
				{
					exercise.Date = WorkoutDatePicker.Date;
                }

				await repository.SaveWorkoutsAsync(Workouts);

                DisplayExercisesOfTheWorkout(workoutWithTheSameName);

                await DisplayAlert("Workout Diary", "You already have a workout with this name, adding, deleting or editing it will affect it " +
					"instead of creating a new one.", "Ok");
				return;
			}

			var workout = new Workout(name, new(), WorkoutDatePicker.Date);

			Workouts.Add(workout);
			await repository.SaveWorkoutsAsync(Workouts);

			DynamicGrid.Clear();
			DynamicGrid.RowDefinitions.Clear();
		}
		else
		{
			isEnabled = true;
            isPressed = false;
            GetReadyButton.Text = "✔️";
            WorkoutNameEntry.IsEnabled = true;

			ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled = 
				CustomSwitch.IsEnabled =SetsEntry.IsEnabled = RepsEntry.IsEnabled = 
				WeightEntry.IsEnabled = CommentEntry.IsEnabled = false;

			var dynamicGridButtons = DynamicGrid.Children.OfType<Button>();

			if (dynamicGridButtons.Any())
			{
				foreach (var button in dynamicGridButtons)
				{
					button.IsEnabled = false;
				}
			}
        }
    }

    private void CustomSwitch_Clicked(object sender, EventArgs e)
    {
		if (isCustom)
		{
			ExerciseNamePickerBorder.IsVisible = ExerciseNamePicker.IsVisible = true;
			ExerciseNameEntryBorder.IsVisible = ExerciseNameEntry.IsVisible = false;

			isCustom = false;

			CustomSwitch.Text = "Click to enter custom exercise name";
        }
		else
		{
			ExerciseNamePickerBorder.IsVisible = ExerciseNamePicker.IsVisible = false;
            ExerciseNameEntryBorder.IsVisible = ExerciseNameEntry.IsVisible = true;

            isCustom = true;

			CustomSwitch.Text = "Click to select exercise from the picker";
        }
    }

    private async void AddExerciseButton_Clicked(object sender, EventArgs e)
    {
		var isCustomAndNull = isCustom && string.IsNullOrEmpty(ExerciseNameEntry.Text);

		var isNotCustomAndNull = !isCustom && ExerciseNamePicker.SelectedItem == null;

		if (isCustomAndNull || isNotCustomAndNull || !int.TryParse(SetsEntry.Text, out int sets) ||
            !int.TryParse(RepsEntry.Text, out int reps) || WeightEntry.Text == string.Empty)
		{
            await DisplayAlert("Workout Diary", "Entries cannot be empty.", "Ok");
            return;
		}

		string name;

		if (isCustom)
		{
			name = ExerciseNameEntry.Text.Trim();
		}
		else
		{
			name = ExerciseNamePicker.SelectedItem!.ToString()!;
		}

		var exercise = new Exercise(name, sets, reps, WeightEntry.Text.Trim(),
			CommentEntry.Text.Trim(), WorkoutDatePicker.Date);

        string workoutName;

        if (isInScrollingMode)
        {
            workoutName = WorkoutNamePicker.SelectedItem.ToString()!;
        }
        else
        {
            workoutName = WorkoutNameEntry.Text.Trim();
        }

            var workout = Workouts.Find(w => w.Name.ToLower() == workoutName.ToLower());

		workout!.Exercises.Add(exercise);
		await repository.SaveWorkoutsAsync(Workouts);
        WorkoutNamePicker.ItemsSource = await repository.GetWorkoutNamesAsync();

        ExerciseNameEntry.Text = string.Empty;
        SetsEntry.Text = RepsEntry.Text = WeightEntry.Text = CommentEntry.Text = string.Empty;

		DisplayExercisesOfTheWorkout(workout);

        currentExercises.Add(exercise); // przemyśleć całkowite usunięcie zmiennej z projektu
    }

    private async void SeeWorkoutsButton_Clicked(object sender, EventArgs e)
    {
        if (SeeWorkoutsButton.Text == "See existing workouts")
        {
            WorkoutNamePicker.ItemsSource = await repository.GetWorkoutNamesAsync();

            WorkoutNameEntryBorder.IsVisible = WorkoutNameEntry.IsVisible = false;
            WorkoutNamePickerBorder.IsVisible = WorkoutNamePicker.IsVisible = true;

            SeeWorkoutsButton.Text = "Create new workout";

            isInScrollingMode = true;
            GetReadyButton.IsEnabled = false;

            if (WorkoutNamePicker.SelectedItem != null)
            {
                var workout = Workouts.Find(w => w.Name == WorkoutNamePicker.SelectedItem.ToString())!;

                DisplayExercisesOfTheWorkout(workout);

                ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled =
                CustomSwitch.IsEnabled = SetsEntry.IsEnabled = RepsEntry.IsEnabled =
                WeightEntry.IsEnabled = CommentEntry.IsEnabled = true;
            }
            else
            {
                ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled =
                CustomSwitch.IsEnabled = SetsEntry.IsEnabled = RepsEntry.IsEnabled =
                WeightEntry.IsEnabled = CommentEntry.IsEnabled = false;
            }
        }
        else
        {
            GetReadyButton.IsEnabled = true;

            WorkoutNameEntryBorder.IsVisible = WorkoutNameEntry.IsVisible = true;
            WorkoutNamePickerBorder.IsVisible = WorkoutNamePicker.IsVisible = false;

            WorkoutNameEntry.Text = string.Empty;

            if (!isPressed)
            {
                ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled =
                CustomSwitch.IsEnabled = SetsEntry.IsEnabled = RepsEntry.IsEnabled =
                WeightEntry.IsEnabled = CommentEntry.IsEnabled = false;
            }

            DynamicGrid.Clear();
            DynamicGrid.RowDefinitions.Clear();

            SeeWorkoutsButton.Text = "See existing workouts";

            isInScrollingMode = false;
        }
        
    }

    private void WorkoutNamePicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        ExerciseNameEntry.IsEnabled = ExerciseNamePicker.IsEnabled = AddExerciseButton.IsEnabled =
                CustomSwitch.IsEnabled = SetsEntry.IsEnabled = RepsEntry.IsEnabled =
                WeightEntry.IsEnabled = CommentEntry.IsEnabled = true;

        var workout = Workouts.Find(w => w.Name == WorkoutNamePicker.SelectedItem.ToString())!;

        DisplayExercisesOfTheWorkout(workout);
    }

    private void DisplayExercisesOfTheWorkout(Workout workout)
	{
		DynamicGrid.Clear();
		DynamicGrid.RowDefinitions.Clear();

		if (workout.Exercises.Count == 0)
		{
            goto zero;
			//return;
		}

        DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

        foreach (var exercise in workout.Exercises)
		{
			var row = workout.Exercises.IndexOf(exercise);

			var nameLabel = new Label();
			nameLabel.FontSize = 20 * MainPage.fontSize;
            nameLabel.Text = exercise.Name;

			var setsLabel = new Label();
            setsLabel.FontSize = 20 * MainPage.fontSize;
			setsLabel.Text = exercise.Sets.ToString();

			var repsLabel = new Label();
			repsLabel.FontSize = 20 * MainPage.fontSize;
            repsLabel.Text = exercise.Reps.ToString();

            var infoButton = new Button();
            infoButton.Text = "ⓘ";
            infoButton.FontSize = 20;

            var editButton = new Button();
            editButton.Text = "✏️";
            editButton.FontSize = 20;

            var controls = new List<IView>
            {
                nameLabel,
                setsLabel,
                repsLabel,
                editButton,
                infoButton
            };

            int localRow = row;
            var localControls = controls.ToList();
            var localExercise = exercise;

            editButton.Clicked += async (sender, e) =>
            {
                foreach (var view in controls)
                {
                    if (view is View v)
                    {
                        v.IsVisible = false;
                    }
                }

                var dynamicGridButtons = DynamicGrid.Children.OfType<Button>().Where(b => b.Text == "✏️");
                if (dynamicGridButtons.Any())
                {
                    foreach (var button in dynamicGridButtons)
                    {
                        button.IsEnabled = false;
                    }
                }

                var controlsToPushDown = DynamicGrid.Children.Where(c => DynamicGrid.GetRow(c) > row).ToList();

                var changeNameEntry = new Entry();
                changeNameEntry.FontSize = 20 * MainPage.fontSize;
                changeNameEntry.Text = exercise.Name;
                changeNameEntry.Placeholder = "Exercise";

                var changeSetsEntry = new Entry();
                changeSetsEntry.FontSize = 20 * MainPage.fontSize;
                changeSetsEntry.Text = exercise.Sets.ToString();
                changeSetsEntry.Placeholder = "Sets";
                changeSetsEntry.Keyboard = Keyboard.Numeric;

                var changeRepsEntry = new Entry();
                changeRepsEntry.FontSize = 20 * MainPage.fontSize;
                changeRepsEntry.Text = exercise.Reps.ToString();
                changeRepsEntry.Placeholder = "Reps";
                changeRepsEntry.Keyboard = Keyboard.Numeric;

                var changeWeightEntry = new Entry();
                changeWeightEntry.FontSize = 20 * MainPage.fontSize;
                changeWeightEntry.Text = exercise.Weight;
                changeWeightEntry.Placeholder = "Weight";

                var changeCommentEntry = new Entry();
                changeCommentEntry.FontSize = 20 * MainPage.fontSize;
                changeCommentEntry.Text = exercise.Comment;
                changeCommentEntry.Placeholder = "Comment (optional)";

                var acceptButton = new Button();
                acceptButton.FontSize = 20;
                acceptButton.Text = "✔️";

                var deleteButton = new Button();
                deleteButton.FontSize = 20;
                deleteButton.Text = "🗑️";
                deleteButton.BackgroundColor = Colors.Red;

                deleteButton.Clicked += async (sender, e) =>
                {
                    workout.Exercises.Remove(exercise);
                    await repository.SaveWorkoutsAsync(this.Workouts);

                    DynamicGrid.Children.Remove(changeNameEntry);
                    DynamicGrid.Children.Remove(changeRepsEntry);
                    DynamicGrid.Children.Remove(changeSetsEntry);
                    DynamicGrid.Children.Remove(changeWeightEntry);
                    DynamicGrid.Children.Remove(acceptButton);
                    DynamicGrid.Children.Remove(changeCommentEntry);
                    DynamicGrid.Remove(deleteButton);

                    foreach (var control in controls)
                    {
                        DynamicGrid.Remove(control);
                    }

                    foreach (var control in controlsToPushDown)
                    {
                        int currentRow = DynamicGrid.GetRow(control);
                        DynamicGrid.SetRow(control, currentRow - 3);
                    }

                    if (dynamicGridButtons.Any())
                    {
                        foreach (var button in dynamicGridButtons)
                        {
                            button.IsEnabled = true;
                        }
                    }
                };

                var tasks = new List<Task>();
                var bindableObject = sender as BindableObject;

                foreach (var control in controlsToPushDown)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await bindableObject.Dispatcher.DispatchAsync(() =>
                        {
                            int currentRow = DynamicGrid.GetRow(control);
                            DynamicGrid.SetRow(control, currentRow + 2);
                        });
                    }));
                }

                await Task.WhenAll(tasks);

                DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
                DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

                acceptButton.Clicked += async (s2, e2) =>
                {
                    if (changeNameEntry.Text.Trim() == string.Empty ||
                        !int.TryParse(changeSetsEntry.Text, out int sets) ||
                        !int.TryParse(changeRepsEntry.Text, out int reps) ||
                        changeWeightEntry.Text.Trim() == string.Empty)
                    {
                        await DisplayAlert("Workout Diary", "Fill all the data.", "Ok");
                        return;
                    }

                    exercise.Name = changeNameEntry.Text.Trim();
                    exercise.Sets = sets;
                    exercise.Reps = reps;
                    exercise.Weight = changeWeightEntry.Text.Trim();
                    exercise.Comment = changeCommentEntry.Text.Trim();

                    await repository.SaveWorkoutsAsync(this.Workouts);
                    WorkoutNamePicker.ItemsSource = await repository.GetWorkoutNamesAsync();

                    DynamicGrid.Children.Remove(changeNameEntry);
                    DynamicGrid.Children.Remove(changeRepsEntry);
                    DynamicGrid.Children.Remove(changeSetsEntry);
                    DynamicGrid.Children.Remove(changeWeightEntry);
                    DynamicGrid.Children.Remove(acceptButton);
                    DynamicGrid.Children.Remove(changeCommentEntry);
                    DynamicGrid.Remove(deleteButton);

                    foreach (var control in controlsToPushDown)
                    {
                        int currentRow = DynamicGrid.GetRow(control);
                        DynamicGrid.SetRow(control, currentRow - 2);
                    }

                    foreach (var view in controls)
                    {
                        if (view is View v)
                        {
                            v.IsVisible = true;
                        }
                    }

                    nameLabel.Text = exercise.Name;
                    setsLabel.Text = exercise.GetSets();
                    repsLabel.Text = exercise.GetReps();

                    if (dynamicGridButtons.Any())
                    {
                        foreach (var button in dynamicGridButtons)
                        {
                            button.IsEnabled = true;
                        }
                    }
                };

                DynamicGrid.Add(changeNameEntry, 0, row);
                DynamicGrid.Add(changeSetsEntry, 1, row);
                DynamicGrid.Add(changeRepsEntry, 2, row);
                DynamicGrid.Add(changeWeightEntry, 3, row);
                DynamicGrid.Add(acceptButton, 4, row);
                DynamicGrid.Add(changeCommentEntry, 0, row + 1);
                DynamicGrid.Add(deleteButton, 4, row + 1);
                DynamicGrid.SetColumnSpan(changeCommentEntry, 4);
            };


            infoButton.Clicked += async (sender, e) =>
            {
                var comment = exercise.Comment;

                if (comment == string.Empty)
                {
                    comment = "No comment";
                }

                string action = await DisplayActionSheet(
                    $"{exercise.Name} ({exercise.Sets} x {exercise.Reps}), {exercise.Weight}",
                    null,
                    null,
                    "⬆️", "⬇️", "Ok");

                switch (action)
                {
                    case "⬆️":
                        if (row > 0)
                        {
                            var upperExercise = workout.Exercises[row - 1];
                            workout.Exercises[row - 1] = exercise;
                            workout.Exercises[row] = upperExercise;

                            await repository.SaveWorkoutsAsync(this.Workouts);

                            DisplayExercisesOfTheWorkout(workout);

                            //var controlsUpper = DynamicGrid.Children.OfType<IView>().Where(i => DynamicGrid.GetRow(i) == row - 1).ToList();

                            //foreach (var control in controlsUpper)
                            //{
                            //    DynamicGrid.SetRow(control, row);
                            //}

                            //foreach (var control in controls)
                            //{
                            //    DynamicGrid.SetRow(control, row - 1);
                            //}

                            // zakomentowane ponieważ powodowało problemy z edycją danych
                                
                        }
                        else
                        {
                            await DisplayAlert("Workout Diary", "This exercise is already at the top.", "Ok");
                        }
                        break;

                    case "⬇️":
                        if (row < workout.Exercises.Count - 1)
                        {
                            var lowerExercise = workout.Exercises[row + 1];
                            workout.Exercises[row + 1] = exercise;
                            workout.Exercises[row] = lowerExercise;

                            await repository.SaveWorkoutsAsync(this.Workouts);

                            DisplayExercisesOfTheWorkout(workout);

                            //var controlsLower = DynamicGrid.Children.OfType<IView>().Where(i => DynamicGrid.GetRow(i) == row + 1).ToList();

                            //foreach (var control in controlsLower)
                            //{
                            //    DynamicGrid.SetRow(control, row);
                            //}
                                
                            //foreach (var control in controls)
                            //{
                            //    DynamicGrid.SetRow(control, row + 1);
                            //}

                            // jak wyżej
                        }
                        else
                        {
                            await DisplayAlert("Workout Diary", "This exercise is already at the bottom.", "Ok");
                        }
                        break;
                }
            };

            DynamicGrid.Add(nameLabel, 0, row);
            DynamicGrid.Add(setsLabel, 1, row);
            DynamicGrid.Add(repsLabel, 2, row);
            DynamicGrid.Add(infoButton, 3, row);
            DynamicGrid.Add(editButton, 4, row);

            DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
        }

		var doWorkoutButton = new Button();
        doWorkoutButton.FontSize = 28;
        doWorkoutButton.Text = "Do workout";
		doWorkoutButton.Clicked += async (sender, e) =>
		{
			var toDo = await DisplayAlert("Workout Diary", "Workout will be done with the date selected on the " +
				"calendar in the upper right corner. Are you sure it's the right date?", "Yes", "No");

			if (toDo)
			{
				foreach (var exercise in workout.Exercises)
				{
					exercise.Date = WorkoutDatePicker.Date;
				}

                var exercises = await repository.GetExercisesAsync();
                exercises.AddRange(workout.Exercises);
                await repository.SaveExercisesAsync(exercises);

                if (MainPage.isDiaryLoaded)
                {
                    Diary.Exercises = exercises;
                }

                await DisplayAlert("Workout Diary", "Workout done, you can see it in the \"Diary\" section", "Ok");
            }
		};

        DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

		DynamicGrid.Add(doWorkoutButton, 0, workout.Exercises.Count);
		DynamicGrid.SetColumnSpan(doWorkoutButton, 5);

    zero:
        var lastRow = workout.Exercises.Count + 1;

        var deleteWorkoutButton = new Button();
        deleteWorkoutButton.FontSize = 28;
        deleteWorkoutButton.BackgroundColor = Colors.Red;
        deleteWorkoutButton.Text = "Delete workout";
        deleteWorkoutButton.Clicked += async (sender, e) =>
        {
            var toDelete = await DisplayAlert("Workout Diary", "Do you really want to delete this workout?", 
                "Yes", "No");

            if (toDelete)
            {
                Workouts.Remove(workout);
                await repository.SaveWorkoutsAsync(Workouts);
                DynamicGrid.Clear();
                DynamicGrid.RowDefinitions.Clear();

                WorkoutNamePicker.ItemsSource = repository.GetWorkoutNames();
            }
        };

        DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

        if (workout.Exercises.Count == 0)
        {
            lastRow = 0;
        }

        DynamicGrid.Add(deleteWorkoutButton, 0, lastRow);
        DynamicGrid.SetColumnSpan(deleteWorkoutButton, 5);
    }
}