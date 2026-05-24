using BetterMPHUD.Behaviors;
using BetterMPHUD.Patches;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace BetterMPHUD
{
    public class SubModule : MBSubModuleBase
    {
        private static Harmony _harmony;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            _harmony = new Harmony("better-mp-hud");
            _harmony.PatchAll(typeof(NativeScoreboardPatch).Assembly);
            WarlordsScoreboardPatch.TryPatch(_harmony);
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Loaded", Colors.Cyan));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            WarlordsScoreboardPatch.TryPatch(_harmony);
            mission.AddMissionBehavior(new HudBehavior());
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Behavior Added", Colors.Cyan));
        }

        public override async void OnInitialState()
        {
            base.OnInitialState();
            try
            {
                for (int i = 0; i < 600; i++) // ~6s ceiling, not infinite
                {
                    var client = NetworkMain.GameClient;
                    if (client == null) { await Task.Delay(10); continue; }

                    var field = client.GetType().GetField("_loadedUnofficialModules",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field == null) return;

                    if (field.GetValue(client) is List<ModuleInfoModel> moduleList)
                    {
                        field.SetValue(client, moduleList.Where(m => m.Id != "BetterMPHUD").ToList());
                        return;
                    }
                    await Task.Delay(10);
                }
            }
            catch { /* never let an async void escape */ }
        }
    }
}
