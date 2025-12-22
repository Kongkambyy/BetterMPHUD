using System;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using BetterMPHUD.ViewModels;

namespace BetterMPHUD.Handlers
{
    public class HealthNumbersHandler
    {
        private GauntletLayer _layer;
        private HealthNumbersVM _viewModel;

        public HealthNumbersVM ViewModel { get { return _viewModel; } }
        public bool IsInitialized { get { return _layer != null; } }

        public void Initialize(MissionScreen screen)
        {
            _viewModel = new HealthNumbersVM();
            _layer = new GauntletLayer("GauntletLayer", 48, false);
            _layer.LoadMovie("HealthNumbers", _viewModel);
            screen.AddLayer(_layer);
        }

        public void Update(HudSettings settings)
        {
            if (_viewModel == null) return;

            Agent mainAgent = null;
            if (Mission.Current != null)
                mainAgent = Mission.Current.MainAgent;

            if (mainAgent == null || !mainAgent.IsActive())
            {
                _viewModel.ClearAll();
                return;
            }

            // her opdatere vi viewmodel med de nyeste settings og health værdier, derudover viser vi synlighed og offsets for de 3 elementer
            _viewModel.UpdateVisibility(
                settings.ShowHealthNumbers && settings.ShowAgentHealth,
                settings.ShowMountHealthNumbers && settings.ShowMountHealth,
                settings.ShowShieldHealthNumbers && settings.ShowShieldHealth
            );
            
            _viewModel.UpdateOffsets(
                settings.AgentHealthCustom,
                settings.MountHealthCustom,
                settings.ShieldHealthCustom
            );
            
            _viewModel.PlayerHealth = GetHealthDisplay(mainAgent);
            _viewModel.MountHealth = GetMountHealthDisplay(mainAgent.MountAgent);
            _viewModel.ShieldHealth = GetShieldHealthDisplay(mainAgent);
        }

        private string GetHealthDisplay(Agent agent)
        {
            if (agent == null) return "";
            return Math.Round(agent.Health) + "/" + Math.Round(agent.HealthLimit);
        }

        private string GetMountHealthDisplay(Agent mountAgent)
        {
            if (mountAgent == null) return "";
            return Math.Round(mountAgent.Health) + "/" + Math.Round(mountAgent.HealthLimit);
        }

        private string GetShieldHealthDisplay(Agent agent)
        {
            if (agent == null) return "";
            
            MissionWeapon offhandWeapon = agent.WieldedOffhandWeapon;
            if (offhandWeapon.IsEmpty) return "";
            
            try
            {
                if (offhandWeapon.IsShield())
                {
                    float hitpoints = (float)offhandWeapon.HitPoints;
                    float maxHitpoints = (float)offhandWeapon.ModifiedMaxHitPoints;
                    return Math.Round(hitpoints) + "/" + Math.Round(maxHitpoints);
                }
            }
            catch
            {
                return "";
            }
            
            return "";
        }

        public void Cleanup(MissionScreen screen)
        {
            if (screen != null && _layer != null)
                screen.RemoveLayer(_layer);

            _layer = null;
            _viewModel = null;
        }
    }
}