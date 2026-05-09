using RobotHri.Controls.Base;
using RobotHri.ViewModels;

namespace RobotHri.Views;

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

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("key", out var rawKey))
        {
            _viewModel.LoadProcedureByKey(rawKey?.ToString());
        }
    }
}
