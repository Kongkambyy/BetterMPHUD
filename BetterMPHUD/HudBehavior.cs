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

        private const float KILLFEED_DURATION = 5f;
        private float _enforceSettingsTimer = 0f;
        
        // Cache for found widgets to avoid re-searching every tick
        private Widget _timeAndScoresWidget;
        private Widget _avatarsWidget;
        private Widget _enemyScoreWidget;
        private List<Widget> _bannerWidgets = new List<Widget>();
        private Widget _moraleWidget;
        private bool _widgetsCached = false;

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (!_initialized) TryInitializeUI();

            if (Input.IsKeyPressed(InputKey.F10)) ToggleMenu();

            // Periodically enforce Native HUD visibility (every 1 sec)
            _enforceSettingsTimer += dt;
            if (_enforceSettingsTimer > 1.0f)
            {
                ApplyTopBarSettings();
                _enforceSettingsTimer = 0f;
            }

            // Killfeed Cleanup logic
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

        private void TryInitializeUI()
        {
            try
            {
                var missionScreen = TaleWorlds.ScreenSystem.ScreenManager.TopScreen as TaleWorlds.MountAndBlade.View.Screens.MissionScreen;
                if (missionScreen == null) return;

                _dataSource = new HudMenuVM();
                _dataSource.OnCloseConfigMenu = CloseMenu;
                _dataSource.OnWarbandKillfeedToggled = OnWarbandKillfeedToggled;
                _dataSource.OnHudSettingsChanged = ApplyTopBarSettings;

                _configLayer = new GauntletLayer("GauntletLayer", 50, false);
                _configMovie = _configLayer.LoadMovie("HudConfig", _dataSource).Movie;
                missionScreen.AddLayer(_configLayer);

                _killfeedVM = new KillfeedVM();
                _killfeedVM.IsVisible = _dataSource.WarbandKillfeedEnabled;

                _killfeedLayer = new GauntletLayer("GauntletLayer", 3, false);
                _killfeedMovie = _killfeedLayer.LoadMovie("WarbandKillfeed", _killfeedVM).Movie;
                missionScreen.AddLayer(_killfeedLayer);

                _initialized = true;
                
                // Initial apply after a short delay to let native HUD initialize
                ApplyTopBarSettings(); 
            }
            catch (System.Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"UI Init Error: {ex.Message}", Colors.Red));
            }
        }

        private void ApplyTopBarSettings()
        {
            if (_dataSource == null || Mission.Current == null) return;

            // Find the Native HUD Behavior
            var hudBehavior = Mission.Current.MissionBehaviors
                .FirstOrDefault(mb => mb.GetType().Name == "MissionMultiplayerHUDExtension");

            if (hudBehavior == null) 
            {
                // Try alternative names
                hudBehavior = Mission.Current.MissionBehaviors
                    .FirstOrDefault(mb => mb.GetType().Name.Contains("HUDExtension"));
                    
                if (hudBehavior == null) return;
            }

            // Try multiple possible field names for the gauntlet layer
            GauntletLayer nativeLayer = null;
            string[] possibleFieldNames = { "_gauntletLayer", "_layer", "_hudLayer", "gauntletLayer" };
            
            foreach (var fieldName in possibleFieldNames)
            {
                var fieldInfo = hudBehavior.GetType().GetField(fieldName, 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    
                if (fieldInfo != null)
                {
                    nativeLayer = fieldInfo.GetValue(hudBehavior) as GauntletLayer;
                    if (nativeLayer != null) break;
                }
            }

            if (nativeLayer == null || nativeLayer.UIContext?.Root == null) return;

            // Cache widgets if not done yet
            if (!_widgetsCached)
            {
                CacheWidgets(nativeLayer.UIContext.Root);
                _widgetsCached = true;
                
                // Debug output
                InformationManager.DisplayMessage(new InformationMessage(
                    $"[BetterMPHUD] Cached: Time={_timeAndScoresWidget != null}, Avatars={_avatarsWidget != null}, EnemyScore={_enemyScoreWidget != null}, Banners={_bannerWidgets.Count}, Morale={_moraleWidget != null}", 
                    Colors.Green));
            }

            // Apply visibility settings
            if (_timeAndScoresWidget != null)
                _timeAndScoresWidget.IsVisible = _dataSource.ShowTimeAndScores;

            if (_avatarsWidget != null)
                _avatarsWidget.IsVisible = _dataSource.ShowAvatars;

            if (_enemyScoreWidget != null)
                _enemyScoreWidget.IsVisible = _dataSource.ShowEnemyScore;

            foreach (var banner in _bannerWidgets)
                banner.IsVisible = _dataSource.ShowBanners;

            if (_moraleWidget != null)
                _moraleWidget.IsVisible = _dataSource.ShowMorale;
        }

        private void CacheWidgets(Widget root)
        {
            _bannerWidgets.Clear();
            SearchWidgetsRecursively(root, 0, null);
        }

        private void SearchWidgetsRecursively(Widget widget, int depth, Widget parent)
        {
            // Prevent infinite recursion
            if (depth > 50) return;

            // --- 1. Time & Scores (Top center bar with timer, banners, scores) ---
            // ListPanel with HorizontalAlignment="Center", VerticalAlignment="Top", MarginTop="5"
            // It's the first ListPanel child of the root HUDExtensionBrushWidget
            if (_timeAndScoresWidget == null && 
                widget is ListPanel &&
                widget.HorizontalAlignment == HorizontalAlignment.Center &&
                widget.VerticalAlignment == VerticalAlignment.Top &&
                widget.MarginTop >= 4 && widget.MarginTop <= 6)
            {
                _timeAndScoresWidget = widget;
            }

            // --- 2. Team Avatars + Class Info ---
            // The ListPanel with SuggestedHeight="75" that contains MultiplayerTeamAvatarsSide
            if (_avatarsWidget == null && 
                widget is ListPanel && 
                widget.HeightSizePolicy == SizePolicy.Fixed && 
                widget.SuggestedHeight >= 74 && widget.SuggestedHeight <= 76)
            {
                // Verify it has the right structure (contains Left/Center/Right widgets)
                if (widget.ChildCount == 3)
                {
                    _avatarsWidget = widget;
                }
            }

            // --- 3. Enemy Score ---
            // Widget with SuggestedWidth="10" and MarginLeft="5"
            if (_enemyScoreWidget == null &&
                widget.WidthSizePolicy == SizePolicy.Fixed &&
                widget.SuggestedWidth >= 9 && widget.SuggestedWidth <= 11 &&
                widget.MarginLeft >= 4 && widget.MarginLeft <= 6)
            {
                _enemyScoreWidget = widget;
            }

            // --- 4. Banners ---
            // 50x50 widgets - check by size
            if (widget.WidthSizePolicy == SizePolicy.Fixed &&
                widget.HeightSizePolicy == SizePolicy.Fixed &&
                widget.SuggestedWidth >= 49 && widget.SuggestedWidth <= 51 &&
                widget.SuggestedHeight >= 49 && widget.SuggestedHeight <= 51)
            {
                // Check if it has a MaskedTextureWidget child (banner indicator)
                bool hasBannerChild = false;
                for (int i = 0; i < widget.ChildCount; i++)
                {
                    var child = widget.GetChild(i);
                    if (child.GetType().Name.Contains("MaskedTextureWidget"))
                    {
                        hasBannerChild = true;
                        break;
                    }
                }
                
                if (hasBannerChild && !_bannerWidgets.Contains(widget))
                {
                    _bannerWidgets.Add(widget);
                }
            }

            // --- 5. Morale ---
            // ListPanel with morale_canvas sprite or containing MoraleWidget children
            if (_moraleWidget == null && widget is ListPanel)
            {
                // Check sprite name
                bool isMoraleBySprite = false;
                if (widget.Sprite != null)
                {
                    string spriteName = widget.Sprite.Name ?? "";
                    isMoraleBySprite = spriteName.Contains("morale_canvas") || spriteName.Contains("morale");
                }

                // Alternative: check for MoraleWidget children
                bool hasMoraleChild = false;
                for (int i = 0; i < widget.ChildCount; i++)
                {
                    var child = widget.GetChild(i);
                    if (child.GetType().Name.Contains("MoraleWidget"))
                    {
                        hasMoraleChild = true;
                        break;
                    }
                }

                if (isMoraleBySprite || hasMoraleChild)
                {
                    _moraleWidget = widget;
                }
            }

            // Recurse into children
            for (int i = 0; i < widget.ChildCount; i++)
            {
                SearchWidgetsRecursively(widget.GetChild(i), depth + 1, widget);
            }
        }

        // Force re-cache when settings are applied (useful if HUD reinitializes)
        public void InvalidateWidgetCache()
        {
            _widgetsCached = false;
            _timeAndScoresWidget = null;
            _avatarsWidget = null;
            _enemyScoreWidget = null;
            _bannerWidgets.Clear();
            _moraleWidget = null;
        }

        private void OnWarbandKillfeedToggled(bool enabled)
        {
            if (_killfeedVM != null)
            {
                _killfeedVM.IsVisible = enabled;
                if (!enabled) _killfeedVM.Clear();
            }
        }

        private void ToggleMenu()
        {
            if (_dataSource == null) return;
            if (_dataSource.IsConfigMenuOpen) CloseMenu(); else OpenMenu();
        }

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
            if (missionScreen != null)
            {
                if (_configLayer != null) missionScreen.RemoveLayer(_configLayer);
                if (_killfeedLayer != null) missionScreen.RemoveLayer(_killfeedLayer);
            }
            
            _dataSource = null;
            _killfeedVM = null;
            _configMovie = null;
            _killfeedMovie = null;
            _initialized = false;
            _widgetsCached = false;
        }
    }
}