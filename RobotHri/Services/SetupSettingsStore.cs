using RobotHri.Models;
using SQLite;

namespace RobotHri.Services
{
    public class SetupSettingsStore : ISetupSettingsStore
    {
        private readonly SQLiteAsyncConnection _connection;
        private bool _initialized;

        public SetupSettingsStore()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "robot_hri.db3");
            _connection = new SQLiteAsyncConnection(dbPath);
        }

        public async Task<SetupSettingsEntity> GetAsync()
        {
            await EnsureInitializedAsync();

            var settings = await _connection.Table<SetupSettingsEntity>()
                .Where(x => x.Id == 1)
                .FirstOrDefaultAsync();

            if (settings is not null)
            {
                if (MigrateLegacySpeedUnits(settings))
                    await _connection.InsertOrReplaceAsync(settings);
                return settings;
            }

            settings = new SetupSettingsEntity { Id = 1 };
            await _connection.InsertAsync(settings);
            return settings;
        }

        public async Task SaveAsync(SetupSettingsEntity settings)
        {
            await EnsureInitializedAsync();
            settings.Id = 1;
            await _connection.InsertOrReplaceAsync(settings);
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized)
                return;

            await _connection.CreateTableAsync<SetupSettingsEntity>();
            _initialized = true;
        }

        /// <summary>
        /// Older builds stored linear speeds as cm/s (typically 10–120). Values greater than 5 are treated as legacy cm/s and converted to m/s.
        /// </summary>
        private static bool MigrateLegacySpeedUnits(SetupSettingsEntity s)
        {
            bool changed = false;

            if (s.SpeedMS > 5.01)
            {
                s.SpeedMS /= 100.0;
                changed = true;
            }

            if (s.RoughTerrainSpeedMS > 5.01)
            {
                s.RoughTerrainSpeedMS /= 100.0;
                changed = true;
            }

            var clampedSpeed = Math.Clamp(s.SpeedMS, 0, 5);
            var clampedRough = Math.Clamp(s.RoughTerrainSpeedMS, 0, 5);
            if (Math.Abs(clampedSpeed - s.SpeedMS) > 1e-9)
            {
                s.SpeedMS = clampedSpeed;
                changed = true;
            }

            if (Math.Abs(clampedRough - s.RoughTerrainSpeedMS) > 1e-9)
            {
                s.RoughTerrainSpeedMS = clampedRough;
                changed = true;
            }

            return changed;
        }
    }
}
