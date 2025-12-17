using TaleWorlds.MountAndBlade;
using BetterMPHUD.Views;

namespace BetterMPHUD
{
    public class SubModule : MBSubModuleBase
    {
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            // Add our View to the mission
            mission.AddMissionBehavior(new HudMissionView());
        }
    }
}