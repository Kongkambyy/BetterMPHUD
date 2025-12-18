using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BetterMPHUD.ViewModels
{
    public class HudMenuVM : ViewModel
    {
        private bool _isConfigMenuOpen;
        
        // menu navigering
        private bool _isKillfeedPageOpen;
        private bool _isTopBarPageOpen;
        private bool _isVisibilityPageOpen;
        private bool _isCustomizePageOpen;
        private bool _isChatPageOpen;
        private bool _isLeaderboardPageOpen;
        private bool _isHPPageOpen;
        private bool _isCrosshairPageOpen;
        
        private bool _nativeKillfeedEnabled;
        private bool _warbandKillfeedEnabled;

        private bool _showTimeAndScores;
        private bool _showAvatars;
        private bool _showEnemyScore;
        private bool _showBanners;
        private bool _showMorale;

        // top bar indstillinger
        private int _selectedElementIndex = 0;
        private string _selectedElementName = "Time & Scores";
        private float _currentOffsetX;
        private float _currentOffsetY;
        private float _currentScale;

        // persistence
        private HudSettings _settings;

        private const float POSITION_STEP = 10f;
        private const float SCALE_STEP = 0.1f;
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 2.0f;

        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        public Action OnHudSettingsChanged;

        public HudMenuVM()
        {
            // Load saved settings
            _settings = ConfigManager.LoadSettings();
            
            // synlighedsindstillinger
            _nativeKillfeedEnabled = _settings.NativeKillfeedEnabled;
            _warbandKillfeedEnabled = _settings.WarbandKillfeedEnabled;
            _showTimeAndScores = _settings.ShowTimeAndScores;
            _showAvatars = _settings.ShowAvatars;
            _showEnemyScore = _settings.ShowEnemyScore;
            _showBanners = _settings.ShowBanners;
            _showMorale = _settings.ShowMorale;
            
            // navigering
            _isKillfeedPageOpen = true; 
            _isTopBarPageOpen = false;
            _isVisibilityPageOpen = false;
            _isCustomizePageOpen = false;
            _isChatPageOpen = false;
            _isLeaderboardPageOpen = false;
            _isHPPageOpen = false;
            _isCrosshairPageOpen = false;

            LoadSelectedElementValues();

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
            IsCrosshairPageOpen = false;
        }

        public void ExecuteClose()
        {
            OnCloseConfigMenu?.Invoke();
        }

        public void ExecuteOpenKillfeedPage()
        {
            CloseAllPages();
            IsKillfeedPageOpen = true;
        }

        public void ExecuteOpenTopBarPage()
        {
            CloseAllPages();
            IsTopBarPageOpen = true;
        }

        public void ExecuteOpenVisibilityPage()
        {
            CloseAllPages();
            IsVisibilityPageOpen = true;
        }

        public void ExecuteOpenCustomizePage()
        {
            CloseAllPages();
            IsCustomizePageOpen = true;
            LoadSelectedElementValues();
        }

        public void ExecuteOpenChatPage()
        {
            CloseAllPages();
            IsChatPageOpen = true;
        }

        public void ExecuteOpenLeaderboardPage()
        {
            CloseAllPages();
            IsLeaderboardPageOpen = true;
        }

        public void ExecuteOpenHPPage()
        {
            CloseAllPages();
            IsHPPageOpen = true;
        }

        public void ExecuteOpenCrosshairPage()
        {
            CloseAllPages();
            IsCrosshairPageOpen = true;
        }

        public void ExecuteBackToTopBar()
        {
            ExecuteOpenTopBarPage();
        }

        // ==================== ELEMENT SELECTION COMMANDS (Top Bar) ====================

        public void ExecuteSelectTimeAndScores()
        {
            SelectedElementIndex = 0;
            SelectedElementName = "Time & Scores";
            LoadSelectedElementValues();
        }

        public void ExecuteSelectTeamAvatars()
        {
            SelectedElementIndex = 1;
            SelectedElementName = "Team Avatars";
            LoadSelectedElementValues();
        }

        public void ExecuteSelectMorale()
        {
            SelectedElementIndex = 2;
            SelectedElementName = "Morale";
            LoadSelectedElementValues();
        }

        // ==================== TOP BAR CUSTOMIZATION COMMANDS ====================

        public void ExecuteIncreaseOffsetX()
        {
            CurrentOffsetX += POSITION_STEP;
            SaveSelectedElementValues();
        }

        public void ExecuteDecreaseOffsetX()
        {
            CurrentOffsetX -= POSITION_STEP;
            SaveSelectedElementValues();
        }

        public void ExecuteIncreaseOffsetY()
        {
            CurrentOffsetY += POSITION_STEP;
            SaveSelectedElementValues();
        }

        public void ExecuteDecreaseOffsetY()
        {
            CurrentOffsetY -= POSITION_STEP;
            SaveSelectedElementValues();
        }

        public void ExecuteIncreaseScale()
        {
            float newScale = CurrentScale + SCALE_STEP;
            if (newScale <= MAX_SCALE)
            {
                CurrentScale = (float)Math.Round(newScale, 1);
                SaveSelectedElementValues();
            }
        }

        public void ExecuteDecreaseScale()
        {
            float newScale = CurrentScale - SCALE_STEP;
            if (newScale >= MIN_SCALE)
            {
                CurrentScale = (float)Math.Round(newScale, 1);
                SaveSelectedElementValues();
            }
        }

        public void ExecuteResetElement()
        {
            CurrentOffsetX = 0f;
            CurrentOffsetY = 0f;
            CurrentScale = 1f;
            SaveSelectedElementValues();
        }

        public void ExecuteResetAllElements()
        {
            _settings.TimeAndScoresCustom.Reset();
            _settings.TeamAvatarsCustom.Reset();
            _settings.MoraleCustom.Reset();
            
            LoadSelectedElementValues();
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        // ==================== KILLFEED CUSTOMIZATION COMMANDS ====================

        public void ExecuteIncreaseKillfeedOffsetX()
        {
            _settings.KillfeedCustom.OffsetX += POSITION_STEP;
            OnPropertyChangedWithValue(KillfeedOffsetXText, "KillfeedOffsetXText");
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        public void ExecuteDecreaseKillfeedOffsetX()
        {
            _settings.KillfeedCustom.OffsetX -= POSITION_STEP;
            OnPropertyChangedWithValue(KillfeedOffsetXText, "KillfeedOffsetXText");
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        public void ExecuteIncreaseKillfeedOffsetY()
        {
            _settings.KillfeedCustom.OffsetY += POSITION_STEP;
            OnPropertyChangedWithValue(KillfeedOffsetYText, "KillfeedOffsetYText");
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        public void ExecuteDecreaseKillfeedOffsetY()
        {
            _settings.KillfeedCustom.OffsetY -= POSITION_STEP;
            OnPropertyChangedWithValue(KillfeedOffsetYText, "KillfeedOffsetYText");
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        public void ExecuteIncreaseKillfeedScale()
        {
            float newScale = _settings.KillfeedCustom.Scale + SCALE_STEP;
            if (newScale <= MAX_SCALE)
            {
                _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 1);
                OnPropertyChangedWithValue(KillfeedScaleText, "KillfeedScaleText");
                SaveCurrentSettings();
                NotifySettingsChanged();
            }
        }

        public void ExecuteDecreaseKillfeedScale()
        {
            float newScale = _settings.KillfeedCustom.Scale - SCALE_STEP;
            if (newScale >= MIN_SCALE)
            {
                _settings.KillfeedCustom.Scale = (float)Math.Round(newScale, 1);
                OnPropertyChangedWithValue(KillfeedScaleText, "KillfeedScaleText");
                SaveCurrentSettings();
                NotifySettingsChanged();
            }
        }

        public void ExecuteResetKillfeed()
        {
            _settings.KillfeedCustom.Reset();
            OnPropertyChangedWithValue(KillfeedOffsetXText, "KillfeedOffsetXText");
            OnPropertyChangedWithValue(KillfeedOffsetYText, "KillfeedOffsetYText");
            OnPropertyChangedWithValue(KillfeedScaleText, "KillfeedScaleText");
            SaveCurrentSettings();
            NotifySettingsChanged();
        }

        // ==================== HELPER METHODS ====================
        
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

        private ElementCustomization GetCurrentElementCustomization()
        {
            return _settings.GetCustomization(GetSelectedElement());
        }

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

        private void ApplyNativeKillfeedSetting()
        {
            if (_nativeKillfeedEnabled)
                ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 0f);
            else
                ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 2f);
        }

        private void NotifySettingsChanged()
        {
            OnHudSettingsChanged?.Invoke();
        }

        private void SaveCurrentSettings()
        {
            _settings.NativeKillfeedEnabled = _nativeKillfeedEnabled;
            _settings.WarbandKillfeedEnabled = _warbandKillfeedEnabled;
            _settings.ShowTimeAndScores = _showTimeAndScores;
            _settings.ShowAvatars = _showAvatars;
            _settings.ShowEnemyScore = _showEnemyScore;
            _settings.ShowBanners = _showBanners;
            _settings.ShowMorale = _showMorale;
            
            ConfigManager.SaveSettings(_settings);
        }

        public HudSettings GetSettings()
        {
            return _settings;
        }

        // ==================== DATA SOURCE PROPERTIES ====================
        
        [DataSourceProperty]
        public bool IsConfigMenuOpen
        {
            get => _isConfigMenuOpen;
            set
            {
                if (value != _isConfigMenuOpen)
                {
                    _isConfigMenuOpen = value;
                    OnPropertyChangedWithValue(value, "IsConfigMenuOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsKillfeedPageOpen
        {
            get => _isKillfeedPageOpen;
            set
            {
                if (value != _isKillfeedPageOpen)
                {
                    _isKillfeedPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsKillfeedPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsTopBarPageOpen
        {
            get => _isTopBarPageOpen;
            set
            {
                if (value != _isTopBarPageOpen)
                {
                    _isTopBarPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsTopBarPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsVisibilityPageOpen
        {
            get => _isVisibilityPageOpen;
            set
            {
                if (value != _isVisibilityPageOpen)
                {
                    _isVisibilityPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsVisibilityPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsCustomizePageOpen
        {
            get => _isCustomizePageOpen;
            set
            {
                if (value != _isCustomizePageOpen)
                {
                    _isCustomizePageOpen = value;
                    OnPropertyChangedWithValue(value, "IsCustomizePageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsChatPageOpen
        {
            get => _isChatPageOpen;
            set
            {
                if (value != _isChatPageOpen)
                {
                    _isChatPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsChatPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsLeaderboardPageOpen
        {
            get => _isLeaderboardPageOpen;
            set
            {
                if (value != _isLeaderboardPageOpen)
                {
                    _isLeaderboardPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsLeaderboardPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsHPPageOpen
        {
            get => _isHPPageOpen;
            set
            {
                if (value != _isHPPageOpen)
                {
                    _isHPPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsHPPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool IsCrosshairPageOpen
        {
            get => _isCrosshairPageOpen;
            set
            {
                if (value != _isCrosshairPageOpen)
                {
                    _isCrosshairPageOpen = value;
                    OnPropertyChangedWithValue(value, "IsCrosshairPageOpen");
                }
            }
        }

        [DataSourceProperty]
        public bool NativeKillfeedEnabled
        {
            get => _nativeKillfeedEnabled;
            set
            {
                if (value != _nativeKillfeedEnabled)
                {
                    _nativeKillfeedEnabled = value;
                    OnPropertyChangedWithValue(value, "NativeKillfeedEnabled");
                    ApplyNativeKillfeedSetting();
                    SaveCurrentSettings();
                }
            }
        }

        [DataSourceProperty]
        public bool WarbandKillfeedEnabled
        {
            get => _warbandKillfeedEnabled;
            set
            {
                if (value != _warbandKillfeedEnabled)
                {
                    _warbandKillfeedEnabled = value;
                    OnPropertyChangedWithValue(value, "WarbandKillfeedEnabled");
                    OnWarbandKillfeedToggled?.Invoke(value);
                    SaveCurrentSettings();
                }
            }
        }
        
        [DataSourceProperty]
        public bool ShowTimeAndScores
        {
            get => _showTimeAndScores;
            set
            {
                if (value != _showTimeAndScores)
                {
                    _showTimeAndScores = value;
                    OnPropertyChangedWithValue(value, "ShowTimeAndScores");
                    NotifySettingsChanged();
                    SaveCurrentSettings();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowAvatars
        {
            get => _showAvatars;
            set
            {
                if (value != _showAvatars)
                {
                    _showAvatars = value;
                    OnPropertyChangedWithValue(value, "ShowAvatars");
                    NotifySettingsChanged();
                    SaveCurrentSettings();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowEnemyScore
        {
            get => _showEnemyScore;
            set
            {
                if (value != _showEnemyScore)
                {
                    _showEnemyScore = value;
                    OnPropertyChangedWithValue(value, "ShowEnemyScore");
                    NotifySettingsChanged();
                    SaveCurrentSettings();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowBanners
        {
            get => _showBanners;
            set
            {
                if (value != _showBanners)
                {
                    _showBanners = value;
                    OnPropertyChangedWithValue(value, "ShowBanners");
                    NotifySettingsChanged();
                    SaveCurrentSettings();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowMorale
        {
            get => _showMorale;
            set
            {
                if (value != _showMorale)
                {
                    _showMorale = value;
                    OnPropertyChangedWithValue(value, "ShowMorale");
                    NotifySettingsChanged();
                    SaveCurrentSettings();
                }
            }
        }
        
        [DataSourceProperty]
        public int SelectedElementIndex
        {
            get => _selectedElementIndex;
            set
            {
                if (value != _selectedElementIndex)
                {
                    _selectedElementIndex = value;
                    OnPropertyChangedWithValue(value, "SelectedElementIndex");
                }
            }
        }

        [DataSourceProperty]
        public string SelectedElementName
        {
            get => _selectedElementName;
            set
            {
                if (value != _selectedElementName)
                {
                    _selectedElementName = value;
                    OnPropertyChangedWithValue(value, "SelectedElementName");
                }
            }
        }

        [DataSourceProperty]
        public float CurrentOffsetX
        {
            get => _currentOffsetX;
            set
            {
                if (value != _currentOffsetX)
                {
                    _currentOffsetX = value;
                    OnPropertyChangedWithValue(value, "CurrentOffsetX");
                    OnPropertyChangedWithValue(CurrentOffsetXText, "CurrentOffsetXText");
                }
            }
        }

        [DataSourceProperty]
        public float CurrentOffsetY
        {
            get => _currentOffsetY;
            set
            {
                if (value != _currentOffsetY)
                {
                    _currentOffsetY = value;
                    OnPropertyChangedWithValue(value, "CurrentOffsetY");
                    OnPropertyChangedWithValue(CurrentOffsetYText, "CurrentOffsetYText");
                }
            }
        }

        [DataSourceProperty]
        public float CurrentScale
        {
            get => _currentScale;
            set
            {
                if (value != _currentScale)
                {
                    _currentScale = value;
                    OnPropertyChangedWithValue(value, "CurrentScale");
                    OnPropertyChangedWithValue(CurrentScaleText, "CurrentScaleText");
                }
            }
        }

        [DataSourceProperty]
        public string CurrentOffsetXText => _currentOffsetX.ToString("F0");

        [DataSourceProperty]
        public string CurrentOffsetYText => _currentOffsetY.ToString("F0");

        [DataSourceProperty]
        public string CurrentScaleText => (_currentScale * 100).ToString("F0") + "%";

        // Killfeed-specific display properties
        [DataSourceProperty]
        public string KillfeedOffsetXText => _settings.KillfeedCustom.OffsetX.ToString("F0");

        [DataSourceProperty]
        public string KillfeedOffsetYText => _settings.KillfeedCustom.OffsetY.ToString("F0");

        [DataSourceProperty]
        public string KillfeedScaleText => (_settings.KillfeedCustom.Scale * 100).ToString("F0") + "%";
    }
}