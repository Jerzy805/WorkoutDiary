using TrainingDiary.Interfaces;
using TrainingDiary.Models;
using BoxView = Microsoft.Maui.Controls.BoxView;

namespace TrainingDiary;

public partial class ProgressTracking : ContentPage
{
	private readonly IRepository repository;
	private static ProgressTracking instance;
    public List<User> Progress;

	public ProgressTracking()
	{
		InitializeComponent();

		repository = new Repository();

		Progress = repository.GetProgress();

		GetStaticGridReady();

        DisplayProgress();

		MainPage.isProgressTrackingLoaded = true;

		instance = this;
	}

	public static ProgressTracking GetProgressTracking()
	{
		return instance;
	}

	private void GetStaticGridReady()
	{
        DateLabel.FontSize = 25 * MainPage.fontSize;

        WeightLabel.FontSize = 25 * MainPage.fontSize;

        BmiLabel.FontSize = 25 * MainPage.fontSize;

		RefreshButton.FontSize = 25 * MainPage.fontSize;
		RefreshButton.Clicked += (sender, e) =>
		{
            DisplayProgress();
		};

		if (Progress.Count == 0)
		{
			var infoLabel = new Label();
			infoLabel.HorizontalTextAlignment = TextAlignment.Center;
			infoLabel.FontSize = 25 * MainPage.fontSize;
			infoLabel.Margin = new Thickness(40, 15);
			infoLabel.Text = "First, congfigure your data in the Main Page";

            StaticGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

            StaticGrid.Add(infoLabel, 1, 2);
			StaticGrid.SetColumnSpan(infoLabel, 4);
        }
    }

    public void DisplayProgress()
	{
		if (Progress.Count == 0)
		{
			return;
		}

        DynamicGrid.Clear();
        DynamicGrid.RowDefinitions.Clear();

		foreach (var progressElement in Progress)
		{
			var row = Progress.IndexOf(progressElement);

            DynamicGrid.AddRowDefinition(new RowDefinition { Height = GridLength.Auto });

			var dateLabel = new Label();
			dateLabel.FontSize = 22 * MainPage.fontSize;
			dateLabel.Text = progressElement.LastUpdated;

			var weightLabel = new Label();
			weightLabel.FontSize = 22 * MainPage.fontSize;
			weightLabel.Text = progressElement.GetWeight();

			var bmiLabel = new Label();
			bmiLabel.FontSize = 22 * MainPage.fontSize;
			bmiLabel.Text = progressElement.GetBmi().ToString("0.0");

			var infoButton = new Button();
			infoButton.FontSize = 22 * MainPage.fontSize;
			infoButton.Text = "ⓘ";
            infoButton.Clicked += async (sender, e) =>
			{
				var toKeep = await DisplayAlert(progressElement.LastUpdated, $"Weight at that time: {weightLabel.Text}" +
					Environment.NewLine + $"Height: {progressElement.GetHeight()}{Environment.NewLine}" +
					$"BMI: {bmiLabel.Text}{Environment.NewLine}" +
					$"Target weight at that time: {progressElement.GetGoalWeight()}", "Ok", "Delete");

				if (!toKeep)
				{
					if (Progress.Count == 1)
					{
						await DisplayAlert("Workout diary", "You can't delete the only progres record, " +
                            "change your stats in \"Settings\"", "Ok");
						return;

						// żeby nie usunęło jedynego zapisu
					}

					var toDelete = await DisplayAlert("Action confirm", "Do you really want to delete this record?", "Yes", "No");

					if (toDelete)
					{
						Progress.Remove(progressElement);
						await repository.SaveProgressAsync(Progress);

                        var controlsToPushUp = new List<IView>();

                        controlsToPushUp.AddRange(DynamicGrid.Where(v => DynamicGrid.GetRow(v) > row));

                        DynamicGrid.Remove(dateLabel);
						DynamicGrid.Remove(weightLabel);
						DynamicGrid.Remove(bmiLabel);
						DynamicGrid.Remove(infoButton);

						if (row < DynamicGrid.RowDefinitions.Count)
						{
                            DynamicGrid.RowDefinitions.RemoveAt(row);
                        }
						
						if (controlsToPushUp.Count == 0) return;

						var tasks = new List<Task>();

						foreach (var control in controlsToPushUp)
						{
							var currentRow = DynamicGrid.GetRow(control);

							var task = Task.Run(() =>
							{
                                MainThread.BeginInvokeOnMainThread(() =>
                                {
                                    DynamicGrid.SetRow(control, currentRow - 1);
                                });
                            });

							tasks.Add(task);
						}

						await Task.WhenAll(tasks);
					}
				}
			};

            DynamicGrid.Add(dateLabel, 0, row);
            DynamicGrid.Add(weightLabel, 1, row);
            DynamicGrid.Add(bmiLabel, 2, row);
            DynamicGrid.Add(infoButton, 3, row);
		}
	}

	public void UpdateFontSize()
	{
		var staticControls = new List<View>()
		{
			DateLabel,
			WeightLabel,
			BmiLabel,
			RefreshButton
		};
        
		foreach (var control in staticControls)
		{
			if (control is Label label)
			{
				label.FontSize = 25 * MainPage.fontSize;
			}
			
			if (control is Button button)
			{
				button.FontSize = 25 * MainPage.fontSize;
			}
		}

		var dynamicControls = DynamicGrid.OfType<View>();

		foreach (var control in dynamicControls)
		{
            if (control is Label label)
            {
                label.FontSize = 22 * MainPage.fontSize;
            }

            if (control is Button button)
            {
                button.FontSize = 22 * MainPage.fontSize;
            }
        }
	}
}