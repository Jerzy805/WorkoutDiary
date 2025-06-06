using System.Threading.Tasks;
using TrainingDiary.Interfaces;
using TrainingDiary.Models;
namespace TrainingDiary;

public partial class Diary : ContentPage
{
	private readonly IRepository repository;
	public static List<Exercise> Exercises; // uzyskane poprzez async voida
	public static bool isByName;
	
	public Diary()
	{
		InitializeComponent();

		repository = new Repository();
		Exercises = repository.GetExercises();

		MuscleSelector.ItemsSource = repository.GetMuscles();

		ExerciseNameEntry.Text = SetsEntry.Text = RepsEntry.Text = 
			WeightEntry.Text = CommentEntry.Text = string.Empty;

		MuscleSelector.IsVisible = false;
		MuscleSelector.Title = "Exercise";

        isByName = false;

		//DisplayExercises(Exercises);
    }

	private async void GetExercises()
	{
		Exercises = await repository.GetExercisesAsync();
		// metoda pomocnicza do obejścia ograniczeń konstruktora
	}

    private async void AddButton_Clicked(object sender, EventArgs e)
    {
		if (ExerciseNameEntry.Text.Trim() == string.Empty || SetsEntry.Text.Trim() == string.Empty ||
			RepsEntry.Text.Trim() == string.Empty || WeightEntry.Text == string.Empty)
		{
			await DisplayAlert("Training Diary", "Entries cannot be empty.", "Ok");
			return;
		}

		var name = ExerciseNameEntry.Text.Trim();
		var sets = Convert.ToInt32(SetsEntry.Text);
		var reps = Convert.ToInt32(RepsEntry.Text);
		var weight = WeightEntry.Text.Trim();
		var comment = CommentEntry.Text.Trim();
        var date = DateEntry.Date;

		var exercise = new Exercise(name, sets, reps, weight, comment, date);

		Exercises.Add(exercise);

		await repository.SaveExercisesAsync(Exercises);

		MuscleSelector.ItemsSource = await repository.GetMusclesAsync();

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

    private void NameDateButton_Clicked(object sender, EventArgs e)
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

        var exercises = Exercises.Where(e => e.Name == MuscleSelector.SelectedItem.ToString()).ToList();

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
			idLabel.FontSize = 20;

			if (isByName)
			{
				idLabel.Text = exercise.GetDate();
			}
			else
			{
				idLabel.Text = exercise.Name;
			}

			var setsLabel = new Label();
			setsLabel.FontSize = 20;
			setsLabel.Text = exercise.Sets.ToString();

			var repsLabel = new Label();
			repsLabel.FontSize = 20;
			repsLabel.Text = exercise.Reps.ToString();

            var editButton = new Button();
            editButton.Text = "✏️";
            editButton.FontSize = 20;

            var infoButton = new Button();
			infoButton.Text = "ⓘ";
			infoButton.FontSize = 20;

			infoButton.Clicked += async (sender, e) =>
			{
				var ifKeep = await DisplayAlert("Training Diary", $"{exercise.Name}{Environment.NewLine}{exercise.GetDate()}" +
					$"{Environment.NewLine}{exercise.Sets} sets for {exercise.Reps} reps{Environment.NewLine}{exercise.Weight}" +
					$"{Environment.NewLine}{exercise.Comment}", "Ok", "Delete");

				if (!ifKeep)
				{
					Exercises.Remove(exercise);
					exercises.Remove(exercise);
					await repository.SaveExercisesAsync(Exercises);

					//MuscleSelector.ItemsSource = await repository.GetMusclesAsync();

                    DynamicGrid.Remove(idLabel);
                    DynamicGrid.Remove(setsLabel);
                    DynamicGrid.Remove(repsLabel);
                    DynamicGrid.Remove(editButton);
                    DynamicGrid.Remove(infoButton);

                    DynamicGrid.Children.Remove(idLabel);
                    DynamicGrid.Children.Remove(setsLabel);
                    DynamicGrid.Children.Remove(repsLabel);
                    DynamicGrid.Children.Remove(editButton);
                    DynamicGrid.Children.Remove(infoButton);

                    DynamicGrid.RowDefinitions.RemoveAt(row);

                    //DisplayExercises(exercises);
				}
				
			};

			editButton.Clicked += (sender, e) =>
			{
				idLabel.IsVisible = setsLabel.IsVisible = repsLabel.IsVisible
				= infoButton.IsVisible = editButton.IsVisible = false;

				// zmiana daty/ nazwy

				var changeNameEntry = new Entry();
                var changeDatePicker = new DatePicker();

				if (isByName)
				{
					changeDatePicker.FontSize = 20;
					changeDatePicker.Date = exercise.Date;

					DynamicGrid.Add(changeDatePicker, 0, row);
				}
				else
				{
					changeNameEntry.FontSize = 20;
					changeNameEntry.Text = exercise.Name;
					changeNameEntry.Placeholder = "Exercise";

					DynamicGrid.Add(changeNameEntry, 0, row);
				}

				var changeSetsEntry = new Entry();
				changeSetsEntry.FontSize = 20;
				changeSetsEntry.Placeholder = "Sets";
				changeSetsEntry.Keyboard = Keyboard.Numeric;
				changeSetsEntry.Text = exercise.Sets.ToString();

				var changeRepsEntry = new Entry();
                changeRepsEntry.FontSize = 20;
                changeRepsEntry.Placeholder = "Reps";
                changeRepsEntry.Keyboard = Keyboard.Numeric;
                changeRepsEntry.Text = exercise.Reps.ToString();

				var changeWeightEntry = new Entry();
				changeWeightEntry.FontSize = 20;
				changeWeightEntry.Placeholder = "Weight";
				changeWeightEntry.Text = exercise.Weight;

				var acceptButton = new Button();
				acceptButton.FontSize = 20;
				acceptButton.Text = "✔️";

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
						await DisplayAlert("hehe", "ale frajer", "wypierdalaj");
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

					await repository.SaveExercisesAsync(Exercises);

					MuscleSelector.ItemsSource = await repository.GetMusclesAsync();

					DynamicGrid.Children.Remove(changeRepsEntry);
                    DynamicGrid.Children.Remove(changeSetsEntry);
                    DynamicGrid.Children.Remove(changeWeightEntry);
					DynamicGrid.Children.Remove(acceptButton);

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