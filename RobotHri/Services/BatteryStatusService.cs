using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RobotHri.Services
{
    public sealed class BatteryStatusService : IBatteryStatusService
    {
        private double? _percent;

        public double? Percent
        {
            get => _percent;
            private set
            {
                if (Nullable.Equals(_percent, value))
                    return;
                _percent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText =>
            _percent.HasValue ? $"{Math.Round(_percent.Value)}%" : "—";

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetPercent(double percent)
        {
            Percent = Math.Clamp(percent, 0, 100);
#if DEBUG
            Debug.WriteLine($"[Battery] Display={DisplayText}, Percent={Percent}");
#endif
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
