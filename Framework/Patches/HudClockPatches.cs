using System;
using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Core;
using BeyondBedtime.Framework.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace BeyondBedtime.Framework.Patches
{
    /// <summary>
    /// Harmony patches to control the HUD clock rendering and time strings.
    /// </summary>
    public static class HudClockPatches
    {
        private static ModConfig _config = null!;
        private static ModState _state = null!;
        
        private static bool _isDrawingVirtualClock;
        private static int _realTimeBackup;

        /// <summary>
        /// Initializes the patch class with necessary dependencies.
        /// </summary>
        public static void Initialize(ModConfig config, ModState state)
        {
            _config = config;
            _state = state;
        }

        /// <summary>
        /// Prefix for DayTimeMoneyBox.draw().
        /// Handles swapping the real time with the virtual time or drawing the hidden clock ("--:--").
        /// </summary>
        public static bool BeforeDayTimeMoneyBoxDraw(DayTimeMoneyBox __instance, SpriteBatch b)
        {
            if (!Context.IsWorldReady || _config == null || _state == null || !_config.ModEnabled)
                return true;

            int configuredPassOut = GetConfiguredPassOutTime();

            if (ShouldShowHiddenClock(configuredPassOut))
            {
                DrawDayTimeMoneyBoxWithCustomTime(__instance, b, "--:--");
                return false;
            }

            if (configuredPassOut <= 2600)
                return true;

            if (!_state.ExtendedNightActive)
                return true;

            _realTimeBackup = Game1.timeOfDay;
            Game1.timeOfDay = _state.VirtualTimeOfDay;
            _isDrawingVirtualClock = true;
            return true;
        }

        /// <summary>
        /// Postfix for DayTimeMoneyBox.draw().
        /// Restores the real time after the virtual clock has been drawn.
        /// </summary>
        public static void AfterDayTimeMoneyBoxDraw()
        {
            if (!_isDrawingVirtualClock)
                return;

            Game1.timeOfDay = _realTimeBackup;
            _isDrawingVirtualClock = false;
        }

        /// <summary>
        /// Postfix for Game1.getTimeOfDayString(int time).
        /// Overrides the time string to show the hidden clock or virtual time.
        /// </summary>
        public static void AfterGetTimeOfDayString(int time, ref string __result)
        {
            if (!Context.IsWorldReady || _config == null || _state == null || !_config.ModEnabled)
                return;

            int configuredPassOut = GetConfiguredPassOutTime();

            if (ShouldShowHiddenClock(configuredPassOut))
            {
                __result = "--:--";
                return;
            }

            if (configuredPassOut <= 2600 || !_state.ExtendedNightActive)
                return;

            if (time >= 2600 && !_state.PassOutDisabledToday)
                __result = TimeUtils.FormatStardewTime(_state.VirtualTimeOfDay);
        }

        private static int GetConfiguredPassOutTime()
        {
            int index = Math.Clamp(_config.PassOutTimeIndex, 0, TimeUtils.ValidTimes.Length - 1);
            return TimeUtils.ValidTimes[index];
        }

        private static bool ShouldShowHiddenClock(int configuredPassOut)
        {
            if (!_state.PassOutDisabledToday)
                return false;

            if (configuredPassOut <= 2600)
                return Game1.timeOfDay >= configuredPassOut;

            if (!_state.ExtendedNightActive)
                return false;

            return _state.VirtualTimeOfDay >= configuredPassOut;
        }

        /// <summary>
        /// Draws the DayTimeMoneyBox UI manually, replacing the time text with a custom string.
        /// </summary>
        private static void DrawDayTimeMoneyBoxWithCustomTime(DayTimeMoneyBox box, SpriteBatch b, string customTimeText)
        {
            SpriteFont font = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko
                ? Game1.smallFont
                : Game1.dialogueFont;

            Vector2 position = new Vector2(Game1.uiViewport.Width - 300, 8f);

            if (Game1.isOutdoorMapSmallerThanViewport())
            {
                position = new Vector2(
                    Math.Min(position.X, -Game1.uiViewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 300),
                    8f
                );
            }

            Utility.makeSafe(ref position, 300, 284);

            box.position = position;
            box.xPositionOnScreen = (int)position.X;
            box.yPositionOnScreen = (int)position.Y;
            box.questButton.bounds = new Rectangle(box.xPositionOnScreen + 212, box.yPositionOnScreen + 240, 44, 46);
            box.zoomOutButton.bounds = new Rectangle(box.xPositionOnScreen + 92, box.yPositionOnScreen + 244, 28, 32);
            box.zoomInButton.bounds = new Rectangle(box.xPositionOnScreen + 124, box.yPositionOnScreen + 244, 28, 32);

            b.Draw(Game1.mouseCursors, position, new Rectangle(333, 431, 71, 43),
                   Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

            string dayText = Game1.shortDayDisplayNameFromDayOfSeason(Game1.dayOfMonth) + ". " + Game1.dayOfMonth;
            Vector2 daySize = font.MeasureString(dayText);
            Vector2 dayPos = new Vector2(333f * 0.5625f - daySize.X / 2f, 431f * 0.1f - daySize.Y / 2f);
            Utility.drawTextWithShadow(b, dayText, font, position + dayPos, Game1.textColor);

            b.Draw(Game1.mouseCursors, position + new Vector2(212f, 68f),
                   new Rectangle(406, 441 + Game1.seasonIndex * 8, 12, 8),
                   Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

            if (Game1.weatherIcon == 999)
                b.Draw(Game1.mouseCursors_1_6, position + new Vector2(116f, 68f),
                       new Rectangle(243, 293, 12, 8),
                       Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            else
                b.Draw(Game1.mouseCursors, position + new Vector2(116f, 68f),
                       new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8),
                       Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);

            Vector2 txtSize = font.MeasureString(customTimeText);
            Vector2 timePos = new Vector2(333f * 0.55f - txtSize.X / 2f, 431f * 0.31f - txtSize.Y / 2f);
            Utility.drawTextWithShadow(b, customTimeText, font, position + timePos, Game1.textColor);

            if (Game1.player.hasVisibleQuests)
                box.questButton.draw(b);

            if (Game1.options.zoomButtons)
            {
                box.zoomInButton.draw(b, Color.White * (Game1.options.desiredBaseZoomLevel >= 2f ? 0.5f : 1f), 1f);
                box.zoomOutButton.draw(b, Color.White * (Game1.options.desiredBaseZoomLevel <= 0.75f ? 0.5f : 1f), 1f);
            }

            box.drawMoneyBox(b);
        }
    }
}
