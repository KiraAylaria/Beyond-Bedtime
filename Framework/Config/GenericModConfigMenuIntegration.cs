using System;
using BeyondBedtime.Framework.Utils;
using StardewModdingAPI;

namespace BeyondBedtime.Framework.Config
{
    /// <summary>
    /// Handles registration with the Generic Mod Config Menu (GMCM).
    /// </summary>
    public class GenericModConfigMenuIntegration
    {
        private readonly IModHelper _helper;
        private readonly IManifest _manifest;
        private readonly Func<ModConfig> _getConfig;
        private readonly Action _resetConfig;
        private readonly Action _saveConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericModConfigMenuIntegration"/> class.
        /// </summary>
        /// <param name="helper">The SMAPI mod helper.</param>
        /// <param name="manifest">The mod manifest.</param>
        /// <param name="getConfig">A function that returns the current mod configuration.</param>
        /// <param name="resetConfig">An action that resets the configuration to default.</param>
        /// <param name="saveConfig">An action that saves the current configuration.</param>
        public GenericModConfigMenuIntegration(
            IModHelper helper, 
            IManifest manifest, 
            Func<ModConfig> getConfig, 
            Action resetConfig, 
            Action saveConfig)
        {
            this._helper = helper;
            this._manifest = manifest;
            this._getConfig = getConfig;
            this._resetConfig = resetConfig;
            this._saveConfig = saveConfig;
        }

        /// <summary>
        /// Registers the mod configuration with GMCM.
        /// </summary>
        public void Register()
        {
            var configMenu = this._helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: this._manifest,
                reset: this._resetConfig,
                save: this._saveConfig
            );

            // Add the master toggle for the entire mod.
            configMenu.AddBoolOption(
                mod: this._manifest,
                name: () => this._helper.Translation.Get("config.mod-enabled.name"),
                tooltip: () => this._helper.Translation.Get("config.mod-enabled.tooltip"),
                getValue: () => this._getConfig().ModEnabled,
                setValue: value => this._getConfig().ModEnabled = value
            );

            // Section: Passout Settings
            configMenu.AddSectionTitle(this._manifest,
                () => this._helper.Translation.Get("config.section.passout"));

            // Add the slider to configure the exact passout time. 
            // It maps an index to the ValidTimes array to ensure only legal Stardew times are used.
            configMenu.AddNumberOption(
                mod: this._manifest,
                name: () => this._helper.Translation.Get("config.passout-time.name"),
                tooltip: () => this._helper.Translation.Get("config.passout-time.tooltip"),
                getValue: () => this._getConfig().PassOutTimeIndex,
                setValue: value => this._getConfig().PassOutTimeIndex = Math.Clamp(value, 0, TimeUtils.ValidTimes.Length - 1),
                min: 0,
                max: TimeUtils.ValidTimes.Length - 1,
                interval: 1,
                formatValue: index => TimeUtils.FormatStardewTime(TimeUtils.ValidTimes[Math.Clamp(index, 0, TimeUtils.ValidTimes.Length - 1)])
            );

            // Add the slider to configure when the morning light transition should start.
            // A value of 0 means the feature is disabled.
            configMenu.AddNumberOption(
                mod: this._manifest,
                name: () => this._helper.Translation.Get("config.morning-light-start.name"),
                tooltip: () => this._helper.Translation.Get("config.morning-light-start.tooltip"),
                getValue: () =>
                {
                    var config = this._getConfig();
                    if (config.MorningLightStartTimeIndex == 0) return 0;
                    return Math.Clamp(config.MorningLightStartTimeIndex - 39, 1, TimeUtils.ValidTimes.Length - 40);
                },
                setValue: value =>
                {
                    var config = this._getConfig();
                    if (value == 0) config.MorningLightStartTimeIndex = 0;
                    else config.MorningLightStartTimeIndex = Math.Clamp(value + 39, 0, TimeUtils.ValidTimes.Length - 1);
                },
                min: 0,
                max: TimeUtils.ValidTimes.Length - 40,
                interval: 1,
                formatValue: val => val == 0 ? this._helper.Translation.Get("config.off") : TimeUtils.FormatStardewTime(TimeUtils.ValidTimes[Math.Clamp(val + 39 - 1, 0, TimeUtils.ValidTimes.Length - 1)])
            );

            // Add the slider to configure the power curve of the morning light transition.
            configMenu.AddNumberOption(
                mod: this._manifest,
                name: () => this._helper.Translation.Get("config.morning-light-power.name"),
                tooltip: () => this._helper.Translation.Get("config.morning-light-power.tooltip"),
                getValue: () => this._getConfig().MorningLightPower,
                setValue: value => this._getConfig().MorningLightPower = value,
                min: 0.5f,
                max: 5.0f,
                interval: 0.1f
            );

            // Section: Hotkeys
            configMenu.AddSectionTitle(this._manifest,
                () => this._helper.Translation.Get("config.section.hotkeys"));

            // Add the hotkey binding to toggle the passout logic on or off for the current day.
            configMenu.AddKeybindList(
                mod: this._manifest,
                name: () => this._helper.Translation.Get("config.toggle-key.name"),
                tooltip: () => this._helper.Translation.Get("config.toggle-key.tooltip"),
                getValue: () => this._getConfig().TogglePassOutKey,
                setValue: value => this._getConfig().TogglePassOutKey = value
            );
        }
    }
}
