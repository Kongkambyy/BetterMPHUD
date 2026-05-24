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
            if (_viewModel == null || settings == null || settings.KillfeedMode == KillfeedMode.Native) return;
            if (victim == null || killer == null || !victim.IsHuman) return;
            if (_viewModel.IsPreviewMode) return;

            bool isTeamkill = killer.Team == victim.Team && killer != victim;
            if (settings.KillfeedHideTeamkills && isTeamkill)
                return;

            if (settings.KillfeedOnlyShowMyKillsDeaths)
            {
                Agent mainAgent = Mission.Current != null ? Mission.Current.MainAgent : null;
                if (mainAgent == null || (killer != mainAgent && victim != mainAgent))
                    return;
            }

            TaleWorlds.Library.Color color;
            var playerTeam = Mission.Current != null ? Mission.Current.PlayerTeam : null;
            
            if (playerTeam == null)
                color = Constants.Colors.Neutral;
            else if (isTeamkill)
                color = Constants.Colors.TeamKill;
            else if (victim.Team == playerTeam)
                color = Constants.Colors.EnemyKill;
            else
                color = Constants.Colors.FriendlyKill;

            float currentTime = Mission.Current != null ? Mission.Current.CurrentTime : 0f;
            float expireTime = currentTime + settings.KillfeedFadeoutTime;
            float scale = settings.KillfeedCustom.Scale;

            _viewModel.GetScaledSizes(scale, settings.KillfeedMode, out int font, out float icon, out float skull, out float row);

            KillfeedItemVM item = new KillfeedItemVM(
                killer.Name, 
                victim.Name, 
                color,
                AgentHelper.GetClassSprite(killer),
                AgentHelper.GetClassSprite(victim),
                Constants.Sprites.Death,
                expireTime,
                delegate(KillfeedItemVM vm) { _viewModel.RemoveKill(vm); });

            item.UpdateStyle(settings.KillfeedMode);
            item.UpdateSizes(font, icon, skull, row);
            
            item.UpdateBackground(
                settings.KillfeedBackgroundEnabled, 
                settings.KillfeedBackgroundColor, 
                settings.KillfeedBackgroundOpacity);
            
            _viewModel.AddKill(item, settings.KillfeedMaxEntries);
        }

        public void UpdateExpiredEntries(float currentTime)
        {
            if (_viewModel == null || _viewModel.KillList == null || _viewModel.KillList.Count == 0) return;
            if (_viewModel.IsPreviewMode) return;
            
            for (int i = _viewModel.KillList.Count - 1; i >= 0; i--)
            {
                if (_viewModel.KillList[i].ExpireTime <= currentTime)
                    _viewModel.RemoveKill(_viewModel.KillList[i]);
            }
        }

        public void UpdatePreview(HudSettings settings)
        {
            if (_viewModel == null) return;
            
            if (!_viewModel.IsPreviewMode)
                _viewModel.ShowPreview(settings);
            else
                _viewModel.UpdatePreview(settings);
            
            ApplyCustomization(settings);
        }

        public void HidePreview(HudSettings settings)
        {
            if (_viewModel == null) return;
            _viewModel.HidePreview();
            _viewModel.IsVisible = settings.KillfeedMode != KillfeedMode.Native;
        }

        public void ApplySettings(HudSettings settings)
        {
            if (_viewModel != null)
            {
                if (!_viewModel.IsPreviewMode)
                    _viewModel.IsVisible = settings.KillfeedMode != KillfeedMode.Native;
                
                _viewModel.UpdateStyle(settings.KillfeedMode);
                _viewModel.UpdateBackgrounds(
                    settings.KillfeedBackgroundEnabled,
                    settings.KillfeedBackgroundColor,
                    settings.KillfeedBackgroundOpacity);
            }

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
                _viewModel.UpdateScale(custom.Scale, settings.KillfeedMode);
        }

        public void SetEnabled(bool enabled)
        {
            if (_viewModel == null) return;
            if (_viewModel.IsPreviewMode) return;
            
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
