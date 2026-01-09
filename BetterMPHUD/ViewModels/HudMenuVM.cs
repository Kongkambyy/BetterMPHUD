using System;
using TaleWorlds.Library;
using BetterMPHUD.ViewModels.Settings;

namespace BetterMPHUD.ViewModels
{
    public class HudMenuVM : ViewModel
    {
        private HudSettings _settings;
        private bool _isConfigMenuOpen;

        // Sub ViewModels
        private NavigationVM _navigation;
        private ProfileManagerVM _profileManager;
        private KillfeedSettingsVM _killfeed;
        private CrosshairSettingsVM _crosshair;
        private DotSettingsVM _dot;
        private TopBarSettingsVM _topBar;
        private AvatarSettingsVM _avatar;
        private AgentStatusSettingsVM _agentStatus;
        private ScoreboardSettingsVM _scoreboard;
        private ChatSettingsVM _chat;
        private MiscSettingsVM _misc;
        private VisibilitySettingsVM _visibility;

        // External callbacks
        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        public Action OnHudSettingsChanged;
        public Action OnCleanupAvatarsRequested;
        public Action OnDebugScoreboardStructure;
        public Action OnDebugChatStructure;
        public Action<bool> OnChatToggled;
        public Action OnDebugAvatarStructure;
        public Action<bool> OnBetterAvatarsToggled;
        public Action<HudSettings> OnHideKillfeedPreview;
        public Action<HudSettings> OnUpdateKillfeedPreview; 

        public HudMenuVM()
        {
            _settings = ConfigManager.LoadSettings();
            InitializeSubViewModels();
            WireUpCallbacks();
        }

        private void InitializeSubViewModels()
        {
            _navigation = new NavigationVM();
            _navigation.SetOnPageChanged(OnNavigationChanged);
            _profileManager = new ProfileManagerVM(OnProfileChanged);
            _profileManager.SetOnPropertyRefresh(RefreshProfileProperties);
            _killfeed = new KillfeedSettingsVM(_settings, OnSettingsChanged);
            _crosshair = new CrosshairSettingsVM(_settings, OnSettingsChanged);
            _dot = new DotSettingsVM(_settings, OnSettingsChanged);
            _topBar = new TopBarSettingsVM(_settings, OnSettingsChanged);
            _avatar = new AvatarSettingsVM(_settings, OnSettingsChanged);
            _agentStatus = new AgentStatusSettingsVM(_settings, OnSettingsChanged);
            _scoreboard = new ScoreboardSettingsVM(_settings, OnSettingsChanged);
            _chat = new ChatSettingsVM(_settings, OnSettingsChanged);
            _misc = new MiscSettingsVM(_settings, OnSettingsChanged);
            _visibility = new VisibilitySettingsVM(_settings, OnSettingsChanged);
        }

        private void WireUpCallbacks()
        {
            _killfeed.OnWarbandKillfeedToggled = (enabled) => OnWarbandKillfeedToggled?.Invoke(enabled);
            _killfeed.OnPreviewUpdate = () => OnUpdateKillfeedPreview?.Invoke(_settings);  
    
            _avatar.OnBetterAvatarsToggled = (enabled) => OnBetterAvatarsToggled?.Invoke(enabled);
            _avatar.OnCleanupAvatarsRequested = () => OnCleanupAvatarsRequested?.Invoke();
            _scoreboard.OnDebugScoreboardStructure = () => OnDebugScoreboardStructure?.Invoke();
            _chat.OnChatToggled = (enabled) => OnChatToggled?.Invoke(enabled);
            _chat.OnDebugChatStructure = () => OnDebugChatStructure?.Invoke();

            _killfeed.SetOnPropertyRefresh(RefreshKillfeedProperties);
            _crosshair.SetOnPropertyRefresh(RefreshCrosshairProperties);
            _dot.SetOnPropertyRefresh(RefreshDotProperties);
            _topBar.SetOnPropertyRefresh(RefreshTopBarProperties);
            _avatar.SetOnPropertyRefresh(RefreshAvatarProperties);
            _agentStatus.SetOnPropertyRefresh(RefreshAgentStatusProperties);
            _scoreboard.SetOnPropertyRefresh(RefreshScoreboardProperties);
            _chat.SetOnPropertyRefresh(RefreshChatProperties);
            _misc.SetOnPropertyRefresh(RefreshMiscProperties);
            _visibility.SetOnPropertyRefresh(RefreshVisibilityProperties);
        }

        private void RefreshKillfeedProperties()
        {
            OnPropertyChangedWithValue(KillfeedOffsetXText, "KillfeedOffsetXText");
            OnPropertyChangedWithValue(KillfeedOffsetYText, "KillfeedOffsetYText");
            OnPropertyChangedWithValue(KillfeedScaleText, "KillfeedScaleText");
            OnPropertyChangedWithValue(KillfeedFadeoutTimeText, "KillfeedFadeoutTimeText");
            OnPropertyChangedWithValue(KillfeedMaxEntriesText, "KillfeedMaxEntriesText");
            OnPropertyChangedWithValue(KillfeedBackgroundOpacityText, "KillfeedBackgroundOpacityText");
            OnPropertyChangedWithValue(KillfeedBackgroundColorText, "KillfeedBackgroundColorText");
            OnPropertyChangedWithValue(NativeKillfeedEnabled, "NativeKillfeedEnabled");
            OnPropertyChangedWithValue(WarbandKillfeedEnabled, "WarbandKillfeedEnabled");
            OnPropertyChangedWithValue(KillfeedBackgroundEnabled, "KillfeedBackgroundEnabled");
        }

        private void RefreshCrosshairProperties()
        {
            OnPropertyChangedWithValue(CustomCrosshairEnabled, "CustomCrosshairEnabled");
            OnPropertyChangedWithValue(CrosshairWidthInt, "CrosshairWidthInt");
            OnPropertyChangedWithValue(CrosshairLengthInt, "CrosshairLengthInt");
            OnPropertyChangedWithValue(CrosshairOffsetInt, "CrosshairOffsetInt");
            OnPropertyChangedWithValue(CrosshairOpacityFloat, "CrosshairOpacityFloat");
            OnPropertyChangedWithValue(CrosshairWidthText, "CrosshairWidthText");
            OnPropertyChangedWithValue(CrosshairLengthText, "CrosshairLengthText");
            OnPropertyChangedWithValue(CrosshairOffsetText, "CrosshairOffsetText");
            OnPropertyChangedWithValue(CrosshairOpacityText, "CrosshairOpacityText");
            OnPropertyChangedWithValue(CrosshairColorText, "CrosshairColorText");
        }

        private void RefreshDotProperties()
        {
            OnPropertyChangedWithValue(DotEnabled, "DotEnabled");
            OnPropertyChangedWithValue(DotWidthInt, "DotWidthInt");
            OnPropertyChangedWithValue(DotHeightInt, "DotHeightInt");
            OnPropertyChangedWithValue(DotWidthText, "DotWidthText");
            OnPropertyChangedWithValue(DotHeightText, "DotHeightText");
            OnPropertyChangedWithValue(DotColorText, "DotColorText");
            OnPropertyChangedWithValue(DotOffsetXText, "DotOffsetXText");
            OnPropertyChangedWithValue(DotOffsetYText, "DotOffsetYText");
        }

        private void RefreshTopBarProperties()
        {
            OnPropertyChangedWithValue(SelectedElementIndex, "SelectedElementIndex");
            OnPropertyChangedWithValue(SelectedElementName, "SelectedElementName");
            OnPropertyChangedWithValue(CurrentOffsetXText, "CurrentOffsetXText");
            OnPropertyChangedWithValue(CurrentOffsetYText, "CurrentOffsetYText");
            OnPropertyChangedWithValue(CurrentScaleText, "CurrentScaleText");
        }

        private void RefreshAvatarProperties()
        {
            OnPropertyChangedWithValue(SelectedAvatarSideName, "SelectedAvatarSideName");
            OnPropertyChangedWithValue(CurrentAvatarOffsetXText, "CurrentAvatarOffsetXText");
            OnPropertyChangedWithValue(CurrentAvatarOffsetYText, "CurrentAvatarOffsetYText");
            OnPropertyChangedWithValue(CurrentAvatarScaleText, "CurrentAvatarScaleText");
            OnPropertyChangedWithValue(CurrentAvatarOrientationText, "CurrentAvatarOrientationText");
            OnPropertyChangedWithValue(CurrentAvatarSpacingText, "CurrentAvatarSpacingText");
            OnPropertyChangedWithValue(IsAllyAvatarSelected, "IsAllyAvatarSelected");
            OnPropertyChangedWithValue(IsEnemyAvatarSelected, "IsEnemyAvatarSelected");
            OnPropertyChangedWithValue(BetterAvatarsEnabled, "BetterAvatarsEnabled");
            OnPropertyChangedWithValue(AvatarSortingEnabled, "AvatarSortingEnabled");
        }

        private void RefreshAgentStatusProperties()
        {
            OnPropertyChangedWithValue(SelectedHPElementIndex, "SelectedHPElementIndex");
            OnPropertyChangedWithValue(SelectedHPElementName, "SelectedHPElementName");
            OnPropertyChangedWithValue(CurrentHPOffsetXText, "CurrentHPOffsetXText");
            OnPropertyChangedWithValue(CurrentHPOffsetYText, "CurrentHPOffsetYText");
            OnPropertyChangedWithValue(CurrentHPScaleText, "CurrentHPScaleText");
        }

        private void RefreshScoreboardProperties()
        {
            OnPropertyChangedWithValue(ScoreboardBackgroundEnabled, "ScoreboardBackgroundEnabled");
            OnPropertyChangedWithValue(ScoreboardStripingEnabled, "ScoreboardStripingEnabled");
            OnPropertyChangedWithValue(ScoreboardDeadPlayerTintEnabled, "ScoreboardDeadPlayerTintEnabled");
            OnPropertyChangedWithValue(HideUIWhenScoreboardOpen, "HideUIWhenScoreboardOpen");
            OnPropertyChangedWithValue(ScoreboardBackgroundOpacityText, "ScoreboardBackgroundOpacityText");
            OnPropertyChangedWithValue(ScoreboardStripingOpacityText, "ScoreboardStripingOpacityText");
            OnPropertyChangedWithValue(ScoreboardDeadPlayerOpacityText, "ScoreboardDeadPlayerOpacityText");
        }

        private void RefreshChatProperties()
        {
            OnPropertyChangedWithValue(ShowChat, "ShowChat");
            OnPropertyChangedWithValue(ChatMinimalMode, "ChatMinimalMode");
            OnPropertyChangedWithValue(ChatOffsetXText, "ChatOffsetXText");
            OnPropertyChangedWithValue(ChatOffsetYText, "ChatOffsetYText");
            OnPropertyChangedWithValue(ChatScaleText, "ChatScaleText");
        }

        private void RefreshMiscProperties()
        {
            OnPropertyChangedWithValue(CameraSnapbackEnabled, "CameraSnapbackEnabled");
        }

        private void RefreshVisibilityProperties()
        {
            OnPropertyChangedWithValue(ShowTimeAndScores, "ShowTimeAndScores");
            OnPropertyChangedWithValue(ShowAvatars, "ShowAvatars");
            OnPropertyChangedWithValue(ShowEnemyScore, "ShowEnemyScore");
            OnPropertyChangedWithValue(ShowBanners, "ShowBanners");
            OnPropertyChangedWithValue(ShowMorale, "ShowMorale");
            OnPropertyChangedWithValue(ShowPowerLevel, "ShowPowerLevel");
            OnPropertyChangedWithValue(ShowAgentHealth, "ShowAgentHealth");
            OnPropertyChangedWithValue(ShowMountHealth, "ShowMountHealth");
            OnPropertyChangedWithValue(ShowShieldHealth, "ShowShieldHealth");
            OnPropertyChangedWithValue(ShowWeaponInfo, "ShowWeaponInfo");
            OnPropertyChangedWithValue(ShowAmmoCount, "ShowAmmoCount");
            OnPropertyChangedWithValue(ShowGoldAmount, "ShowGoldAmount");
            OnPropertyChangedWithValue(ShowTroopCount, "ShowTroopCount");
            OnPropertyChangedWithValue(ShowCouchLanceState, "ShowCouchLanceState");
            OnPropertyChangedWithValue(ShowDamageFeed, "ShowDamageFeed");
            OnPropertyChangedWithValue(ShowHealthNumbers, "ShowHealthNumbers");
            OnPropertyChangedWithValue(ShowMountHealthNumbers, "ShowMountHealthNumbers");
            OnPropertyChangedWithValue(ShowShieldHealthNumbers, "ShowShieldHealthNumbers");
        }

        private void RefreshProfileProperties()
        {
            OnPropertyChangedWithValue(CurrentProfileName, "CurrentProfileName");
            OnPropertyChangedWithValue(IsSaveNameButtonVisible, "IsSaveNameButtonVisible");
            OnPropertyChangedWithValue(ProfileCountText, "ProfileCountText");
            OnPropertyChangedWithValue(CanDeleteProfile, "CanDeleteProfile");
        }

        private void OnSettingsChanged()
        {
            ConfigManager.SaveSettings(_settings);
            OnHudSettingsChanged?.Invoke();
        }

        private void OnNavigationChanged()
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
            OnPropertyChangedWithValue(IsProfilesPageOpen, "IsProfilesPageOpen");
            OnPropertyChangedWithValue(IsAvatarSidesPageOpen, "IsAvatarSidesPageOpen");
            OnPropertyChangedWithValue(IsScoreboardPageOpen, "IsScoreboardPageOpen");
            OnPropertyChangedWithValue(ShouldShiftMenuRight, "ShouldShiftMenuRight");
            OnPropertyChangedWithValue(MenuHorizontalOffset, "MenuHorizontalOffset");
            
            if (!IsKillfeedPageOpen)
                OnHideKillfeedPreview?.Invoke(_settings);
        }

        private void OnProfileChanged()
        {
            _settings = BetterMPHUD.ProfileManager.GetActiveSettings();
            RecreateSubViewModels();
            RefreshAllSettings();
            OnHudSettingsChanged?.Invoke();
        }

        private void RecreateSubViewModels()
        {
            _killfeed = new KillfeedSettingsVM(_settings, OnSettingsChanged);
            _crosshair = new CrosshairSettingsVM(_settings, OnSettingsChanged);
            _dot = new DotSettingsVM(_settings, OnSettingsChanged);
            _topBar = new TopBarSettingsVM(_settings, OnSettingsChanged);
            _avatar = new AvatarSettingsVM(_settings, OnSettingsChanged);
            _agentStatus = new AgentStatusSettingsVM(_settings, OnSettingsChanged);
            _scoreboard = new ScoreboardSettingsVM(_settings, OnSettingsChanged);
            _chat = new ChatSettingsVM(_settings, OnSettingsChanged);
            _misc = new MiscSettingsVM(_settings, OnSettingsChanged);
            _visibility = new VisibilitySettingsVM(_settings, OnSettingsChanged);
            WireUpCallbacks();
        }

        private void RefreshAllSettings()
        {
            _killfeed.RefreshAll();
            _crosshair.RefreshAll();
            _dot.RefreshAll();
            _topBar.RefreshAll();
            _avatar.RefreshAll();
            _agentStatus.RefreshAll();
            _scoreboard.RefreshAll();
            _chat.RefreshAll();
            _misc.RefreshAll();
            _visibility.RefreshAll();
        }

        public HudSettings GetSettings() => _settings;

        public void ExecuteClose()
        {
            OnHideKillfeedPreview?.Invoke(_settings); 
            OnCloseConfigMenu?.Invoke();
        }

        public void ExecuteDebugAvatarStructure() => OnDebugAvatarStructure?.Invoke();

        [DataSourceProperty]
        public bool IsConfigMenuOpen
        {
            get => _isConfigMenuOpen;
            set
            {
                if (_isConfigMenuOpen != value)
                {
                    _isConfigMenuOpen = value;
                    OnPropertyChangedWithValue(value, "IsConfigMenuOpen");
                }
            }
        }


        #region Sub ViewModel Properties

        [DataSourceProperty] public NavigationVM Navigation => _navigation;
        [DataSourceProperty] public ProfileManagerVM ProfileManager => _profileManager;
        [DataSourceProperty] public KillfeedSettingsVM Killfeed => _killfeed;
        [DataSourceProperty] public CrosshairSettingsVM Crosshair => _crosshair;
        [DataSourceProperty] public DotSettingsVM Dot => _dot;
        [DataSourceProperty] public TopBarSettingsVM TopBar => _topBar;
        [DataSourceProperty] public AvatarSettingsVM Avatar => _avatar;
        [DataSourceProperty] public AgentStatusSettingsVM AgentStatus => _agentStatus;
        [DataSourceProperty] public ScoreboardSettingsVM Scoreboard => _scoreboard;
        [DataSourceProperty] public ChatSettingsVM Chat => _chat;
        [DataSourceProperty] public MiscSettingsVM Misc => _misc;
        [DataSourceProperty] public VisibilitySettingsVM Visibility => _visibility;

        #endregion

        #region Navigation Delegation

        public void ExecuteOpenKillfeedPage() => _navigation.ExecuteOpenKillfeedPage();
        public void ExecuteOpenTopBarPage() => _navigation.ExecuteOpenTopBarPage();
        public void ExecuteOpenVisibilityPage() => _navigation.ExecuteOpenVisibilityPage();
        public void ExecuteOpenCustomizePage() => _navigation.ExecuteOpenCustomizePage();
        public void ExecuteOpenChatPage() => _navigation.ExecuteOpenChatPage();
        public void ExecuteOpenLeaderboardPage() => _navigation.ExecuteOpenLeaderboardPage();
        public void ExecuteOpenHPPage() => _navigation.ExecuteOpenHPPage();
        public void ExecuteOpenHPVisibilityPage() => _navigation.ExecuteOpenHPVisibilityPage();
        public void ExecuteOpenHPCustomizePage() => _navigation.ExecuteOpenHPCustomizePage();
        public void ExecuteOpenCrosshairPage() => _navigation.ExecuteOpenCrosshairPage();
        public void ExecuteOpenCrosshairSettingsPage() => _navigation.ExecuteOpenCrosshairSettingsPage();
        public void ExecuteOpenDotSettingsPage() => _navigation.ExecuteOpenDotSettingsPage();
        public void ExecuteOpenMiscPage() => _navigation.ExecuteOpenMiscPage();
        public void ExecuteOpenProfilesPage() => _navigation.ExecuteOpenProfilesPage();
        public void ExecuteOpenAvatarSidesPage() => _navigation.ExecuteOpenAvatarSidesPage();
        public void ExecuteOpenScoreboardPage() => _navigation.ExecuteOpenScoreboardPage();
        public void ExecuteBackToTopBar() => _navigation.ExecuteBackToTopBar();
        public void ExecuteBackToHP() => _navigation.ExecuteBackToHP();
        public void ExecuteBackToCrosshair() => _navigation.ExecuteBackToCrosshair();
        public void ExecuteBackToCustomize() => _navigation.ExecuteBackToCustomize();

        [DataSourceProperty] public bool IsKillfeedPageOpen => _navigation.IsKillfeedPageOpen;
        [DataSourceProperty] public bool IsTopBarPageOpen => _navigation.IsTopBarPageOpen;
        [DataSourceProperty] public bool IsVisibilityPageOpen => _navigation.IsVisibilityPageOpen;
        [DataSourceProperty] public bool IsCustomizePageOpen => _navigation.IsCustomizePageOpen;
        [DataSourceProperty] public bool IsChatPageOpen => _navigation.IsChatPageOpen;
        [DataSourceProperty] public bool IsLeaderboardPageOpen => _navigation.IsLeaderboardPageOpen;
        [DataSourceProperty] public bool IsHPPageOpen => _navigation.IsHPPageOpen;
        [DataSourceProperty] public bool IsHPVisibilityPageOpen => _navigation.IsHPVisibilityPageOpen;
        [DataSourceProperty] public bool IsHPCustomizePageOpen => _navigation.IsHPCustomizePageOpen;
        [DataSourceProperty] public bool IsCrosshairPageOpen => _navigation.IsCrosshairPageOpen;
        [DataSourceProperty] public bool IsCrosshairSettingsPageOpen => _navigation.IsCrosshairSettingsPageOpen;
        [DataSourceProperty] public bool IsDotSettingsPageOpen => _navigation.IsDotSettingsPageOpen;
        [DataSourceProperty] public bool IsMiscPageOpen => _navigation.IsMiscPageOpen;
        [DataSourceProperty] public bool IsProfilesPageOpen => _navigation.IsProfilesPageOpen;
        [DataSourceProperty] public bool IsAvatarSidesPageOpen => _navigation.IsAvatarSidesPageOpen;
        [DataSourceProperty] public bool IsScoreboardPageOpen => _navigation.IsScoreboardPageOpen;
        [DataSourceProperty] public bool ShouldShiftMenuRight => _navigation.ShouldShiftMenuRight;
        [DataSourceProperty] public float MenuHorizontalOffset => _navigation.MenuHorizontalOffset;

        #endregion

        #region Killfeed Delegation

        public void ExecuteIncreaseFadeoutTime() => _killfeed.ExecuteIncreaseFadeoutTime();
        public void ExecuteDecreaseFadeoutTime() => _killfeed.ExecuteDecreaseFadeoutTime();
        public void ExecuteIncreaseFadeoutTimeLarge() => _killfeed.ExecuteIncreaseFadeoutTimeLarge();
        public void ExecuteDecreaseFadeoutTimeLarge() => _killfeed.ExecuteDecreaseFadeoutTimeLarge();
        public void ExecuteIncreaseKillfeedOffsetX() => _killfeed.ExecuteIncreaseOffsetX();
        public void ExecuteDecreaseKillfeedOffsetX() => _killfeed.ExecuteDecreaseOffsetX();
        public void ExecuteIncreaseKillfeedOffsetY() => _killfeed.ExecuteIncreaseOffsetY();
        public void ExecuteDecreaseKillfeedOffsetY() => _killfeed.ExecuteDecreaseOffsetY();
        public void ExecuteIncreaseKillfeedOffsetXLarge() => _killfeed.ExecuteIncreaseOffsetXLarge();
        public void ExecuteDecreaseKillfeedOffsetXLarge() => _killfeed.ExecuteDecreaseOffsetXLarge();
        public void ExecuteIncreaseKillfeedOffsetYLarge() => _killfeed.ExecuteIncreaseOffsetYLarge();
        public void ExecuteDecreaseKillfeedOffsetYLarge() => _killfeed.ExecuteDecreaseOffsetYLarge();
        public void ExecuteIncreaseKillfeedScale() => _killfeed.ExecuteIncreaseScale();
        public void ExecuteDecreaseKillfeedScale() => _killfeed.ExecuteDecreaseScale();
        public void ExecuteIncreaseKillfeedScaleLarge() => _killfeed.ExecuteIncreaseScaleLarge();
        public void ExecuteDecreaseKillfeedScaleLarge() => _killfeed.ExecuteDecreaseScaleLarge();
        public void ExecuteIncreaseKillfeedBackgroundOpacity() => _killfeed.ExecuteIncreaseBackgroundOpacity();
        public void ExecuteDecreaseKillfeedBackgroundOpacity() => _killfeed.ExecuteDecreaseBackgroundOpacity();
        public void ExecuteIncreaseKillfeedBackgroundOpacityLarge() => _killfeed.ExecuteIncreaseBackgroundOpacityLarge();
        public void ExecuteDecreaseKillfeedBackgroundOpacityLarge() => _killfeed.ExecuteDecreaseBackgroundOpacityLarge();
        public void ExecuteIncreaseKillfeedMaxEntries() => _killfeed.ExecuteIncreaseMaxEntries();
        public void ExecuteDecreaseKillfeedMaxEntries() => _killfeed.ExecuteDecreaseMaxEntries();
        public void ExecuteIncreaseKillfeedMaxEntriesLarge() => _killfeed.ExecuteIncreaseMaxEntriesLarge();
        public void ExecuteDecreaseKillfeedMaxEntriesLarge() => _killfeed.ExecuteDecreaseMaxEntriesLarge();
        public void ExecuteResetKillfeed() => _killfeed.ExecuteReset();

        [DataSourceProperty] public bool NativeKillfeedEnabled { get => _killfeed.NativeKillfeedEnabled; set => _killfeed.NativeKillfeedEnabled = value; }
        [DataSourceProperty] public bool WarbandKillfeedEnabled { get => _killfeed.WarbandKillfeedEnabled; set => _killfeed.WarbandKillfeedEnabled = value; }
        [DataSourceProperty] public bool KillfeedBackgroundEnabled { get => _killfeed.KillfeedBackgroundEnabled; set => _killfeed.KillfeedBackgroundEnabled = value; }
        [DataSourceProperty] public string KillfeedOffsetXText => _killfeed.OffsetXText;
        [DataSourceProperty] public string KillfeedOffsetYText => _killfeed.OffsetYText;
        [DataSourceProperty] public string KillfeedScaleText => _killfeed.ScaleText;
        [DataSourceProperty] public string KillfeedFadeoutTimeText => _killfeed.FadeoutTimeText;
        [DataSourceProperty] public string KillfeedMaxEntriesText => _killfeed.MaxEntriesText;
        [DataSourceProperty] public string KillfeedBackgroundOpacityText => _killfeed.BackgroundOpacityText;
        [DataSourceProperty] public string KillfeedBackgroundColorText => _killfeed.BackgroundColorText;

        #endregion

        #region TopBar Delegation

        public void ExecuteSelectTimeAndScores() => _topBar.SelectTimeAndScores();
        public void ExecuteSelectTeamAvatars() => _topBar.SelectTeamAvatars();
        public void ExecuteSelectMorale() => _topBar.SelectMorale();
        public void ExecuteSelectPowerLevel() => _topBar.SelectPowerLevel();
        public void ExecuteIncreaseOffsetX() => _topBar.ExecuteIncreaseOffsetX();
        public void ExecuteDecreaseOffsetX() => _topBar.ExecuteDecreaseOffsetX();
        public void ExecuteIncreaseOffsetY() => _topBar.ExecuteIncreaseOffsetY();
        public void ExecuteDecreaseOffsetY() => _topBar.ExecuteDecreaseOffsetY();
        public void ExecuteIncreaseOffsetXLarge() => _topBar.ExecuteIncreaseOffsetXLarge();
        public void ExecuteDecreaseOffsetXLarge() => _topBar.ExecuteDecreaseOffsetXLarge();
        public void ExecuteIncreaseOffsetYLarge() => _topBar.ExecuteIncreaseOffsetYLarge();
        public void ExecuteDecreaseOffsetYLarge() => _topBar.ExecuteDecreaseOffsetYLarge();
        public void ExecuteIncreaseScale() => _topBar.ExecuteIncreaseScale();
        public void ExecuteDecreaseScale() => _topBar.ExecuteDecreaseScale();
        public void ExecuteIncreaseScaleLarge() => _topBar.ExecuteIncreaseScaleLarge();
        public void ExecuteDecreaseScaleLarge() => _topBar.ExecuteDecreaseScaleLarge();
        public void ExecuteResetElement() => _topBar.ExecuteResetElement();
        public void ExecuteResetAllElements() => _topBar.ExecuteResetAll();

        [DataSourceProperty] public int SelectedElementIndex => _topBar.SelectedIndex;
        [DataSourceProperty] public string SelectedElementName => _topBar.SelectedName;
        [DataSourceProperty] public string CurrentOffsetXText => _topBar.OffsetXText;
        [DataSourceProperty] public string CurrentOffsetYText => _topBar.OffsetYText;
        [DataSourceProperty] public string CurrentScaleText => _topBar.ScaleText;

        #endregion

        #region AgentStatus (HP) Delegation

        public void ExecuteSelectAgentHealth() => _agentStatus.SelectAgentHealth();
        public void ExecuteSelectMountHealth() => _agentStatus.SelectMountHealth();
        public void ExecuteSelectShieldHealth() => _agentStatus.SelectShieldHealth();
        public void ExecuteSelectWeaponInfo() => _agentStatus.SelectWeaponInfo();
        public void ExecuteSelectGoldAmount() => _agentStatus.SelectGoldAmount();
        public void ExecuteSelectTroopCount() => _agentStatus.SelectTroopCount();
        public void ExecuteSelectDamageFeed() => _agentStatus.SelectDamageFeed();
        public void ExecuteIncreaseHPOffsetX() => _agentStatus.ExecuteIncreaseOffsetX();
        public void ExecuteDecreaseHPOffsetX() => _agentStatus.ExecuteDecreaseOffsetX();
        public void ExecuteIncreaseHPOffsetY() => _agentStatus.ExecuteIncreaseOffsetY();
        public void ExecuteDecreaseHPOffsetY() => _agentStatus.ExecuteDecreaseOffsetY();
        public void ExecuteIncreaseHPOffsetXLarge() => _agentStatus.ExecuteIncreaseOffsetXLarge();
        public void ExecuteDecreaseHPOffsetXLarge() => _agentStatus.ExecuteDecreaseOffsetXLarge();
        public void ExecuteIncreaseHPOffsetYLarge() => _agentStatus.ExecuteIncreaseOffsetYLarge();
        public void ExecuteDecreaseHPOffsetYLarge() => _agentStatus.ExecuteDecreaseOffsetYLarge();
        public void ExecuteIncreaseHPScale() => _agentStatus.ExecuteIncreaseScale();
        public void ExecuteDecreaseHPScale() => _agentStatus.ExecuteDecreaseScale();
        public void ExecuteIncreaseHPScaleLarge() => _agentStatus.ExecuteIncreaseScaleLarge();
        public void ExecuteDecreaseHPScaleLarge() => _agentStatus.ExecuteDecreaseScaleLarge();
        public void ExecuteResetHPElement() => _agentStatus.ExecuteResetElement();
        public void ExecuteResetAllHPElements() => _agentStatus.ExecuteResetAll();

        [DataSourceProperty] public int SelectedHPElementIndex => _agentStatus.SelectedIndex;
        [DataSourceProperty] public string SelectedHPElementName => _agentStatus.SelectedName;
        [DataSourceProperty] public string CurrentHPOffsetXText => _agentStatus.OffsetXText;
        [DataSourceProperty] public string CurrentHPOffsetYText => _agentStatus.OffsetYText;
        [DataSourceProperty] public string CurrentHPScaleText => _agentStatus.ScaleText;

        #endregion

        #region Visibility Delegation

        [DataSourceProperty] public bool ShowTimeAndScores { get => _visibility.ShowTimeAndScores; set => _visibility.ShowTimeAndScores = value; }
        [DataSourceProperty] public bool ShowAvatars { get => _visibility.ShowAvatars; set => _visibility.ShowAvatars = value; }
        [DataSourceProperty] public bool ShowEnemyScore { get => _visibility.ShowEnemyScore; set => _visibility.ShowEnemyScore = value; }
        [DataSourceProperty] public bool ShowBanners { get => _visibility.ShowBanners; set => _visibility.ShowBanners = value; }
        [DataSourceProperty] public bool ShowMorale { get => _visibility.ShowMorale; set => _visibility.ShowMorale = value; }
        [DataSourceProperty] public bool ShowPowerLevel { get => _visibility.ShowPowerLevel; set => _visibility.ShowPowerLevel = value; }
        [DataSourceProperty] public bool ShowAgentHealth { get => _visibility.ShowAgentHealth; set => _visibility.ShowAgentHealth = value; }
        [DataSourceProperty] public bool ShowMountHealth { get => _visibility.ShowMountHealth; set => _visibility.ShowMountHealth = value; }
        [DataSourceProperty] public bool ShowShieldHealth { get => _visibility.ShowShieldHealth; set => _visibility.ShowShieldHealth = value; }
        [DataSourceProperty] public bool ShowWeaponInfo { get => _visibility.ShowWeaponInfo; set => _visibility.ShowWeaponInfo = value; }
        [DataSourceProperty] public bool ShowAmmoCount { get => _visibility.ShowAmmoCount; set => _visibility.ShowAmmoCount = value; }
        [DataSourceProperty] public bool ShowGoldAmount { get => _visibility.ShowGoldAmount; set => _visibility.ShowGoldAmount = value; }
        [DataSourceProperty] public bool ShowTroopCount { get => _visibility.ShowTroopCount; set => _visibility.ShowTroopCount = value; }
        [DataSourceProperty] public bool ShowCouchLanceState { get => _visibility.ShowCouchLanceState; set => _visibility.ShowCouchLanceState = value; }
        [DataSourceProperty] public bool ShowDamageFeed { get => _visibility.ShowDamageFeed; set => _visibility.ShowDamageFeed = value; }
        [DataSourceProperty] public bool ShowHealthNumbers { get => _visibility.ShowHealthNumbers; set => _visibility.ShowHealthNumbers = value; }
        [DataSourceProperty] public bool ShowMountHealthNumbers { get => _visibility.ShowMountHealthNumbers; set => _visibility.ShowMountHealthNumbers = value; }
        [DataSourceProperty] public bool ShowShieldHealthNumbers { get => _visibility.ShowShieldHealthNumbers; set => _visibility.ShowShieldHealthNumbers = value; }

        #endregion

        #region Crosshair Delegation

        public void ExecuteIncreaseCrosshairWidth() => _crosshair.ExecuteIncreaseWidth();
        public void ExecuteDecreaseCrosshairWidth() => _crosshair.ExecuteDecreaseWidth();
        public void ExecuteIncreaseCrosshairWidthLarge() => _crosshair.ExecuteIncreaseWidthLarge();
        public void ExecuteDecreaseCrosshairWidthLarge() => _crosshair.ExecuteDecreaseWidthLarge();
        public void ExecuteIncreaseCrosshairLength() => _crosshair.ExecuteIncreaseLength();
        public void ExecuteDecreaseCrosshairLength() => _crosshair.ExecuteDecreaseLength();
        public void ExecuteIncreaseCrosshairLengthLarge() => _crosshair.ExecuteIncreaseLengthLarge();
        public void ExecuteDecreaseCrosshairLengthLarge() => _crosshair.ExecuteDecreaseLengthLarge();
        public void ExecuteIncreaseCrosshairOffset() => _crosshair.ExecuteIncreaseOffset();
        public void ExecuteDecreaseCrosshairOffset() => _crosshair.ExecuteDecreaseOffset();
        public void ExecuteIncreaseCrosshairOffsetLarge() => _crosshair.ExecuteIncreaseOffsetLarge();
        public void ExecuteDecreaseCrosshairOffsetLarge() => _crosshair.ExecuteDecreaseOffsetLarge();
        public void ExecuteIncreaseCrosshairOpacity() => _crosshair.ExecuteIncreaseOpacity();
        public void ExecuteDecreaseCrosshairOpacity() => _crosshair.ExecuteDecreaseOpacity();
        public void ExecuteIncreaseCrosshairOpacityLarge() => _crosshair.ExecuteIncreaseOpacityLarge();
        public void ExecuteDecreaseCrosshairOpacityLarge() => _crosshair.ExecuteDecreaseOpacityLarge();
        public void ExecuteSetCrosshairColorRed() => _crosshair.ExecuteSetColorRed();
        public void ExecuteSetCrosshairColorGreen() => _crosshair.ExecuteSetColorGreen();
        public void ExecuteSetCrosshairColorBlue() => _crosshair.ExecuteSetColorBlue();
        public void ExecuteSetCrosshairColorWhite() => _crosshair.ExecuteSetColorWhite();
        public void ExecuteSetCrosshairColorYellow() => _crosshair.ExecuteSetColorYellow();
        public void ExecuteSetCrosshairColorCyan() => _crosshair.ExecuteSetColorCyan();
        public void ExecuteSetCrosshairColorMagenta() => _crosshair.ExecuteSetColorMagenta();
        public void ExecuteSetCrosshairColorOrange() => _crosshair.ExecuteSetColorOrange();
        public void ExecuteResetCrosshair() => _crosshair.ExecuteReset();

        [DataSourceProperty] public bool CustomCrosshairEnabled { get => _crosshair.CustomCrosshairEnabled; set => _crosshair.CustomCrosshairEnabled = value; }
        [DataSourceProperty] public int CrosshairWidthInt { get => _crosshair.WidthInt; set => _crosshair.WidthInt = value; }
        [DataSourceProperty] public int CrosshairLengthInt { get => _crosshair.LengthInt; set => _crosshair.LengthInt = value; }
        [DataSourceProperty] public int CrosshairOffsetInt { get => _crosshair.OffsetInt; set => _crosshair.OffsetInt = value; }
        [DataSourceProperty] public float CrosshairOpacityFloat { get => _crosshair.OpacityFloat; set => _crosshair.OpacityFloat = value; }
        [DataSourceProperty] public string CrosshairWidthText => _crosshair.WidthInt.ToString();
        [DataSourceProperty] public string CrosshairLengthText => _crosshair.LengthInt.ToString();
        [DataSourceProperty] public string CrosshairOffsetText => _crosshair.OffsetInt.ToString();
        [DataSourceProperty] public string CrosshairOpacityText => (_crosshair.OpacityFloat * 100).ToString("F0") + "%";
        [DataSourceProperty] public string CrosshairColorText => _crosshair.ColorText;
        [DataSourceProperty] public MBBindingList<ColorItemVM> CrosshairColorList => _crosshair.ColorList;

        #endregion

        #region Dot Delegation

        public void ExecuteIncreaseDotWidth() => _dot.ExecuteIncreaseWidth();
        public void ExecuteDecreaseDotWidth() => _dot.ExecuteDecreaseWidth();
        public void ExecuteIncreaseDotWidthLarge() => _dot.ExecuteIncreaseWidthLarge();
        public void ExecuteDecreaseDotWidthLarge() => _dot.ExecuteDecreaseWidthLarge();
        public void ExecuteIncreaseDotHeight() => _dot.ExecuteIncreaseHeight();
        public void ExecuteDecreaseDotHeight() => _dot.ExecuteDecreaseHeight();
        public void ExecuteIncreaseDotHeightLarge() => _dot.ExecuteIncreaseHeightLarge();
        public void ExecuteDecreaseDotHeightLarge() => _dot.ExecuteDecreaseHeightLarge();
        public void ExecuteIncreaseDotOffsetX() => _dot.ExecuteIncreaseOffsetX();
        public void ExecuteDecreaseDotOffsetX() => _dot.ExecuteDecreaseOffsetX();
        public void ExecuteIncreaseDotOffsetXLarge() => _dot.ExecuteIncreaseOffsetXLarge();
        public void ExecuteDecreaseDotOffsetXLarge() => _dot.ExecuteDecreaseOffsetXLarge();
        public void ExecuteIncreaseDotOffsetY() => _dot.ExecuteIncreaseOffsetY();
        public void ExecuteDecreaseDotOffsetY() => _dot.ExecuteDecreaseOffsetY();
        public void ExecuteIncreaseDotOffsetYLarge() => _dot.ExecuteIncreaseOffsetYLarge();
        public void ExecuteDecreaseDotOffsetYLarge() => _dot.ExecuteDecreaseOffsetYLarge();
        public void ExecuteSetDotColorRed() => _dot.ExecuteSetColorRed();
        public void ExecuteSetDotColorGreen() => _dot.ExecuteSetColorGreen();
        public void ExecuteSetDotColorBlue() => _dot.ExecuteSetColorBlue();
        public void ExecuteSetDotColorWhite() => _dot.ExecuteSetColorWhite();
        public void ExecuteSetDotColorYellow() => _dot.ExecuteSetColorYellow();
        public void ExecuteSetDotColorCyan() => _dot.ExecuteSetColorCyan();
        public void ExecuteSetDotColorMagenta() => _dot.ExecuteSetColorMagenta();
        public void ExecuteSetDotColorOrange() => _dot.ExecuteSetColorOrange();
        public void ExecuteSetDotCircular() => _dot.ExecuteSetCircular();
        public void ExecuteSetDotSquare() => _dot.ExecuteSetSquare();
        public void ExecuteResetDot() => _dot.ExecuteReset();

        [DataSourceProperty] public bool DotEnabled { get => _dot.DotEnabled; set => _dot.DotEnabled = value; }
        [DataSourceProperty] public int DotWidthInt { get => _dot.WidthInt; set => _dot.WidthInt = value; }
        [DataSourceProperty] public int DotHeightInt { get => _dot.HeightInt; set => _dot.HeightInt = value; }
        [DataSourceProperty] public string DotWidthText => _dot.WidthText;
        [DataSourceProperty] public string DotHeightText => _dot.HeightText;
        [DataSourceProperty] public string DotColorText => _dot.ColorText;
        [DataSourceProperty] public string DotOffsetXText => _dot.OffsetXText;
        [DataSourceProperty] public string DotOffsetYText => _dot.OffsetYText;
        [DataSourceProperty] public MBBindingList<ColorItemVM> DotColorList => _dot.ColorList;

        #endregion

        #region Avatar Delegation

        public void ExecuteSelectAllyAvatars() => _avatar.SelectAlly();
        public void ExecuteSelectEnemyAvatars() => _avatar.SelectEnemy();
        public void ExecuteIncreaseAvatarOffsetX() => _avatar.ExecuteIncreaseOffsetX();
        public void ExecuteDecreaseAvatarOffsetX() => _avatar.ExecuteDecreaseOffsetX();
        public void ExecuteIncreaseAvatarOffsetY() => _avatar.ExecuteIncreaseOffsetY();
        public void ExecuteDecreaseAvatarOffsetY() => _avatar.ExecuteDecreaseOffsetY();
        public void ExecuteIncreaseAvatarOffsetXLarge() => _avatar.ExecuteIncreaseOffsetXLarge();
        public void ExecuteDecreaseAvatarOffsetXLarge() => _avatar.ExecuteDecreaseOffsetXLarge();
        public void ExecuteIncreaseAvatarOffsetYLarge() => _avatar.ExecuteIncreaseOffsetYLarge();
        public void ExecuteDecreaseAvatarOffsetYLarge() => _avatar.ExecuteDecreaseOffsetYLarge();
        public void ExecuteIncreaseAvatarScale() => _avatar.ExecuteIncreaseScale();
        public void ExecuteDecreaseAvatarScale() => _avatar.ExecuteDecreaseScale();
        public void ExecuteIncreaseAvatarScaleLarge() => _avatar.ExecuteIncreaseScaleLarge();
        public void ExecuteDecreaseAvatarScaleLarge() => _avatar.ExecuteDecreaseScaleLarge();
        public void ExecuteIncreaseAvatarSpacing() => _avatar.ExecuteIncreaseSpacing();
        public void ExecuteDecreaseAvatarSpacing() => _avatar.ExecuteDecreaseSpacing();
        public void ExecuteIncreaseAvatarSpacingLarge() => _avatar.ExecuteIncreaseSpacingLarge();
        public void ExecuteDecreaseAvatarSpacingLarge() => _avatar.ExecuteDecreaseSpacingLarge();
        public void ExecuteToggleAvatarSideOrientation() => _avatar.ExecuteToggleOrientation();
        public void ExecuteResetAvatarSide() => _avatar.ExecuteResetCurrent();
        public void ExecuteResetAllAvatarSides() => _avatar.ExecuteResetAll();
        public void ExecuteCleanupAvatars() => _avatar.ExecuteCleanupAvatars();

        [DataSourceProperty] public string SelectedAvatarSideName => _avatar.SelectedName;
        [DataSourceProperty] public string CurrentAvatarOffsetXText => _avatar.OffsetXText;
        [DataSourceProperty] public string CurrentAvatarOffsetYText => _avatar.OffsetYText;
        [DataSourceProperty] public string CurrentAvatarScaleText => _avatar.ScaleText;
        [DataSourceProperty] public string CurrentAvatarOrientationText => _avatar.OrientationText;
        [DataSourceProperty] public string CurrentAvatarSpacingText => _avatar.SpacingText;
        [DataSourceProperty] public bool IsAllyAvatarSelected => _avatar.IsAllySelected;
        [DataSourceProperty] public bool IsEnemyAvatarSelected => _avatar.IsEnemySelected;
        [DataSourceProperty] public bool BetterAvatarsEnabled { get => _avatar.BetterAvatarsEnabled; set => _avatar.BetterAvatarsEnabled = value; }
        [DataSourceProperty] public bool AvatarSortingEnabled { get => _avatar.AvatarSortingEnabled; set => _avatar.AvatarSortingEnabled = value; }

        #endregion

        #region Scoreboard Delegation

        public void ExecuteIncreaseScoreboardOpacity() => _scoreboard.ExecuteIncreaseBackgroundOpacity();
        public void ExecuteDecreaseScoreboardOpacity() => _scoreboard.ExecuteDecreaseBackgroundOpacity();
        public void ExecuteIncreaseScoreboardOpacityLarge() => _scoreboard.ExecuteIncreaseBackgroundOpacityLarge();
        public void ExecuteDecreaseScoreboardOpacityLarge() => _scoreboard.ExecuteDecreaseBackgroundOpacityLarge();
        public void ExecuteIncreaseStripingOpacity() => _scoreboard.ExecuteIncreaseStripingOpacity();
        public void ExecuteDecreaseStripingOpacity() => _scoreboard.ExecuteDecreaseStripingOpacity();
        public void ExecuteIncreaseStripingOpacityLarge() => _scoreboard.ExecuteIncreaseStripingOpacityLarge();
        public void ExecuteDecreaseStripingOpacityLarge() => _scoreboard.ExecuteDecreaseStripingOpacityLarge();
        public void ExecuteIncreaseDeadPlayerOpacity() => _scoreboard.ExecuteIncreaseDeadPlayerOpacity();
        public void ExecuteDecreaseDeadPlayerOpacity() => _scoreboard.ExecuteDecreaseDeadPlayerOpacity();
        public void ExecuteIncreaseDeadPlayerOpacityLarge() => _scoreboard.ExecuteIncreaseDeadPlayerOpacityLarge();
        public void ExecuteDecreaseDeadPlayerOpacityLarge() => _scoreboard.ExecuteDecreaseDeadPlayerOpacityLarge();
        public void ExecuteSetDeadPlayerColorRed() => _scoreboard.ExecuteSetDeadPlayerColorRed();
        public void ExecuteSetDeadPlayerColorGray() => _scoreboard.ExecuteSetDeadPlayerColorGray();
        public void ExecuteSetDeadPlayerColorDark() => _scoreboard.ExecuteSetDeadPlayerColorDark();
        public void ExecuteSetDeadPlayerColorWhite() => _scoreboard.ExecuteSetDeadPlayerColorWhite();
        public void ExecuteSetDeadPlayerColorOrange() => _scoreboard.ExecuteSetDeadPlayerColorOrange();
        public void ExecuteSetDeadPlayerColorPurple() => _scoreboard.ExecuteSetDeadPlayerColorPurple();
        public void ExecuteDebugScoreboard() => _scoreboard.ExecuteDebugStructure();
        public void ExecuteResetScoreboard() => _scoreboard.ExecuteReset();

        [DataSourceProperty] public bool ScoreboardBackgroundEnabled { get => _scoreboard.BackgroundEnabled; set => _scoreboard.BackgroundEnabled = value; }
        [DataSourceProperty] public bool ScoreboardStripingEnabled { get => _scoreboard.StripingEnabled; set => _scoreboard.StripingEnabled = value; }
        [DataSourceProperty] public bool ScoreboardDeadPlayerTintEnabled { get => _scoreboard.DeadPlayerTintEnabled; set => _scoreboard.DeadPlayerTintEnabled = value; }
        [DataSourceProperty] public bool HideUIWhenScoreboardOpen { get => _scoreboard.HideUIWhenOpen; set => _scoreboard.HideUIWhenOpen = value; }
        [DataSourceProperty] public string ScoreboardBackgroundOpacityText => _scoreboard.BackgroundOpacityText;
        [DataSourceProperty] public string ScoreboardStripingOpacityText => _scoreboard.StripingOpacityText;
        [DataSourceProperty] public string ScoreboardDeadPlayerOpacityText => _scoreboard.DeadPlayerOpacityText;

        #endregion

        #region Chat Delegation

        public void ExecuteIncreaseChatOffsetX() => _chat.ExecuteIncreaseOffsetX();
        public void ExecuteDecreaseChatOffsetX() => _chat.ExecuteDecreaseOffsetX();
        public void ExecuteIncreaseChatOffsetY() => _chat.ExecuteIncreaseOffsetY();
        public void ExecuteDecreaseChatOffsetY() => _chat.ExecuteDecreaseOffsetY();
        public void ExecuteIncreaseChatOffsetXLarge() => _chat.ExecuteIncreaseOffsetXLarge();
        public void ExecuteDecreaseChatOffsetXLarge() => _chat.ExecuteDecreaseOffsetXLarge();
        public void ExecuteIncreaseChatOffsetYLarge() => _chat.ExecuteIncreaseOffsetYLarge();
        public void ExecuteDecreaseChatOffsetYLarge() => _chat.ExecuteDecreaseOffsetYLarge();
        public void ExecuteIncreaseChatScale() => _chat.ExecuteIncreaseScale();
        public void ExecuteDecreaseChatScale() => _chat.ExecuteDecreaseScale();
        public void ExecuteIncreaseChatScaleLarge() => _chat.ExecuteIncreaseScaleLarge();
        public void ExecuteDecreaseChatScaleLarge() => _chat.ExecuteDecreaseScaleLarge();
        public void ExecuteDebugChat() => _chat.ExecuteDebugStructure();
        public void ExecuteResetChat() => _chat.ExecuteReset();

        [DataSourceProperty] public bool ShowChat { get => _chat.ShowChat; set => _chat.ShowChat = value; }
        [DataSourceProperty] public bool ChatMinimalMode { get => _chat.MinimalMode; set => _chat.MinimalMode = value; }
        [DataSourceProperty] public string ChatOffsetXText => _chat.OffsetXText;
        [DataSourceProperty] public string ChatOffsetYText => _chat.OffsetYText;
        [DataSourceProperty] public string ChatScaleText => _chat.ScaleText;

        #endregion

        #region Profile Delegation

        public void ExecuteNextProfile() => _profileManager.ExecuteNextProfile();
        public void ExecutePreviousProfile() => _profileManager.ExecutePreviousProfile();
        public void ExecuteCreateProfile() => _profileManager.ExecuteCreateProfile();
        public void ExecuteDuplicateProfile() => _profileManager.ExecuteDuplicateProfile();
        public void ExecuteDeleteProfile() => _profileManager.ExecuteDeleteProfile();
        public void ExecuteSaveProfileName() => _profileManager.ExecuteSaveProfileName();

        [DataSourceProperty] public string CurrentProfileName { get => _profileManager.CurrentProfileName; set => _profileManager.CurrentProfileName = value; }
        [DataSourceProperty] public bool IsSaveNameButtonVisible => _profileManager.IsSaveNameButtonVisible;
        [DataSourceProperty] public string ProfileCountText => _profileManager.ProfileCountText;
        [DataSourceProperty] public bool CanDeleteProfile => _profileManager.CanDeleteProfile;

        #endregion

        #region Misc Delegation

        [DataSourceProperty] 
        public bool CameraSnapbackEnabled 
        { 
            get => _misc.CameraSnapbackEnabled; 
            set => _misc.CameraSnapbackEnabled = value; 
        }

        #endregion
    }
}