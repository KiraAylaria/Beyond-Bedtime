using System;
using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Utils;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BeyondBedtime.Framework.Core
{
    /// <summary>
    /// Manages the passout logic, including the toggle hotkey and triggering the passout sequence.
    /// </summary>
    public class PassoutManager
    {
        private readonly ModState _state;
        private readonly ModConfig _config;
        private readonly IMonitor _monitor;
        private readonly ITranslationHelper _translation;

        private long _lastToggleTime = 0;
        private const long ToggleCooldownMs = 3500;

        /// <summary>
        /// Initializes a new instance of the <see cref="PassoutManager"/> class.
        /// </summary>
        public PassoutManager(ModState state, ModConfig config, IMonitor monitor, ITranslationHelper translation)
        {
            _state = state;
            _config = config;
            _monitor = monitor;
            _translation = translation;
        }

        /// <summary>
        /// Handles the passout toggle hotkey. A cooldown prevents accidental double-triggers.
        /// </summary>
        public void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !_config.ModEnabled)
                return;

            if (!_config.TogglePassOutKey.JustPressed())
                return;

            long now = Environment.TickCount64;
            if (now - _lastToggleTime < ToggleCooldownMs)
                return;
            _lastToggleTime = now;

            _state.PassOutDisabledToday = !_state.PassOutDisabledToday;

            if (_state.PassOutDisabledToday)
            {
                Game1.addHUDMessage(new HUDMessage(_translation.Get("passout-disabled"), HUDMessage.newQuest_type));
                _monitor.Log("Passout disabled for today.", LogLevel.Info);
            }
            else
            {
                int configuredPassOut = GetConfiguredPassOutTime();

                // Clamp clock back to configured time if it ran beyond it while disabled
                if (configuredPassOut > 2600 && _state.ExtendedNightActive && _state.VirtualTimeOfDay > configuredPassOut)
                    _state.VirtualTimeOfDay = configuredPassOut;

                bool shouldPassOutNow =
                    configuredPassOut <= 2600
                        ? Game1.timeOfDay >= configuredPassOut
                        : _state.ExtendedNightActive && _state.VirtualTimeOfDay >= configuredPassOut;

                Game1.addHUDMessage(new HUDMessage(_translation.Get("passout-enabled"), HUDMessage.achievement_type));
                _monitor.Log("Passout re-enabled for today.", LogLevel.Info);

                if (shouldPassOutNow && Game1.player is not null && !Game1.player.passedOut && !Game1.player.isInBed.Value)
                {
                    _state.AllowForcedPassOut = true;
                    TriggerPassOut();
                    _state.AllowForcedPassOut = false;
                }
            }
        }

        /// <summary>
        /// Handles passout for configured times at or before 2:00 AM (timeOfDay &lt;= 2600).
        /// </summary>
        public void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (!Context.IsWorldReady || !_config.ModEnabled || Game1.player is null)
                return;

            if (_state.PassOutDisabledToday)
                return;

            int configuredPassOut = GetConfiguredPassOutTime();

            if (configuredPassOut > 2600)
                return;

            if (Game1.player.passedOut || Game1.player.isInBed.Value)
                return;

            if (e.NewTime >= configuredPassOut)
            {
                _state.AllowForcedPassOut = true;
                TriggerPassOut();
                _state.AllowForcedPassOut = false;
            }
        }

        /// <summary>
        /// Checks if the virtual passout time has been reached during an extended night.
        /// </summary>
        public void CheckVirtualPassout()
        {
            if (_state.PassOutDisabledToday)
                return;

            int configuredPassOut = GetConfiguredPassOutTime();
            if (_state.VirtualTimeOfDay >= configuredPassOut)
            {
                _state.AllowForcedPassOut = true;
                TriggerPassOut();
                _state.AllowForcedPassOut = false;
            }
        }

        /// <summary>
        /// Performs the actual passout sequence.
        /// </summary>
        public static void TriggerPassOut()
        {
            if (Game1.player is null)
                return;

            Game1.player.mount?.dismount();

            if (Game1.player.IsSitting())
                Game1.player.StopSitting(animate: false);

            if (Game1.player.UsingTool && Game1.player.CurrentTool != null)
                Game1.player.completelyStopAnimatingOrDoingAction();

            Game1.player.CanMove = false;
            Game1.player.freezePause = 7000;
            Game1.player.stamina = -15f;
            Game1.player.startToPassOut();
        }

        private int GetConfiguredPassOutTime()
        {
            int index = Math.Clamp(_config.PassOutTimeIndex, 0, TimeUtils.ValidTimes.Length - 1);
            return TimeUtils.ValidTimes[index];
        }
    }
}
