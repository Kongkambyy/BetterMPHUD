using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews; 
using BetterMPHUD.ViewModels;
using TaleWorlds.GauntletUI.Data;

namespace BetterMPHUD
{
    public class HudBehavior : MissionBehavior
    {
        private GauntletLayer _configLayer;
        private GauntletLayer _killfeedLayer;
        private HudMenuVM _dataSource;
        private KillfeedVM _killfeedVM;
        private IGauntletMovie _configMovie;
        private IGauntletMovie _killfeedMovie;
        private bool _initialized = false;

        private const float KILLFEED_DURATION = 8f;
        private float _enforceSettingsTimer = 0f;
        
        private Widget _timeAndScoresWidget;
        private Widget _avatarsWidget;
        private Widget _enemyScoreWidget;
        private List<Widget> _bannerWidgets = new List<Widget>();
        private Widget _moraleWidget;
        private bool _widgetsCached = false;

        private WidgetOriginalValues _timeAndScoresOriginal;
        private WidgetOriginalValues _avatarsOriginal;
        private WidgetOriginalValues _moraleOriginal;
        private Dictionary<Widget, WidgetOriginalValues> _childOriginals = new Dictionary<Widget, WidgetOriginalValues>();

        // Killfeed colors
        private static readonly Color FriendlyKillColor = new Color(0.27f, 1f, 0.27f, 1f);   // Green - friendly killed enemy
        private static readonly Color EnemyKillColor = new Color(1f, 0.27f, 0.27f, 1f);      // Red - enemy killed friendly
        private static readonly Color NeutralColor = new Color(1f, 1f, 1f, 1f);              // White - default

        private struct WidgetOriginalValues
        {
            public float X; public float Y; public float Width; public float Height;
            public float MarginTop; public float MarginBottom; public float MarginLeft; public float MarginRight;
            public bool IsValid;
        }

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!_initialized) TryInitializeUI();
            if (Input.IsKeyPressed(InputKey.F10)) ToggleMenu();

            _enforceSettingsTimer += dt;
            if (_enforceSettingsTimer > 1.0f)
            {
                ApplyAllSettings();
                _enforceSettingsTimer = 0f;
            }

            if (_killfeedVM != null && _killfeedVM.KillList.Count > 0)
            {
                float currentTime = Mission.Current?.CurrentTime ?? 0f;
                List<KillfeedItemVM> toRemove = new List<KillfeedItemVM>();
                foreach (var item in _killfeedVM.KillList)
                {
                    if (item.ExpireTime <= currentTime) toRemove.Add(item);
                }
                foreach (var item in toRemove) _killfeedVM.RemoveKill(item);
            }
        }
        
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (_killfeedVM != null && _dataSource != null && _dataSource.WarbandKillfeedEnabled 
                && affectedAgent != null && affectorAgent != null && affectedAgent.IsHuman)
            {
                Color rowColor = NeutralColor;
                Team playerTeam = Mission.Current?.PlayerTeam;

                if (playerTeam != null)
                {
                    if (affectedAgent.Team == playerTeam)
                        rowColor = EnemyKillColor;      // Red - a friendly died (enemy killed our teammate)
                    else
                        rowColor = FriendlyKillColor;   // Green - an enemy died (our team got a kill)
                }

                float expireTime = (Mission.Current?.CurrentTime ?? 0f) + KILLFEED_DURATION;

                _killfeedVM.AddKill(new KillfeedItemVM(
                    affectorAgent.Name, 
                    affectedAgent.Name, 
                    rowColor, 
                    expireTime, 
                    (item) => _killfeedVM.RemoveKill(item)));
            }
        }

        private void TryInitializeUI()
        {
            try
            {
                var missionScreen = TaleWorlds.ScreenSystem.ScreenManager.TopScreen as TaleWorlds.MountAndBlade.View.Screens.MissionScreen;
                if (missionScreen == null) return;

                _dataSource = new HudMenuVM();
                _dataSource.OnCloseConfigMenu = CloseMenu;
                _dataSource.OnWarbandKillfeedToggled = OnWarbandKillfeedToggled;
                _dataSource.OnHudSettingsChanged = ApplyAllSettings;

                _configLayer = new GauntletLayer("GauntletLayer", 50, false);
                _configMovie = _configLayer.LoadMovie("HudConfig", _dataSource).Movie;
                missionScreen.AddLayer(_configLayer);

                _killfeedVM = new KillfeedVM();
                _killfeedVM.IsVisible = _dataSource.WarbandKillfeedEnabled;

                _killfeedLayer = new GauntletLayer("GauntletLayer", 25, false); 
                _killfeedMovie = _killfeedLayer.LoadMovie("WarbandKillfeed", _killfeedVM).Movie;
                missionScreen.AddLayer(_killfeedLayer);

                _initialized = true;
                ApplyAllSettings(); 
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"UI Init Error: {ex.Message}", Colors.Red));
            }
        }

        private void ApplyAllSettings()
        {
            if (_dataSource == null || Mission.Current == null) return;
            
            if (_killfeedVM != null)
                _killfeedVM.IsVisible = _dataSource.WarbandKillfeedEnabled;

            var hudBehavior = Mission.Current.MissionBehaviors
                .FirstOrDefault(mb => mb.GetType().Name == "MissionMultiplayerHUDExtension" || mb.GetType().Name.Contains("HUDExtension"));

            if (hudBehavior == null) return;

            GauntletLayer nativeLayer = null;
            string[] possibleFieldNames = { "_gauntletLayer", "_layer", "_hudLayer", "gauntletLayer" };
            foreach (var fieldName in possibleFieldNames)
            {
                var fieldInfo = hudBehavior.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    nativeLayer = fieldInfo.GetValue(hudBehavior) as GauntletLayer;
                    if (nativeLayer != null) break;
                }
            }

            if (nativeLayer == null || nativeLayer.UIContext?.Root == null) return;

            if (!_widgetsCached)
            {
                CacheWidgets(nativeLayer.UIContext.Root);
                _widgetsCached = true;
                StoreOriginalValues();
            }

            ApplyVisibilitySettings();
            ApplyCustomizationSettings();
        }

        private WidgetOriginalValues CaptureWidgetValues(Widget widget)
        {
            return new WidgetOriginalValues {
                X = widget.PositionXOffset, Y = widget.PositionYOffset,
                Width = widget.SuggestedWidth, Height = widget.SuggestedHeight,
                MarginTop = widget.MarginTop, MarginBottom = widget.MarginBottom,
                MarginLeft = widget.MarginLeft, MarginRight = widget.MarginRight,
                IsValid = true
            };
        }

        private void StoreOriginalValues()
        {
            _childOriginals.Clear();
            if (_timeAndScoresWidget != null) { _timeAndScoresOriginal = CaptureWidgetValues(_timeAndScoresWidget); StoreChildrenOriginals(_timeAndScoresWidget); }
            if (_avatarsWidget != null) { _avatarsOriginal = CaptureWidgetValues(_avatarsWidget); StoreChildrenOriginals(_avatarsWidget); }
            if (_moraleWidget != null) { _moraleOriginal = CaptureWidgetValues(_moraleWidget); StoreChildrenOriginals(_moraleWidget); }
        }

        private void StoreChildrenOriginals(Widget parent)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (!_childOriginals.ContainsKey(child)) _childOriginals[child] = CaptureWidgetValues(child);
                StoreChildrenOriginals(child);
            }
        }

        private void ApplyVisibilitySettings()
        {
            if (_timeAndScoresWidget != null) _timeAndScoresWidget.IsVisible = _dataSource.ShowTimeAndScores;
            if (_avatarsWidget != null) _avatarsWidget.IsVisible = _dataSource.ShowAvatars;
            if (_enemyScoreWidget != null) _enemyScoreWidget.IsVisible = _dataSource.ShowEnemyScore;
            foreach (var banner in _bannerWidgets) banner.IsVisible = _dataSource.ShowBanners;
            if (_moraleWidget != null) _moraleWidget.IsVisible = _dataSource.ShowMorale;
        }

        private void ApplyCustomizationSettings()
        {
            var settings = _dataSource.GetSettings();
            if (_timeAndScoresWidget != null && _timeAndScoresOriginal.IsValid) ApplyWidgetCustomization(_timeAndScoresWidget, _timeAndScoresOriginal, settings.TimeAndScoresCustom);
            if (_avatarsWidget != null && _avatarsOriginal.IsValid) ApplyWidgetCustomization(_avatarsWidget, _avatarsOriginal, settings.TeamAvatarsCustom);
            if (_moraleWidget != null && _moraleOriginal.IsValid) ApplyWidgetCustomization(_moraleWidget, _moraleOriginal, settings.MoraleCustom);
        }

        private void ApplyWidgetCustomization(Widget widget, WidgetOriginalValues original, ElementCustomization custom)
        {
            widget.PositionXOffset = original.X + custom.OffsetX;
            widget.PositionYOffset = original.Y + custom.OffsetY;
            if (custom.Scale != 1f)
            {
                if (widget.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0) widget.SuggestedWidth = original.Width * custom.Scale;
                if (widget.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0) widget.SuggestedHeight = original.Height * custom.Scale;
                ApplyScaleToChildren(widget, custom.Scale);
            }
            else
            {
                if (widget.WidthSizePolicy == SizePolicy.Fixed) widget.SuggestedWidth = original.Width;
                if (widget.HeightSizePolicy == SizePolicy.Fixed) widget.SuggestedHeight = original.Height;
                ResetChildrenToOriginal(widget);
            }
        }

        private void ApplyScaleToChildren(Widget parent, float scale)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (_childOriginals.TryGetValue(child, out WidgetOriginalValues original))
                {
                    if (child.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0) child.SuggestedWidth = original.Width * scale;
                    if (child.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0) child.SuggestedHeight = original.Height * scale;
                    child.MarginTop = original.MarginTop * scale; child.MarginBottom = original.MarginBottom * scale;
                    child.MarginLeft = original.MarginLeft * scale; child.MarginRight = original.MarginRight * scale;
                }
                ApplyScaleToChildren(child, scale);
            }
        }

        private void ResetChildrenToOriginal(Widget parent)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (_childOriginals.TryGetValue(child, out WidgetOriginalValues original))
                {
                    if (child.WidthSizePolicy == SizePolicy.Fixed) child.SuggestedWidth = original.Width;
                    if (child.HeightSizePolicy == SizePolicy.Fixed) child.SuggestedHeight = original.Height;
                    child.MarginTop = original.MarginTop; child.MarginBottom = original.MarginBottom;
                    child.MarginLeft = original.MarginLeft; child.MarginRight = original.MarginRight;
                }
                ResetChildrenToOriginal(child);
            }
        }

        private void CacheWidgets(Widget root)
        {
            _bannerWidgets.Clear();
            SearchWidgetsRecursively(root, 0, null);
        }

        private void SearchWidgetsRecursively(Widget widget, int depth, Widget parent)
        {
            if (depth > 50) return;

            if (_timeAndScoresWidget == null && widget is ListPanel && widget.HorizontalAlignment == HorizontalAlignment.Center && widget.VerticalAlignment == VerticalAlignment.Top && widget.MarginTop >= 4 && widget.MarginTop <= 6)
                _timeAndScoresWidget = widget;

            if (_avatarsWidget == null && widget is ListPanel && widget.HeightSizePolicy == SizePolicy.Fixed && widget.SuggestedHeight >= 74 && widget.SuggestedHeight <= 76 && widget.ChildCount == 3)
                _avatarsWidget = widget;

            if (_enemyScoreWidget == null && widget.WidthSizePolicy == SizePolicy.Fixed && widget.SuggestedWidth >= 9 && widget.SuggestedWidth <= 11 && widget.MarginLeft >= 4 && widget.MarginLeft <= 6)
                _enemyScoreWidget = widget;

            if (widget.WidthSizePolicy == SizePolicy.Fixed && widget.HeightSizePolicy == SizePolicy.Fixed && widget.SuggestedWidth >= 49 && widget.SuggestedWidth <= 51 && widget.SuggestedHeight >= 49 && widget.SuggestedHeight <= 51)
            {
                bool hasBannerChild = false;
                for (int i = 0; i < widget.ChildCount; i++) { if (widget.GetChild(i).GetType().Name.Contains("MaskedTextureWidget")) { hasBannerChild = true; break; } }
                if (hasBannerChild && !_bannerWidgets.Contains(widget)) _bannerWidgets.Add(widget);
            }

            if (_moraleWidget == null && widget is ListPanel)
            {
                bool isMoraleBySprite = widget.Sprite != null && (widget.Sprite.Name.Contains("morale_canvas") || widget.Sprite.Name.Contains("morale"));
                bool hasMoraleChild = false;
                for (int i = 0; i < widget.ChildCount; i++) { if (widget.GetChild(i).GetType().Name.Contains("MoraleWidget")) { hasMoraleChild = true; break; } }
                if (isMoraleBySprite || hasMoraleChild) _moraleWidget = widget;
            }

            for (int i = 0; i < widget.ChildCount; i++) SearchWidgetsRecursively(widget.GetChild(i), depth + 1, widget);
        }

        public void InvalidateWidgetCache()
        {
            _widgetsCached = false; _timeAndScoresWidget = null; _avatarsWidget = null;
            _enemyScoreWidget = null; _bannerWidgets.Clear(); _moraleWidget = null; _childOriginals.Clear();
        }

        private void OnWarbandKillfeedToggled(bool enabled)
        {
            if (_killfeedVM != null) { _killfeedVM.IsVisible = enabled; if (!enabled) _killfeedVM.Clear(); }
        }

        private void ToggleMenu() { if (_dataSource == null) return; if (_dataSource.IsConfigMenuOpen) CloseMenu(); else OpenMenu(); }

        private void OpenMenu()
        {
            if (_dataSource == null || _configLayer == null) return;
            _dataSource.IsConfigMenuOpen = true;
            _configLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            TaleWorlds.ScreenSystem.ScreenManager.TrySetFocus(_configLayer);
        }

        private void CloseMenu()
        {
            if (_dataSource == null || _configLayer == null) return;
            _dataSource.IsConfigMenuOpen = false;
            _configLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Invalid);
            TaleWorlds.ScreenSystem.ScreenManager.TryLoseFocus(_configLayer);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 0f);
            var missionScreen = TaleWorlds.ScreenSystem.ScreenManager.TopScreen as TaleWorlds.MountAndBlade.View.Screens.MissionScreen;
            if (missionScreen != null) { if (_configLayer != null) missionScreen.RemoveLayer(_configLayer); if (_killfeedLayer != null) missionScreen.RemoveLayer(_killfeedLayer); }
            _dataSource = null; _killfeedVM = null; _configMovie = null; _killfeedMovie = null;
            _initialized = false; _widgetsCached = false; _childOriginals.Clear();
        }
    }
}