using System.Windows.Input;
using RobotHri.Languages;
using RobotHri.Services;

namespace RobotHri.Controls
{
    /// <summary>
    /// Collapsible navigation rail that sits on the left edge of a page.
    /// Collapsed (default): NavRailCollapsedWidth — icons only.
    /// Expanded: NavRailExpandedWidth — icons + labels, toggled by the ☰ "Main" button.
    /// </summary>
    public partial class NavRail : ContentView
    {
        // ─── Command BindableProperties ───────────────────────────────────────────

        public static readonly BindableProperty HomeCommandProperty =
            BindableProperty.Create(nameof(HomeCommand), typeof(ICommand), typeof(NavRail));

        public static readonly BindableProperty MainCommandProperty =
            BindableProperty.Create(nameof(MainCommand), typeof(ICommand), typeof(NavRail));

        public static readonly BindableProperty NaviCommandProperty =
            BindableProperty.Create(nameof(NaviCommand), typeof(ICommand), typeof(NavRail));

        public static readonly BindableProperty DeliCommandProperty =
            BindableProperty.Create(nameof(DeliCommand), typeof(ICommand), typeof(NavRail));

        // ─── Label BindableProperties ─────────────────────────────────────────────

        public static readonly BindableProperty MenuLabelProperty =
            BindableProperty.Create(nameof(MenuLabel), typeof(string), typeof(NavRail), "Menu",
                propertyChanged: (b, _, n) => ((NavRail)b).MenuText.Text = (string)n);

        public static readonly BindableProperty HomeLabelProperty =
            BindableProperty.Create(nameof(HomeLabel), typeof(string), typeof(NavRail), "Home",
                propertyChanged: (b, _, n) => ((NavRail)b).HomeText.Text = (string)n);

        public static readonly BindableProperty MainLabelProperty =
            BindableProperty.Create(nameof(MainLabel), typeof(string), typeof(NavRail), "Main",
                propertyChanged: (b, _, n) => ((NavRail)b).MainPageText.Text = (string)n);

        public static readonly BindableProperty NaviLabelProperty =
            BindableProperty.Create(nameof(NaviLabel), typeof(string), typeof(NavRail), "Navigation",
                propertyChanged: (b, _, n) => ((NavRail)b).NaviText.Text = (string)n);

        public static readonly BindableProperty DeliLabelProperty =
            BindableProperty.Create(nameof(DeliLabel), typeof(string), typeof(NavRail), "Delivery",
                propertyChanged: (b, _, n) => ((NavRail)b).DeliText.Text = (string)n);
        
        public static readonly BindableProperty SetupLabelProperty =
            BindableProperty.Create(nameof(SetupLabel), typeof(string), typeof(NavRail), "Setup",
                propertyChanged: (b, _, n) => ((NavRail)b).SetupText.Text = (string)n);

        // ─── CLR wrappers ─────────────────────────────────────────────────────────

        public ICommand? HomeCommand    { get => (ICommand?)GetValue(HomeCommandProperty);    set => SetValue(HomeCommandProperty, value); }
        public ICommand? MainCommand    { get => (ICommand?)GetValue(MainCommandProperty);    set => SetValue(MainCommandProperty, value); }
        public ICommand? NaviCommand    { get => (ICommand?)GetValue(NaviCommandProperty);    set => SetValue(NaviCommandProperty, value); }
        public ICommand? DeliCommand    { get => (ICommand?)GetValue(DeliCommandProperty);    set => SetValue(DeliCommandProperty, value); }

        public string MenuLabel    { get => (string)GetValue(MenuLabelProperty);    set => SetValue(MenuLabelProperty, value); }
        public string HomeLabel    { get => (string)GetValue(HomeLabelProperty);    set => SetValue(HomeLabelProperty, value); }
        public string MainLabel    { get => (string)GetValue(MainLabelProperty);    set => SetValue(MainLabelProperty, value); }
        public string NaviLabel    { get => (string)GetValue(NaviLabelProperty);    set => SetValue(NaviLabelProperty, value); }
        public string DeliLabel    { get => (string)GetValue(DeliLabelProperty);    set => SetValue(DeliLabelProperty, value); }
        public string SetupLabel   { get => (string)GetValue(SetupLabelProperty);   set => SetValue(SetupLabelProperty, value); }

        // ─── State ────────────────────────────────────────────────────────────────

        private bool _isExpanded;
        private bool _isAnimating;
        private readonly ILocalizationService? _localization;

        // Read animation widths from the shared resource dictionary so they stay
        // in sync with NavRailCollapsedWidth / NavRailExpandedWidth in Dimens.xaml.
        private double CollapsedWidth =>
            Application.Current?.Resources.TryGetValue("NavRailCollapsedWidth", out var v) == true
                ? (double)v : 72;

        private double ExpandedWidth =>
            Application.Current?.Resources.TryGetValue("NavRailExpandedWidth", out var v) == true
                ? (double)v : 200;

        // ─── Constructor ──────────────────────────────────────────────────────────

        public NavRail()
        {
            InitializeComponent();
            _localization = IPlatformApplication.Current?.Services.GetService<ILocalizationService>();
            if (_localization != null)
            {
                _localization.LanguageChanged += OnLanguageChanged;
            }

            RefreshLocalizedLabels();

            MenuTap.Tapped    += OnMenuTapped;
            HomeTap.Tapped    += async (_, _) => await ExecuteOrNavigateAsync(HomeCommand, "//home");
            MainTap.Tapped    += async (_, _) => await ExecuteOrNavigateAsync(MainCommand, "//main");
            NaviTap.Tapped    += async (_, _) => await ExecuteOrNavigateAsync(NaviCommand, "//navi");
            DeliTap.Tapped    += async (_, _) => await ExecuteOrNavigateAsync(DeliCommand, "//deli");
            SetupTap.Tapped   += async (_, _) => await NavigateAsync("//setup");
        }

        // ─── Toggle logic ─────────────────────────────────────────────────────────

        private async void OnMenuTapped(object? sender, TappedEventArgs e)
        {
            if (_isAnimating) return;
            _isAnimating = true;
            _isExpanded  = !_isExpanded;

            if (_isExpanded)
            {
                await AnimateWidth(CollapsedWidth, ExpandedWidth);
                SetLabelsVisible(true);
            }
            else
            {
                SetLabelsVisible(false);
                await AnimateWidth(ExpandedWidth, CollapsedWidth);
            }

            _isAnimating = false;
        }

        private Task AnimateWidth(double from, double to)
        {
            var tcs       = new TaskCompletionSource();
            var animation = new Animation(v => RailContainer.WidthRequest = v, from, to);
            animation.Commit(this, "RailWidth", length: 250, easing: Easing.CubicInOut,
                finished: (_, _) => tcs.TrySetResult());
            return tcs.Task;
        }

        private void SetLabelsVisible(bool visible)
        {
            MenuText.IsVisible    = visible;
            HomeText.IsVisible    = visible;
            MainPageText.IsVisible = visible;
            NaviText.IsVisible    = visible;
            DeliText.IsVisible    = visible;
            SetupText.IsVisible   = visible;
        }

        private static async Task ExecuteOrNavigateAsync(ICommand? command, string route)
        {
            if (command?.CanExecute(null) == true)
            {
                command.Execute(null);
                return;
            }

            await NavigateAsync(route);
        }

        private static async Task NavigateAsync(string route)
        {
            if (Shell.Current is null)
            {
                return;
            }

            await Shell.Current.GoToAsync(route);
        }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(RefreshLocalizedLabels);
        }

        private void RefreshLocalizedLabels()
        {
            MenuText.Text = "Menu";
            HomeText.Text = StringIds.COMMON_HOME.GetString();
            MainPageText.Text = StringIds.MAIN_TITLE.GetString();
            NaviText.Text = StringIds.MAIN_NAVIGATION.GetString();
            DeliText.Text = StringIds.MAIN_DELIVERY.GetString();
            SetupText.Text = StringIds.SETUP_TITLE.GetString();
        }
    }
}
