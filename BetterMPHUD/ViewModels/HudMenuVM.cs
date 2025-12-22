using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels
{
    public class HudMenuVM : ViewModel
    {
        private HudSettings _settings;
        private ElementEditorVM _topBarEditor;
        private ElementEditorVM _hpEditor;

        private bool _isConfigMenuOpen;
        private string _currentPage = "Killfeed";

        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        public Action OnHudSettingsChanged;

        public HudMenuVM()
        {
            _settings = ConfigManager.LoadSettings();
            
            _topBarEditor = new ElementEditorVM(0, _settings.TimeAndScoresCustom, "Time & Scores");
            _topBarEditor.SetOnChanged(OnSettingsChanged);

            _hpEditor = new ElementEditorVM(0, _settings.AgentHealthCustom, "Agent Health");
            _hpEditor.SetOnChanged(OnSettingsChanged);

            ApplyNativeKillfeedSetting();
        }

        private void OnSettingsChanged()
        {
            ConfigManager.SaveSettings(_settings);
            if (OnHudSettingsChanged != null)
                OnHudSettingsChanged();
        }

        private void ApplyNativeKillfeedSetting()
        {
            float value = _settings.NativeKillfeedEnabled ? 0f : 2f;
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, value);
        }

        public HudSettings GetSettings() { return _settings; }

        // Navigation
        public void ExecuteClose() 
        { 
            if (OnCloseConfigMenu != null) 
                OnCloseConfigMenu(); 
        }
        
        private void SetPage(string page) 
        { 
            _currentPage = page; 
            RefreshPageVisibility(); 
        }
        
        public void ExecuteOpenKillfeedPage() { SetPage("Killfeed"); }
        public void ExecuteOpenTopBarPage() { SetPage("TopBar"); }
        public void ExecuteOpenVisibilityPage() { SetPage("Visibility"); }
        public void ExecuteOpenCustomizePage() { SetPage("Customize"); }
        public void ExecuteOpenChatPage() { SetPage("Chat"); }
        public void ExecuteOpenLeaderboardPage() { SetPage("Leaderboard"); }
        public void ExecuteOpenHPPage() { SetPage("HP"); }
        public void ExecuteOpenHPVisibilityPage() { SetPage("HPVisibility"); }
        public void ExecuteOpenHPCustomizePage() { SetPage("HPCustomize"); }
        public void ExecuteOpenCrosshairPage() { SetPage("Crosshair"); }
        public void ExecuteOpenMiscPage() { SetPage("Misc"); }
        public void ExecuteBackToTopBar() { SetPage("TopBar"); }
        public void ExecuteBackToHP() { SetPage("HP"); }

        private void RefreshPageVisibility()
        {
            OnPropertyChangedWithValue(IsKillfeedPageOpen, "IsKillfeedPageOpen");
            OnPropertyChangedWithValue(IsTopBarPageOpen, "IsTopBarPageOpen");
            OnPropertyChangedWithValue(IsVisibilityPageOpen, "IsVisibilityPageOpen");
            OnPropertyChangedWithValue(IsCustomizePageOpen, "IsCustomizePageOpen");
            OnPropertyChangedWithValue(IsChatPageOpen, "IsChatPageOpen");
            OnPropertyChangedWithValue(IsLeaderboardPageOpen, "IsLeaderboardPageOpen");
            OnPropertyChangedWithValue(IsHPPageOpen, "IsHPPageOpen");
            OnPropertyChangedWithValue(IsHPVisibilityPageOpen, "IsHPVisibilityPageOpen");
            OnPropertyChangedWithValue(IsHPCustomizePageOpen, "IsHPCustomizePageOpen");
            OnPropertyChangedWithValue(IsCrosshairPageOpen, "IsCrosshairPageOpen");
            OnPropertyChangedWithValue(IsMiscPageOpen, "IsMiscPageOpen");
        }

        // Top Bar Element Selection - pass the actual customization object
        public void ExecuteSelectTimeAndScores() { _topBarEditor.SelectElement(0, "Time & Scores", _settings.TimeAndScoresCustom); RefreshTopBarDisplay(); }
        public void ExecuteSelectTeamAvatars() { _topBarEditor.SelectElement(1, "Team Avatars", _settings.TeamAvatarsCustom); RefreshTopBarDisplay(); }
        public void ExecuteSelectMorale() { _topBarEditor.SelectElement(2, "Morale", _settings.MoraleCustom); RefreshTopBarDisplay(); }

        // HP Element Selection - pass the actual customization object
        public void ExecuteSelectAgentHealth() { _hpEditor.SelectElement(0, "Agent Health", _settings.AgentHealthCustom); RefreshHPDisplay(); }
        public void ExecuteSelectMountHealth() { _hpEditor.SelectElement(1, "Mount Health", _settings.MountHealthCustom); RefreshHPDisplay(); }
        public void ExecuteSelectShieldHealth() { _hpEditor.SelectElement(2, "Shield Health", _settings.ShieldHealthCustom); RefreshHPDisplay(); }
        public void ExecuteSelectWeaponInfo() { _hpEditor.SelectElement(3, "Weapon Info", _settings.WeaponInfoCustom); RefreshHPDisplay(); }
        public void ExecuteSelectGoldAmount() { _hpEditor.SelectElement(4, "Gold Amount", _settings.GoldAmountCustom); RefreshHPDisplay(); }
        public void ExecuteSelectTroopCount() { _hpEditor.SelectElement(5, "Troop Count", _settings.TroopCountCustom); RefreshHPDisplay(); }
        public void ExecuteSelectDamageFeed() { _hpEditor.SelectElement(6, "Damage Feed", _settings.DamageFeedCustom); RefreshHPDisplay(); }

        private void RefreshTopBarDisplay()
        {
            OnPropertyChangedWithValue(SelectedElementIndex, "SelectedElementIndex");
            OnPropertyChangedWithValue(SelectedElementName, "SelectedElementName");
            OnPropertyChangedWithValue(CurrentOffsetXText, "CurrentOffsetXText");
            OnPropertyChangedWithValue(CurrentOffsetYText, "CurrentOffsetYText");
            OnPropertyChangedWithValue(CurrentScaleText, "CurrentScaleText");
        }

        private void RefreshHPDisplay()
        {
            OnPropertyChangedWithValue(SelectedHPElementIndex, "SelectedHPElementIndex");
            OnPropertyChangedWithValue(SelectedHPElementName, "SelectedHPElementName");
            OnPropertyChangedWithValue(CurrentHPOffsetXText, "CurrentHPOffsetXText");
            OnPropertyChangedWithValue(CurrentHPOffsetYText, "CurrentHPOffsetYText");
            OnPropertyChangedWithValue(CurrentHPScaleText, "CurrentHPScaleText");
        }

        // Top Bar Offset/Scale
        public void ExecuteIncreaseOffsetX() { _topBarEditor.AdjustOffsetX(Constants.Adjustment.PositionStep); RefreshTopBarDisplay(); }
        public void ExecuteDecreaseOffsetX() { _topBarEditor.AdjustOffsetX(-Constants.Adjustment.PositionStep); RefreshTopBarDisplay(); }
        public void ExecuteIncreaseOffsetY() { _topBarEditor.AdjustOffsetY(Constants.Adjustment.PositionStep); RefreshTopBarDisplay(); }
        public void ExecuteDecreaseOffsetY() { _topBarEditor.AdjustOffsetY(-Constants.Adjustment.PositionStep); RefreshTopBarDisplay(); }
        public void ExecuteIncreaseOffsetXLarge() { _topBarEditor.AdjustOffsetX(Constants.Adjustment.PositionStepLarge); RefreshTopBarDisplay(); }
        public void ExecuteDecreaseOffsetXLarge() { _topBarEditor.AdjustOffsetX(-Constants.Adjustment.PositionStepLarge); RefreshTopBarDisplay(); }
        public void ExecuteIncreaseOffsetYLarge() { _topBarEditor.AdjustOffsetY(Constants.Adjustment.PositionStepLarge); RefreshTopBarDisplay(); }
        public void ExecuteDecreaseOffsetYLarge() { _topBarEditor.AdjustOffsetY(-Constants.Adjustment.PositionStepLarge); RefreshTopBarDisplay(); }
        public void ExecuteIncreaseScale() { _topBarEditor.AdjustScale(Constants.Adjustment.ScaleStep); RefreshTopBarDisplay(); }
        public void ExecuteDecreaseScale() { _topBarEditor.AdjustScale(-Constants.Adjustment.ScaleStep); RefreshTopBarDisplay(); }
        public void ExecuteIncreaseScaleLarge() { _topBarEditor.AdjustScale(Constants.Adjustment.ScaleStepLarge); RefreshTopBarDisplay(); }
        public void ExecuteDecreaseScaleLarge() { _topBarEditor.AdjustScale(-Constants.Adjustment.ScaleStepLarge); RefreshTopBarDisplay(); }
        public void ExecuteResetElement() { _topBarEditor.Reset(); RefreshTopBarDisplay(); }

        public void ExecuteResetAllElements()
        {
            _settings.TimeAndScoresCustom.Reset();
            _settings.TeamAvatarsCustom.Reset();
            _settings.MoraleCustom.Reset();
            ExecuteSelectTimeAndScores();
            OnSettingsChanged();
        }

        // HP Offset/Scale
        public void ExecuteIncreaseHPOffsetX() { _hpEditor.AdjustOffsetX(Constants.Adjustment.PositionStep); RefreshHPDisplay(); }
        public void ExecuteDecreaseHPOffsetX() { _hpEditor.AdjustOffsetX(-Constants.Adjustment.PositionStep); RefreshHPDisplay(); }
        public void ExecuteIncreaseHPOffsetY() { _hpEditor.AdjustOffsetY(Constants.Adjustment.PositionStep); RefreshHPDisplay(); }
        public void ExecuteDecreaseHPOffsetY() { _hpEditor.AdjustOffsetY(-Constants.Adjustment.PositionStep); RefreshHPDisplay(); }
        public void ExecuteIncreaseHPOffsetXLarge() { _hpEditor.AdjustOffsetX(Constants.Adjustment.PositionStepLarge); RefreshHPDisplay(); }
        public void ExecuteDecreaseHPOffsetXLarge() { _hpEditor.AdjustOffsetX(-Constants.Adjustment.PositionStepLarge); RefreshHPDisplay(); }
        public void ExecuteIncreaseHPOffsetYLarge() { _hpEditor.AdjustOffsetY(Constants.Adjustment.PositionStepLarge); RefreshHPDisplay(); }
        public void ExecuteDecreaseHPOffsetYLarge() { _hpEditor.AdjustOffsetY(-Constants.Adjustment.PositionStepLarge); RefreshHPDisplay(); }
        public void ExecuteIncreaseHPScale() { _hpEditor.AdjustScale(Constants.Adjustment.ScaleStep); RefreshHPDisplay(); }
        public void ExecuteDecreaseHPScale() { _hpEditor.AdjustScale(-Constants.Adjustment.ScaleStep); RefreshHPDisplay(); }
        public void ExecuteIncreaseHPScaleLarge() { _hpEditor.AdjustScale(Constants.Adjustment.ScaleStepLarge); RefreshHPDisplay(); }
        public void ExecuteDecreaseHPScaleLarge() { _hpEditor.AdjustScale(-Constants.Adjustment.ScaleStepLarge); RefreshHPDisplay(); }
        public void ExecuteResetHPElement() { _hpEditor.Reset(); RefreshHPDisplay(); }

        public void ExecuteResetAllHPElements()
        {
            _settings.AgentHealthCustom.Reset();
            _settings.MountHealthCustom.Reset();
            _settings.ShieldHealthCustom.Reset();
            _settings.WeaponInfoCustom.Reset();
            _settings.GoldAmountCustom.Reset();
            _settings.TroopCountCustom.Reset();
            _settings.DamageFeedCustom.Reset();
            ExecuteSelectAgentHealth();
            OnSettingsChanged();
        }

        // Killfeed Controls
        private void AdjustFadeout(float delta)
        {
            float newTime = _settings.KillfeedFadeoutTime + delta;
            if (newTime >= Constants.Adjustment.MinFadeout && newTime <= Constants.Adjustment.MaxFadeout)
            {
                _settings.KillfeedFadeoutTime = newTime;
                OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText");
                OnSettingsChanged();
            }
        }

        public void ExecuteIncreaseFadeoutTime() { AdjustFadeout(Constants.Adjustment.FadeoutStep); }
        public void ExecuteDecreaseFadeoutTime() { AdjustFadeout(-Constants.Adjustment.FadeoutStep); }
        public void ExecuteIncreaseFadeoutTimeLarge() { AdjustFadeout(5f); }
        public void ExecuteDecreaseFadeoutTimeLarge() { AdjustFadeout(-5f); }

        private void AdjustKillfeedOffset(float dx, float dy)
        {
            _settings.KillfeedCustom.OffsetX += dx;
            _settings.KillfeedCustom.OffsetY += dy;
            RefreshKillfeedDisplay();
        }

        private void AdjustKillfeedScale(float delta)
        {
            float newScale = _settings.KillfeedCustom.Scale + delta;
            if (newScale >= Constants.Adjustment.MinScale && newScale <= Constants.Adjustment.MaxScale)
            {
                _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 2);
                RefreshKillfeedDisplay();
            }
        }

        public void ExecuteIncreaseKillfeedOffsetX() { AdjustKillfeedOffset(Constants.Adjustment.PositionStep, 0); }
        public void ExecuteDecreaseKillfeedOffsetX() { AdjustKillfeedOffset(-Constants.Adjustment.PositionStep, 0); }
        public void ExecuteIncreaseKillfeedOffsetY() { AdjustKillfeedOffset(0, Constants.Adjustment.PositionStep); }
        public void ExecuteDecreaseKillfeedOffsetY() { AdjustKillfeedOffset(0, -Constants.Adjustment.PositionStep); }
        public void ExecuteIncreaseKillfeedOffsetXLarge() { AdjustKillfeedOffset(Constants.Adjustment.PositionStepLarge, 0); }
        public void ExecuteDecreaseKillfeedOffsetXLarge() { AdjustKillfeedOffset(-Constants.Adjustment.PositionStepLarge, 0); }
        public void ExecuteIncreaseKillfeedOffsetYLarge() { AdjustKillfeedOffset(0, Constants.Adjustment.PositionStepLarge); }
        public void ExecuteDecreaseKillfeedOffsetYLarge() { AdjustKillfeedOffset(0, -Constants.Adjustment.PositionStepLarge); }
        public void ExecuteIncreaseKillfeedScale() { AdjustKillfeedScale(Constants.Adjustment.ScaleStep); }
        public void ExecuteDecreaseKillfeedScale() { AdjustKillfeedScale(-Constants.Adjustment.ScaleStep); }
        public void ExecuteIncreaseKillfeedScaleLarge() { AdjustKillfeedScale(Constants.Adjustment.ScaleStepLarge); }
        public void ExecuteDecreaseKillfeedScaleLarge() { AdjustKillfeedScale(-Constants.Adjustment.ScaleStepLarge); }

        public void ExecuteResetKillfeed()
        {
            _settings.KillfeedCustom.Reset();
            _settings.KillfeedFadeoutTime = Constants.Adjustment.DefaultFadeout;
            RefreshKillfeedDisplay();
            OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText");
        }

        private void RefreshKillfeedDisplay()
        {
            OnPropertyChangedWithValue(KillfeedOffsetXText, "KillfeedOffsetXText");
            OnPropertyChangedWithValue(KillfeedOffsetYText, "KillfeedOffsetYText");
            OnPropertyChangedWithValue(KillfeedScaleText, "KillfeedScaleText");
            OnSettingsChanged();
        }

        // Page Visibility Properties
        [DataSourceProperty]
        public bool IsConfigMenuOpen 
        { 
            get { return _isConfigMenuOpen; } 
            set { if (_isConfigMenuOpen != value) { _isConfigMenuOpen = value; OnPropertyChangedWithValue(value, "IsConfigMenuOpen"); } } 
        }
        
        [DataSourceProperty] public bool IsKillfeedPageOpen { get { return _currentPage == "Killfeed"; } }
        [DataSourceProperty] public bool IsTopBarPageOpen { get { return _currentPage == "TopBar"; } }
        [DataSourceProperty] public bool IsVisibilityPageOpen { get { return _currentPage == "Visibility"; } }
        [DataSourceProperty] public bool IsCustomizePageOpen { get { return _currentPage == "Customize"; } }
        [DataSourceProperty] public bool IsChatPageOpen { get { return _currentPage == "Chat"; } }
        [DataSourceProperty] public bool IsLeaderboardPageOpen { get { return _currentPage == "Leaderboard"; } }
        [DataSourceProperty] public bool IsHPPageOpen { get { return _currentPage == "HP"; } }
        [DataSourceProperty] public bool IsHPVisibilityPageOpen { get { return _currentPage == "HPVisibility"; } }
        [DataSourceProperty] public bool IsHPCustomizePageOpen { get { return _currentPage == "HPCustomize"; } }
        [DataSourceProperty] public bool IsCrosshairPageOpen { get { return _currentPage == "Crosshair"; } }
        [DataSourceProperty] public bool IsMiscPageOpen { get { return _currentPage == "Misc"; } }

        // Settings Properties
        [DataSourceProperty]
        public bool NativeKillfeedEnabled 
        { 
            get { return _settings.NativeKillfeedEnabled; } 
            set { if (_settings.NativeKillfeedEnabled != value) { _settings.NativeKillfeedEnabled = value; OnPropertyChangedWithValue(value, "NativeKillfeedEnabled"); ApplyNativeKillfeedSetting(); OnSettingsChanged(); } } 
        }
        
        [DataSourceProperty]
        public bool WarbandKillfeedEnabled 
        { 
            get { return _settings.WarbandKillfeedEnabled; } 
            set { if (_settings.WarbandKillfeedEnabled != value) { _settings.WarbandKillfeedEnabled = value; OnPropertyChangedWithValue(value, "WarbandKillfeedEnabled"); if (OnWarbandKillfeedToggled != null) OnWarbandKillfeedToggled(value); OnSettingsChanged(); } } 
        }
        
        [DataSourceProperty]
        public bool ShowTimeAndScores { get { return _settings.ShowTimeAndScores; } set { if (_settings.ShowTimeAndScores != value) { _settings.ShowTimeAndScores = value; OnPropertyChangedWithValue(value, "ShowTimeAndScores"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowAvatars { get { return _settings.ShowAvatars; } set { if (_settings.ShowAvatars != value) { _settings.ShowAvatars = value; OnPropertyChangedWithValue(value, "ShowAvatars"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowEnemyScore { get { return _settings.ShowEnemyScore; } set { if (_settings.ShowEnemyScore != value) { _settings.ShowEnemyScore = value; OnPropertyChangedWithValue(value, "ShowEnemyScore"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowBanners { get { return _settings.ShowBanners; } set { if (_settings.ShowBanners != value) { _settings.ShowBanners = value; OnPropertyChangedWithValue(value, "ShowBanners"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowMorale { get { return _settings.ShowMorale; } set { if (_settings.ShowMorale != value) { _settings.ShowMorale = value; OnPropertyChangedWithValue(value, "ShowMorale"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowAgentHealth { get { return _settings.ShowAgentHealth; } set { if (_settings.ShowAgentHealth != value) { _settings.ShowAgentHealth = value; OnPropertyChangedWithValue(value, "ShowAgentHealth"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowMountHealth { get { return _settings.ShowMountHealth; } set { if (_settings.ShowMountHealth != value) { _settings.ShowMountHealth = value; OnPropertyChangedWithValue(value, "ShowMountHealth"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowShieldHealth { get { return _settings.ShowShieldHealth; } set { if (_settings.ShowShieldHealth != value) { _settings.ShowShieldHealth = value; OnPropertyChangedWithValue(value, "ShowShieldHealth"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowWeaponInfo { get { return _settings.ShowWeaponInfo; } set { if (_settings.ShowWeaponInfo != value) { _settings.ShowWeaponInfo = value; OnPropertyChangedWithValue(value, "ShowWeaponInfo"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowAmmoCount { get { return _settings.ShowAmmoCount; } set { if (_settings.ShowAmmoCount != value) { _settings.ShowAmmoCount = value; OnPropertyChangedWithValue(value, "ShowAmmoCount"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowGoldAmount { get { return _settings.ShowGoldAmount; } set { if (_settings.ShowGoldAmount != value) { _settings.ShowGoldAmount = value; OnPropertyChangedWithValue(value, "ShowGoldAmount"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowTroopCount { get { return _settings.ShowTroopCount; } set { if (_settings.ShowTroopCount != value) { _settings.ShowTroopCount = value; OnPropertyChangedWithValue(value, "ShowTroopCount"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowCouchLanceState { get { return _settings.ShowCouchLanceState; } set { if (_settings.ShowCouchLanceState != value) { _settings.ShowCouchLanceState = value; OnPropertyChangedWithValue(value, "ShowCouchLanceState"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowDamageFeed { get { return _settings.ShowDamageFeed; } set { if (_settings.ShowDamageFeed != value) { _settings.ShowDamageFeed = value; OnPropertyChangedWithValue(value, "ShowDamageFeed"); OnSettingsChanged(); } } }
        
        // Health Numbers Properties
        [DataSourceProperty]
        public bool ShowHealthNumbers { get { return _settings.ShowHealthNumbers; } set { if (_settings.ShowHealthNumbers != value) { _settings.ShowHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowHealthNumbers"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowMountHealthNumbers { get { return _settings.ShowMountHealthNumbers; } set { if (_settings.ShowMountHealthNumbers != value) { _settings.ShowMountHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowMountHealthNumbers"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowShieldHealthNumbers { get { return _settings.ShowShieldHealthNumbers; } set { if (_settings.ShowShieldHealthNumbers != value) { _settings.ShowShieldHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowShieldHealthNumbers"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowChat { get { return _settings.ShowChat; } set { if (_settings.ShowChat != value) { _settings.ShowChat = value; OnPropertyChangedWithValue(value, "ShowChat"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ChatAlwaysVisible { get { return _settings.ChatAlwaysVisible; } set { if (_settings.ChatAlwaysVisible != value) { _settings.ChatAlwaysVisible = value; OnPropertyChangedWithValue(value, "ChatAlwaysVisible"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool CameraSnapbackEnabled { get { return _settings.CameraSnapbackEnabled; } set { if (_settings.CameraSnapbackEnabled != value) { _settings.CameraSnapbackEnabled = value; OnPropertyChangedWithValue(value, "CameraSnapbackEnabled"); OnSettingsChanged(); } } }

        // Element Editor Delegation
        [DataSourceProperty] public int SelectedElementIndex { get { return _topBarEditor.SelectedIndex; } }
        [DataSourceProperty] public string SelectedElementName { get { return _topBarEditor.SelectedName; } }
        [DataSourceProperty] public string CurrentOffsetXText { get { return _topBarEditor.OffsetXText; } }
        [DataSourceProperty] public string CurrentOffsetYText { get { return _topBarEditor.OffsetYText; } }
        [DataSourceProperty] public string CurrentScaleText { get { return _topBarEditor.ScaleText; } }

        [DataSourceProperty] public int SelectedHPElementIndex { get { return _hpEditor.SelectedIndex; } }
        [DataSourceProperty] public string SelectedHPElementName { get { return _hpEditor.SelectedName; } }
        [DataSourceProperty] public string CurrentHPOffsetXText { get { return _hpEditor.OffsetXText; } }
        [DataSourceProperty] public string CurrentHPOffsetYText { get { return _hpEditor.OffsetYText; } }
        [DataSourceProperty] public string CurrentHPScaleText { get { return _hpEditor.ScaleText; } }

        // Killfeed Display
        [DataSourceProperty] public string KillfeedOffsetXText { get { return _settings.KillfeedCustom.OffsetX.ToString("F0"); } }
        [DataSourceProperty] public string KillfeedOffsetYText { get { return _settings.KillfeedCustom.OffsetY.ToString("F0"); } }
        [DataSourceProperty] public string KillfeedScaleText { get { return (_settings.KillfeedCustom.Scale * 100).ToString("F0") + "%"; } }
        [DataSourceProperty] public string KillfeedFadeoutTimeText { get { return _settings.KillfeedFadeoutTime.ToString("F0") + "s"; } }
    }
}