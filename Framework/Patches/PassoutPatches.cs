using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BeyondBedtime.Framework.Config;
using BeyondBedtime.Framework.Core;
using BeyondBedtime.Framework.Utils;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace BeyondBedtime.Framework.Patches
{
    /// <summary>
    /// Harmony patches to intercept and control the vanilla passout logic at 2:00 AM.
    /// </summary>
    public static class PassoutPatches
    {
        private static IMonitor _monitor = null!;
        private static ModConfig _config = null!;
        private static ModState _state = null!;

        /// <summary>
        /// Initializes the patch class with necessary dependencies.
        /// </summary>
        public static void Initialize(IMonitor monitor, ModConfig config, ModState state)
        {
            _monitor = monitor;
            _config = config;
            _state = state;
        }

        /// <summary>
        /// Rewrites the IL of Game1.UpdateOther to replace the hard-coded
        /// "if (timeOfDay >= 2600)" passout check with a custom method.
        /// </summary>
        public static IEnumerable<CodeInstruction> TranspileUpdateOther(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var fieldTimeOfDay = AccessTools.Field(typeof(Game1), nameof(Game1.timeOfDay));
            var methodCheck = AccessTools.Method(typeof(PassoutPatches), nameof(ShouldTriggerVanillaPassOut));
            bool patched = false;

            for (int i = 0; i < codes.Count - 2 && !patched; i++)
            {
                if (codes[i].opcode == OpCodes.Ldsfld
                    && Equals(codes[i].operand, fieldTimeOfDay)
                    && codes[i + 1].LoadsConstant(2600)
                    && (codes[i + 2].opcode == OpCodes.Bge
                        || codes[i + 2].opcode == OpCodes.Bge_S
                        || codes[i + 2].opcode == OpCodes.Bge_Un
                        || codes[i + 2].opcode == OpCodes.Bge_Un_S))
                {
                    var branchTarget = codes[i + 2].operand;

                    codes[i] = new CodeInstruction(OpCodes.Call, methodCheck).MoveLabelsFrom(codes[i]);
                    codes[i + 1] = new CodeInstruction(OpCodes.Brtrue, branchTarget);
                    codes[i + 2] = new CodeInstruction(OpCodes.Nop);
                    patched = true;

                    _monitor?.Log("[PassingOutControl] Transpiler: timeOfDay>=2600 branch patched.", LogLevel.Debug);
                }
            }

            if (!patched)
                _monitor?.Log("[PassingOutControl] Transpiler: WARNING - pattern not found!", LogLevel.Warn);

            return codes;
        }

        /// <summary>
        /// Called by the patched IL in place of the vanilla timeOfDay >= 2600 check.
        /// Returns true if the vanilla passout branch should run.
        /// </summary>
        public static bool ShouldTriggerVanillaPassOut()
        {
            if (_config is null || _state is null || !_config.ModEnabled || !Context.IsWorldReady || Game1.player is null)
                return Game1.timeOfDay >= 2600;

            if (_state.PassOutDisabledToday)
                return false;

            if (_state.AllowForcedPassOut)
                return Game1.timeOfDay >= 2600;

            int configuredPassOut = TimeUtils.ValidTimes[Math.Clamp(_config.PassOutTimeIndex, 0, TimeUtils.ValidTimes.Length - 1)];

            if (configuredPassOut <= 2600)
                return Game1.timeOfDay >= 2600;

            if (Game1.timeOfDay >= 2600)
                return false;

            return false;
        }
    }
}
