using BetterMPHUD.Behaviors;
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
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Loaded", Colors.Cyan));
            UIConfig.DoNotUseGeneratedPrefabs = true;
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new HudBehavior());
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Behavior Added", Colors.Cyan));
        }

        public override async void OnInitialState()
        {
            base.OnInitialState();
            var field = NetworkMain.GameClient.GetType().GetField("_loadedUnofficialModules", BindingFlags.Instance | BindingFlags.NonPublic);

            List<ModuleInfoModel> moduleList;
            while ((moduleList = field.GetValue(NetworkMain.GameClient) as List<ModuleInfoModel>) == null)
                await Task.Delay(1);

            field.SetValue(NetworkMain.GameClient, moduleList.Where(m => m.Id != "BetterMPHUD").ToList());
            
        }
    }
}