using RobotHri.Controls.Base;
using RobotHri.ViewModels;

namespace RobotHri.Views
{
    [QueryProperty(nameof(NavigationGoal), "goal")]
    public partial class NaviPage : BaseContentPage, IQueryAttributable
    {
        private readonly NaviViewModel _viewModel;
        private string? _pendingGoalKey;
        private bool _hasAppearedOnce;

        public NaviPage(NaviViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            Header.BackTapped += (s, e) => viewModel.GoHomeCommand.Execute(null);
            Header.LanguageToggled += (s, e) => viewModel.ToggleLanguageCommand.Execute(null);
        }

        public string? NavigationGoal
        {
            set => QueueGoalFromQuery(value);
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("goal", out var raw))
                QueueGoalFromQuery(raw?.ToString());
        }

        private void QueueGoalFromQuery(string? raw)
        {
            var key = DecodeGoalKey(raw);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            _pendingGoalKey = key;
            MaybeStartNavigationFromProcedure();
        }

        private void MaybeStartNavigationFromProcedure()
        {
            if (string.IsNullOrEmpty(_pendingGoalKey) || !_hasAppearedOnce)
            {
                return;
            }

            var key = _pendingGoalKey;
            _pendingGoalKey = null;
            _viewModel.AttachMqttHandlers();
            _viewModel.SelectRoomCommand.Execute(key);
        }

        private static string? DecodeGoalKey(string? raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }

            try
            {
                return Uri.UnescapeDataString(raw);
            }
            catch (UriFormatException)
            {
                return raw;
            }
        }

        private void OnRoomSelected(object sender, string roomKey)
        {
            _viewModel.SelectRoomCommand.Execute(roomKey);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.AttachMqttHandlers();
            _hasAppearedOnce = true;
            MaybeStartNavigationFromProcedure();
        }

        protected override void OnDisappearing()
        {
            _viewModel.DetachMqttHandlers();
            _ = _viewModel.StopSpeechAsync();
            base.OnDisappearing();
        }
    }
}
