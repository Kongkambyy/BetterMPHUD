using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BetterMPHUD.ViewModels
{
    public class HudMenuVM : ViewModel
    {
        private bool _isConfigMenuOpen;
        
        private bool _isKillfeedPageOpen;
        private bool _isTopBarPageOpen;
        private bool _isVisibilityPageOpen;
        private bool _isCustomizePageOpen;
        private bool _isChatPageOpen;
        private bool _isLeaderboardPageOpen;
        private bool _isHPPageOpen;
        private bool _isHPVisibilityPageOpen;
        private bool _isHPCustomizePageOpen;
        private bool _isCrosshairPageOpen;
        private bool _isMiscPageOpen;
        
        private bool _nativeKillfeedEnabled;
        private bool _warbandKillfeedEnabled;

        private bool _showTimeAndScores;
        private bool _showAvatars;
        private bool _showEnemyScore;
        private bool _showBanners;
        private bool _showMorale;

        private bool _showChat;
        private bool _chatAlwaysVisible;
        private bool _cameraSnapbackEnabled;

        // Agent Status Visibility
        private bool _showAgentHealth;
        private bool _showMountHealth;
        private bool _showShieldHealth;
        private bool _showWeaponInfo;
        private bool _showAmmoCount;
        private bool _showGoldAmount;
        private bool _showTroopCount;
        private bool _showCouchLanceState;
        private bool _showDamageFeed;

        private int _selectedElementIndex = 0;
        private string _selectedElementName = "Time & Scores";
        private float _currentOffsetX;
        private float _currentOffsetY;
        private float _currentScale;

        // HP Element Selection
        private int _selectedHPElementIndex = 0;
        private string _selectedHPElementName = "Agent Health";
        private float _currentHPOffsetX;
        private float _currentHPOffsetY;
        private float _currentHPScale;

        private HudSettings _settings;

        private const float POSITION_STEP = 10f;
        private const float POSITION_STEP_LARGE = 50f;
        private const float SCALE_STEP = 0.1f;
        private const float SCALE_STEP_LARGE = 0.25f;
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 2.0f;
        private const float FADEOUT_STEP = 1f;
        private const float MIN_FADEOUT = 1f;
        private const float MAX_FADEOUT = 30f;

        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        public Action OnHudSettingsChanged;

        public HudMenuVM()
        {
            _settings = ConfigManager.LoadSettings();
            
            _nativeKillfeedEnabled = _settings.NativeKillfeedEnabled;
            _warbandKillfeedEnabled = _settings.WarbandKillfeedEnabled;
            _showTimeAndScores = _settings.ShowTimeAndScores;
            _showAvatars = _settings.ShowAvatars;
            _showEnemyScore = _settings.ShowEnemyScore;
            _showBanners = _settings.ShowBanners;
            _showMorale = _settings.ShowMorale;
            _cameraSnapbackEnabled = _settings.CameraSnapbackEnabled;
            
            // Load Agent Status Settings
            _showAgentHealth = _settings.ShowAgentHealth;
            _showMountHealth = _settings.ShowMountHealth;
            _showShieldHealth = _settings.ShowShieldHealth;
            _showWeaponInfo = _settings.ShowWeaponInfo;
            _showAmmoCount = _settings.ShowAmmoCount;
            _showGoldAmount = _settings.ShowGoldAmount;
            _showTroopCount = _settings.ShowTroopCount;
            _showCouchLanceState = _settings.ShowCouchLanceState;
            _showDamageFeed = _settings.ShowDamageFeed;
            
            _isKillfeedPageOpen = true; 

            LoadSelectedElementValues();
            LoadSelectedHPElementValues();
            ApplyNativeKillfeedSetting();
        }

        private void CloseAllPages()
        {
            IsKillfeedPageOpen = false;
            IsTopBarPageOpen = false;
            IsVisibilityPageOpen = false;
            IsCustomizePageOpen = false;
            IsChatPageOpen = false;
            IsLeaderboardPageOpen = false;
            IsHPPageOpen = false;
            IsHPVisibilityPageOpen = false;
            IsHPCustomizePageOpen = false;
            IsCrosshairPageOpen = false;
            IsMiscPageOpen = false;
        }

        public void ExecuteClose() => OnCloseConfigMenu?.Invoke();

        public void ExecuteOpenKillfeedPage() { CloseAllPages(); IsKillfeedPageOpen = true; }
        public void ExecuteOpenTopBarPage() { CloseAllPages(); IsTopBarPageOpen = true; }
        public void ExecuteOpenVisibilityPage() { CloseAllPages(); IsVisibilityPageOpen = true; }
        public void ExecuteOpenCustomizePage() { CloseAllPages(); IsCustomizePageOpen = true; LoadSelectedElementValues(); }
        public void ExecuteOpenChatPage() { CloseAllPages(); IsChatPageOpen = true; }
        public void ExecuteOpenLeaderboardPage() { CloseAllPages(); IsLeaderboardPageOpen = true; }
        public void ExecuteOpenHPPage() { CloseAllPages(); IsHPPageOpen = true; }
        public void ExecuteOpenHPVisibilityPage() { CloseAllPages(); IsHPVisibilityPageOpen = true; }
        public void ExecuteOpenHPCustomizePage() { CloseAllPages(); IsHPCustomizePageOpen = true; LoadSelectedHPElementValues(); }
        public void ExecuteOpenCrosshairPage() { CloseAllPages(); IsCrosshairPageOpen = true; }
        public void ExecuteOpenMiscPage() { CloseAllPages(); IsMiscPageOpen = true; }
        
        public void ExecuteBackToTopBar() => ExecuteOpenTopBarPage();
        public void ExecuteBackToHP() => ExecuteOpenHPPage();

        // Top Bar Element Selection
        public void ExecuteSelectTimeAndScores() { SelectedElementIndex = 0; SelectedElementName = "Time & Scores"; LoadSelectedElementValues(); }
        public void ExecuteSelectTeamAvatars() { SelectedElementIndex = 1; SelectedElementName = "Team Avatars"; LoadSelectedElementValues(); }
        public void ExecuteSelectMorale() { SelectedElementIndex = 2; SelectedElementName = "Morale"; LoadSelectedElementValues(); }

        // HP Element Selection
        public void ExecuteSelectAgentHealth() { SelectedHPElementIndex = 0; SelectedHPElementName = "Agent Health"; LoadSelectedHPElementValues(); }
        public void ExecuteSelectMountHealth() { SelectedHPElementIndex = 1; SelectedHPElementName = "Mount Health"; LoadSelectedHPElementValues(); }
        public void ExecuteSelectShieldHealth() { SelectedHPElementIndex = 2; SelectedHPElementName = "Shield Health"; LoadSelectedHPElementValues(); }
        public void ExecuteSelectWeaponInfo() { SelectedHPElementIndex = 3; SelectedHPElementName = "Weapon Info"; LoadSelectedHPElementValues(); }
        public void ExecuteSelectGoldAmount() { SelectedHPElementIndex = 4; SelectedHPElementName = "Gold Amount"; LoadSelectedHPElementValues(); }
        public void ExecuteSelectTroopCount() { SelectedHPElementIndex = 5; SelectedHPElementName = "Troop Count"; LoadSelectedHPElementValues(); }
        public void ExecuteSelectDamageFeed() { SelectedHPElementIndex = 6; SelectedHPElementName = "Damage Feed"; LoadSelectedHPElementValues(); }

        // Top Bar Offset Controls
        public void ExecuteIncreaseOffsetX() { CurrentOffsetX += POSITION_STEP; SaveSelectedElementValues(); }
        public void ExecuteDecreaseOffsetX() { CurrentOffsetX -= POSITION_STEP; SaveSelectedElementValues(); }
        public void ExecuteIncreaseOffsetY() { CurrentOffsetY += POSITION_STEP; SaveSelectedElementValues(); }
        public void ExecuteDecreaseOffsetY() { CurrentOffsetY -= POSITION_STEP; SaveSelectedElementValues(); }
        public void ExecuteIncreaseOffsetXLarge() { CurrentOffsetX += POSITION_STEP_LARGE; SaveSelectedElementValues(); }
        public void ExecuteDecreaseOffsetXLarge() { CurrentOffsetX -= POSITION_STEP_LARGE; SaveSelectedElementValues(); }
        public void ExecuteIncreaseOffsetYLarge() { CurrentOffsetY += POSITION_STEP_LARGE; SaveSelectedElementValues(); }
        public void ExecuteDecreaseOffsetYLarge() { CurrentOffsetY -= POSITION_STEP_LARGE; SaveSelectedElementValues(); }

        public void ExecuteIncreaseScale()
        {
            float newScale = CurrentScale + SCALE_STEP;
            if (newScale <= MAX_SCALE) { CurrentScale = (float)Math.Round(newScale, 1); SaveSelectedElementValues(); }
        }

        public void ExecuteDecreaseScale()
        {
            float newScale = CurrentScale - SCALE_STEP;
            if (newScale >= MIN_SCALE) { CurrentScale = (float)Math.Round(newScale, 1); SaveSelectedElementValues(); }
        }

        public void ExecuteIncreaseScaleLarge()
        {
            float newScale = CurrentScale + SCALE_STEP_LARGE;
            if (newScale <= MAX_SCALE) { CurrentScale = (float)Math.Round(newScale, 2); SaveSelectedElementValues(); }
        }

        public void ExecuteDecreaseScaleLarge()
        {
            float newScale = CurrentScale - SCALE_STEP_LARGE;
            if (newScale >= MIN_SCALE) { CurrentScale = (float)Math.Round(newScale, 2); SaveSelectedElementValues(); }
        }

        public void ExecuteResetElement() { CurrentOffsetX = 0f; CurrentOffsetY = 0f; CurrentScale = 1f; SaveSelectedElementValues(); }

        public void ExecuteResetAllElements()
        {
            _settings.TimeAndScoresCustom.Reset();
            _settings.TeamAvatarsCustom.Reset();
            _settings.MoraleCustom.Reset();
            LoadSelectedElementValues();
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        // HP Element Offset Controls
        public void ExecuteIncreaseHPOffsetX() { CurrentHPOffsetX += POSITION_STEP; SaveSelectedHPElementValues(); }
        public void ExecuteDecreaseHPOffsetX() { CurrentHPOffsetX -= POSITION_STEP; SaveSelectedHPElementValues(); }
        public void ExecuteIncreaseHPOffsetY() { CurrentHPOffsetY += POSITION_STEP; SaveSelectedHPElementValues(); }
        public void ExecuteDecreaseHPOffsetY() { CurrentHPOffsetY -= POSITION_STEP; SaveSelectedHPElementValues(); }
        public void ExecuteIncreaseHPOffsetXLarge() { CurrentHPOffsetX += POSITION_STEP_LARGE; SaveSelectedHPElementValues(); }
        public void ExecuteDecreaseHPOffsetXLarge() { CurrentHPOffsetX -= POSITION_STEP_LARGE; SaveSelectedHPElementValues(); }
        public void ExecuteIncreaseHPOffsetYLarge() { CurrentHPOffsetY += POSITION_STEP_LARGE; SaveSelectedHPElementValues(); }
        public void ExecuteDecreaseHPOffsetYLarge() { CurrentHPOffsetY -= POSITION_STEP_LARGE; SaveSelectedHPElementValues(); }

        public void ExecuteIncreaseHPScale()
        {
            float newScale = CurrentHPScale + SCALE_STEP;
            if (newScale <= MAX_SCALE) { CurrentHPScale = (float)Math.Round(newScale, 1); SaveSelectedHPElementValues(); }
        }

        public void ExecuteDecreaseHPScale()
        {
            float newScale = CurrentHPScale - SCALE_STEP;
            if (newScale >= MIN_SCALE) { CurrentHPScale = (float)Math.Round(newScale, 1); SaveSelectedHPElementValues(); }
        }

        public void ExecuteIncreaseHPScaleLarge()
        {
            float newScale = CurrentHPScale + SCALE_STEP_LARGE;
            if (newScale <= MAX_SCALE) { CurrentHPScale = (float)Math.Round(newScale, 2); SaveSelectedHPElementValues(); }
        }

        public void ExecuteDecreaseHPScaleLarge()
        {
            float newScale = CurrentHPScale - SCALE_STEP_LARGE;
            if (newScale >= MIN_SCALE) { CurrentHPScale = (float)Math.Round(newScale, 2); SaveSelectedHPElementValues(); }
        }

        public void ExecuteResetHPElement() { CurrentHPOffsetX = 0f; CurrentHPOffsetY = 0f; CurrentHPScale = 1f; SaveSelectedHPElementValues(); }

        public void ExecuteResetAllHPElements()
        {
            _settings.AgentHealthCustom.Reset();
            _settings.MountHealthCustom.Reset();
            _settings.ShieldHealthCustom.Reset();
            _settings.WeaponInfoCustom.Reset();
            _settings.GoldAmountCustom.Reset();
            _settings.TroopCountCustom.Reset();
            _settings.DamageFeedCustom.Reset();
            LoadSelectedHPElementValues();
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        // Killfeed Controls
        public void ExecuteIncreaseFadeoutTime()
        {
            float newTime = _settings.KillfeedFadeoutTime + FADEOUT_STEP;
            if (newTime <= MAX_FADEOUT) { _settings.KillfeedFadeoutTime = newTime; OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText"); SaveCurrentSettings(); }
        }

        public void ExecuteDecreaseFadeoutTime()
        {
            float newTime = _settings.KillfeedFadeoutTime - FADEOUT_STEP;
            if (newTime >= MIN_FADEOUT) { _settings.KillfeedFadeoutTime = newTime; OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText"); SaveCurrentSettings(); }
        }

        public void ExecuteIncreaseFadeoutTimeLarge()
        {
            float newTime = _settings.KillfeedFadeoutTime + 5f;
            if (newTime <= MAX_FADEOUT) { _settings.KillfeedFadeoutTime = newTime; OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText"); SaveCurrentSettings(); }
        }

        public void ExecuteDecreaseFadeoutTimeLarge()
        {
            float newTime = _settings.KillfeedFadeoutTime - 5f;
            if (newTime >= MIN_FADEOUT) { _settings.KillfeedFadeoutTime = newTime; OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText"); SaveCurrentSettings(); }
        }

        public void ExecuteIncreaseKillfeedOffsetX() { _settings.KillfeedCustom.OffsetX += POSITION_STEP; RefreshKillfeedDisplay(); }
        public void ExecuteDecreaseKillfeedOffsetX() { _settings.KillfeedCustom.OffsetX -= POSITION_STEP; RefreshKillfeedDisplay(); }
        public void ExecuteIncreaseKillfeedOffsetY() { _settings.KillfeedCustom.OffsetY += POSITION_STEP; RefreshKillfeedDisplay(); }
        public void ExecuteDecreaseKillfeedOffsetY() { _settings.KillfeedCustom.OffsetY -= POSITION_STEP; RefreshKillfeedDisplay(); }
        public void ExecuteIncreaseKillfeedOffsetXLarge() { _settings.KillfeedCustom.OffsetX += POSITION_STEP_LARGE; RefreshKillfeedDisplay(); }
        public void ExecuteDecreaseKillfeedOffsetXLarge() { _settings.KillfeedCustom.OffsetX -= POSITION_STEP_LARGE; RefreshKillfeedDisplay(); }
        public void ExecuteIncreaseKillfeedOffsetYLarge() { _settings.KillfeedCustom.OffsetY += POSITION_STEP_LARGE; RefreshKillfeedDisplay(); }
        public void ExecuteDecreaseKillfeedOffsetYLarge() { _settings.KillfeedCustom.OffsetY -= POSITION_STEP_LARGE; RefreshKillfeedDisplay(); }

        public void ExecuteIncreaseKillfeedScale()
        {
            float newScale = _settings.KillfeedCustom.Scale + SCALE_STEP;
            if (newScale <= MAX_SCALE) { _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 1); RefreshKillfeedDisplay(); }
        }

        public void ExecuteDecreaseKillfeedScale()
        {
            float newScale = _settings.KillfeedCustom.Scale - SCALE_STEP;
            if (newScale >= MIN_SCALE) { _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 1); RefreshKillfeedDisplay(); }
        }

        public void ExecuteIncreaseKillfeedScaleLarge()
        {
            float newScale = _settings.KillfeedCustom.Scale + SCALE_STEP_LARGE;
            if (newScale <= MAX_SCALE) { _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 2); RefreshKillfeedDisplay(); }
        }

        public void ExecuteDecreaseKillfeedScaleLarge()
        {
            float newScale = _settings.KillfeedCustom.Scale - SCALE_STEP_LARGE;
            if (newScale >= MIN_SCALE) { _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 2); RefreshKillfeedDisplay(); }
        }

        public void ExecuteResetKillfeed()
        {
            _settings.KillfeedCustom.Reset();
            _settings.KillfeedFadeoutTime = 8f;
            RefreshKillfeedDisplay();
            OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText");
        }

        private void RefreshKillfeedDisplay()
        {
            OnPropertyChangedWithValue(KillfeedOffsetXText, "KillfeedOffsetXText");
            OnPropertyChangedWithValue(KillfeedOffsetYText, "KillfeedOffsetYText");
            OnPropertyChangedWithValue(KillfeedScaleText, "KillfeedScaleText");
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        private HudElement GetSelectedElement()
        {
            switch (_selectedElementIndex)
            {
                case 0: return HudElement.TimeAndScores;
                case 1: return HudElement.TeamAvatars;
                case 2: return HudElement.Morale;
                default: return HudElement.TimeAndScores;
            }
        }

        private HudElement GetSelectedHPElement()
        {
            switch (_selectedHPElementIndex)
            {
                case 0: return HudElement.AgentHealth;
                case 1: return HudElement.MountHealth;
                case 2: return HudElement.ShieldHealth;
                case 3: return HudElement.WeaponInfo;
                case 4: return HudElement.GoldAmount;
                case 5: return HudElement.TroopCount;
                case 6: return HudElement.DamageFeed;
                default: return HudElement.AgentHealth;
            }
        }

        private ElementCustomization GetCurrentElementCustomization() => _settings.GetCustomization(GetSelectedElement());
        private ElementCustomization GetCurrentHPElementCustomization() => _settings.GetCustomization(GetSelectedHPElement());

        private void LoadSelectedElementValues()
        {
            var custom = GetCurrentElementCustomization();
            _currentOffsetX = custom.OffsetX;
            _currentOffsetY = custom.OffsetY;
            _currentScale = custom.Scale;
            
            OnPropertyChangedWithValue(_currentOffsetX, "CurrentOffsetX");
            OnPropertyChangedWithValue(_currentOffsetY, "CurrentOffsetY");
            OnPropertyChangedWithValue(_currentScale, "CurrentScale");
            OnPropertyChangedWithValue(CurrentOffsetXText, "CurrentOffsetXText");
            OnPropertyChangedWithValue(CurrentOffsetYText, "CurrentOffsetYText");
            OnPropertyChangedWithValue(CurrentScaleText, "CurrentScaleText");
        }

        private void LoadSelectedHPElementValues()
        {
            var custom = GetCurrentHPElementCustomization();
            _currentHPOffsetX = custom.OffsetX;
            _currentHPOffsetY = custom.OffsetY;
            _currentHPScale = custom.Scale;
            
            OnPropertyChangedWithValue(_currentHPOffsetX, "CurrentHPOffsetX");
            OnPropertyChangedWithValue(_currentHPOffsetY, "CurrentHPOffsetY");
            OnPropertyChangedWithValue(_currentHPScale, "CurrentHPScale");
            OnPropertyChangedWithValue(CurrentHPOffsetXText, "CurrentHPOffsetXText");
            OnPropertyChangedWithValue(CurrentHPOffsetYText, "CurrentHPOffsetYText");
            OnPropertyChangedWithValue(CurrentHPScaleText, "CurrentHPScaleText");
        }

        private void SaveSelectedElementValues()
        {
            var custom = GetCurrentElementCustomization();
            custom.OffsetX = _currentOffsetX;
            custom.OffsetY = _currentOffsetY;
            custom.Scale = _currentScale;
            
            OnPropertyChangedWithValue(CurrentOffsetXText, "CurrentOffsetXText");
            OnPropertyChangedWithValue(CurrentOffsetYText, "CurrentOffsetYText");
            OnPropertyChangedWithValue(CurrentScaleText, "CurrentScaleText");
            
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        private void SaveSelectedHPElementValues()
        {
            var custom = GetCurrentHPElementCustomization();
            custom.OffsetX = _currentHPOffsetX;
            custom.OffsetY = _currentHPOffsetY;
            custom.Scale = _currentHPScale;
            
            OnPropertyChangedWithValue(CurrentHPOffsetXText, "CurrentHPOffsetXText");
            OnPropertyChangedWithValue(CurrentHPOffsetYText, "CurrentHPOffsetYText");
            OnPropertyChangedWithValue(CurrentHPScaleText, "CurrentHPScaleText");
            
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        private void ApplyNativeKillfeedSetting()
        {
            float value = _nativeKillfeedEnabled ? 0f : 2f;
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, value);
        }

        private void NotifySettingsChanged() => OnHudSettingsChanged?.Invoke();

        private void SaveCurrentSettings()
        {
            _settings.NativeKillfeedEnabled = _nativeKillfeedEnabled;
            _settings.WarbandKillfeedEnabled = _warbandKillfeedEnabled;
            _settings.ShowTimeAndScores = _showTimeAndScores;
            _settings.ShowAvatars = _showAvatars;
            _settings.ShowEnemyScore = _showEnemyScore;
            _settings.ShowBanners = _showBanners;
            _settings.ShowMorale = _showMorale;
            _settings.CameraSnapbackEnabled = _cameraSnapbackEnabled;
            
            _settings.ShowAgentHealth = _showAgentHealth;
            _settings.ShowMountHealth = _showMountHealth;
            _settings.ShowShieldHealth = _showShieldHealth;
            _settings.ShowWeaponInfo = _showWeaponInfo;
            _settings.ShowAmmoCount = _showAmmoCount;
            _settings.ShowGoldAmount = _showGoldAmount;
            _settings.ShowTroopCount = _showTroopCount;
            _settings.ShowCouchLanceState = _showCouchLanceState;
            _settings.ShowDamageFeed = _showDamageFeed;
            
            ConfigManager.SaveSettings(_settings);
        }

        public HudSettings GetSettings() => _settings;

        // Page Properties
        [DataSourceProperty] public bool IsConfigMenuOpen { get => _isConfigMenuOpen; set { if (value != _isConfigMenuOpen) { _isConfigMenuOpen = value; OnPropertyChangedWithValue(value, "IsConfigMenuOpen"); } } }
        [DataSourceProperty] public bool IsKillfeedPageOpen { get => _isKillfeedPageOpen; set { if (value != _isKillfeedPageOpen) { _isKillfeedPageOpen = value; OnPropertyChangedWithValue(value, "IsKillfeedPageOpen"); } } }
        [DataSourceProperty] public bool IsTopBarPageOpen { get => _isTopBarPageOpen; set { if (value != _isTopBarPageOpen) { _isTopBarPageOpen = value; OnPropertyChangedWithValue(value, "IsTopBarPageOpen"); } } }
        [DataSourceProperty] public bool IsVisibilityPageOpen { get => _isVisibilityPageOpen; set { if (value != _isVisibilityPageOpen) { _isVisibilityPageOpen = value; OnPropertyChangedWithValue(value, "IsVisibilityPageOpen"); } } }
        [DataSourceProperty] public bool IsCustomizePageOpen { get => _isCustomizePageOpen; set { if (value != _isCustomizePageOpen) { _isCustomizePageOpen = value; OnPropertyChangedWithValue(value, "IsCustomizePageOpen"); } } }
        [DataSourceProperty] public bool IsChatPageOpen { get => _isChatPageOpen; set { if (value != _isChatPageOpen) { _isChatPageOpen = value; OnPropertyChangedWithValue(value, "IsChatPageOpen"); } } }
        [DataSourceProperty] public bool IsLeaderboardPageOpen { get => _isLeaderboardPageOpen; set { if (value != _isLeaderboardPageOpen) { _isLeaderboardPageOpen = value; OnPropertyChangedWithValue(value, "IsLeaderboardPageOpen"); } } }
        [DataSourceProperty] public bool IsHPPageOpen { get => _isHPPageOpen; set { if (value != _isHPPageOpen) { _isHPPageOpen = value; OnPropertyChangedWithValue(value, "IsHPPageOpen"); } } }
        [DataSourceProperty] public bool IsHPVisibilityPageOpen { get => _isHPVisibilityPageOpen; set { if (value != _isHPVisibilityPageOpen) { _isHPVisibilityPageOpen = value; OnPropertyChangedWithValue(value, "IsHPVisibilityPageOpen"); } } }
        [DataSourceProperty] public bool IsHPCustomizePageOpen { get => _isHPCustomizePageOpen; set { if (value != _isHPCustomizePageOpen) { _isHPCustomizePageOpen = value; OnPropertyChangedWithValue(value, "IsHPCustomizePageOpen"); } } }
        [DataSourceProperty] public bool IsCrosshairPageOpen { get => _isCrosshairPageOpen; set { if (value != _isCrosshairPageOpen) { _isCrosshairPageOpen = value; OnPropertyChangedWithValue(value, "IsCrosshairPageOpen"); } } }
        [DataSourceProperty] public bool IsMiscPageOpen { get => _isMiscPageOpen; set { if (value != _isMiscPageOpen) { _isMiscPageOpen = value; OnPropertyChangedWithValue(value, "IsMiscPageOpen"); } } }

        // Killfeed Properties
        [DataSourceProperty] public bool NativeKillfeedEnabled { get => _nativeKillfeedEnabled; set { if (value != _nativeKillfeedEnabled) { _nativeKillfeedEnabled = value; OnPropertyChangedWithValue(value, "NativeKillfeedEnabled"); ApplyNativeKillfeedSetting(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool WarbandKillfeedEnabled { get => _warbandKillfeedEnabled; set { if (value != _warbandKillfeedEnabled) { _warbandKillfeedEnabled = value; OnPropertyChangedWithValue(value, "WarbandKillfeedEnabled"); OnWarbandKillfeedToggled?.Invoke(value); SaveCurrentSettings(); } } }
        
        // Top Bar Visibility Properties
        [DataSourceProperty] public bool ShowTimeAndScores { get => _showTimeAndScores; set { if (value != _showTimeAndScores) { _showTimeAndScores = value; OnPropertyChangedWithValue(value, "ShowTimeAndScores"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowAvatars { get => _showAvatars; set { if (value != _showAvatars) { _showAvatars = value; OnPropertyChangedWithValue(value, "ShowAvatars"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowEnemyScore { get => _showEnemyScore; set { if (value != _showEnemyScore) { _showEnemyScore = value; OnPropertyChangedWithValue(value, "ShowEnemyScore"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowBanners { get => _showBanners; set { if (value != _showBanners) { _showBanners = value; OnPropertyChangedWithValue(value, "ShowBanners"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowMorale { get => _showMorale; set { if (value != _showMorale) { _showMorale = value; OnPropertyChangedWithValue(value, "ShowMorale"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        
        // Agent Status Visibility Properties
        [DataSourceProperty] public bool ShowAgentHealth { get => _showAgentHealth; set { if (value != _showAgentHealth) { _showAgentHealth = value; OnPropertyChangedWithValue(value, "ShowAgentHealth"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowMountHealth { get => _showMountHealth; set { if (value != _showMountHealth) { _showMountHealth = value; OnPropertyChangedWithValue(value, "ShowMountHealth"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowShieldHealth { get => _showShieldHealth; set { if (value != _showShieldHealth) { _showShieldHealth = value; OnPropertyChangedWithValue(value, "ShowShieldHealth"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowWeaponInfo { get => _showWeaponInfo; set { if (value != _showWeaponInfo) { _showWeaponInfo = value; OnPropertyChangedWithValue(value, "ShowWeaponInfo"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowAmmoCount { get => _showAmmoCount; set { if (value != _showAmmoCount) { _showAmmoCount = value; OnPropertyChangedWithValue(value, "ShowAmmoCount"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowGoldAmount { get => _showGoldAmount; set { if (value != _showGoldAmount) { _showGoldAmount = value; OnPropertyChangedWithValue(value, "ShowGoldAmount"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowTroopCount { get => _showTroopCount; set { if (value != _showTroopCount) { _showTroopCount = value; OnPropertyChangedWithValue(value, "ShowTroopCount"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowCouchLanceState { get => _showCouchLanceState; set { if (value != _showCouchLanceState) { _showCouchLanceState = value; OnPropertyChangedWithValue(value, "ShowCouchLanceState"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ShowDamageFeed { get => _showDamageFeed; set { if (value != _showDamageFeed) { _showDamageFeed = value; OnPropertyChangedWithValue(value, "ShowDamageFeed"); NotifySettingsChanged(); SaveCurrentSettings(); } } }

        // Other Properties
        [DataSourceProperty] public bool ShowChat { get => _showChat; set { if (value != _showChat) { _showChat = value; OnPropertyChangedWithValue(value, "ShowChat"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool ChatAlwaysVisible { get => _chatAlwaysVisible; set { if (value != _chatAlwaysVisible) { _chatAlwaysVisible = value; OnPropertyChangedWithValue(value, "ChatAlwaysVisible"); NotifySettingsChanged(); SaveCurrentSettings(); } } }
        [DataSourceProperty] public bool CameraSnapbackEnabled { get => _cameraSnapbackEnabled; set { if (value != _cameraSnapbackEnabled) { _cameraSnapbackEnabled = value; OnPropertyChangedWithValue(value, "CameraSnapbackEnabled"); SaveCurrentSettings(); } } }
        
        // Top Bar Element Selection
        [DataSourceProperty] public int SelectedElementIndex { get => _selectedElementIndex; set { if (value != _selectedElementIndex) { _selectedElementIndex = value; OnPropertyChangedWithValue(value, "SelectedElementIndex"); } } }
        [DataSourceProperty] public string SelectedElementName { get => _selectedElementName; set { if (value != _selectedElementName) { _selectedElementName = value; OnPropertyChangedWithValue(value, "SelectedElementName"); } } }
        [DataSourceProperty] public float CurrentOffsetX { get => _currentOffsetX; set { if (value != _currentOffsetX) { _currentOffsetX = value; OnPropertyChangedWithValue(value, "CurrentOffsetX"); OnPropertyChangedWithValue(CurrentOffsetXText, "CurrentOffsetXText"); } } }
        [DataSourceProperty] public float CurrentOffsetY { get => _currentOffsetY; set { if (value != _currentOffsetY) { _currentOffsetY = value; OnPropertyChangedWithValue(value, "CurrentOffsetY"); OnPropertyChangedWithValue(CurrentOffsetYText, "CurrentOffsetYText"); } } }
        [DataSourceProperty] public float CurrentScale { get => _currentScale; set { if (value != _currentScale) { _currentScale = value; OnPropertyChangedWithValue(value, "CurrentScale"); OnPropertyChangedWithValue(CurrentScaleText, "CurrentScaleText"); } } }

        // HP Element Selection
        [DataSourceProperty] public int SelectedHPElementIndex { get => _selectedHPElementIndex; set { if (value != _selectedHPElementIndex) { _selectedHPElementIndex = value; OnPropertyChangedWithValue(value, "SelectedHPElementIndex"); } } }
        [DataSourceProperty] public string SelectedHPElementName { get => _selectedHPElementName; set { if (value != _selectedHPElementName) { _selectedHPElementName = value; OnPropertyChangedWithValue(value, "SelectedHPElementName"); } } }
        [DataSourceProperty] public float CurrentHPOffsetX { get => _currentHPOffsetX; set { if (value != _currentHPOffsetX) { _currentHPOffsetX = value; OnPropertyChangedWithValue(value, "CurrentHPOffsetX"); OnPropertyChangedWithValue(CurrentHPOffsetXText, "CurrentHPOffsetXText"); } } }
        [DataSourceProperty] public float CurrentHPOffsetY { get => _currentHPOffsetY; set { if (value != _currentHPOffsetY) { _currentHPOffsetY = value; OnPropertyChangedWithValue(value, "CurrentHPOffsetY"); OnPropertyChangedWithValue(CurrentHPOffsetYText, "CurrentHPOffsetYText"); } } }
        [DataSourceProperty] public float CurrentHPScale { get => _currentHPScale; set { if (value != _currentHPScale) { _currentHPScale = value; OnPropertyChangedWithValue(value, "CurrentHPScale"); OnPropertyChangedWithValue(CurrentHPScaleText, "CurrentHPScaleText"); } } }

        // Text Display Properties
        [DataSourceProperty] public string CurrentOffsetXText => _currentOffsetX.ToString("F0");
        [DataSourceProperty] public string CurrentOffsetYText => _currentOffsetY.ToString("F0");
        [DataSourceProperty] public string CurrentScaleText => (_currentScale * 100).ToString("F0") + "%";
        
        [DataSourceProperty] public string CurrentHPOffsetXText => _currentHPOffsetX.ToString("F0");
        [DataSourceProperty] public string CurrentHPOffsetYText => _currentHPOffsetY.ToString("F0");
        [DataSourceProperty] public string CurrentHPScaleText => (_currentHPScale * 100).ToString("F0") + "%";
        
        [DataSourceProperty] public string KillfeedOffsetXText => _settings.KillfeedCustom.OffsetX.ToString("F0");
        [DataSourceProperty] public string KillfeedOffsetYText => _settings.KillfeedCustom.OffsetY.ToString("F0");
        [DataSourceProperty] public string KillfeedScaleText => (_settings.KillfeedCustom.Scale * 100).ToString("F0") + "%";
        [DataSourceProperty] public string KillfeedFadeoutTimeText => _settings.KillfeedFadeoutTime.ToString("F0") + "s";
    }
}