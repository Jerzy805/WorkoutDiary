using Microsoft.Maui.Storage;
using Newtonsoft.Json;
using TrainingDiary.Interfaces;
using TrainingDiary.Models;

namespace TrainingDiary
{
    public partial class MainPage : ContentPage
    {
        private readonly IRepository repository;
        private User? user;
        public static double fontSize;
        public static Label WelcomeLabel2;
        public static Dictionary<Label, double> Labels;
        public static bool isDiaryLoaded = false;
        public static bool isWorkoutPlanningLoaded = false;
        public static bool isProgressTrackingLoaded = false;
        public static bool isSettingsLoaded = false;
        private static bool isGuideClosed;

        public MainPage()
        {
            InitializeComponent();

            repository = new Repository();

            user = repository.GetUser();

            fontSize = repository.GetFontSize();

            isGuideClosed = repository.GetClosureInfo();

            Labels = stackLayout.Children.OfType<Label>()
                .ToDictionary(label => label, label => label.FontSize);

            UpdateFontSize();

            WelcomeLabel2 = new Label();
            WelcomeLabel2.HorizontalOptions = LayoutOptions.Center;
            WelcomeLabel2.HorizontalTextAlignment = TextAlignment.Center;
            WelcomeLabel2.FontSize = WelcomeLabel1.FontSize;
            WelcomeLabel2.Margin = new Thickness(0, 15);

            var fontSizeSlider = new Slider();
            fontSizeSlider.Minimum = 0.8;
            fontSizeSlider.Maximum = 1.8;
            fontSizeSlider.Value = fontSize;
            fontSizeSlider.Margin = new Thickness(15);
            fontSizeSlider.ValueChanged += async (sender, e) =>
            {
                fontSize = fontSizeSlider.Value;

                UpdateFontSize();

                if (isDiaryLoaded)
                {
                    var diary = Diary.GetDiary();
                    diary.UpdateButtonsFontSize();
                    diary.UpdateLabelsFontSize();
                    // rozpatrzyć redukcję do jednej funkcji o nazwie UpdateFontSize
                }

                if (isWorkoutPlanningLoaded)
                {
                    WorkoutPlanning.UpdateFontSize();
                }

                if (isProgressTrackingLoaded)
                {
                    ProgressTracking.UpdateFontSize();
                }
                 
                if (isSettingsLoaded)
                {
                    Settings.UpdateFontSize();
                }
            };

            fontSizeSlider.DragCompleted += async (sender, e) =>
            {
                await repository.SaveFontSizeAsync(fontSize);
            };

            var infoLabel = new Label();
            infoLabel.FontSize = 22 * fontSize;
            infoLabel.HorizontalOptions = LayoutOptions.Center;
            infoLabel.Margin = new Thickness(15);
            infoLabel.Text = "Font size";

            stackLayout.Add(WelcomeLabel2);
            stackLayout.Add(fontSizeSlider);
            stackLayout.Add(infoLabel);

            var texts = new List<string>()
            {
                "1. Here you have basic info of your progress and font size setting.",
                "2. In \"Diary\" section you can record your workout by adding all done exercises.",
                "3. \"Workout\" section lets you plan all exercises you want to do, so when e.g. there's chest day, you can " +
                "just load earlier prepared workout for that day with proper date.",
                "4. \"Progress Tracking\" section, as the name suggests, lets you track your progress, which is meant as your weight records.",
                "5. \"Settings\" section is pretty self-explanatory."
            };

            var guideLabel = new Label();
            guideLabel.HorizontalTextAlignment = TextAlignment.Center;
            guideLabel.IsVisible = !isGuideClosed;
            guideLabel.FontSize = 25 * fontSize;
            guideLabel.Text = "Quick app guide:";

            var labels = new List<Label>();

            foreach (var labelText in texts)
            {
                var label = new Label();
                label.FontSize = 25 * fontSize;
                label.Text = labelText;
                label.IsVisible = !isGuideClosed;
                label.Margin = new Thickness(10, 7);

                labels.Add(label);
            }

            var closeButton = new Button();
            closeButton.FontSize = 25 * fontSize;

            if (isGuideClosed)
            {
                closeButton.Text = "Open app guide";
                closeButton.Margin = new Thickness(60, 15);
            }
            else
            {
                closeButton.Text = "Close";
                closeButton.Margin = new Thickness(140, 15);
            }

            closeButton.Clicked += async (sender, e) =>
            {
                if (isGuideClosed)
                {
                    closeButton.Text = "Close";
                    closeButton.Margin = new Thickness(140, 15);
                    isGuideClosed = false;
                }
                else
                {
                    closeButton.Text = "Open app guide";
                    closeButton.Margin = new Thickness(60, 15);
                    isGuideClosed = true;
                }

                foreach (var label in labels)
                {
                    label.IsVisible = !isGuideClosed;
                }

                guideLabel.IsVisible = !isGuideClosed;
                
                await repository.SaveClosureInfo(isGuideClosed);
            };

            stackLayout.Add(guideLabel);

            foreach (var label in labels)
            {
                stackLayout.Add(label);
            }

            stackLayout.Add(closeButton);

            if (user == null)
            {
                WelcomeLabel1.Text = "Welcome to Workout Diary!";
                WelcomeLabel2.Text = "Enter your data for progress tracking.";

                var nameEntry = new Entry();
                nameEntry.FontSize = 25 * fontSize;
                nameEntry.Placeholder = "Name";
                nameEntry.Text = string.Empty;

                var heightEntry = new Entry();
                heightEntry.FontSize = 25 * fontSize;
                heightEntry.Keyboard = Keyboard.Numeric;
                heightEntry.Placeholder = "Height (cm)";
                heightEntry.Text = string.Empty;

                var weightEntry = new Entry();
                weightEntry.FontSize = 25 * fontSize;
                weightEntry.Keyboard = Keyboard.Numeric;
                weightEntry.Placeholder = "Weight (kg)";
                weightEntry.Text = string.Empty;

                var goalWeightEntry = new Entry();
                goalWeightEntry.FontSize = 25 * fontSize;
                goalWeightEntry.Keyboard = Keyboard.Numeric;
                goalWeightEntry.Placeholder = "Target weight (kg)";
                goalWeightEntry.Text = string.Empty;

                var heightUnitPicker = new Picker();
                heightUnitPicker.FontSize = 25 * fontSize;
                heightUnitPicker.Title = "Height unit";
                heightUnitPicker.ItemsSource = new List<string>() {"cm", "ft"};

                var weightUnitPicker = new Picker();
                weightUnitPicker.FontSize = 25 * fontSize;
                weightUnitPicker.Title = "Weight unit";
                weightUnitPicker.ItemsSource = new List<string>() { "kg", "lb" };

                var enterButton = new Button();
                enterButton.FontSize = 25 * fontSize;
                enterButton.BackgroundColor = Colors.LightGreen;
                enterButton.Text = "Enter";

                enterButton.Clicked += async (sender, e) =>
                {
                    var name = nameEntry.Text.Trim();
                    var heightText = heightEntry.Text.Trim();
                    var weightText = weightEntry.Text.Trim();
                    var goalWeightText = goalWeightEntry.Text.Trim();
                    var usedHeightUnit = heightUnitPicker.SelectedItem.ToString();
                    var usedWeightUnit = weightUnitPicker.SelectedItem.ToString();

                    if (name == string.Empty || !float.TryParse(heightText, out float height) || 
                        !float.TryParse(weightText, out float weight) || !float.TryParse(goalWeightText, out float goalWeight) ||
                        heightUnitPicker.SelectedItem == null || weightUnitPicker.SelectedItem == null)
                    {
                        await DisplayAlert("Workout Diary", "Please enter all your data in desired format.", "Ok");
                        return;
                    }

                    var user = new User(name, height, weight, goalWeight, usedHeightUnit, usedWeightUnit);

                    await repository.SaveUserAsync(user);
                    await DisplayAlert("Workout Diary", $"Welcome, {name}!", "Ok.");

                    UserInputGrid.Children.Clear();
                    UserInputGrid.Clear();
                    UserInputGrid.IsVisible = false;
                    WelcomeLabel1.Text = $"Welcome, {name}";

                    string WelcomeLabeltext = "Target weight achieved! Now just maintain it.";

                    var suffix = $": {Math.Abs(weight - goalWeight)} {usedWeightUnit}";

                    if (weight > goalWeight)
                    {
                        WelcomeLabeltext = $"Fat to burn{suffix}";
                    }

                    if (weight < goalWeight)
                    {
                        WelcomeLabeltext = $"Weight to gain{suffix}";
                    }

                    WelcomeLabel2.Text = WelcomeLabeltext;
                };

                UserInputGrid.Add(nameEntry, 0, 0);
                UserInputGrid.Add(heightEntry, 1, 0);
                UserInputGrid.Add(weightEntry, 0, 1);
                UserInputGrid.Add(goalWeightEntry, 1, 1);
                UserInputGrid.Add(heightUnitPicker, 0, 2);
                UserInputGrid.Add(weightUnitPicker, 1, 2);
                UserInputGrid.Add(enterButton, 0, 3);
                UserInputGrid.SetColumnSpan(enterButton, 2);
                return;
            }

            UserInputGrid.IsVisible = false;
            WelcomeLabel1.Text = $"Welcome, {user.Name}";

            string text = "Target weight achieved! Now just maintain it.";

            var suffix = $": {(Math.Abs(user.Weight - user.GoalWeight)).ToString("0.0")} {user.UsedWeightUnit}";  

            if (user.Weight > user.GoalWeight)
            {
                text = $"Fat to burn{suffix}";
            }

            if (user.Weight < user.GoalWeight)
            {
                text = $"Weight to gain{suffix}";
            }

            WelcomeLabel2.Text = text;
        }

        private void UpdateFontSize()
        {
            var labels = stackLayout.OfType<Label>();

            foreach (var label in labels)
            {
                label.FontSize = 22 * fontSize;
            }

            var entries = UserInputGrid.OfType<Entry>();

            foreach (var entry in entries)
            {
                entry.FontSize = 25 * fontSize;
            }

            var pickers = UserInputGrid.OfType<Picker>();

            foreach (var picker in pickers)
            {
                picker.FontSize = 25 * fontSize;
            }

            var buttons = stackLayout.OfType<Button>();

            foreach (var button in buttons)
            {
                button.FontSize = 25 * fontSize;
            } 
        }
    }

}
