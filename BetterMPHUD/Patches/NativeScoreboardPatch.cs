using HarmonyLib;
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
    }
}
