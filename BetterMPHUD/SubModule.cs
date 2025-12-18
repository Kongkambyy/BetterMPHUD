using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BetterMPHUD
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Loaded!", Colors.Cyan));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new HudBehavior());
            InformationManager.DisplayMessage(new InformationMessage("BetterMPHUD Behavior Added!", Colors.Cyan));
        }
    }
}