using System.Windows.Input;
using RobotHri.Languages;
using RobotHri.Services;

namespace RobotHri.ViewModels;

public class ProcedureDetailViewModel : BaseViewModel
{
    private string _titleText = string.Empty;
    private string _homeText = string.Empty;
    private string _languageLabel = "VI";
    private string _requiredDocsTitle = string.Empty;
    private string _counterTitle = string.Empty;
    private string _noteTitle = string.Empty;
    private string _navigateButtonText = string.Empty;
    private string _listButtonText = string.Empty;
    private ProcedureItem? _selectedProcedure;

    public string TitleText { get => _titleText; set => SetProperty(ref _titleText, value); }
    public string HomeText { get => _homeText; set => SetProperty(ref _homeText, value); }
    public string LanguageLabel { get => _languageLabel; set => SetProperty(ref _languageLabel, value); }
    public string RequiredDocsTitle { get => _requiredDocsTitle; set => SetProperty(ref _requiredDocsTitle, value); }
    public string CounterTitle { get => _counterTitle; set => SetProperty(ref _counterTitle, value); }
    public string NoteTitle { get => _noteTitle; set => SetProperty(ref _noteTitle, value); }
    public string NavigateButtonText { get => _navigateButtonText; set => SetProperty(ref _navigateButtonText, value); }
    public string ListButtonText { get => _listButtonText; set => SetProperty(ref _listButtonText, value); }

    public ProcedureItem? SelectedProcedure
    {
        get => _selectedProcedure;
        set => SetProperty(ref _selectedProcedure, value);
    }

    public ICommand GoHomeCommand { get; }
    public ICommand ToggleLanguageCommand { get; }
    public ICommand OpenListCommand { get; }
    public ICommand NavigateToCounterCommand { get; }

    public ProcedureDetailViewModel(ILocalizationService localization) : base(localization)
    {
        GoHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//main"));
        ToggleLanguageCommand = new Command(Localization.ToggleLanguage);
        OpenListCommand = new Command(async () => await Shell.Current.GoToAsync("//procedure"));
        NavigateToCounterCommand = new Command(async () =>
        {
            if (SelectedProcedure is null)
            {
                return;
            }
            await Shell.Current.GoToAsync("//navi");
        });

        RefreshLocalizedProperties();
    }

    public void LoadProcedureByKey(string? key)
    {
        SelectedProcedure = ProcedureCatalog.BuildItems().FirstOrDefault(p => p.DestinationKey == key)
            ?? ProcedureCatalog.BuildItems().FirstOrDefault();
    }

    protected override void RefreshLocalizedProperties()
    {
        TitleText = StringIds.PROCEDURE_TITLE.GetString();
        HomeText = StringIds.COMMON_HOME.GetString();
        LanguageLabel = Localization.GetCurrentLanguageName();
        RequiredDocsTitle = StringIds.PROCEDURE_REQUIRED_DOCS.GetString();
        CounterTitle = StringIds.PROCEDURE_PROCESSING_COUNTER.GetString();
        NoteTitle = StringIds.PROCEDURE_NOTE.GetString();
        NavigateButtonText = StringIds.PROCEDURE_START_NAV.GetString();
        ListButtonText = StringIds.PROCEDURE_LIST_BUTTON.GetString();
        LoadProcedureByKey(SelectedProcedure?.DestinationKey ?? "DestinationPoint1");
    }
}
