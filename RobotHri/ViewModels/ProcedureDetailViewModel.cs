using System.Windows.Input;
using RobotHri.Languages;
using RobotHri.Services;

namespace RobotHri.ViewModels;

public class ProcedureDetailViewModel : BaseViewModel
{
    private readonly IProcedureDocumentExtractor _documentExtractor;
    private string _titleText = string.Empty;
    private string _homeText = string.Empty;
    private string _languageLabel = "VI";
    private string _dossierSectionTitle = string.Empty;
    private string _implementationSectionTitle = string.Empty;
    private string _counterTitle = string.Empty;
    private string _noteTitle = string.Empty;
    private string _navigateButtonText = string.Empty;
    private string _listButtonText = string.Empty;
    private string _dossierBody = string.Empty;
    private string _implementationBody = string.Empty;
    private ProcedureItem? _selectedProcedure;

    public string TitleText { get => _titleText; set => SetProperty(ref _titleText, value); }
    public string HomeText { get => _homeText; set => SetProperty(ref _homeText, value); }
    public string LanguageLabel { get => _languageLabel; set => SetProperty(ref _languageLabel, value); }
    public string DossierSectionTitle { get => _dossierSectionTitle; set => SetProperty(ref _dossierSectionTitle, value); }
    public string ImplementationSectionTitle { get => _implementationSectionTitle; set => SetProperty(ref _implementationSectionTitle, value); }
    public string CounterTitle { get => _counterTitle; set => SetProperty(ref _counterTitle, value); }
    public string NoteTitle { get => _noteTitle; set => SetProperty(ref _noteTitle, value); }
    public string NavigateButtonText { get => _navigateButtonText; set => SetProperty(ref _navigateButtonText, value); }
    public string ListButtonText { get => _listButtonText; set => SetProperty(ref _listButtonText, value); }
    public string DossierBody { get => _dossierBody; set => SetProperty(ref _dossierBody, value); }
    public string ImplementationBody { get => _implementationBody; set => SetProperty(ref _implementationBody, value); }

    public ProcedureItem? SelectedProcedure
    {
        get => _selectedProcedure;
        set => SetProperty(ref _selectedProcedure, value);
    }

    public ICommand GoHomeCommand { get; }
    public ICommand ToggleLanguageCommand { get; }
    public ICommand OpenListCommand { get; }
    public ICommand NavigateToCounterCommand { get; }

    public ProcedureDetailViewModel(
        ILocalizationService localization,
        IProcedureDocumentExtractor documentExtractor) : base(localization)
    {
        _documentExtractor = documentExtractor;
        GoHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//main"));
        ToggleLanguageCommand = new Command(Localization.ToggleLanguage);
        OpenListCommand = new Command(async () => await Shell.Current.GoToAsync("//procedure"));
        NavigateToCounterCommand = new Command(async () =>
        {
            if (SelectedProcedure is null)
            {
                return;
            }

            var goal = Uri.EscapeDataString(SelectedProcedure.DestinationKey);
            await Shell.Current.GoToAsync($"//navi?goal={goal}");
        });

        RefreshLocalizedProperties();
    }

    public void LoadProcedureByKey(string? key)
    {
        var items = ProcedureCatalog.BuildItems();
        SelectedProcedure = items.FirstOrDefault(p => p.DestinationKey == key)
            ?? items.FirstOrDefault();
        _ = LoadDocumentSectionsAsync();
    }

    protected override void RefreshLocalizedProperties()
    {
        TitleText = StringIds.PROCEDURE_TITLE.GetString();
        HomeText = StringIds.COMMON_HOME.GetString();
        LanguageLabel = Localization.GetCurrentLanguageName();
        DossierSectionTitle = StringIds.PROCEDURE_SECTION_DOSSIER.GetString();
        ImplementationSectionTitle = StringIds.PROCEDURE_SECTION_IMPLEMENTATION.GetString();
        CounterTitle = StringIds.PROCEDURE_PROCESSING_COUNTER.GetString();
        NoteTitle = StringIds.PROCEDURE_NOTE.GetString();
        NavigateButtonText = StringIds.PROCEDURE_START_NAV.GetString();
        ListButtonText = StringIds.PROCEDURE_LIST_BUTTON.GetString();
        _ = LoadDocumentSectionsAsync();
    }

    private async Task LoadDocumentSectionsAsync()
    {
        var item = SelectedProcedure;
        if (item is null)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DossierBody = string.Empty;
                ImplementationBody = string.Empty;
            });
            return;
        }

        var lang = Localization.CurrentLanguageCode;
        try
        {
            var extracted = await _documentExtractor
                .ExtractAsync(item.RawAssetPath, lang)
                .ConfigureAwait(false);

            var dossier = extracted.Dossier.Trim();
            var impl = extracted.Implementation.Trim();

            if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase) && item.ProcedureIndex > 0)
            {
                var idx = item.ProcedureIndex;
                var implKey = $"procedure_en_extract_impl_{idx}";
                var iResolved = implKey.GetString();
                if (!LooksLikeMissingKey(implKey, iResolved))
                    impl = iResolved;
            }

            if (string.IsNullOrWhiteSpace(dossier))
                dossier = item.RequiredDocs;

            var dFinal = dossier;
            var iFinal = impl;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DossierBody = dFinal;
                ImplementationBody = iFinal;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProcedureDetail] Extract failed: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DossierBody = item.RequiredDocs;
                ImplementationBody = string.Empty;
            });
        }
    }

    private static bool LooksLikeMissingKey(string key, string resolved) =>
        string.IsNullOrWhiteSpace(resolved) || string.Equals(resolved, key, StringComparison.Ordinal);
}
