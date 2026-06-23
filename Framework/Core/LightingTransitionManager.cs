using System;
using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Utils;
using Microsoft.Xna.Framework;
using StardewValley;

namespace BeyondBedtime.Framework.Core
{
    /// <summary>
    /// Handles the smooth morning light transition during an extended night.
    /// </summary>
    public class LightingTransitionManager
    {
        private readonly ModState _state;
        private readonly ModConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightingTransitionManager"/> class.
        /// </summary>
        public LightingTransitionManager(ModState state, ModConfig config)
        {
            _state = state;
            _config = config;
        }

        /// <summary>
        /// Updates the lighting logic, fading from night to morning if applicable.
        /// </summary>
        /// <param name="ticksPerTenMinutes">The number of game ticks per 10 in-game minutes.</param>
        public void UpdateLighting(int ticksPerTenMinutes)
        {
            if (!_state.ExtendedNightActive || _config.MorningLightStartTimeIndex <= 0)
                return;

            // If the player changes locations, update the base colors.
            if (Game1.currentLocation != _state.LastLocation)
            {
                _state.LastLocation = Game1.currentLocation;
                _state.SavedNightAmbientLight = Game1.ambientLight;
                _state.SavedNightOutdoorLight = Game1.outdoorLight;
            }

            if (_state.SavedNightAmbientLight.HasValue && _state.SavedNightOutdoorLight.HasValue)
            {
                int startTime = TimeUtils.ValidTimes[_config.MorningLightStartTimeIndex - 1];
                int endTime = 3000; // 6:00 AM
                
                float currentMins = TimeUtils.GetSmoothTimeInMinutes(_state.VirtualTimeOfDay, _state.TickCounter, ticksPerTenMinutes);
                float startMins = TimeUtils.StardewTimeToMinutes(startTime);
                float endMins = TimeUtils.StardewTimeToMinutes(endTime);
                
                if (currentMins >= startMins)
                {
                    float rawT = Math.Clamp((currentMins - startMins) / (endMins - startMins), 0f, 1f);
                    
                    // Apply the configured power curve
                    float t = (float)Math.Pow(rawT, _config.MorningLightPower);
                    
                    // Native transparency at night is 0.93f. We fade it to 0f for full daylight.
                    float transparency = MathHelper.Lerp(0.93f, 0f, t);
                    
                    if (rawT >= 1.0f)
                    {
                        // Transition complete. Use Transparent.
                        Game1.outdoorLight = Color.Transparent;
                        Game1.ambientLight = Color.Transparent;
                    }
                    else
                    {
                        // Recalculate outdoorLight with fading transparency.
                        Game1.outdoorLight = (Game1.currentLocation != null && Game1.currentLocation.IsRainingHere() 
                            ? Game1.ambientLight : Game1.eveningColor) * transparency;
                            
                        // Indoor ambient light fade (only if it was actually dark at night)
                        if (_state.SavedNightAmbientLight.Value != Color.White)
                        {
                            Game1.ambientLight = Color.Lerp(_state.SavedNightAmbientLight.Value, Color.Transparent, t);
                        }
                    }

                    // Trigger native Stardew Valley window states halfway through the transition.
                    if (Game1.currentLocation != null && !Game1.isDarkOut(Game1.currentLocation))
                    {
                        foreach (var f in Game1.currentLocation.furniture)
                        {
                            if (f.furniture_type.Value == 13) // Window
                            {
                                f.removeLights();
                            }
                        }
                    }
                }
            }
        }
    }
}
