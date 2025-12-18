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

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (!_initialized) TryInitializeUI();

            if (Input.IsKeyPressed(InputKey.F10)) ToggleMenu();

            // Periodically enforce Native HUD visibility (every 1 sec)
            // This is necessary because game logic might try to set IsVisible back to true
            _enforceSettingsTimer += dt;
            if (_enforceSettingsTimer > 1.0f)
            {
                ApplyTopBarSettings();
                _enforceSettingsTimer = 0f;
            }

            // ... Existing Killfeed Cleanup logic ...
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

        // ... OnAgentRemoved Logic (Same as before) ...

        private void TryInitializeUI()
        {
            try
            {
                var missionScreen = TaleWorlds.ScreenSystem.ScreenManager.TopScreen as TaleWorlds.MountAndBlade.View.Screens.MissionScreen;
                if (missionScreen == null) return;

                _dataSource = new HudMenuVM();
                _dataSource.OnCloseConfigMenu = CloseMenu;
                _dataSource.OnWarbandKillfeedToggled = OnWarbandKillfeedToggled;
                
                // IMPORTANT: When user clicks a toggle, run the logic immediately
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
                
                // Try applying settings once on load
                ApplyTopBarSettings(); 
            }
            catch (System.Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"UI Init Error: {ex.Message}", Colors.Red));
            }
        }

        // --- NEW LOGIC: FIND AND HIDE NATIVE WIDGETS ---
        private void ApplyTopBarSettings()
        {
            if (_dataSource == null || Mission.Current == null) return;

            // 1. Find the Native HUD Behavior using Reflection logic to avoid crashes if types change
            var hudBehavior = Mission.Current.MissionBehaviors
                .FirstOrDefault(mb => mb.GetType().Name == "MissionMultiplayerHUDExtension");

            if (hudBehavior == null) return;

            // 2. Access the private '_gauntletLayer' field
            var fieldInfo = hudBehavior.GetType().GetField("_gauntletLayer", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return;

            var nativeLayer = fieldInfo.GetValue(hudBehavior) as GauntletLayer;
            if (nativeLayer == null || nativeLayer.UIContext?.Root == null) return;

            // 3. Search the widget tree
            foreach (var widget in nativeLayer.UIContext.Root.Children)
            {
                ApplyVisibilityRecursively(widget);
            }
        }

        private void ApplyVisibilityRecursively(Widget widget)
        {
            // --- 1. Team Avatars + Class Info ---
            // Identification: It's a ListPanel with SuggestedHeight="75"
            // (Looking at your provided XML: <ListPanel ... SuggestedHeight="75">)
            if (widget is ListPanel && widget.HeightSizePolicy == SizePolicy.Fixed && widget.SuggestedHeight == 75)
            {
                widget.IsVisible = _dataSource.ShowAvatars;
            }

            // --- 2. Enemy Score ---
            // Identification: The widget containing the score has MarginLeft="5" in the XML
            // <Widget ... MarginLeft="5" ...>
            if (widget.WidthSizePolicy == SizePolicy.Fixed && widget.MarginLeft == 5)
            {
                // We check if it has a child that is a TextWidget just to be safe
                // This targets the Enemy Score specifically
                widget.IsVisible = _dataSource.ShowEnemyScore;
            }

            // --- 3. Banners ---
            // Identification: Sprite="BlankWhiteCircle"
            if (widget.Sprite != null && widget.Sprite.Name == "BlankWhiteCircle")
            {
                widget.IsVisible = _dataSource.ShowBanners;
            }

            // --- 4. Morale ---
            // Identification: Sprite="MPHud\morale_canvas"
            // Note: C# strings escape backslashes, so we check both possible path formats
            if (widget.Sprite != null && (widget.Sprite.Name == @"MPHud\morale_canvas" || widget.Sprite.Name == "MPHud\\morale_canvas"))
            {
                widget.IsVisible = _dataSource.ShowMorale;
            }

            // Recursive loop for children
            for (int i = 0; i < widget.ChildCount; i++)
            {
                ApplyVisibilityRecursively(widget.GetChild(i));
            }
        }

        // ... Rest of the standard boilerplate (CloseMenu, OnRemoveBehavior, etc) ...
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
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 0f); // Reset native setting
            
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
        }
    }
}