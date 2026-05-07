using RobotHri.Controls.Base;
using RobotHri.Languages;

namespace RobotHri.Views
{
    public partial class HomePage : BaseContentPage
    {
        public HomePage()
        {
            InitializeComponent();
            Header.LanguageToggled += (_, _) => Localization?.ToggleLanguage();
            RefreshLocalizedText();
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

            await DisplayAlert(
                StringIds.MAIN_HOME_NO_THANKS_TITLE.GetString(),
                StringIds.MAIN_HOME_NO_THANKS_MESSAGE.GetString(),
                StringIds.OK.GetString());
        }

        protected override void RefreshLocalizedText()
        {
            Header.Title = StringIds.COMMON_HOME.GetString();
            Header.LanguageLabelText = Localization?.GetCurrentLanguageName() ?? "EN";

            PromptLabel.Text = StringIds.MAIN_HOME_PROMPT.GetString();
            YesButton.Text = StringIds.MAIN_HOME_YES.GetString();
            NoButton.Text = StringIds.MAIN_HOME_NO_THANKS.GetString();
        }
    }
}
