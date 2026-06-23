using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Core;
using BeyondBedtime.Framework.Patches;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace BeyondBedtime
{
    /// <summary>
    /// The main entry point for the BeyondBedtime mod.
    /// </summary>
    public class ModEntry : Mod
    {
        private ModConfig _config = null!;
        private ModState _state = null!;
        private VirtualClockManager _virtualClockManager = null!;
        private PassoutManager _passoutManager = null!;

        /// <summary>
        /// The mod entry point, called after the mod is first loaded.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _state = new ModState();

            var lightingManager = new LightingTransitionManager(_state, _config);
            _passoutManager = new PassoutManager(_state, _config, this.Monitor, this.Helper.Translation);
            _virtualClockManager = new VirtualClockManager(_state, _config, lightingManager, _passoutManager);

            ApplyHarmonyPatches();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.DayStarted += _virtualClockManager.OnDayStarted;
            helper.Events.GameLoop.ReturnedToTitle += _virtualClockManager.OnReturnedToTitle;
            helper.Events.GameLoop.UpdateTicked += _virtualClockManager.OnUpdateTicked;
            helper.Events.GameLoop.TimeChanged += _passoutManager.OnTimeChanged;
            helper.Events.Input.ButtonsChanged += _passoutManager.OnButtonsChanged;
        }

        private void ApplyHarmonyPatches()
        {
            PassoutPatches.Initialize(this.Monitor, _config, _state);
            LightingPatches.Initialize(_config, _state);
            HudClockPatches.Initialize(_config, _state);

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateOther)),
                transpiler: new HarmonyMethod(typeof(PassoutPatches), nameof(PassoutPatches.TranspileUpdateOther))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.isTimeToTurnOffLighting)),
                prefix: new HarmonyMethod(typeof(LightingPatches), nameof(LightingPatches.Prefix_isTimeToTurnOffLighting))
            );

            harmony.Patch(
                original: AccessTools.Method(
                    typeof(DayTimeMoneyBox),
                    nameof(DayTimeMoneyBox.draw),
                    new[] { typeof(SpriteBatch) }
                ),
                prefix: new HarmonyMethod(typeof(HudClockPatches), nameof(HudClockPatches.BeforeDayTimeMoneyBoxDraw)),
                postfix: new HarmonyMethod(typeof(HudClockPatches), nameof(HudClockPatches.AfterDayTimeMoneyBoxDraw))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.getTimeOfDayString), new[] { typeof(int) }),
                postfix: new HarmonyMethod(typeof(HudClockPatches), nameof(HudClockPatches.AfterGetTimeOfDayString))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.isDarkOut)),
                prefix: new HarmonyMethod(typeof(LightingPatches), nameof(LightingPatches.BeforeIsDarkOut))
            );
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            var configIntegration = new GenericModConfigMenuIntegration(
                this.Helper,
                this.ModManifest,
                () => _config,
                () => _config = new ModConfig(),
                () => this.Helper.WriteConfig(_config)
            );

            configIntegration.Register();
        }
    }
}