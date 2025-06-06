using TrainingDiary.Interfaces;
using TrainingDiary.Models;

namespace TrainingDiary
{
    public partial class MainPage : ContentPage
    {
        private readonly IRepository repository;
        private User? user;

        public MainPage()
        {
            InitializeComponent();

            repository = new Repository();

            user = repository.GetUser();

            if (user == null)
            {
                WelcomeLabel.Text = $"Welcome to Training Diary!{Environment.NewLine}Enter your data for ";
            }
        }
    }

}
