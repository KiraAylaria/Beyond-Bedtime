using System;
using StardewModdingAPI.Utilities;
using BeyondBedtime.Framework.Utils;

namespace BeyondBedtime.Framework.Config
{
    /// <summary>
    /// Persisted configuration for the PassingOutControl mod.
    /// SMAPI serialises this class to/from config.json automatically.
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Index into <see cref="TimeUtils.ValidTimes"/> that determines when the player passes out.
        /// Storing an index rather than a raw time value guarantees that only legal Stardew
        /// time increments (every 10 minutes) can be configured.
        ///
        /// Default: the index of 2600 (2:00 AM) in ValidTimes, which matches vanilla behaviour.
        /// The conditional expression guards against the unlikely case where 2600 is not present
        /// in the array, falling back to index 0 (6:30 AM) to avoid an invalid value.
        /// </summary>
        public int PassOutTimeIndex { get; set; } = Array.IndexOf(TimeUtils.ValidTimes, 2600) is int i && i >= 0 ? i : 0;

        /// <summary>
        /// The key (or key combination) the player presses to toggle passout on/off for today.
        /// Stored as a <see cref="KeybindList"/> so the player can assign a single key,
        /// a multi-key chord (e.g. LeftShift + F10), or even multiple alternative bindings.
        ///
        /// Default: RightAlt — an uncommon key that is unlikely to conflict with other mods or the game.
        /// </summary>
        public KeybindList TogglePassOutKey { get; set; } = KeybindList.Parse("RightAlt");

        /// <summary>
        /// Index into <see cref="TimeUtils.ValidTimes"/> for when the morning light transition should start.
        /// 0 means the feature is disabled (Off).
        /// 1 corresponds to ValidTimes[0], 2 corresponds to ValidTimes[1], etc.
        /// </summary>
        public int MorningLightStartTimeIndex { get; set; } = 0;

        /// <summary>
        /// Power of the morning light transition curve.
        /// 1.0 is linear, 2.0 is square (slower start, faster finish), etc.
        /// </summary>
        public float MorningLightPower { get; set; } = 2.0f;

        /// <summary>
        /// Master toggle to enable or disable the entire mod's functionality.
        /// </summary>
        public bool ModEnabled { get; set; } = true;
    }
}
