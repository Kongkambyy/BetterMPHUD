using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;
using BetterMPHUD.Core;
using BetterMPHUD.Handlers;
using BetterMPHUD.ViewModels;

namespace BetterMPHUD.Behaviors
{
    public class HudBehavior : MissionBehavior
    {
        private GauntletLayer _configLayer;
        private HudMenuVM _menuVM;
        private bool _initialized;
        private float _settingsTimer;

        private TopBarHandler _topBar;
        private AgentStatusHandler _agentStatus;
        private KillfeedHandler _killfeed;
        private CameraSnapbackHandler _camera;
        private HealthNumbersHandler _healthNumbers;

        public HudBehavior()
        {
            _topBar = new TopBarHandler();
            _agentStatus = new AgentStatusHandler();
            _killfeed = new KillfeedHandler();
            _camera = new CameraSnapbackHandler();
            _healthNumbers = new HealthNumbersHandler();
        }

        public override MissionBehaviorType BehaviorType 
        { 
            get { return MissionBehaviorType.Other; } 
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            if (!_initialized) 
                TryInitialize();
            
            if (Input.IsKeyPressed(InputKey.F10)) 
                ToggleMenu();

            PeriodicSettingsEnforce(dt);
            UpdateKillfeed();
            UpdateHealthNumbers();
            HandleCameraSnapback();
        }

        private void PeriodicSettingsEnforce(float dt)
        {
            _settingsTimer += dt;
            if (_settingsTimer > Constants.UI.SettingsEnforceInterval)
            {
                ApplyAllSettings();
                _settingsTimer = 0f;
            }
        }

        private void UpdateKillfeed()
        {
            float currentTime = 0f;
            if (Mission.Current != null)
                currentTime = Mission.Current.CurrentTime;
            _killfeed.UpdateExpiredEntries(currentTime);
        }

        private void UpdateHealthNumbers()
        {
            if (_menuVM == null) return;
            _healthNumbers.Update(_menuVM.GetSettings());
        }

        private void HandleCameraSnapback()
        {
            if (_menuVM == null || !_menuVM.CameraSnapbackEnabled) 
                return;
            
            MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
            if (screen == null) 
                return;
            
            if (screen.SceneLayer.Input.IsGameKeyReleased(25))
                _camera.OnLookAroundReleased(screen, true);
        }

        public override void OnAgentRemoved(Agent victim, Agent killer, AgentState state, KillingBlow blow)
        {
            base.OnAgentRemoved(victim, killer, state, blow);
            
            HudSettings settings = null;
            if (_menuVM != null)
                settings = _menuVM.GetSettings();
            
            _killfeed.OnAgentKilled(victim, killer, settings);
        }

        private void TryInitialize()
        {
            try
            {
                MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
                if (screen == null) 
                    return;

                InitializeConfigMenu(screen);
                _killfeed.Initialize(screen);
                _killfeed.SetEnabled(_menuVM.WarbandKillfeedEnabled);
                _healthNumbers.Initialize(screen);

                _initialized = true;
                ApplyAllSettings();
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("UI Init Error: " + ex.Message, Colors.Red));
            }
        }

        private void InitializeConfigMenu(MissionScreen screen)
        {
            _menuVM = new HudMenuVM();
            _menuVM.OnCloseConfigMenu = CloseMenu;
            _menuVM.OnWarbandKillfeedToggled = OnKillfeedToggled;
            _menuVM.OnHudSettingsChanged = ApplyAllSettings;

            _configLayer = new GauntletLayer("GauntletLayer", 50, false);
            _configLayer.LoadMovie("HudConfig", _menuVM);
            screen.AddLayer(_configLayer);
        }

        private void OnKillfeedToggled(bool enabled)
        {
            _killfeed.SetEnabled(enabled);
        }

        private void ApplyAllSettings()
        {
            if (_menuVM == null || Mission.Current == null) 
                return;

            HudSettings settings = _menuVM.GetSettings();
            _killfeed.ApplySettings(settings);
            _topBar.Apply(settings, Mission.Current);
            _agentStatus.Apply(settings, Mission.Current);
        }

        private void ToggleMenu()
        {
            if (_menuVM == null) return;
            
            if (_menuVM.IsConfigMenuOpen) 
                CloseMenu();
            else 
                OpenMenu();
        }

        private void OpenMenu()
        {
            if (_menuVM == null || _configLayer == null) return;
            
            _menuVM.IsConfigMenuOpen = true;
            _configLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus(_configLayer);
        }

        private void CloseMenu()
        {
            if (_menuVM == null || _configLayer == null) return;
            
            _menuVM.IsConfigMenuOpen = false;
            _configLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Invalid);
            ScreenManager.TryLoseFocus(_configLayer);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 0f);

            MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
            if (screen != null && _configLayer != null)
                screen.RemoveLayer(_configLayer);
            
            _killfeed.Cleanup(screen);
            _healthNumbers.Cleanup(screen);
            _topBar.Reset();
            _agentStatus.Reset();
            _camera.Reset();
            
            _menuVM = null;
            _initialized = false;
        }
    }
}