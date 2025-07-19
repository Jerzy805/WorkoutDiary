using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;          // dodajemy do ustawienia paska statusu
using Android.Graphics;       // do Color.ParseColor

namespace TrainingDiary;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Ustaw kolor status baru na #e55d13
        Window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#e55d13"));

        //// (opcjonalnie) ustaw ikony status baru na jasne (białe), jeśli tło jest ciemne
        //Window.DecorView.SystemUiVisibility = 0; // 0 - oznacza ciemne tło, jasne ikony
        //// Jeśli chcesz ciemne ikony (na jasnym tle), użyj:
        //// Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
    }
}
