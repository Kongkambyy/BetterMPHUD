using System;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
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
        private bool _isCleaningUp;
        private float _settingsTimer;
        private float _originalReportCasualtiesType;

        private TopBarHandler _topBar;
        private AgentStatusHandler _agentStatus;
        private KillfeedHandler _killfeed;
        private CameraSnapbackHandler _camera;
        private HealthNumbersHandler _healthNumbers;
        private CrosshairHandler _crosshair;
        private ScoreboardHandler _scoreboard;
        private ChatHandler _chat;

        public HudBehavior()
        {
            _originalReportCasualtiesType = ManagedOptions.GetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType);
            _topBar = new TopBarHandler();
            _agentStatus = new AgentStatusHandler();
            _killfeed = new KillfeedHandler();
            _camera = new CameraSnapbackHandler();
            _healthNumbers = new HealthNumbersHandler();
            _crosshair = new CrosshairHandler();
            _scoreboard = new ScoreboardHandler();
            _chat = new ChatHandler();
        }

        public override MissionBehaviorType BehaviorType
        {
            get { return MissionBehaviorType.Other; }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (_isCleaningUp) return;
            
            if (!_initialized)
                TryInitialize();

            if (Input.IsKeyPressed(InputKey.F10))
                ToggleMenu();

            PeriodicSettingsEnforce(dt);
            UpdateKillfeed();
            UpdateHealthNumbers();

            UpdateCrosshair();

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

        private void UpdateCrosshair()
        {
            if (_menuVM == null || Mission.Current == null) return;

            MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
            if (screen == null) return;

            _crosshair.Update(_menuVM.GetSettings(), screen);
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

                _crosshair.Initialize(screen);

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
            _menuVM.OnCleanupAvatarsRequested = OnCleanupAvatars;
            _menuVM.OnBetterAvatarsToggled = OnBetterAvatarsToggled;
            _menuVM.OnChatToggled = OnChatToggled; 
            
            _menuVM.OnHideKillfeedPreview = (settings) => _killfeed.HidePreview(settings);
            _menuVM.OnUpdateKillfeedPreview = (settings) => _killfeed.UpdatePreview(settings);
            _menuVM.OnRestoreAvatarsRequested = () => _topBar.RestoreAllAvatars();

            _configLayer = new GauntletLayer("GauntletLayer", 50, false);
            _configLayer.LoadMovie("HudConfig", _menuVM);
            screen.AddLayer(_configLayer);
        }
        
        private void OnChatToggled(bool enabled)
        {
            _chat.SetChatVisible(enabled);
        }
        
        private void OnKillfeedToggled(bool enabled)
        {
            _killfeed.SetEnabled(enabled);
        }
        
        private void OnBetterAvatarsToggled(bool enabled)
        {
            _topBar.ApplyBetterAvatars(enabled);
        }

        private void OnCleanupAvatars()
        {
            _topBar.SyncAvatarsToTeams();
        }

        private void ApplyAllSettings()
        {
            if (_menuVM == null || Mission.Current == null)
                return;

            HudSettings settings = _menuVM.GetSettings();

            _killfeed.ApplySettings(settings);
            _topBar.Apply(settings, Mission.Current);
            _agentStatus.Apply(settings, Mission.Current);
            _topBar.ApplyBetterAvatars(settings.BetterAvatarsEnabled);
            _scoreboard.Apply(settings, Mission.Current);
            _chat.Apply(settings, Mission.Current);


            MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
            if (screen != null)
            {
                _crosshair.Update(settings, screen);
            }
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
            _isCleaningUp = true;
            base.OnRemoveBehavior();
    
            try
            {
                ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, _originalReportCasualtiesType);

                MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
                
                if (_crosshair != null) _crosshair.Cleanup(screen);
        
                if (screen != null && _configLayer != null)
                    screen.RemoveLayer(_configLayer);
        
                if (_killfeed != null) _killfeed.Cleanup(screen);
                if (_healthNumbers != null) _healthNumbers.Cleanup(screen);
                if (_topBar != null) _topBar.Reset();
                if (_agentStatus != null) _agentStatus.Reset();
                if (_camera != null) _camera.Reset();
                if (_crosshair != null) _crosshair.Cleanup(screen);
                if (_scoreboard != null) _scoreboard.Reset();
                if (_chat != null) _chat.Reset();
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[BetterMPHUD] Cleanup error: " + ex.Message, Colors.Red));
            }
    
            _menuVM = null;
            _configLayer = null;
            _initialized = false;
        }
    }
}