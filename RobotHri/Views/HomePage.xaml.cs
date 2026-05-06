using RobotHri.Controls.Base;

namespace RobotHri.Views
{
    public partial class HomePage : BaseContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnYesClicked(object sender, EventArgs e)
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync("//main");
        }

        private async void OnNoClicked(object sender, EventArgs e)
        {
            if (Shell.Current is null)
            {
                return;
            }

            await DisplayAlert("Info", "No problem. Let me know whenever you need help.", "OK");
        }
    }
}
