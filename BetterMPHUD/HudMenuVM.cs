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
        private bool _showAvatars = true;
        private bool _showEnemyScore = true;
        private bool _showBanners = true;
        private bool _showMorale = true;

        public Action OnCloseConfigMenu;
        public Action<bool> OnWarbandKillfeedToggled;
        
        // Event to trigger HUD updates in Behavior
        public Action OnHudSettingsChanged;

        public HudMenuVM()
        {
            _nativeKillfeedEnabled = false;
            _warbandKillfeedEnabled = true;
            
            // Default to Killfeed page open
            _isKillfeedPageOpen = true; 
            _isTopBarPageOpen = false;

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
                }
            }
        }

        // --- Top Bar Properties ---

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
                }
            }
        }
    }
}