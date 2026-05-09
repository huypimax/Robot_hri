using System.ComponentModel;

namespace RobotHri.Services
{
    /// <summary>
    /// Latest robot battery level from MQTT (0–100%). Shared across the app for header display.
    /// </summary>
    public interface IBatteryStatusService : INotifyPropertyChanged
    {
        /// <summary>Null until the first message is received.</summary>
        double? Percent { get; }

        /// <summary>Formatted for UI, e.g. "87%" or "—" when unknown.</summary>
        string DisplayText { get; }

        void SetPercent(double percent);
    }
}
