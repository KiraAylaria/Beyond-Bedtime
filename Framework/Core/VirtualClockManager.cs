using System;
using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BeyondBedtime.Framework.Core
{
    /// <summary>
    /// Drives the virtual clock for extended nights and delegates lighting and passout logic during tick updates.
    /// </summary>
    public class VirtualClockManager
    {
        private readonly ModState _state;
        private readonly ModConfig _config;
        private readonly LightingTransitionManager _lightingManager;
        private readonly PassoutManager _passoutManager;

        // Number of game update ticks that equal ten in-game minutes.
        // At 60 ticks/second, 7 game minutes = 7 * 60 = 420 ticks.
        private const int TicksPerTenMinutes = 420;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualClockManager"/> class.
        /// </summary>
        public VirtualClockManager(ModState state, ModConfig config, LightingTransitionManager lightingManager, PassoutManager passoutManager)
        {
            _state = state;
            _config = config;
            _lightingManager = lightingManager;
            _passoutManager = passoutManager;
        }

        /// <summary>
        /// Resets daily state and syncs the virtual clock to the start of the day.
        /// </summary>
        public void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            _state.ResetDailyState();
            _state.VirtualTimeOfDay = Game1.timeOfDay;
        }

        /// <summary>
        /// Clears all mod state to prevent carryover between game sessions.
        /// </summary>
        public void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            _state.ResetDailyState();
        }

        /// <summary>
        /// Drives the extended night logic, updates lighting, pauses for menus, and advances the virtual time.
        /// </summary>
        public void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !_config.ModEnabled || Game1.player is null)
                return;

            int configuredPassOut = GetConfiguredPassOutTime();

            // This handler is only needed when the target time is beyond 2:00 AM.
            if (configuredPassOut <= 2600)
                return;

            if (!_state.ExtendedNightActive)
            {
                if (Game1.timeOfDay >= 2600)
                {
                    _state.ExtendedNightActive = true;
                    _state.VirtualTimeOfDay = 2600;
                    _state.TickCounter = 0;
                    _state.LastLocation = Game1.currentLocation;
                    _state.SavedNightAmbientLight = Game1.ambientLight;
                    _state.SavedNightOutdoorLight = Game1.outdoorLight;
                }
                else
                {
                    _state.VirtualTimeOfDay = Game1.timeOfDay;
                    return;
                }
            }

            // Perform the lighting transition logic
            _lightingManager.UpdateLighting(TicksPerTenMinutes);

            // Pause the virtual clock when time is not supposed to pass.
            // This natively handles multiplayer logic (menus don't pause time in MP unless host pauses)
            // as well as singleplayer cutscenes, dialogues, etc.
            if (!Game1.shouldTimePass())
                return;

            _state.TickCounter++;
            if (_state.TickCounter < TicksPerTenMinutes)
                return;

            _state.TickCounter = 0;
            _state.VirtualTimeOfDay = TimeUtils.AddTenMinutes(_state.VirtualTimeOfDay);

            // Trigger passout if the extended virtual clock has reached the configured time
            _passoutManager.CheckVirtualPassout();
        }

        private int GetConfiguredPassOutTime()
        {
            int index = Math.Clamp(_config.PassOutTimeIndex, 0, TimeUtils.ValidTimes.Length - 1);
            return TimeUtils.ValidTimes[index];
        }
    }
}
