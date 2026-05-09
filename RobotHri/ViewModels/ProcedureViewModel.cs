using System.Collections.ObjectModel;
using System.Windows.Input;
using RobotHri.Languages;
using RobotHri.Services;

namespace RobotHri.ViewModels;

public class ProcedureViewModel : BaseViewModel
{
    private string _titleText = string.Empty;
    private string _homeText = string.Empty;
    private string _languageLabel = "VI";
    private string _supportedListButtonText = string.Empty;

    public ObservableCollection<ProcedureItem> Procedures { get; } = new();

    public string TitleText { get => _titleText; set => SetProperty(ref _titleText, value); }
    public string HomeText { get => _homeText; set => SetProperty(ref _homeText, value); }
    public string LanguageLabel { get => _languageLabel; set => SetProperty(ref _languageLabel, value); }
    public string SupportedListButtonText { get => _supportedListButtonText; set => SetProperty(ref _supportedListButtonText, value); }

    public ICommand GoHomeCommand { get; }
    public ICommand ToggleLanguageCommand { get; }
    public ICommand OpenProcedureCommand { get; }
    public ICommand OpenSupportedListCommand { get; }

    public ProcedureViewModel(ILocalizationService localization) : base(localization)
    {
        GoHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//main"));
        ToggleLanguageCommand = new Command(Localization.ToggleLanguage);
        OpenProcedureCommand = new Command<ProcedureItem>(async item =>
        {
            if (item is null)
            {
                return;
            }
            var key = Uri.EscapeDataString(item.DestinationKey);
            await Shell.Current.GoToAsync($"//proceduredetail?key={key}");
        });
        OpenSupportedListCommand = new Command(async () => await Shell.Current.GoToAsync("//procedurelist"));

        RefreshLocalizedProperties();
    }

    protected override void RefreshLocalizedProperties()
    {
        TitleText = StringIds.PROCEDURE_TITLE.GetString();
        HomeText = StringIds.COMMON_HOME.GetString();
        LanguageLabel = Localization.GetCurrentLanguageName();
        SupportedListButtonText = StringIds.PROCEDURE_SUPPORTED_LIST_BUTTON.GetString();
        Procedures.Clear();
        foreach (var item in ProcedureCatalog.BuildItems())
        {
            Procedures.Add(item);
        }
    }
}
