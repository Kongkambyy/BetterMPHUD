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
using BetterMPHUD.Patches;
using BetterMPHUD.ViewModels;

namespace BetterMPHUD.Behaviors
{
    public class HudBehavior : MissionBehavior
    {
        private const float ScoreboardRoundEndForceSeconds = 4f;
        private static readonly InputKey[] ConfigMenuCaptureKeys = (InputKey[])Enum.GetValues(typeof(InputKey));

        private GauntletLayer _configLayer;
        private HudMenuVM _menuVM;
        private bool _initialized;
        private bool _isCleaningUp;
        private bool _isCustomScoreboardPinned;
        private bool _isCustomScoreboardForcePinned;
        private bool _suppressCustomScoreboardUntilReleased;
        private bool _wasScoreboardForceStateActive;
        private float _scoreboardForceUntilTime;
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

            if (_menuVM != null && (_menuVM.IsCapturingConfigMenuHotkey || _menuVM.IsCapturingScoreboardShowCursorHotkey))
            {
                if (CaptureHotkey())
                    return;
            }
            else if (IsConfigMenuHotkeyPressed())
                ToggleMenu();

            PeriodicSettingsEnforce(dt);
            UpdateKillfeed();
            UpdateHealthNumbers();

            UpdateCrosshair();
            UpdateScoreboard();

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

        private void UpdateScoreboard()
        {
            if (_menuVM == null || Mission.Current == null) return;

            MissionScreen screen = ScreenManager.TopScreen as MissionScreen;
            HudSettings settings = _menuVM.GetSettings();

            bool useCustomScoreboard = settings.ScoreboardMode == ScoreboardMode.Custom && !IsWarlordsActive();
            ScoreboardPatchState.UseCustomScoreboard = useCustomScoreboard;
            if (!useCustomScoreboard)
            {
                if (_isCustomScoreboardPinned)
                    UnpinCustomScoreboard();

                _scoreboard.UpdateCustomScoreboard(settings, Mission.Current, screen, false);
                _scoreboard.ApplyNativeScoreboardSettings(settings, Mission.Current);
                return;
            }

            bool isHotkeyDown = _scoreboard.IsNativeScoreboardHotkeyDown(screen);
            bool isHotkeyPressed = _scoreboard.IsNativeScoreboardHotkeyPressed(screen);
            bool forceScoreboardOpen = IsScoreboardForcedOpen();

            if (!forceScoreboardOpen && _isCustomScoreboardForcePinned)
            {
                _isCustomScoreboardForcePinned = false;
                UnpinCustomScoreboard();
                _scoreboard.UpdateCustomScoreboard(settings, Mission.Current, screen, false);
                return;
            }

            if (_suppressCustomScoreboardUntilReleased && !forceScoreboardOpen)
            {
                if (!isHotkeyDown)
                    _suppressCustomScoreboardUntilReleased = false;

                _scoreboard.UpdateCustomScoreboard(settings, Mission.Current, screen, false);
                _scoreboard.SetInteractive(false);
                return;
            }

            if (_isCustomScoreboardPinned && isHotkeyPressed && !forceScoreboardOpen)
            {
                _isCustomScoreboardForcePinned = false;
                UnpinCustomScoreboard();
                _scoreboard.UpdateCustomScoreboard(settings, Mission.Current, screen, false);
                return;
            }

            bool shouldShow = forceScoreboardOpen || isHotkeyDown || _isCustomScoreboardPinned;
            _scoreboard.UpdateCustomScoreboard(settings, Mission.Current, screen, shouldShow);

            if (forceScoreboardOpen && !_isCustomScoreboardPinned)
            {
                PinCustomScoreboard();
                _isCustomScoreboardForcePinned = true;
            }
            else if (shouldShow && !_isCustomScoreboardPinned && IsShowCursorActionPressed())
            {
                _isCustomScoreboardForcePinned = false;
                PinCustomScoreboard();
            }

            _scoreboard.SetInteractive(_isCustomScoreboardPinned);
        }

        private bool IsScoreboardForcedOpen()
        {
            if (Mission.Current == null)
                return false;

            Mission mission = Mission.Current;
            MissionLobbyComponent lobbyComponent = mission.GetMissionBehavior<MissionLobbyComponent>();
            bool isForceStateActive = lobbyComponent != null
                && lobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Ending;

            if (!isForceStateActive)
            {
                _wasScoreboardForceStateActive = false;
                _scoreboardForceUntilTime = 0f;
                return false;
            }

            if (!_wasScoreboardForceStateActive)
            {
                _wasScoreboardForceStateActive = true;
                _scoreboardForceUntilTime = mission.CurrentTime + ScoreboardRoundEndForceSeconds;
            }

            return mission.CurrentTime <= _scoreboardForceUntilTime;
        }

        private bool IsShowCursorActionPressed()
        {
            return Input.IsKeyPressed(GetScoreboardShowCursorHotkey());
        }

        private bool IsConfigMenuHotkeyPressed()
        {
            return Input.IsKeyPressed(GetConfigMenuHotkey());
        }

        private bool CaptureHotkey()
        {
            for (int i = 0; i < ConfigMenuCaptureKeys.Length; i++)
            {
                InputKey key = ConfigMenuCaptureKeys[i];
                if (IsUnsafeCapturePollKey(key))
                    continue;

                if (!Input.IsKeyPressed(key))
                    continue;

                if (key == InputKey.Escape)
                {
                    _menuVM.CancelConfigMenuHotkeyCapture();
                    _menuVM.CancelScoreboardShowCursorHotkeyCapture();
                    return true;
                }

                if (_menuVM.IsCapturingConfigMenuHotkey)
                {
                    if (IsIgnoredConfigMenuCaptureKey(key))
                        return true;

                    _menuVM.TrySetCapturedConfigMenuHotkey(key.ToString());
                }
                else if (_menuVM.IsCapturingScoreboardShowCursorHotkey)
                {
                    if (IsIgnoredScoreboardShowCursorCaptureKey(key))
                        return true;

                    _menuVM.TrySetCapturedScoreboardShowCursorHotkey(key.ToString());
                }

                return true;
            }

            return false;
        }

        private bool IsUnsafeCapturePollKey(InputKey key)
        {
            string keyName = key.ToString();
            return key == InputKey.Invalid
                || keyName == "Extended";
        }

        private bool IsIgnoredConfigMenuCaptureKey(InputKey key)
        {
            string keyName = key.ToString();
            return key == InputKey.Invalid
                || keyName == "Extended"
                || keyName.IndexOf("Mouse", StringComparison.OrdinalIgnoreCase) >= 0
                || keyName.IndexOf("Controller", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsIgnoredScoreboardShowCursorCaptureKey(InputKey key)
        {
            string keyName = key.ToString();
            if (key == InputKey.Invalid
                || keyName == "Extended"
                || keyName.IndexOf("Controller", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (key == InputKey.LeftMouseButton
                || key == InputKey.RightMouseButton
                || key == InputKey.MouseScrollUp
                || key == InputKey.MouseScrollDown)
            {
                return true;
            }

            return false;
        }

        private InputKey GetConfigMenuHotkey()
        {
            string keyName = null;
            if (_menuVM != null && _menuVM.GetSettings() != null)
                keyName = _menuVM.GetSettings().ConfigMenuHotkey;

            if (string.IsNullOrWhiteSpace(keyName))
                return InputKey.F10;

            try
            {
                object key = Enum.Parse(typeof(InputKey), keyName, true);
                if (key is InputKey)
                    return (InputKey)key;
            }
            catch
            {
            }

            return InputKey.F10;
        }

        private InputKey GetScoreboardShowCursorHotkey()
        {
            string keyName = null;
            if (_menuVM != null && _menuVM.GetSettings() != null)
                keyName = _menuVM.GetSettings().ScoreboardShowCursorHotkey;

            if (string.IsNullOrWhiteSpace(keyName))
                return InputKey.X1MouseButton;

            try
            {
                object key = Enum.Parse(typeof(InputKey), keyName, true);
                if (key is InputKey)
                    return (InputKey)key;
            }
            catch
            {
            }

            return InputKey.X1MouseButton;
        }

        private static bool IsWarlordsActive()
        {
            return Type.GetType("MBWarlords.LastConcord.WarlordsPeerComponent, MBWarlords.LastConcord") != null
                || FindType("MBWarlords.LastConcord.WarlordsPeerComponent") != null;
        }

        private static Type FindType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName, false);
                if (type != null)
                    return type;
            }

            return null;
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
                _scoreboard.OnShowCursorRequested = PinCustomScoreboard;
                _scoreboard.OnCloseRequested = CloseCustomScoreboardFromAction;

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
            _topBar.CleanupDisconnectedAvatars();
        }

        private void ApplyAllSettings()
        {
            if (_menuVM == null || Mission.Current == null)
                return;

            HudSettings settings = _menuVM.GetSettings();
            ScoreboardPatchState.UseCustomScoreboard = settings.ScoreboardMode == ScoreboardMode.Custom && !IsWarlordsActive();

            _killfeed.ApplySettings(settings);
            _topBar.Apply(settings, Mission.Current);
            _agentStatus.Apply(settings, Mission.Current);
            _topBar.ApplyBetterAvatars(settings.BetterAvatarsEnabled);
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

            UnpinCustomScoreboard();
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

        private void PinCustomScoreboard()
        {
            _suppressCustomScoreboardUntilReleased = false;
            _isCustomScoreboardPinned = true;
            if (_scoreboard != null)
                _scoreboard.SetInteractive(true);
        }

        private void CloseCustomScoreboardFromAction()
        {
            _isCustomScoreboardForcePinned = false;
            _suppressCustomScoreboardUntilReleased = true;
            UnpinCustomScoreboard();
        }

        private void UnpinCustomScoreboard()
        {
            _isCustomScoreboardPinned = false;
            _isCustomScoreboardForcePinned = false;
            if (_scoreboard != null)
                _scoreboard.SetInteractive(false);
        }

        public override void OnRemoveBehavior()
        {
            _isCleaningUp = true;
            base.OnRemoveBehavior();
    
            try
            {
                ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, _originalReportCasualtiesType);
                ScoreboardPatchState.UseCustomScoreboard = false;

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
                if (_scoreboard != null) _scoreboard.Cleanup(screen);
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
