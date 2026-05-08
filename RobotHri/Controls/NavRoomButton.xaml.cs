namespace RobotHri.Controls
{
    /// <summary>
    /// Room navigation button with press animation.
    /// Mirrors RoomWaterIntake..f in the original Qt UI.
    /// </summary>
    public partial class NavRoomButton : ContentView
    {
        public static readonly BindableProperty RoomNameProperty =
            BindableProperty.Create(nameof(RoomName), typeof(string), typeof(NavRoomButton), string.Empty,
                propertyChanged: (b, o, n) => ((NavRoomButton)b).RoomLabel.Text = (string)n);

        public static readonly BindableProperty RoomKeyProperty =
            BindableProperty.Create(nameof(RoomKey), typeof(string), typeof(NavRoomButton), string.Empty,
                propertyChanged: (b, o, n) =>
                {
                    var control = (NavRoomButton)b;
                    control.UpdateActiveState(control.IsActive);
                });

        public static readonly BindableProperty IconSourceProperty =
            BindableProperty.Create(nameof(IconSource), typeof(string), typeof(NavRoomButton), null,
                propertyChanged: (b, o, n) => ((NavRoomButton)b).UpdateIcon((string?)n));

        public static readonly BindableProperty IsActiveProperty =
            BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(NavRoomButton), false,
                propertyChanged: (b, o, n) => ((NavRoomButton)b).UpdateActiveState((bool)n));

        public string RoomName
        {
            get => (string)GetValue(RoomNameProperty);
            set => SetValue(RoomNameProperty, value);
        }

        public string RoomKey
        {
            get => (string)GetValue(RoomKeyProperty);
            set => SetValue(RoomKeyProperty, value);
        }

        public string? IconSource
        {
            get => (string?)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public event EventHandler<string>? RoomSelected;

        public NavRoomButton()
        {
            InitializeComponent();
            RoomTap.Tapped += OnRoomTapped;
        }

        private async void OnRoomTapped(object? sender, TappedEventArgs e)
        {
            await RoomBorder.ScaleTo(0.94, 80);
            await RoomBorder.ScaleTo(1.0, 80);
            RoomSelected?.Invoke(this, RoomKey);
        }

        private void UpdateIcon(string? source)
        {
            RoomIcon.IsVisible = !string.IsNullOrEmpty(source);
            if (!string.IsNullOrEmpty(source))
                RoomIcon.Source = source;
        }

        private void UpdateActiveState(bool active)
        {
            if (active)
            {
                RoomBorder.BackgroundColor = Color.FromArgb("#69FF3D");
                RoomBorder.Stroke = Color.FromArgb("#4AA92A");
                RoomLabel.TextColor = Color.FromArgb("#1B2F10");
                return;
            }

            RoomBorder.BackgroundColor = GetPastelColor(RoomKey);
            RoomBorder.Stroke = Color.FromArgb("#668D8D8D");
            RoomLabel.TextColor = Color.FromArgb("#870002");
        }

        private static Color GetPastelColor(string? roomKey)
        {
            if (string.IsNullOrWhiteSpace(roomKey))
                return Color.FromArgb("#C8FFD9F7");

            var index = roomKey[^1];
            return index switch
            {
                '1' or '4' or '7' => Color.FromArgb("#C8FFD9F7"),
                '2' or '5' or '8' => Color.FromArgb("#C8C0FAFF"),
                _ => Color.FromArgb("#C8FFD3C5")
            };
        }
    }
}
