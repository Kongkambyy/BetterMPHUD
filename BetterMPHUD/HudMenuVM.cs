using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BetterMPHUD.ViewModels
{
    public class HudMenuVM : ViewModel
    {
        private bool _isConfigMenuOpen;
        
        // Menu Navigation State
        private bool _isKillfeedPageOpen;
        private bool _isTopBarPageOpen;

        // Killfeed Settings
        private bool _nativeKillfeedEnabled;
        private bool _warbandKillfeedEnabled;

        // Top Bar Settings (Default True = Visible)
        private bool _showTimeAndScores;
        private bool _showAvatars;
        private bool _showEnemyScore;
        private bool _showBanners;
        private bool _showMorale;

        // Settings object for persistence
        private HudSettings _settings;

        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        
        // Event to trigger HUD updates in Behavior
        public Action OnHudSettingsChanged;

        public HudMenuVM()
        {
            // Load saved settings (or defaults if no config exists)
            _settings = ConfigManager.LoadSettings();
            
            // Apply loaded settings to private fields (don't use properties to avoid triggering saves)
            _nativeKillfeedEnabled = _settings.NativeKillfeedEnabled;
            _warbandKillfeedEnabled = _settings.WarbandKillfeedEnabled;
            _showTimeAndScores = _settings.ShowTimeAndScores;
            _showAvatars = _settings.ShowAvatars;
            _showEnemyScore = _settings.ShowEnemyScore;
            _showBanners = _settings.ShowBanners;
            _showMorale = _settings.ShowMorale;
            
            // Default to Killfeed page open
            _isKillfeedPageOpen = true; 
            _isTopBarPageOpen = false;

            // Apply native killfeed setting immediately
            ApplyNativeKillfeedSetting();
        }

        public void ExecuteClose()
        {
            OnCloseConfigMenu?.Invoke();
        }

        // --- Navigation Commands ---
        public void ExecuteOpenKillfeedPage()
        {
            IsKillfeedPageOpen = true;
            IsTopBarPageOpen = false;
        }

        public void ExecuteOpenTopBarPage()
        {
            IsKillfeedPageOpen = false;
            IsTopBarPageOpen = true;
        }

        // --- Settings Logic ---
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

        /// <summary>
        /// Saves current settings to the config file
        /// </summary>
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

        // --- Properties ---

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

        // --- Top Bar Properties ---

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
    }
}