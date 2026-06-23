using System;
using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Core;
using BeyondBedtime.Framework.Utils;

namespace BeyondBedtime.Framework.Patches
{
    /// <summary>
    /// Harmony patches to control the native Stardew Valley lighting logic.
    /// </summary>
    public static class LightingPatches
    {
        private static ModConfig _config = null!;
        private static ModState _state = null!;

        // Number of game update ticks that equal ten in-game minutes.
        private const int TicksPerTenMinutes = 420;

        /// <summary>
        /// Initializes the patch class with necessary dependencies.
        /// </summary>
        public static void Initialize(ModConfig config, ModState state)
        {
            _config = config;
            _state = state;
        }

        /// <summary>
        /// Prefix for Game1.isDarkOut.
        /// Forces the game to think it's daylight halfway through the morning transition,
        /// which makes indoor windows shine and turns off night lamps automatically.
        /// </summary>
        public static bool BeforeIsDarkOut(ref bool __result)
        {
            if (_config == null || _state == null || !_config.ModEnabled || !_state.ExtendedNightActive || _config.MorningLightStartTimeIndex <= 0)
                return true; // continue original

            int startTime = TimeUtils.ValidTimes[_config.MorningLightStartTimeIndex - 1];
            int endTime = 3000; // 6:00 AM
            float currentMins = TimeUtils.GetSmoothTimeInMinutes(_state.VirtualTimeOfDay, _state.TickCounter, TicksPerTenMinutes);
            float startMins = TimeUtils.StardewTimeToMinutes(startTime);
            float endMins = TimeUtils.StardewTimeToMinutes(endTime);

            // Midpoint of the transition
            float midpointMins = startMins + (endMins - startMins) / 2f;

            if (currentMins >= midpointMins)
            {
                __result = false;
                return false; // Skip original
            }

            return true; // Continue to original
        }

        /// <summary>
        /// Ensures that daytime lighting (like window rays) is allowed during our morning transition.
        /// </summary>
        public static bool Prefix_isTimeToTurnOffLighting(ref bool __result)
        {
            if (_config == null || _state == null || !_config.ModEnabled || !_state.ExtendedNightActive)
                return true; // continue original

            if (_config.MorningLightStartTimeIndex > 0)
            {
                int startTime = TimeUtils.ValidTimes[_config.MorningLightStartTimeIndex - 1];
                if (_state.VirtualTimeOfDay >= startTime)
                {
                    // It is no longer "time to turn off lighting", the sun is rising!
                    __result = false;
                    return false; // skip original
                }
            }
            return true;
        }
    }
}
