using RobotHri.Controls.Base;
using RobotHri.ViewModels;

namespace RobotHri.Views;

public partial class ProcedurePage : BaseContentPage
{
    public ProcedurePage(ProcedureViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Header.BackTapped += (s, e) => viewModel.GoHomeCommand.Execute(null);
        Header.LanguageToggled += (s, e) => viewModel.ToggleLanguageCommand.Execute(null);
    }
}
