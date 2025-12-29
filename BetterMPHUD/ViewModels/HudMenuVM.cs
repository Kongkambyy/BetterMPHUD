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
        private int _selectedProfileIndex;
        private int _newProfileCounter = 1;
        private string _tempProfileName;
        
        public Action OnDebugAvatarStructure;
        public Action<bool> OnBetterAvatarsToggled;

        private bool _isConfigMenuOpen;
        private string _currentPage = "Killfeed";
        private ElementEditorVM _avatarSideEditor;
        private int _selectedAvatarSide = 0;
        
        private MBBindingList<ColorItemVM> _crosshairColorList;
        private MBBindingList<ColorItemVM> _dotColorList;

        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        public Action OnHudSettingsChanged;
        public Action OnCleanupAvatarsRequested;
        public Action OnDebugScoreboardStructure;
        
        public void ExecuteDebugScoreboard()
        {
            if (OnDebugScoreboardStructure != null)
                OnDebugScoreboardStructure();
        }
        public HudMenuVM()
        {
            _settings = ConfigManager.LoadSettings();
    
            _topBarEditor = new ElementEditorVM(0, _settings.TimeAndScoresCustom, "Time & Scores");
            _topBarEditor.SetOnChanged(OnSettingsChanged);

            _hpEditor = new ElementEditorVM(0, _settings.AgentHealthCustom, "Agent Health");
            _hpEditor.SetOnChanged(OnSettingsChanged);
            _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
            _tempProfileName = ProfileManager.GetActiveProfile().Name;
            _avatarSideEditor = new ElementEditorVM(0, _settings.AllyAvatarsCustom, "Ally (Left)");
            _avatarSideEditor.SetOnChanged(OnSettingsChanged);

            ApplyNativeKillfeedSetting();
        }
        
        [DataSourceProperty]
        public bool AvatarSortingEnabled 
        { 
            get { return _settings.AvatarSortingEnabled; } 
            set 
            { 
                if (_settings.AvatarSortingEnabled != value) 
                { 
                    _settings.AvatarSortingEnabled = value; 
                    OnPropertyChangedWithValue(value, "AvatarSortingEnabled"); 
                    OnSettingsChanged(); 
                } 
            } 
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

        public void ExecuteClose() 
        { 
            if (OnCloseConfigMenu != null) 
                OnCloseConfigMenu(); 
        }
        
        public void ExecuteDebugAvatarStructure()
        {
            if (OnDebugAvatarStructure != null)
                OnDebugAvatarStructure();
        }
        
        private void SetPage(string page) 
        { 
            _currentPage = page; 
            RefreshPageVisibility(); 
        }
        
        private void InitializeCrosshairColorList()
        {
            _crosshairColorList = new MBBindingList<ColorItemVM>();
            _dotColorList = new MBBindingList<ColorItemVM>();

            string[] colors = new string[]
            {
                "#FF0000FF", "#00FF00FF", "#0000FFFF", "#FFFFFFFF",
                "#FFFF00FF", "#00FFFFFF", "#FF00FFFF", "#FFA500FF",
                "#FF6666FF", "#66FF66FF", "#6666FFFF", "#000000FF",
                "#888888FF", "#FF1493FF", "#8B4513FF", "#4B0082FF"
            };

            foreach (string color in colors)
            {
                _crosshairColorList.Add(new ColorItemVM(color, OnCrosshairColorSelected));
                _dotColorList.Add(new ColorItemVM(color, OnDotColorSelected));
            }

            SelectCurrentCrosshairColor();
            SelectCurrentDotColor();
        }
        
        private void AdjustDotOffsetX(int delta)
        {
            int newValue = _settings.CrosshairSettings.DotOffsetX + delta;
            if (newValue >= -200 && newValue <= 200)
            {
                _settings.CrosshairSettings.DotOffsetX = newValue;
                OnPropertyChangedWithValue(DotOffsetXText, "DotOffsetXText");
                OnSettingsChanged();
            }
        }

        private void AdjustDotOffsetY(int delta)
        {
            int newValue = _settings.CrosshairSettings.DotOffsetY + delta;
            if (newValue >= -200 && newValue <= 200)
            {
                _settings.CrosshairSettings.DotOffsetY = newValue;
                OnPropertyChangedWithValue(DotOffsetYText, "DotOffsetYText");
                OnSettingsChanged();
            }
        }
        
        public void ExecuteIncreaseDotOffsetX() { AdjustDotOffsetX(1); }
        public void ExecuteDecreaseDotOffsetX() { AdjustDotOffsetX(-1); }
        public void ExecuteIncreaseDotOffsetXLarge() { AdjustDotOffsetX(5); }
        public void ExecuteDecreaseDotOffsetXLarge() { AdjustDotOffsetX(-5); }
        public void ExecuteIncreaseDotOffsetY() { AdjustDotOffsetY(1); }
        public void ExecuteDecreaseDotOffsetY() { AdjustDotOffsetY(-1); }
        public void ExecuteIncreaseDotOffsetYLarge() { AdjustDotOffsetY(5); }
        public void ExecuteDecreaseDotOffsetYLarge() { AdjustDotOffsetY(-5); }
        
        [DataSourceProperty] 
        public string DotOffsetXText { get { return _settings.CrosshairSettings.DotOffsetX.ToString(); } }

        [DataSourceProperty] 
        public string DotOffsetYText { get { return _settings.CrosshairSettings.DotOffsetY.ToString(); } }
        
        
        
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
        public void ExecuteOpenCrosshairSettingsPage() { SetPage("CrosshairSettings"); }
        public void ExecuteOpenDotSettingsPage() { SetPage("DotSettings"); }
        public void ExecuteOpenMiscPage() { SetPage("Misc"); }
        public void ExecuteBackToTopBar() { SetPage("TopBar"); }
        public void ExecuteBackToHP() { SetPage("HP"); }
        public void ExecuteBackToCrosshair() { SetPage("Crosshair"); }
        public void ExecuteOpenProfilesPage() { SetPage("Profiles"); }
        public void ExecuteOpenAvatarSidesPage() { SetPage("AvatarSides"); }
        public void ExecuteBackToCustomize() { SetPage("Customize"); }
        

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
            OnPropertyChangedWithValue(IsCrosshairSettingsPageOpen, "IsCrosshairSettingsPageOpen");
            OnPropertyChangedWithValue(IsDotSettingsPageOpen, "IsDotSettingsPageOpen");
            OnPropertyChangedWithValue(IsMiscPageOpen, "IsMiscPageOpen");
            OnPropertyChangedWithValue(ShouldShiftMenuRight, "ShouldShiftMenuRight");
            OnPropertyChangedWithValue(MenuHorizontalOffset, "MenuHorizontalOffset");
            OnPropertyChangedWithValue(IsProfilesPageOpen, "IsProfilesPageOpen");
            OnPropertyChangedWithValue(IsAvatarSidesPageOpen, "IsAvatarSidesPageOpen");
            OnPropertyChangedWithValue(IsScoreboardPageOpen, "IsScoreboardPageOpen");
        }

        public void ExecuteSelectTimeAndScores() { _topBarEditor.SelectElement(0, "Time & Scores", _settings.TimeAndScoresCustom); RefreshTopBarDisplay(); }
        public void ExecuteSelectTeamAvatars() { _topBarEditor.SelectElement(1, "Team Avatars", _settings.TeamAvatarsCustom); RefreshTopBarDisplay(); }
        public void ExecuteSelectMorale() { _topBarEditor.SelectElement(2, "Morale", _settings.MoraleCustom); RefreshTopBarDisplay(); }

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

        private void AdjustKillfeedBackgroundOpacity(float delta)
        {
            float newValue = _settings.KillfeedBackgroundOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                _settings.KillfeedBackgroundOpacity = (float)Math.Round(newValue, 2);
                RefreshKillfeedDisplay();
                
                if (OnHudSettingsChanged != null)
                    OnHudSettingsChanged();
            }
        }

        public void ExecuteIncreaseKillfeedBackgroundOpacity() { AdjustKillfeedBackgroundOpacity(0.05f); }
        public void ExecuteDecreaseKillfeedBackgroundOpacity() { AdjustKillfeedBackgroundOpacity(-0.05f); }
        public void ExecuteIncreaseKillfeedBackgroundOpacityLarge() { AdjustKillfeedBackgroundOpacity(0.1f); }
        public void ExecuteDecreaseKillfeedBackgroundOpacityLarge() { AdjustKillfeedBackgroundOpacity(-0.1f); }

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
            OnPropertyChangedWithValue(KillfeedBackgroundOpacityText, "KillfeedBackgroundOpacityText");
            OnPropertyChangedWithValue(KillfeedBackgroundColorText, "KillfeedBackgroundColorText");
            OnPropertyChangedWithValue(KillfeedBackgroundEnabled, "KillfeedBackgroundEnabled");
            OnPropertyChangedWithValue(KillfeedMaxEntriesText, "KillfeedMaxEntriesText");  // ADD THIS LINE
            OnSettingsChanged();
        }

        
        public void ExecuteSelectAllyAvatars() 
        { 
            _selectedAvatarSide = 0;
            _avatarSideEditor.SelectElement(0, "Ally (Left)", _settings.AllyAvatarsCustom); 
            RefreshAvatarSideDisplay(); 
        }

        public void ExecuteSelectEnemyAvatars() 
        { 
            _selectedAvatarSide = 1;
            _avatarSideEditor.SelectElement(1, "Enemy (Right)", _settings.EnemyAvatarsCustom); 
            RefreshAvatarSideDisplay(); 
        }
        
        private void RefreshAvatarSideDisplay()
        {
            OnPropertyChangedWithValue(SelectedAvatarSideName, "SelectedAvatarSideName");
            OnPropertyChangedWithValue(CurrentAvatarOffsetXText, "CurrentAvatarOffsetXText");
            OnPropertyChangedWithValue(CurrentAvatarOffsetYText, "CurrentAvatarOffsetYText");
            OnPropertyChangedWithValue(CurrentAvatarScaleText, "CurrentAvatarScaleText");
            OnPropertyChangedWithValue(CurrentAvatarOrientationText, "CurrentAvatarOrientationText");
            OnPropertyChangedWithValue(CurrentAvatarSpacingText, "CurrentAvatarSpacingText");
            OnPropertyChangedWithValue(IsAllyAvatarSelected, "IsAllyAvatarSelected");
            OnPropertyChangedWithValue(IsEnemyAvatarSelected, "IsEnemyAvatarSelected");
        }
        
        // Crosshair methods
        private void AdjustCrosshairWidth(int delta)
        {
            int newValue = _settings.CrosshairSettings.SizeHorizontal + delta;
            if (newValue >= 1 && newValue <= 75)
            {
                _settings.CrosshairSettings.SizeHorizontal = newValue;
                RefreshCrosshairDisplay();
            }
        }
        
        private void AdjustAvatarSpacing(float delta)
        {
            if (_selectedAvatarSide == 0)
            {
                float newValue = _settings.AllyAvatarsSpacing + delta;
                if (newValue >= -20f && newValue <= 50f)
                    _settings.AllyAvatarsSpacing = newValue;
            }
            else
            {
                float newValue = _settings.EnemyAvatarsSpacing + delta;
                if (newValue >= -20f && newValue <= 50f)
                    _settings.EnemyAvatarsSpacing = newValue;
            }
            RefreshAvatarSideDisplay();
            OnSettingsChanged();
        }
        
        public void ExecuteIncreaseAvatarSpacing() { AdjustAvatarSpacing(1f); }
        public void ExecuteDecreaseAvatarSpacing() { AdjustAvatarSpacing(-1f); }
        public void ExecuteIncreaseAvatarSpacingLarge() { AdjustAvatarSpacing(5f); }
        public void ExecuteDecreaseAvatarSpacingLarge() { AdjustAvatarSpacing(-5f); }

        private void AdjustCrosshairLength(int delta)
        {
            int newValue = _settings.CrosshairSettings.SizeVertical + delta;
            if (newValue >= 1 && newValue <= 75)
            {
                _settings.CrosshairSettings.SizeVertical = newValue;
                RefreshCrosshairDisplay();
            }
        }

        private void AdjustCrosshairOffset(int delta)
        {
            int newValue = _settings.CrosshairSettings.Offset + delta;
            if (newValue >= 0 && newValue <= 15)
            {
                _settings.CrosshairSettings.Offset = newValue;
                RefreshCrosshairDisplay();
            }
        }

        private void AdjustCrosshairOpacity(float delta)
        {
            float newValue = _settings.CrosshairSettings.Opacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                _settings.CrosshairSettings.Opacity = (float)Math.Round(newValue, 2);
                RefreshCrosshairDisplay();
            }
        }
        
        public void ExecuteResetDot()
        {
            _settings.CrosshairSettings.DotEnabled = false;
            _settings.CrosshairSettings.DotColor = "#FFFFFFFF";
            _settings.CrosshairSettings.DotSizeWidth = 6;
            _settings.CrosshairSettings.DotSizeHeight = 6;
            _settings.CrosshairSettings.DotIsCircular = true;
            _settings.CrosshairSettings.DotOffsetX = 0;      
            _settings.CrosshairSettings.DotOffsetY = 0;      

            OnPropertyChangedWithValue(DotEnabled, "DotEnabled");
            OnPropertyChangedWithValue(DotWidthText, "DotWidthText");
            OnPropertyChangedWithValue(DotHeightText, "DotHeightText");
            OnPropertyChangedWithValue(DotColorText, "DotColorText");
            OnPropertyChangedWithValue(DotOffsetXText, "DotOffsetXText"); 
            OnPropertyChangedWithValue(DotOffsetYText, "DotOffsetYText");  

            OnSettingsChanged();
        }

        private void AdjustDotWidth(int delta)
        {
            int newValue = _settings.CrosshairSettings.DotSizeWidth + delta;
            if (newValue >= 1 && newValue <= 20)
            {
                _settings.CrosshairSettings.DotSizeWidth = newValue;
                RefreshCrosshairDisplay();
            }
        }

        private void AdjustDotHeight(int delta)
        {
            int newValue = _settings.CrosshairSettings.DotSizeHeight + delta;
            if (newValue >= 1 && newValue <= 20)
            {
                _settings.CrosshairSettings.DotSizeHeight = newValue;
                RefreshCrosshairDisplay();
            }
        }

        public void ExecuteIncreaseCrosshairWidth() { AdjustCrosshairWidth(1); }
        public void ExecuteDecreaseCrosshairWidth() { AdjustCrosshairWidth(-1); }
        public void ExecuteIncreaseCrosshairWidthLarge() { AdjustCrosshairWidth(5); }
        public void ExecuteDecreaseCrosshairWidthLarge() { AdjustCrosshairWidth(-5); }
        public void ExecuteIncreaseCrosshairLength() { AdjustCrosshairLength(1); }
        public void ExecuteDecreaseCrosshairLength() { AdjustCrosshairLength(-1); }
        public void ExecuteIncreaseCrosshairLengthLarge() { AdjustCrosshairLength(5); }
        public void ExecuteDecreaseCrosshairLengthLarge() { AdjustCrosshairLength(-5); }
        public void ExecuteIncreaseCrosshairOffset() { AdjustCrosshairOffset(1); }
        public void ExecuteDecreaseCrosshairOffset() { AdjustCrosshairOffset(-1); }
        public void ExecuteIncreaseCrosshairOffsetLarge() { AdjustCrosshairOffset(5); }
        public void ExecuteDecreaseCrosshairOffsetLarge() { AdjustCrosshairOffset(-5); }
        public void ExecuteIncreaseCrosshairOpacity() { AdjustCrosshairOpacity(0.05f); }
        public void ExecuteDecreaseCrosshairOpacity() { AdjustCrosshairOpacity(-0.05f); }
        public void ExecuteIncreaseCrosshairOpacityLarge() { AdjustCrosshairOpacity(0.1f); }
        public void ExecuteDecreaseCrosshairOpacityLarge() { AdjustCrosshairOpacity(-0.1f); }

        public void ExecuteSetCrosshairColorRed() { _settings.CrosshairSettings.Color = "#FF0000FF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorGreen() { _settings.CrosshairSettings.Color = "#00FF00FF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorBlue() { _settings.CrosshairSettings.Color = "#0000FFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorWhite() { _settings.CrosshairSettings.Color = "#FFFFFFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorYellow() { _settings.CrosshairSettings.Color = "#FFFF00FF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorCyan() { _settings.CrosshairSettings.Color = "#00FFFFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorMagenta() { _settings.CrosshairSettings.Color = "#FF00FFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetCrosshairColorOrange() { _settings.CrosshairSettings.Color = "#FFA500FF"; RefreshCrosshairDisplay(); }

        public void ExecuteIncreaseDotWidth() { AdjustDotWidth(1); }
        public void ExecuteDecreaseDotWidth() { AdjustDotWidth(-1); }
        public void ExecuteIncreaseDotWidthLarge() { AdjustDotWidth(2); }
        public void ExecuteDecreaseDotWidthLarge() { AdjustDotWidth(-2); }
        public void ExecuteIncreaseDotHeight() { AdjustDotHeight(1); }
        public void ExecuteDecreaseDotHeight() { AdjustDotHeight(-1); }
        public void ExecuteIncreaseDotHeightLarge() { AdjustDotHeight(2); }
        public void ExecuteDecreaseDotHeightLarge() { AdjustDotHeight(-2); }

        public void ExecuteSetDotColorRed() { _settings.CrosshairSettings.DotColor = "#FF0000FF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorGreen() { _settings.CrosshairSettings.DotColor = "#00FF00FF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorBlue() { _settings.CrosshairSettings.DotColor = "#0000FFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorWhite() { _settings.CrosshairSettings.DotColor = "#FFFFFFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorYellow() { _settings.CrosshairSettings.DotColor = "#FFFF00FF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorCyan() { _settings.CrosshairSettings.DotColor = "#00FFFFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorMagenta() { _settings.CrosshairSettings.DotColor = "#FF00FFFF"; RefreshCrosshairDisplay(); }
        public void ExecuteSetDotColorOrange() { _settings.CrosshairSettings.DotColor = "#FFA500FF"; RefreshCrosshairDisplay(); }

        public void ExecuteResetCrosshair()
        {
            _settings.CrosshairSettings.Reset();
            
            string resetColor = _settings.CrosshairSettings.Color;
            if (_crosshairColorList != null)
            {
                foreach (var item in _crosshairColorList)
                    item.IsSelected = string.Equals(item.ColorHex, resetColor, StringComparison.OrdinalIgnoreCase);
            }
    
            OnPropertyChangedWithValue(CrosshairWidthInt, "CrosshairWidthInt");
            OnPropertyChangedWithValue(CrosshairLengthInt, "CrosshairLengthInt");
            OnPropertyChangedWithValue(CrosshairOffsetInt, "CrosshairOffsetInt");
            OnPropertyChangedWithValue(CrosshairOpacityFloat, "CrosshairOpacityFloat");
            OnPropertyChangedWithValue(CustomCrosshairEnabled, "CustomCrosshairEnabled");
            OnPropertyChangedWithValue(DotEnabled, "DotEnabled");
            OnPropertyChangedWithValue(DotColorText, "DotColorText");
            OnPropertyChangedWithValue(DotWidthText, "DotWidthText");
            OnPropertyChangedWithValue(DotHeightText, "DotHeightText");
    
            OnSettingsChanged();
        }
        
        public void ExecuteIncreaseAvatarOffsetX() { _avatarSideEditor.AdjustOffsetX(Constants.Adjustment.PositionStep); RefreshAvatarSideDisplay(); }
        public void ExecuteDecreaseAvatarOffsetX() { _avatarSideEditor.AdjustOffsetX(-Constants.Adjustment.PositionStep); RefreshAvatarSideDisplay(); }
        public void ExecuteIncreaseAvatarOffsetY() { _avatarSideEditor.AdjustOffsetY(Constants.Adjustment.PositionStep); RefreshAvatarSideDisplay(); }
        public void ExecuteDecreaseAvatarOffsetY() { _avatarSideEditor.AdjustOffsetY(-Constants.Adjustment.PositionStep); RefreshAvatarSideDisplay(); }
        public void ExecuteIncreaseAvatarOffsetXLarge() { _avatarSideEditor.AdjustOffsetX(Constants.Adjustment.PositionStepLarge); RefreshAvatarSideDisplay(); }
        public void ExecuteDecreaseAvatarOffsetXLarge() { _avatarSideEditor.AdjustOffsetX(-Constants.Adjustment.PositionStepLarge); RefreshAvatarSideDisplay(); }
        public void ExecuteIncreaseAvatarOffsetYLarge() { _avatarSideEditor.AdjustOffsetY(Constants.Adjustment.PositionStepLarge); RefreshAvatarSideDisplay(); }
        public void ExecuteDecreaseAvatarOffsetYLarge() { _avatarSideEditor.AdjustOffsetY(-Constants.Adjustment.PositionStepLarge); RefreshAvatarSideDisplay(); }
        public void ExecuteIncreaseAvatarScale() { _avatarSideEditor.AdjustScale(Constants.Adjustment.ScaleStep); RefreshAvatarSideDisplay(); }
        public void ExecuteDecreaseAvatarScale() { _avatarSideEditor.AdjustScale(-Constants.Adjustment.ScaleStep); RefreshAvatarSideDisplay(); }
        public void ExecuteIncreaseAvatarScaleLarge() { _avatarSideEditor.AdjustScale(Constants.Adjustment.ScaleStepLarge); RefreshAvatarSideDisplay(); }
        public void ExecuteDecreaseAvatarScaleLarge() { _avatarSideEditor.AdjustScale(-Constants.Adjustment.ScaleStepLarge); RefreshAvatarSideDisplay(); }
        
        public void ExecuteToggleAvatarSideOrientation()
        {
            if (_selectedAvatarSide == 0)
                _settings.AllyAvatarsVertical = !_settings.AllyAvatarsVertical;
            else
                _settings.EnemyAvatarsVertical = !_settings.EnemyAvatarsVertical;
    
            RefreshAvatarSideDisplay();
            OnSettingsChanged();
        }

        public void ExecuteResetAvatarSide() 
        { 
            _avatarSideEditor.Reset();
            if (_selectedAvatarSide == 0)
                _settings.AllyAvatarsVertical = false;
            else
                _settings.EnemyAvatarsVertical = false;
            RefreshAvatarSideDisplay(); 
        }

        public void ExecuteResetAllAvatarSides()
        {
            _settings.AllyAvatarsCustom.Reset();
            _settings.EnemyAvatarsCustom.Reset();
            _settings.AllyAvatarsVertical = false;
            _settings.EnemyAvatarsVertical = false;
            ExecuteSelectAllyAvatars();
            OnSettingsChanged();
        }
        
        public void ExecuteSelectPowerLevel() 
        { 
            _topBarEditor.SelectElement(3, "Power Level", _settings.PowerLevelCustom); 
            RefreshTopBarDisplay(); 
        }
        
        

        private void RefreshCrosshairDisplay()
        {
            if (_settings == null || _settings.CrosshairSettings == null)
                return;
        
            OnPropertyChangedWithValue(CrosshairWidthInt, "CrosshairWidthInt");
            OnPropertyChangedWithValue(CrosshairLengthInt, "CrosshairLengthInt");
            OnPropertyChangedWithValue(CrosshairOffsetInt, "CrosshairOffsetInt");
            OnPropertyChangedWithValue(CrosshairOpacityFloat, "CrosshairOpacityFloat");
            OnPropertyChangedWithValue(CustomCrosshairEnabled, "CustomCrosshairEnabled");
            OnPropertyChangedWithValue(DotEnabled, "DotEnabled");
            OnPropertyChangedWithValue(DotColorText, "DotColorText");
            OnPropertyChangedWithValue(DotWidthText, "DotWidthText");
            OnPropertyChangedWithValue(DotHeightText, "DotHeightText");
    
            SelectCurrentCrosshairColor();
            SelectCurrentDotColor();
    
            OnSettingsChanged();
        }

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
        [DataSourceProperty] public bool IsCrosshairSettingsPageOpen { get { return _currentPage == "CrosshairSettings"; } }
        [DataSourceProperty] public bool IsDotSettingsPageOpen { get { return _currentPage == "DotSettings"; } }
        [DataSourceProperty] public bool IsMiscPageOpen { get { return _currentPage == "Misc"; } }
        
        [DataSourceProperty] 
        public bool IsAvatarSidesPageOpen { get { return _currentPage == "AvatarSides"; } }
        
        [DataSourceProperty]
        public string SelectedAvatarSideName { get { return _avatarSideEditor.SelectedName; } }

        [DataSourceProperty]
        public string CurrentAvatarOffsetXText { get { return _avatarSideEditor.OffsetXText; } }

        [DataSourceProperty]
        public string CurrentAvatarOffsetYText { get { return _avatarSideEditor.OffsetYText; } }

        [DataSourceProperty]
        public string CurrentAvatarScaleText { get { return _avatarSideEditor.ScaleText; } }

        [DataSourceProperty]
        public string CurrentAvatarOrientationText 
        { 
            get 
            { 
                if (_selectedAvatarSide == 0)
                    return _settings.AllyAvatarsVertical ? "Vertical" : "Horizontal";
                else
                    return _settings.EnemyAvatarsVertical ? "Vertical" : "Horizontal";
            } 
        }
        
        [DataSourceProperty]
        public bool IsAllyAvatarSelected { get { return _selectedAvatarSide == 0; } }

        [DataSourceProperty]
        public bool IsEnemyAvatarSelected { get { return _selectedAvatarSide == 1; } }

        [DataSourceProperty] 
        public bool ShouldShiftMenuRight 
        { 
            get { return IsCrosshairSettingsPageOpen || IsDotSettingsPageOpen; } 
        }

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
        public bool ShowPowerLevel { get { return _settings.ShowPowerLevel; } set { if (_settings.ShowPowerLevel != value) { _settings.ShowPowerLevel = value; OnPropertyChangedWithValue(value, "ShowPowerLevel"); OnSettingsChanged(); } } }
        
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
        
        [DataSourceProperty]
        public bool ShowHealthNumbers { get { return _settings.ShowHealthNumbers; } set { if (_settings.ShowHealthNumbers != value) { _settings.ShowHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowHealthNumbers"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowMountHealthNumbers { get { return _settings.ShowMountHealthNumbers; } set { if (_settings.ShowMountHealthNumbers != value) { _settings.ShowMountHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowMountHealthNumbers"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool ShowShieldHealthNumbers { get { return _settings.ShowShieldHealthNumbers; } set { if (_settings.ShowShieldHealthNumbers != value) { _settings.ShowShieldHealthNumbers = value; OnPropertyChangedWithValue(value, "ShowShieldHealthNumbers"); OnSettingsChanged(); } } }
        
        [DataSourceProperty]
        public bool CameraSnapbackEnabled { get { return _settings.CameraSnapbackEnabled; } set { if (_settings.CameraSnapbackEnabled != value) { _settings.CameraSnapbackEnabled = value; OnPropertyChangedWithValue(value, "CameraSnapbackEnabled"); OnSettingsChanged(); } } }

        [DataSourceProperty]
        public bool CustomCrosshairEnabled { get { return _settings.CrosshairSettings.CustomCrosshairEnabled; } set { if (_settings.CrosshairSettings.CustomCrosshairEnabled != value) { _settings.CrosshairSettings.CustomCrosshairEnabled = value; OnPropertyChangedWithValue(value, "CustomCrosshairEnabled"); OnSettingsChanged(); } } }

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

        [DataSourceProperty] public string KillfeedOffsetXText { get { return _settings.KillfeedCustom.OffsetX.ToString("F0"); } }
        [DataSourceProperty] public string KillfeedOffsetYText { get { return _settings.KillfeedCustom.OffsetY.ToString("F0"); } }
        [DataSourceProperty] public string KillfeedScaleText { get { return (_settings.KillfeedCustom.Scale * 100).ToString("F0") + "%"; } }
        [DataSourceProperty] public string KillfeedFadeoutTimeText { get { return _settings.KillfeedFadeoutTime.ToString("F0") + "s"; } }

        [DataSourceProperty] public string CrosshairWidthText { get { return _settings.CrosshairSettings.SizeHorizontal.ToString(); } }
        [DataSourceProperty] public string CrosshairLengthText { get { return _settings.CrosshairSettings.SizeVertical.ToString(); } }
        [DataSourceProperty] public string CrosshairOffsetText { get { return _settings.CrosshairSettings.Offset.ToString(); } }
        [DataSourceProperty] public string CrosshairOpacityText { get { return (_settings.CrosshairSettings.Opacity * 100).ToString("F0") + "%"; } }
        [DataSourceProperty] public string CrosshairColorText { get { return _settings.CrosshairSettings.Color; } }

        [DataSourceProperty]
        public bool DotEnabled 
        { 
            get { return _settings.CrosshairSettings.DotEnabled; } 
            set 
            { 
                if (_settings.CrosshairSettings.DotEnabled != value) 
                { 
                    _settings.CrosshairSettings.DotEnabled = value; 
                    OnPropertyChangedWithValue(value, "DotEnabled"); 
                    OnSettingsChanged(); 
                } 
            } 
        }

        [DataSourceProperty] 
        public string DotColorText { get { return _settings.CrosshairSettings.DotColor; } }

        [DataSourceProperty] 
        public string DotWidthText { get { return _settings.CrosshairSettings.DotSizeWidth.ToString(); } }
        
        [DataSourceProperty] 
        public string DotHeightText { get { return _settings.CrosshairSettings.DotSizeHeight.ToString(); } }

        public void ExecuteSetDotCircular() 
        { 
            _settings.CrosshairSettings.DotIsCircular = true; 
            OnPropertyChangedWithValue(true, "DotIsCircular");
            OnSettingsChanged(); 
        }

        public void ExecuteSetDotSquare() 
        { 
            _settings.CrosshairSettings.DotIsCircular = false; 
            OnPropertyChangedWithValue(false, "DotIsCircular");
            OnSettingsChanged(); 
        }
        
        [DataSourceProperty]
        public float MenuHorizontalOffset
        {
            get { return ShouldShiftMenuRight ? 610f : 0f; }
        }

        [DataSourceProperty]
        public bool KillfeedBackgroundEnabled 
        { 
            get { return _settings.KillfeedBackgroundEnabled; } 
            set 
            { 
                if (_settings.KillfeedBackgroundEnabled != value) 
                { 
                    _settings.KillfeedBackgroundEnabled = value; 
                    OnPropertyChangedWithValue(value, "KillfeedBackgroundEnabled"); 
                    OnSettingsChanged(); 
                } 
            } 
        }

        [DataSourceProperty] 
        public string KillfeedBackgroundOpacityText 
        { 
            get { return (_settings.KillfeedBackgroundOpacity * 100).ToString("F0") + "%"; } 
        }

        [DataSourceProperty] 
        public string KillfeedBackgroundColorText 
        { 
            get { return _settings.KillfeedBackgroundColor; } 
        }
        
        [DataSourceProperty] 
        public bool IsProfilesPageOpen { get { return _currentPage == "Profiles"; } }

        [DataSourceProperty]
        public string CurrentProfileName
        {
            get 
            { 
                return _tempProfileName; 
            }
            set
            {
                if (_tempProfileName != value)
                {
                    // Just update the variable in memory. 
                    // NO saving to disk happens here. No crashing.
                    _tempProfileName = value;
                    OnPropertyChanged("CurrentProfileName");
            
                    // Show the save button if the text differs from the saved name
                    OnPropertyChanged("IsSaveNameButtonVisible"); 
                }
            }
        }
        
        [DataSourceProperty]
        public bool IsSaveNameButtonVisible
        {
            get 
            { 
                return _tempProfileName != ProfileManager.GetActiveProfile().Name; 
            }
        }

        [DataSourceProperty]
        public string ProfileCountText 
        { 
            get 
            { 
                return (_selectedProfileIndex + 1) + "/" + ProfileManager.GetProfileCount(); 
            } 
        }

        [DataSourceProperty]
        public bool CanDeleteProfile { get { return ProfileManager.GetProfileCount() > 1; } }
        
        
        public void ExecuteSaveProfileName()
        {
            string currentRealName = ProfileManager.GetActiveProfile().Name;
    
            // Attempt to rename
            bool success = ProfileManager.RenameProfile(currentRealName, _tempProfileName);
    
            if (success)
            {
                // If it worked, hide the button (because temp name now equals real name)
                OnPropertyChanged("IsSaveNameButtonVisible");
            }
            else
            {
                // If it failed (e.g. name taken), revert text back to the real name
                _tempProfileName = currentRealName;
                OnPropertyChanged("CurrentProfileName");
                OnPropertyChanged("IsSaveNameButtonVisible");
            }
        }
        
        public void ExecuteCleanupAvatars()
        {
            if (OnCleanupAvatarsRequested != null)
                OnCleanupAvatarsRequested();
            InformationManager.DisplayMessage(new InformationMessage("[BetterMPHUD] Cleaned up disconnected avatars.", Colors.Green));
        }
        
        private void AdjustKillfeedMaxEntries(int delta)
        {
            int newValue = _settings.KillfeedMaxEntries + delta;
            if (newValue >= 1 && newValue <= 15)
            {
                _settings.KillfeedMaxEntries = newValue;
                OnPropertyChangedWithValue(KillfeedMaxEntriesText, "KillfeedMaxEntriesText");
                OnSettingsChanged();
            }
        }
        
        public void ExecuteNextProfile()
        {
            int count = ProfileManager.GetProfileCount();
            if (count <= 1) return;
    
            _selectedProfileIndex = (_selectedProfileIndex + 1) % count;
            ProfileManager.SetActiveProfileByIndex(_selectedProfileIndex);
            ReloadSettingsFromProfile();
        }
        
        public void ExecutePreviousProfile()
        {
            int count = ProfileManager.GetProfileCount();
            if (count <= 1) return;
    
            _selectedProfileIndex = (_selectedProfileIndex - 1 + count) % count;
            ProfileManager.SetActiveProfileByIndex(_selectedProfileIndex);
            ReloadSettingsFromProfile();
        }
        
        public void ExecuteCreateProfile()
        {
            string newName = "Profile " + _newProfileCounter;
            while (ProfileManager.GetProfileNames().Contains(newName))
            {
                _newProfileCounter++;
                newName = "Profile " + _newProfileCounter;
            }
    
            if (ProfileManager.CreateProfile(newName))
            {
                _newProfileCounter++;
                RefreshProfileDisplay();
            }
        }
        
        [DataSourceProperty]
        public string CurrentAvatarSpacingText 
        { 
            get 
            { 
                float spacing = _selectedAvatarSide == 0 ? _settings.AllyAvatarsSpacing : _settings.EnemyAvatarsSpacing;
                return spacing.ToString("F0");
            } 
        }
        
        public void ExecuteDuplicateProfile()
        {
            string currentName = ProfileManager.GetActiveProfile().Name;
            string newName = currentName + " Copy";
            int copyNum = 1;
    
            while (ProfileManager.GetProfileNames().Contains(newName))
            {
                copyNum++;
                newName = currentName + " Copy " + copyNum;
            }
    
            if (ProfileManager.DuplicateProfile(currentName, newName))
            {
                ProfileManager.SetActiveProfile(newName);
                _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
                ReloadSettingsFromProfile();
            }
        }
        
        public void ExecuteDeleteProfile()
        {
            string currentName = ProfileManager.GetActiveProfile().Name;
            if (ProfileManager.DeleteProfile(currentName))
            {
                _selectedProfileIndex = ProfileManager.GetActiveProfileIndex();
                ReloadSettingsFromProfile();
            }
        }
        
        private void ReloadSettingsFromProfile()
        {
            _settings = ProfileManager.GetActiveSettings();

            _topBarEditor = new ElementEditorVM(0, _settings.TimeAndScoresCustom, "Time & Scores");
            _topBarEditor.SetOnChanged(OnSettingsChanged);
            _hpEditor = new ElementEditorVM(0, _settings.AgentHealthCustom, "Agent Health");
            _hpEditor.SetOnChanged(OnSettingsChanged);
    
            _avatarSideEditor = new ElementEditorVM(0, _settings.AllyAvatarsCustom, "Ally (Left)");
            _avatarSideEditor.SetOnChanged(OnSettingsChanged);

            ApplyNativeKillfeedSetting();
            RefreshProfileDisplay();
            RefreshAllSettingsDisplay();

            if (OnHudSettingsChanged != null)
                OnHudSettingsChanged();
        }
        
        private void RefreshProfileDisplay()
        {
            _tempProfileName = ProfileManager.GetActiveProfile().Name;
    
            OnPropertyChangedWithValue(CurrentProfileName, "CurrentProfileName");
            OnPropertyChangedWithValue(ProfileCountText, "ProfileCountText");
            OnPropertyChangedWithValue(CanDeleteProfile, "CanDeleteProfile");
            OnPropertyChangedWithValue(IsSaveNameButtonVisible, "IsSaveNameButtonVisible");
        }
        
        private void RefreshAllSettingsDisplay()
        {
            OnPropertyChangedWithValue(NativeKillfeedEnabled, "NativeKillfeedEnabled");
            OnPropertyChangedWithValue(WarbandKillfeedEnabled, "WarbandKillfeedEnabled");
            OnPropertyChangedWithValue(ShowTimeAndScores, "ShowTimeAndScores");
            OnPropertyChangedWithValue(ShowAvatars, "ShowAvatars");
            OnPropertyChangedWithValue(ShowEnemyScore, "ShowEnemyScore");
            OnPropertyChangedWithValue(ShowBanners, "ShowBanners");
            OnPropertyChangedWithValue(ShowMorale, "ShowMorale");
            OnPropertyChangedWithValue(ShowAgentHealth, "ShowAgentHealth");
            OnPropertyChangedWithValue(ShowMountHealth, "ShowMountHealth");
            OnPropertyChangedWithValue(ShowShieldHealth, "ShowShieldHealth");
            OnPropertyChangedWithValue(ShowWeaponInfo, "ShowWeaponInfo");
            OnPropertyChangedWithValue(ShowGoldAmount, "ShowGoldAmount");
            OnPropertyChangedWithValue(ShowTroopCount, "ShowTroopCount");
            OnPropertyChangedWithValue(ShowCouchLanceState, "ShowCouchLanceState");
            OnPropertyChangedWithValue(ShowDamageFeed, "ShowDamageFeed");
            OnPropertyChangedWithValue(CustomCrosshairEnabled, "CustomCrosshairEnabled");
            OnPropertyChangedWithValue(DotEnabled, "DotEnabled");
            OnPropertyChangedWithValue(CameraSnapbackEnabled, "CameraSnapbackEnabled");
            OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText");
            OnPropertyChangedWithValue(KillfeedMaxEntriesText, "KillfeedMaxEntriesText");
            OnPropertyChangedWithValue(KillfeedBackgroundEnabled, "KillfeedBackgroundEnabled");
            OnPropertyChangedWithValue(KillfeedBackgroundOpacityText, "KillfeedBackgroundOpacityText");
            OnPropertyChangedWithValue(_settings.AllyAvatarsVertical, "AllyAvatarsVertical");
            OnPropertyChangedWithValue(_settings.EnemyAvatarsVertical, "EnemyAvatarsVertical");
            OnPropertyChangedWithValue(BetterAvatarsEnabled, "BetterAvatarsEnabled");
            OnPropertyChangedWithValue(AvatarSortingEnabled, "AvatarSortingEnabled");
            OnPropertyChangedWithValue(ScoreboardBackgroundEnabled, "ScoreboardBackgroundEnabled");
            OnPropertyChangedWithValue(ScoreboardStripingEnabled, "ScoreboardStripingEnabled");
            OnPropertyChangedWithValue(ScoreboardBackgroundOpacityText, "ScoreboardBackgroundOpacityText");
            OnPropertyChangedWithValue(ScoreboardStripingOpacityText, "ScoreboardStripingOpacityText");
            OnPropertyChangedWithValue(ScoreboardDeadPlayerOpacityText, "ScoreboardDeadPlayerOpacityText");
            OnPropertyChangedWithValue(ScoreboardDeadPlayerTintEnabled, "ScoreboardDeadPlayerTintEnabled");
            OnPropertyChangedWithValue(HideUIWhenScoreboardOpen , "HideUIWhenScoreboardOpen");
            
            RefreshAvatarSideDisplay();
            RefreshCrosshairDisplay();
            RefreshTopBarDisplay();
            RefreshHPDisplay();
            RefreshKillfeedDisplay();
        }
        
        private void SelectCurrentColors()
        {
            foreach (var item in _crosshairColorList)
                item.IsSelected = item.ColorHex == _settings.CrosshairSettings.Color;
    
            foreach (var item in _dotColorList)
                item.IsSelected = item.ColorHex == _settings.CrosshairSettings.DotColor;
        }

        private void OnCrosshairColorSelected(ColorItemVM selected)
        {
            foreach (var item in _crosshairColorList)
                item.IsSelected = false;
    
            selected.IsSelected = true;
            _settings.CrosshairSettings.Color = selected.ColorHex;
            OnSettingsChanged();
        }
        
        private void OnDotColorSelected(ColorItemVM selected)
        {
            foreach (var item in _dotColorList)
                item.IsSelected = false;
    
            selected.IsSelected = true;
            _settings.CrosshairSettings.DotColor = selected.ColorHex;
            OnSettingsChanged();
        }
        
        private void SelectCurrentCrosshairColor()
        {
            if (_crosshairColorList == null || _settings == null || _settings.CrosshairSettings == null) 
                return;
        
            string currentColor = _settings.CrosshairSettings.Color;
            if (string.IsNullOrEmpty(currentColor))
                currentColor = "#FF0000FF";
    
            foreach (var item in _crosshairColorList)
            {
                if (item != null)
                    item.IsSelected = string.Equals(item.ColorHex, currentColor, StringComparison.OrdinalIgnoreCase);
            }
        }
        
        private void SelectCurrentDotColor()
        {
            if (_dotColorList == null || _settings == null || _settings.CrosshairSettings == null) 
                return;
        
            string currentColor = _settings.CrosshairSettings.DotColor;
            if (string.IsNullOrEmpty(currentColor))
                currentColor = "#FFFFFFFF";
    
            foreach (var item in _dotColorList)
            {
                if (item != null)
                    item.IsSelected = string.Equals(item.ColorHex, currentColor, StringComparison.OrdinalIgnoreCase);
            }
        }
        
        
        
        public void ExecuteIncreaseKillfeedMaxEntries() { AdjustKillfeedMaxEntries(1); }
        public void ExecuteDecreaseKillfeedMaxEntries() { AdjustKillfeedMaxEntries(-1); }
        public void ExecuteIncreaseKillfeedMaxEntriesLarge() { AdjustKillfeedMaxEntries(5); }
        public void ExecuteDecreaseKillfeedMaxEntriesLarge() { AdjustKillfeedMaxEntries(-5); }

        [DataSourceProperty] 
        public string KillfeedMaxEntriesText 
        { 
            get { return _settings.KillfeedMaxEntries.ToString(); } 
        }
        
        [DataSourceProperty]
        public MBBindingList<ColorItemVM> CrosshairColorList
        {
            get { return _crosshairColorList; }
        }

        [DataSourceProperty]
        public MBBindingList<ColorItemVM> DotColorList
        {
            get { return _dotColorList; }
        }

        [DataSourceProperty]
        public int CrosshairWidthInt
        {
            get { return _settings.CrosshairSettings.SizeHorizontal; }
            set
            {
                if (_settings.CrosshairSettings.SizeHorizontal != value)
                {
                    _settings.CrosshairSettings.SizeHorizontal = value;
                    OnPropertyChangedWithValue(value, "CrosshairWidthInt");
                    OnSettingsChanged();
                }
            }
        }
        
        [DataSourceProperty]
        public int CrosshairLengthInt
        {
            get { return _settings.CrosshairSettings.SizeVertical; }
            set
            {
                if (_settings.CrosshairSettings.SizeVertical != value)
                {
                    _settings.CrosshairSettings.SizeVertical = value;
                    OnPropertyChangedWithValue(value, "CrosshairLengthInt");
                    OnSettingsChanged();
                }
            }
        }

        [DataSourceProperty]
        public int CrosshairOffsetInt
        {
            get { return _settings.CrosshairSettings.Offset; }
            set
            {
                if (_settings.CrosshairSettings.Offset != value)
                {
                    _settings.CrosshairSettings.Offset = value;
                    OnPropertyChangedWithValue(value, "CrosshairOffsetInt");
                    OnSettingsChanged();
                }
            }
        }

        [DataSourceProperty]
        public float CrosshairOpacityFloat
        {
            get { return _settings.CrosshairSettings.Opacity; }
            set
            {
                float rounded = (float)Math.Round(value, 2);
                if (_settings.CrosshairSettings.Opacity != rounded)
                {
                    _settings.CrosshairSettings.Opacity = rounded;
                    OnPropertyChangedWithValue(rounded, "CrosshairOpacityFloat");
                    OnSettingsChanged();
                }
            }
        }
        
        [DataSourceProperty]
        public int DotWidthInt
        {
            get { return _settings.CrosshairSettings.DotSizeWidth; }
            set
            {
                if (_settings.CrosshairSettings.DotSizeWidth != value)
                {
                    _settings.CrosshairSettings.DotSizeWidth = value;
                    OnPropertyChangedWithValue(value, "DotWidthInt");
                    OnSettingsChanged();
                }
            }
        }

        [DataSourceProperty]
        public int DotHeightInt
        {
            get { return _settings.CrosshairSettings.DotSizeHeight; }
            set
            {
                if (_settings.CrosshairSettings.DotSizeHeight != value)
                {
                    _settings.CrosshairSettings.DotSizeHeight = value;
                    OnPropertyChangedWithValue(value, "DotHeightInt");
                    OnSettingsChanged();
                }
            }
        }
        
        [DataSourceProperty]
        public bool BetterAvatarsEnabled 
        { 
            get { return _settings.BetterAvatarsEnabled; } 
            set 
            { 
                if (_settings.BetterAvatarsEnabled != value) 
                { 
                    _settings.BetterAvatarsEnabled = value; 
                    OnPropertyChangedWithValue(value, "BetterAvatarsEnabled"); 
                    if (OnBetterAvatarsToggled != null)
                        OnBetterAvatarsToggled(value);
                    OnSettingsChanged(); 
                } 
            } 
        }
        
        [DataSourceProperty]
        public bool ScoreboardBackgroundEnabled 
        { 
            get { return _settings.ScoreboardBackgroundEnabled; } 
            set 
            { 
                if (_settings.ScoreboardBackgroundEnabled != value) 
                { 
                    _settings.ScoreboardBackgroundEnabled = value; 
                    OnPropertyChangedWithValue(value, "ScoreboardBackgroundEnabled"); 
                    OnSettingsChanged(); 
                } 
            } 
        }
        
        [DataSourceProperty]
        public bool ScoreboardStripingEnabled 
        { 
            get { return _settings.ScoreboardStripingEnabled; } 
            set 
            { 
                if (_settings.ScoreboardStripingEnabled != value) 
                { 
                    _settings.ScoreboardStripingEnabled = value; 
                    OnPropertyChangedWithValue(value, "ScoreboardStripingEnabled"); 
                    OnSettingsChanged(); 
                } 
            } 
        }
        
        [DataSourceProperty]
        public string ScoreboardBackgroundOpacityText 
        { 
            get { return (_settings.ScoreboardBackgroundOpacity * 100).ToString("F0") + "%"; } 
        }

        [DataSourceProperty]
        public string ScoreboardStripingOpacityText 
        { 
            get { return (_settings.ScoreboardStripingOpacity * 100).ToString("F0") + "%"; } 
        }

        [DataSourceProperty]
        public bool IsScoreboardPageOpen { get { return _currentPage == "Scoreboard"; } }

        public void ExecuteOpenScoreboardPage() { SetPage("Scoreboard"); }

        private void AdjustScoreboardBackgroundOpacity(float delta)
        {
            float newValue = _settings.ScoreboardBackgroundOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                _settings.ScoreboardBackgroundOpacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(ScoreboardBackgroundOpacityText, "ScoreboardBackgroundOpacityText");
                OnSettingsChanged();
            }
        }
        
        private void AdjustScoreboardStripingOpacity(float delta)
        {
            float newValue = _settings.ScoreboardStripingOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                _settings.ScoreboardStripingOpacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(ScoreboardStripingOpacityText, "ScoreboardStripingOpacityText");
                OnSettingsChanged();
            }
        }
        
        public void ExecuteIncreaseScoreboardOpacity() { AdjustScoreboardBackgroundOpacity(0.05f); }
        public void ExecuteDecreaseScoreboardOpacity() { AdjustScoreboardBackgroundOpacity(-0.05f); }
        public void ExecuteIncreaseScoreboardOpacityLarge() { AdjustScoreboardBackgroundOpacity(0.1f); }
        public void ExecuteDecreaseScoreboardOpacityLarge() { AdjustScoreboardBackgroundOpacity(-0.1f); }

        public void ExecuteIncreaseStripingOpacity() { AdjustScoreboardStripingOpacity(0.05f); }
        public void ExecuteDecreaseStripingOpacity() { AdjustScoreboardStripingOpacity(-0.05f); }
        public void ExecuteIncreaseStripingOpacityLarge() { AdjustScoreboardStripingOpacity(0.1f); }
        public void ExecuteDecreaseStripingOpacityLarge() { AdjustScoreboardStripingOpacity(-0.1f); }
        public void ExecuteIncreaseDeadPlayerOpacity() { AdjustDeadPlayerOpacity(0.05f); }
        public void ExecuteDecreaseDeadPlayerOpacity() { AdjustDeadPlayerOpacity(-0.05f); }
        public void ExecuteIncreaseDeadPlayerOpacityLarge() { AdjustDeadPlayerOpacity(0.1f); }
        public void ExecuteDecreaseDeadPlayerOpacityLarge() { AdjustDeadPlayerOpacity(-0.1f); }

        public void ExecuteSetDeadPlayerColorRed() { _settings.ScoreboardDeadPlayerColor = "#FF4444FF"; OnSettingsChanged(); }
        public void ExecuteSetDeadPlayerColorGray() { _settings.ScoreboardDeadPlayerColor = "#888888FF"; OnSettingsChanged(); }
        public void ExecuteSetDeadPlayerColorDark() { _settings.ScoreboardDeadPlayerColor = "#444444FF"; OnSettingsChanged(); }
        public void ExecuteSetDeadPlayerColorWhite() { _settings.ScoreboardDeadPlayerColor = "#CCCCCCFF"; OnSettingsChanged(); }
        public void ExecuteSetDeadPlayerColorOrange() { _settings.ScoreboardDeadPlayerColor = "#FF8800FF"; OnSettingsChanged(); }
        public void ExecuteSetDeadPlayerColorPurple() { _settings.ScoreboardDeadPlayerColor = "#8844FFFF"; OnSettingsChanged(); }


        [DataSourceProperty]
        public bool HideUIWhenScoreboardOpen 
        { 
            get { return _settings.HideUIWhenScoreboardOpen; } 
            set 
            { 
                if (_settings.HideUIWhenScoreboardOpen != value) 
                { 
                    _settings.HideUIWhenScoreboardOpen = value; 
                    OnPropertyChangedWithValue(value, "HideUIWhenScoreboardOpen"); 
                    OnSettingsChanged(); 
                } 
            } 
        }

        [DataSourceProperty]
        public bool ScoreboardDeadPlayerTintEnabled 
        { 
            get { return _settings.ScoreboardDeadPlayerTintEnabled; } 
            set 
            { 
                if (_settings.ScoreboardDeadPlayerTintEnabled != value) 
                { 
                    _settings.ScoreboardDeadPlayerTintEnabled = value; 
                    OnPropertyChangedWithValue(value, "ScoreboardDeadPlayerTintEnabled"); 
                    OnSettingsChanged(); 
                } 
            } 
        }
        
        [DataSourceProperty]
        public string ScoreboardDeadPlayerOpacityText 
        { 
            get { return (_settings.ScoreboardDeadPlayerOpacity * 100).ToString("F0") + "%"; } 
        }

        private void AdjustDeadPlayerOpacity(float delta)
        {
            float newValue = _settings.ScoreboardDeadPlayerOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                _settings.ScoreboardDeadPlayerOpacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(ScoreboardDeadPlayerOpacityText, "ScoreboardDeadPlayerOpacityText");
                OnSettingsChanged();
            }
        }
        
        public void ExecuteResetScoreboard()
        {
            _settings.ScoreboardBackgroundEnabled = true;
            _settings.ScoreboardBackgroundOpacity = 0.8f;
            _settings.ScoreboardStripingEnabled = true;
            _settings.ScoreboardStripingOpacity = 0.2f;
            _settings.ScoreboardDeadPlayerOpacity = 0.3f;
            _settings.ScoreboardDeadPlayerColor = "";
            _settings.ScoreboardDeadPlayerTintEnabled = false;
            _settings.HideUIWhenScoreboardOpen = false;
    
            RefreshScoreboardDisplay();
            OnSettingsChanged();
        }
        
        private void RefreshScoreboardDisplay()
        {
            OnPropertyChangedWithValue(ScoreboardBackgroundEnabled, "ScoreboardBackgroundEnabled");
            OnPropertyChangedWithValue(ScoreboardBackgroundOpacityText, "ScoreboardBackgroundOpacityText");
            OnPropertyChangedWithValue(ScoreboardStripingEnabled, "ScoreboardStripingEnabled");
            OnPropertyChangedWithValue(ScoreboardStripingOpacityText, "ScoreboardStripingOpacityText");
            OnPropertyChangedWithValue(ScoreboardDeadPlayerOpacityText, "ScoreboardDeadPlayerOpacityText");
            OnPropertyChangedWithValue(ScoreboardDeadPlayerTintEnabled, "ScoreboardDeadPlayerTintEnabled");
            OnPropertyChangedWithValue(HideUIWhenScoreboardOpen, "HideUIWhenScoreboardOpen");
        }
    }
}