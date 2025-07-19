using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific;
using TrainingDiary.Interfaces;
using TrainingDiary.Models;
using BoxView = Microsoft.Maui.Controls.BoxView;

namespace TrainingDiary;

public partial class ProgressTracking : ContentPage
{
	private readonly static IRepository repository = new Repository();
    public static List<User> Progress;
    public static Label DateLabel;
	public static Label WeightLabel;
	public static Label BmiLabel;
	public static Button RefreshButton;
	public static BoxView _BoxView;
	public static Grid DynamicGrid;

	public ProgressTracking()
	{
		InitializeComponent();

		//repository = new Repository();

        Progress = repository.GetProgress();

        DateLabel = new Label();
        WeightLabel = new Label();
        BmiLabel = new Label();
		RefreshButton = new Button();
        _BoxView = new BoxView();

		GetStaticGridReady();

        DynamicGrid = new Grid();

		GetProgressGridReady();

        DisplayProgress();

		MainPage.isProgressTrackingLoaded = true;
	}

	private void GetStaticGridReady()
	{
		StaticGrid.Margin = new Thickness(0, 0, 0, 20);

        DateLabel.FontSize = 25 * MainPage.fontSize;
        DateLabel.Text = "Date";

        WeightLabel.FontSize = 25 * MainPage.fontSize;
        WeightLabel.Text = "Weight";

        BmiLabel.FontSize = 25 * MainPage.fontSize;
        BmiLabel.Text = "BMI";

		RefreshButton.FontSize = 25 * MainPage.fontSize;
		RefreshButton.Text = "⟲";
		RefreshButton.Clicked += async (sender, e) =>
		{
            DisplayProgress();
		};

        _BoxView.HeightRequest = 1;

        StaticGrid.Add(DateLabel, 0, 0);
        StaticGrid.Add(WeightLabel, 1, 0);
        StaticGrid.Add(BmiLabel, 2, 0);
		StaticGrid.Add(RefreshButton, 3, 0);
        StaticGrid.Add(_BoxView, 0, 1);
		StaticGrid.SetColumnSpan(_BoxView, 4);

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

	private void GetProgressGridReady()
	{
		stackLayout.Children.Add(DynamicGrid);

        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(35, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(25, GridUnitType.Star) });
        DynamicGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15, GridUnitType.Star) });

        DynamicGrid.RowSpacing = 10;
        DynamicGrid.ColumnSpacing = 15;
    }

    public static void DisplayProgress()
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
				var toKeep = await Application.Current.MainPage.DisplayAlert(progressElement.LastUpdated, $"Weight at that time: {weightLabel.Text}" +
					Environment.NewLine + $"Height: {progressElement.GetHeight()}{Environment.NewLine}" +
					$"BMI: {bmiLabel.Text}{Environment.NewLine}" +
					$"Target weight at that time: {progressElement.GetGoalWeight()}", "Ok", "Delete");

				if (!toKeep)
				{
					if (Progress.Count == 1)
					{
						await Application.Current.MainPage.DisplayAlert("Workout diary", "You can't delete the only progres record, " +
                            "change your stats in \"Settings\"", "Ok");
						return;

						// żeby nie usunęło jedynego zapisu
					}

					var toDelete = await Application.Current.MainPage.DisplayAlert("Action confirm", "Do you really want to delete this record?", "Yes", "No");

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

	public static void UpdateFontSize()
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