using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using BetterMPHUD.Core;
using BetterMPHUD.Services;
using BetterMPHUD.ViewModels;

namespace BetterMPHUD.Handlers
{
    public class KillfeedHandler
    {
        private GauntletLayer _layer;
        private KillfeedVM _viewModel;
        private Widget _rootWidget;
        private WidgetOriginalValues _original;

        public KillfeedVM ViewModel { get { return _viewModel; } }
        public bool IsInitialized { get { return _layer != null; } }

        public void Initialize(MissionScreen screen)
        {
            _viewModel = new KillfeedVM();
            _viewModel.IsVisible = false;
            
            _layer = new GauntletLayer("GauntletLayer", 25, false);
            _layer.LoadMovie("WarbandKillfeed", _viewModel);
            screen.AddLayer(_layer);

            if (_layer.UIContext != null && _layer.UIContext.Root != null && _layer.UIContext.Root.ChildCount > 0)
                _rootWidget = _layer.UIContext.Root.GetChild(0);
        }

        public void OnAgentKilled(Agent victim, Agent killer, HudSettings settings)
        {
            if (_viewModel == null || settings == null || !settings.WarbandKillfeedEnabled) return;
            if (victim == null || killer == null || !victim.IsHuman) return;

            TaleWorlds.Library.Color color;
            TaleWorlds.MountAndBlade.Team playerTeam = Mission.Current != null ? Mission.Current.PlayerTeam : null;
            
            if (playerTeam == null)
                color = Constants.Colors.Neutral;
            else if (victim.Team == playerTeam)
                color = Constants.Colors.EnemyKill;
            else
                color = Constants.Colors.FriendlyKill;

            float currentTime = Mission.Current != null ? Mission.Current.CurrentTime : 0f;
            float expireTime = currentTime + settings.KillfeedFadeoutTime;
            float scale = settings.KillfeedCustom.Scale;
            
            int font, icon, skull, row;
            _viewModel.GetScaledSizes(scale, out font, out icon, out skull, out row);

            KillfeedItemVM item = new KillfeedItemVM(
                killer.Name, 
                victim.Name, 
                color,
                AgentHelper.GetClassSprite(killer),
                AgentHelper.GetClassSprite(victim),
                Constants.Sprites.Death,
                expireTime,
                delegate(KillfeedItemVM vm) { _viewModel.RemoveKill(vm); });

            item.UpdateSizes(font, icon, skull, row);
            _viewModel.AddKill(item);
        }

        public void UpdateExpiredEntries(float currentTime)
        {
            if (_viewModel == null || _viewModel.KillList == null || _viewModel.KillList.Count == 0) return;
            
            List<KillfeedItemVM> expired = _viewModel.KillList.Where(i => i.ExpireTime <= currentTime).ToList();
            foreach (KillfeedItemVM item in expired)
                _viewModel.RemoveKill(item);
        }

        public void ApplySettings(HudSettings settings)
        {
            if (_viewModel != null)
                _viewModel.IsVisible = settings.WarbandKillfeedEnabled;

            ApplyCustomization(settings);
        }

        private void ApplyCustomization(HudSettings settings)
        {
            if (_rootWidget == null) return;

            if (!_original.IsValid)
                _original = WidgetOriginalValues.Capture(_rootWidget);

            ElementCustomization custom = settings.KillfeedCustom;
            _rootWidget.PositionXOffset = _original.X + custom.OffsetX;
            _rootWidget.PositionYOffset = _original.Y + custom.OffsetY;
            
            if (_viewModel != null)
                _viewModel.UpdateScale(custom.Scale);
        }

        public void SetEnabled(bool enabled)
        {
            if (_viewModel == null) return;
            _viewModel.IsVisible = enabled;
            if (!enabled) _viewModel.Clear();
        }

        public void Cleanup(MissionScreen screen)
        {
            if (screen != null && _layer != null)
                screen.RemoveLayer(_layer);
            
            _layer = null;
            _viewModel = null;
            _rootWidget = null;
            _original = default(WidgetOriginalValues);
        }
    }
}