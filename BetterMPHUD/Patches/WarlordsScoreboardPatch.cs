using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using TaleWorlds.InputSystem;

namespace BetterMPHUD.Patches
{
    public static class WarlordsScoreboardPatch
    {
        private const string WarlordsScoreboardTypeName =
            "MBWarlords.LastConcord.GauntletUI.MissionViews.WLMissionScoreboardView";

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
            MethodInfo transpilerMethod = AccessTools.Method(typeof(WarlordsScoreboardPatch), nameof(Transpiler));
            if (tickMethod == null || prefixMethod == null || transpilerMethod == null)
                return false;

            harmony.Patch(
                tickMethod,
                prefix: new HarmonyMethod(prefixMethod),
                transpiler: new HarmonyMethod(transpilerMethod));
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

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo original = AccessTools.Method(typeof(InputContext), nameof(InputContext.IsGameKeyPressed), new[] { typeof(int) });
            MethodInfo replacement = AccessTools.Method(typeof(NativeScoreboardPatch), nameof(NativeScoreboardPatch.IsScoreboardGameKeyPressed));

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(original))
                    yield return new CodeInstruction(OpCodes.Call, replacement);
                else
                    yield return instruction;
            }
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
