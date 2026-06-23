using System;

namespace BeyondBedtime.Framework.Utils
{
    /// <summary>
    /// Utility methods for Stardew Valley time conversions and calculations.
    /// </summary>
    public static class TimeUtils
    {
        /// <summary>
        /// All valid in-game times the player can select as their passout time.
        /// Stardew Valley uses integers like 630 = 6:30 AM, 2600 = 2:00 AM (next day), etc.
        /// </summary>
        public static readonly int[] ValidTimes =
        {
             630,  700,  730,  800,  830,  900,  930,
            1000, 1030, 1100, 1130, 1200, 1230, 1300, 1330,
            1400, 1430, 1500, 1530, 1600, 1630, 1700, 1730,
            1800, 1830, 1900, 1930, 2000, 2030,
            2100, 2130, 2200, 2230, 2300, 2330,
            2400, 2430, 2500, 2530, 2600, 2630,
            2700, 2730, 2800, 2830, 2900, 2930, 3000
        };

        /// <summary>
        /// Adds ten minutes to a Stardew time integer, correctly rolling over minutes.
        /// E.g. 2650 + 10 would naively give 2660, but Stardew uses base-60 minutes,
        /// so it should become 2700. This method handles that rollover.
        /// </summary>
        /// <param name="time">The Stardew time integer to add to.</param>
        /// <returns>The resulting time integer.</returns>
        public static int AddTenMinutes(int time)
        {
            int result = time + 10;
            if (result % 100 >= 60)
                result = result - result % 100 + 100;
            return result;
        }

        /// <summary>
        /// Converts a Stardew time integer (HHMM) to total minutes.
        /// </summary>
        /// <param name="time">The Stardew time integer.</param>
        /// <returns>The total minutes elapsed.</returns>
        public static int StardewTimeToMinutes(int time)
        {
            int hour = time / 100;
            int minute = time % 100;
            return hour * 60 + minute;
        }

        /// <summary>
        /// Returns the total elapsed minutes, adding the fraction of the current 10-minute tick window.
        /// This allows smooth per-frame interpolation.
        /// </summary>
        /// <param name="virtualTime">The current virtual time.</param>
        /// <param name="tickCounter">The current ticks since the last 10-minute update.</param>
        /// <param name="ticksPerTenMinutes">The number of ticks per 10-minute interval.</param>
        /// <returns>The interpolated time in minutes.</returns>
        public static float GetSmoothTimeInMinutes(int virtualTime, int tickCounter, int ticksPerTenMinutes)
        {
            float baseMinutes = StardewTimeToMinutes(virtualTime);
            float fraction = (float)tickCounter / ticksPerTenMinutes;
            return baseMinutes + (fraction * 10f);
        }

        /// <summary>
        /// Converts a Stardew time integer to a human-readable 12-hour string.
        /// E.g. 2600 → "2:00 AM", 1430 → "2:30 PM".
        /// Hours >= 2400 wrap around using modulo 24 to handle post-midnight times.
        /// </summary>
        /// <param name="stardewTime">The Stardew time integer.</param>
        /// <returns>The formatted time string.</returns>
        public static string FormatStardewTime(int stardewTime)
        {
            int hour = (stardewTime / 100) % 24;
            int minute = stardewTime % 100;
            string period = hour < 12 ? "AM" : "PM";
            int displayHour = hour % 12;
            if (displayHour == 0)
                displayHour = 12;
            return $"{displayHour}:{minute:D2} {period}";
        }
    }
}
