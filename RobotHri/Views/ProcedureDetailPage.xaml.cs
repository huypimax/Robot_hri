using RobotHri.Controls.Base;
using RobotHri.ViewModels;

namespace RobotHri.Views;

/// <summary>
/// Shell query <c>?key=DestinationPoint1</c> is delivered reliably via <see cref="QueryPropertyAttribute"/>;
/// <see cref="IQueryAttributable"/> alone can miss updates when the tab content is reused.
/// </summary>
[QueryProperty(nameof(NavigationKey), "key")]
public partial class ProcedureDetailPage : BaseContentPage, IQueryAttributable
{
    private readonly ProcedureDetailViewModel _viewModel;

    public ProcedureDetailPage(ProcedureDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        Header.BackTapped += (s, e) => viewModel.GoHomeCommand.Execute(null);
        Header.LanguageToggled += (s, e) => viewModel.ToggleLanguageCommand.Execute(null);
    }

    /// <summary>Bound by Shell from the route query string.</summary>
    public string? NavigationKey
    {
        set => _viewModel.LoadProcedureByKey(DecodeKey(value));
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("key", out var rawKey))
            _viewModel.LoadProcedureByKey(DecodeKey(rawKey?.ToString()));
    }

    private static string? DecodeKey(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
            return null;
        try
        {
            return Uri.UnescapeDataString(raw);
        }
        catch (UriFormatException)
        {
            return raw;
        }
    }
}
