using TrainingDiary.Interfaces;
using TrainingDiary.Models;
namespace TrainingDiary;

public partial class Diary : ContentPage
{
	private readonly IRepository repository;
	public static List<Exercise> Exercises; // uzyskane poprzez async voida
	public static bool isByName;
	public static bool isNameCustom;
	public static Button AddButton;
	public static Button NameDateButton;
	public static Grid DynamicGrid;
	
	public Diary()
	{
		InitializeComponent();

		repository = new Repository();
		Exercises = repository.GetExercises();

		MuscleSelector.ItemsSource = repository.GetMuscles();

        ExerciseNamePicker.ItemsSource = repository.GetMuscles();

        ExerciseNameEntry.Text = SetsEntry.Text = RepsEntry.Text = 
			WeightEntry.Text = CommentEntry.Text = string.Empty;

		MuscleSelector.IsVisible = false;
		MuscleSelector.Title = "Exercise";

        ExerciseNamePicker.Title = "Exercise";

        isByName = false;
		isNameCustom = false;

		AddButton = new Button();
		AddButton.Text = "Add";
		AddButton.BackgroundColor = Colors.LightGreen;
		AddButton.Clicked += (sender, e) =>
		{
			AddButton_Clicked();
        };

		NameDateButton = new Button();
		NameDateButton.Text = "Name";
		NameDateButton.Clicked += (sender, e) =>
		{
			NameDateButton_Clicked();
        };

		InputGrid.Add(AddButton, 0, 3);
		InputGrid.SetColumnSpan(AddButton, 3);

		InputGrid.Add(NameDateButton, 1, 5);
		InputGrid.SetColumnSpan(NameDateButton, 2);

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

        UpdateButtonsFontSize();

		//DateSelectorChange();

		MainPage.isDiaryLoaded = true;
    }

	private async void GetExercises()
	{
		Exercises = await repository.GetExercisesAsync();
		// metoda pomocnicza do obejścia ograniczeń konstruktora
	}

    private async void AddButton_Clicked()
    {
		var isCustomAndNull = isNameCustom && 
			ExerciseNameEntry.Text.Trim() == string.Empty;

		var isNotCustomAndNull = !isNameCustom && ExerciseNamePicker.SelectedItem == null;


        if (isCustomAndNull || isNotCustomAndNull || SetsEntry.Text.Trim() == string.Empty ||
			RepsEntry.Text.Trim() == string.Empty || WeightEntry.Text == string.Empty)
		{
			await DisplayAlert("Workout Diary", "Entries cannot be empty.", "Ok");
			return;
		}

		string name;

		if (isNameCustom)
		{
            name = ExerciseNameEntry.Text.Trim();
        }
		else
		{
			name = ExerciseNamePicker.SelectedItem!.ToString()!;
		}

		var sets = Convert.ToInt32(SetsEntry.Text);
		var reps = Convert.ToInt32(RepsEntry.Text);
		var weight = WeightEntry.Text.Trim();
		var comment = CommentEntry.Text.Trim();
        var date = DateEntry.Date;

		var exercise = new Exercise(name, sets, reps, weight, comment, date);

		Exercises.Add(exercise);

		await repository.SaveExercisesAsync(Exercises);

		MuscleSelector.ItemsSource = await repository.GetMusclesAsync();

		ExerciseNamePicker.ItemsSource = await repository.GetMusclesAsync();

        ExerciseNameEntry.Text = SetsEntry.Text = RepsEntry.Text =
            WeightEntry.Text = CommentEntry.Text = string.Empty;

		//DisplayExercises(Exercises);

		if (isByName)
		{
			MuscleSelectorChange();
		}
		else
		{
			DateSelectorChange();
		}
    }

    private void NameDateButton_Clicked()
    {
		if (isByName)
		{
            isByName = false;

            DynamicGrid.Clear();
            DynamicGrid.RowDefinitions.Clear();

            NameDateButton.Text = "Name";
			NameDateLabel.Text = "Exercise";
			DateSelector.IsVisible = true;
			MuscleSelector.IsVisible = false;

			DateSelectorChange();
        }
        else
		{
            DynamicGrid.Clear();
            DynamicGrid.RowDefinitions.Clear();

            isByName = true;

            NameDateButton.Text = "Date";
            NameDateLabel.Text = "Date";
			DateSelector.IsVisible = false;
			MuscleSelector.IsVisible = true;

			MuscleSelectorChange();
        }
    }

	public static void UpdateButtonsFontSize()
	{
		NameDateButton.FontSize = 20 * MainPage.fontSize;
        AddButton.FontSize = 20 * MainPage.fontSize;
    }

	public static void UpdateLabelsFontSize()
	{
		var labels = DynamicGrid.OfType<Label>();

		foreach (var label in labels)
		{
			label.FontSize = 20 * MainPage.fontSize;
		}
	}

    private void CustomMuscleNameSelector_Clicked(object sender, EventArgs e)
    {
		if (isNameCustom)
		{
            isNameCustom = false;
            ExerciseNameEntryBorder.IsVisible = ExerciseNameEntry.IsVisible = false;
            ExerciseNamePickerBorder.IsVisible = ExerciseNamePicker.IsVisible = true;

			CustomMuscleNameSelector.Text = "Click to enter custom exercise name";
        }
		else
		{
			isNameCustom = true;
            ExerciseNameEntryBorder.IsVisible = ExerciseNameEntry.IsVisible = true;
			ExerciseNamePickerBorder.IsVisible = ExerciseNamePicker.IsVisible = false;

            CustomMuscleNameSelector.Text = "Click to select exercise from the picker";
        }
    }

    private void DateSelector_DateSelected(object sender, DateChangedEventArgs e)
    {
		DateSelectorChange();
	}

    private void MuscleSelector_SelectedIndexChanged(object sender, EventArgs e)
    {
		MuscleSelectorChange();
	}

	private void MuscleSelectorChange()
	{
        if (MuscleSelector.SelectedItem == null)
        {
            return;
        }

        var exercises = Exercises.Where(e => e.Name == MuscleSelector.SelectedItem.ToString())
			.OrderByDescending(e => e.Date).ToList();

        DisplayExercises(exercises);
    }

    private void DateSelectorChange()
	{
		var exercises = Exercises.Where(e => e.Date.DayOfYear == DateSelector.Date.DayOfYear
				&& e.Date.Year == DateSelector.Date.Year).ToList();

        DisplayExercises(exercises);
    }

    private void DisplayExercises(List<Exercise> exercises)
	{
        DynamicGrid.Clear();
        DynamicGrid.RowDefinitions.Clear();

        if (exercises.Count == 0)
		{
			return;
		}

        DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

        foreach (var exercise in exercises)
		{
			int row = exercises.IndexOf(exercise);

			var idLabel = new Label();
			idLabel.FontSize = 20 * MainPage.fontSize;

			if (isByName)
			{
				idLabel.Text = exercise.GetDate();
			}
			else
			{
				idLabel.Text = exercise.Name;
			}

			var setsLabel = new Label();
			setsLabel.FontSize = 20 * MainPage.fontSize;
			setsLabel.Text = exercise.Sets.ToString();

			var repsLabel = new Label();
			repsLabel.FontSize = 20 * MainPage.fontSize;
            repsLabel.Text = exercise.Reps.ToString();

            var editButton = new Button();
            editButton.Text = "✏️";
            editButton.FontSize = 20;

            var infoButton = new Button();
			infoButton.Text = "ⓘ";
			infoButton.FontSize = 20;

            var controls = new List<IView>
			{
				idLabel,
				setsLabel,
				repsLabel,
				editButton,
				infoButton
			};

            infoButton.Clicked += async (sender, e) =>
			{
                var comment = exercise.Comment;

                if (comment == string.Empty)
                {
                    comment = "No comment";
                }

                var ifKeep = await DisplayAlert(exercise.Name, $"{exercise.GetDate()}" +
                    $"{Environment.NewLine}{exercise.Sets} sets for {exercise.Reps} reps{Environment.NewLine}{exercise.Weight}" +
                    $"{Environment.NewLine}{comment}", "Ok", "Delete");

                if (!ifKeep)
				{
					Exercises.Remove(exercise);
					exercises.Remove(exercise);
					await repository.SaveExercisesAsync(Exercises);

					//MuscleSelector.ItemsSource = await repository.GetMusclesAsync();

					var tasks = new List<Task>();

					var bindableObject = sender as BindableObject;

                    foreach (var control in controls)
					{
						var task = Task.Run(async () =>
						{
							await bindableObject!.Dispatcher.DispatchAsync(() =>
							{
                                DynamicGrid.Remove(control);
                                DynamicGrid.Children.Remove(control);
                            });
						});

						tasks.Add(task);
					}

					await Task.WhenAll(tasks);

                    DynamicGrid.RowDefinitions.RemoveAt(row);
				}
				
			};

			editButton.Clicked += async (sender, e) =>
			{
				idLabel.IsVisible = setsLabel.IsVisible = repsLabel.IsVisible
				= infoButton.IsVisible = editButton.IsVisible = false;

				// zmiana daty/ nazwy

				var changeNameEntry = new Entry();
                var changeDatePicker = new DatePicker();

				if (isByName)
				{
					changeDatePicker.FontSize = 20 * MainPage.fontSize;
                    changeDatePicker.Date = exercise.Date;

					DynamicGrid.Add(changeDatePicker, 0, row);
				}
				else
				{
					changeNameEntry.FontSize = 20 * MainPage.fontSize;
                    changeNameEntry.Text = exercise.Name;
					changeNameEntry.Placeholder = "Exercise";

					DynamicGrid.Add(changeNameEntry, 0, row);
				}

				var changeSetsEntry = new Entry();
				changeSetsEntry.FontSize = 20 * MainPage.fontSize;
                changeSetsEntry.Placeholder = "Sets";
				changeSetsEntry.Keyboard = Keyboard.Numeric;
				changeSetsEntry.Text = exercise.Sets.ToString();

				var changeRepsEntry = new Entry();
                changeRepsEntry.FontSize = 20 * MainPage.fontSize;
                changeRepsEntry.Placeholder = "Reps";
                changeRepsEntry.Keyboard = Keyboard.Numeric;
                changeRepsEntry.Text = exercise.Reps.ToString();

				var changeWeightEntry = new Entry();
				changeWeightEntry.FontSize = 20 * MainPage.fontSize;
                changeWeightEntry.Placeholder = "Weight";
				changeWeightEntry.Text = exercise.Weight;

				var changeCommentEntry = new Entry();
				changeCommentEntry.FontSize = 20 * MainPage.fontSize;
                changeCommentEntry.Text = exercise.Comment;
				changeCommentEntry.Placeholder = "Comment";

				var acceptButton = new Button();
				acceptButton.FontSize = 20;
				acceptButton.Text = "✔️";

				var controlsToPushDown = DynamicGrid.Children.Where(c => DynamicGrid.GetRow(c) > row);

				var tasks = new List<Task>();

				var bindableObject = sender as BindableObject;

				foreach (var control in controlsToPushDown)
				{
					var task = Task.Run(async () =>
					{
                        await bindableObject.Dispatcher.DispatchAsync(() =>
                        {
                            int currentRow = DynamicGrid.GetRow(control);
                            DynamicGrid.SetRow(control, currentRow + 1);
                        });
                    });

					tasks.Add(task);
				}

				await Task.WhenAll(tasks);

				DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

                acceptButton.Clicked += async (sender, e) =>
				{
                    bool isIncorrect;

                    bool setsOk = int.TryParse(changeSetsEntry.Text, out int sets);
                    bool repsOk = int.TryParse(changeRepsEntry.Text, out int reps);
                    bool weightOk = !string.IsNullOrWhiteSpace(changeWeightEntry.Text);

                    if (isByName)
                    {
                        isIncorrect = !setsOk || !repsOk || !weightOk;
                    }
                    else
                    {
                        bool nameOk = !string.IsNullOrWhiteSpace(changeNameEntry.Text);
                        isIncorrect = !nameOk || !setsOk || !repsOk || !weightOk;
                    }

                    if (isIncorrect)
					{
						await DisplayAlert("Workout Diary", "Fill all the data.", "Ok");
						return;
					}

					if (isByName)
					{
						exercise.Date = changeDatePicker.Date;
					}
					else
					{
						exercise.Name = changeNameEntry.Text.Trim();
					}

					exercise.Sets = sets;
					exercise.Reps = reps;
					exercise.Weight = changeWeightEntry.Text.Trim();
					exercise.Comment = changeCommentEntry.Text.Trim();

					await repository.SaveExercisesAsync(Exercises);

					MuscleSelector.ItemsSource = await repository.GetMusclesAsync();
                    ExerciseNamePicker.ItemsSource = await repository.GetMusclesAsync();

                    DynamicGrid.Children.Remove(changeRepsEntry);
                    DynamicGrid.Children.Remove(changeSetsEntry);
                    DynamicGrid.Children.Remove(changeWeightEntry);
					DynamicGrid.Children.Remove(acceptButton);
					DynamicGrid.Children.Remove(changeCommentEntry);

					foreach (var control in controlsToPushDown)
					{
						int currentRow = DynamicGrid.GetRow(control);
						DynamicGrid.SetRow(control, currentRow - 1);
					}

					if (isByName)
					{
                        DynamicGrid.Children.Remove(changeDatePicker);
						idLabel.Text = exercise.GetDate();
                    }
                    else
					{
                        DynamicGrid.Children.Remove(changeNameEntry);
						idLabel.Text = exercise.Name;
                    }

					idLabel.IsVisible = setsLabel.IsVisible = repsLabel.IsVisible
						= infoButton.IsVisible = editButton.IsVisible = true;

					setsLabel.Text = sets.ToString();
					repsLabel.Text = reps.ToString();
                };

				DynamicGrid.Add(changeSetsEntry, 1, row);
                DynamicGrid.Add(changeRepsEntry, 2, row);
                DynamicGrid.Add(changeWeightEntry, 3, row);
				DynamicGrid.Add(acceptButton, 4, row);
				DynamicGrid.Add(changeCommentEntry, 0, row + 1);
				DynamicGrid.SetColumnSpan(changeCommentEntry, 5);

            };

			DynamicGrid.Add(idLabel, 0, row);
			DynamicGrid.Add(setsLabel, 1, row);
            DynamicGrid.Add(repsLabel, 2, row);
            DynamicGrid.Add(infoButton, 3, row);
            DynamicGrid.Add(editButton, 4, row);

            DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });
		}
	}
}