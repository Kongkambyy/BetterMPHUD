using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Scoreboard;

namespace BetterMPHUD.Patches
{
    [HarmonyPatch(typeof(MissionGauntletMultiplayerScoreboard), "OnMissionTick")]
    public static class NativeScoreboardPatch
    {
        private static readonly AccessTools.FieldRef<MissionGauntletMultiplayerScoreboard, MissionScoreboardVM> DataSource =
            AccessTools.FieldRefAccess<MissionGauntletMultiplayerScoreboard, MissionScoreboardVM>("_dataSource");

        private static readonly AccessTools.FieldRef<MissionGauntletMultiplayerScoreboard, bool> IsActive =
            AccessTools.FieldRefAccess<MissionGauntletMultiplayerScoreboard, bool>("_isActive");

        private static readonly AccessTools.FieldRef<MissionGauntletMultiplayerScoreboard, bool> MouseRequested =
            AccessTools.FieldRefAccess<MissionGauntletMultiplayerScoreboard, bool>("_mouseRequstedWhileScoreboardActive");

        private static bool Prefix(MissionGauntletMultiplayerScoreboard __instance)
        {
            if (!ScoreboardPatchState.UseCustomScoreboard)
                return true;

            MissionScoreboardVM dataSource = DataSource(__instance);
            if (dataSource != null)
                dataSource.IsActive = false;

            IsActive(__instance) = false;
            MouseRequested(__instance) = false;
            return false;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo original = AccessTools.Method(typeof(InputContext), nameof(InputContext.IsGameKeyPressed), new[] { typeof(int) });
            MethodInfo replacement = AccessTools.Method(typeof(NativeScoreboardPatch), nameof(IsScoreboardGameKeyPressed));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(original))
                    yield return new CodeInstruction(OpCodes.Call, replacement);
                else
                    yield return instruction;
            }
        }

        public static bool IsScoreboardGameKeyPressed(InputContext input, int gameKey)
        {
            if (gameKey != 35)
                return input != null && input.IsGameKeyPressed(gameKey);

            return Input.IsKeyPressed(GetConfiguredShowCursorHotkey());
        }

        private static InputKey GetConfiguredShowCursorHotkey()
        {
            HudSettings settings = ConfigManager.LoadSettings();
            string keyName = settings != null ? settings.ScoreboardShowCursorHotkey : null;
            if (string.IsNullOrWhiteSpace(keyName))
                return InputKey.X1MouseButton;

            try
            {
                object key = Enum.Parse(typeof(InputKey), keyName, true);
                if (key is InputKey)
                    return (InputKey)key;
            }
            catch
            {
            }

            return InputKey.X1MouseButton;
        }
    }
}
