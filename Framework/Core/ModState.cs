using Microsoft.Xna.Framework;
using StardewValley;

namespace BeyondBedtime.Framework.Core
{
    /// <summary>
    /// Holds the global state for the BeyondBedtime mod.
    /// This allows static Harmony patches to access and modify the current state
    /// without relying on a singleton ModEntry instance.
    /// </summary>
    public class ModState
    {
        /// <summary>
        /// Gets or sets a value indicating whether the real game clock has frozen at 2:00 AM
        /// and the virtual clock has taken over.
        /// </summary>
        public bool ExtendedNightActive { get; set; }

        /// <summary>
        /// Gets or sets the custom time counter used when the configured passout time is after 2:00 AM.
        /// </summary>
        public int VirtualTimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the game ticks since the last virtual ten-minute increment.
        /// </summary>
        public int TickCounter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the player has used the hotkey to suppress passout for today.
        /// </summary>
        public bool PassOutDisabledToday { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mod is intentionally triggering a passout,
        /// signaling the Transpiler to allow the vanilla 2:00 AM branch to fire.
        /// </summary>
        public bool AllowForcedPassOut { get; set; }

        /// <summary>
        /// Gets or sets the last location the player was in, used to update base lighting when walking indoors/outdoors.
        /// </summary>
        public GameLocation? LastLocation { get; set; }

        /// <summary>
        /// Gets or sets the saved base night ambient light color, to prevent interpolation feedback loops.
        /// </summary>
        public Color? SavedNightAmbientLight { get; set; }

        /// <summary>
        /// Gets or sets the saved base night outdoor light color, to prevent interpolation feedback loops.
        /// </summary>
        public Color? SavedNightOutdoorLight { get; set; }

        /// <summary>
        /// Resets the daily state at the start of a new day or when returning to title.
        /// </summary>
        public void ResetDailyState()
        {
            this.ExtendedNightActive = false;
            this.VirtualTimeOfDay = 0;
            this.TickCounter = 0;
            this.PassOutDisabledToday = false;
            this.AllowForcedPassOut = false;
            this.LastLocation = null;
            this.SavedNightAmbientLight = null;
            this.SavedNightOutdoorLight = null;
        }
    }
}
