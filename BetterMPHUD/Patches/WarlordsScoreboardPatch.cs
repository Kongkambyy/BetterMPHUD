using System;
using System.Reflection;
using HarmonyLib;

namespace BetterMPHUD.Patches
{
    public static class WarlordsScoreboardPatch
    {
        private const string WarlordsScoreboardTypeName =
            "MBWarlords.LastConcord.GauntletUI.MissionViews.WLMissionGauntletMultiplayerScoreboard";

        private static bool _patched;

        public static bool TryPatch(Harmony harmony)
        {
            if (_patched)
                return true;

            if (harmony == null)
                return false;

            Type scoreboardType = AccessTools.TypeByName(WarlordsScoreboardTypeName);
            if (scoreboardType == null)
                return false;

            MethodInfo tickMethod = AccessTools.Method(scoreboardType, "OnMissionTick");
            MethodInfo prefixMethod = AccessTools.Method(typeof(WarlordsScoreboardPatch), nameof(Prefix));
            if (tickMethod == null || prefixMethod == null)
                return false;

            harmony.Patch(tickMethod, prefix: new HarmonyMethod(prefixMethod));
            _patched = true;
            return true;
        }

        private static bool Prefix(object __instance)
        {
            if (!ScoreboardPatchState.UseCustomScoreboard)
                return true;

            SetDataSourceInactive(__instance);
            SetField(__instance, "_isActive", false);
            SetField(__instance, "_mouseRequstedWhileScoreboardActive", false);
            SetField(__instance, "_isMouseVisible", false);
            return false;
        }

        private static void SetDataSourceInactive(object instance)
        {
            object dataSource = GetField(instance, "_dataSource");
            if (dataSource == null)
                return;

            PropertyInfo isActive = dataSource.GetType().GetProperty("IsActive", BindingFlags.Instance | BindingFlags.Public);
            if (isActive != null && isActive.CanWrite)
                isActive.SetValue(dataSource, false, null);
        }

        private static object GetField(object instance, string fieldName)
        {
            if (instance == null)
                return null;

            FieldInfo field = AccessTools.Field(instance.GetType(), fieldName);
            return field != null ? field.GetValue(instance) : null;
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            if (instance == null)
                return;

            FieldInfo field = AccessTools.Field(instance.GetType(), fieldName);
            if (field != null)
                field.SetValue(instance, value);
        }
    }
}
