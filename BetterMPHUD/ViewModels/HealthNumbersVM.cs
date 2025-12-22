using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class HealthNumbersVM : ViewModel
    {
        private string _playerHealth = "";
        private string _mountHealth = "";
        private string _shieldHealth = "";
        
        private bool _showPlayerHealth = true;
        private bool _showMountHealth = true;
        private bool _showShieldHealth = true;
        
        private float _playerHealthOffsetX = 0f;
        private float _playerHealthOffsetY = 0f;
        private float _mountHealthOffsetX = 0f;
        private float _mountHealthOffsetY = 0f;
        private float _shieldHealthOffsetX = 0f;
        private float _shieldHealthOffsetY = 0f;

        [DataSourceProperty]
        public string PlayerHealth
        {
            get { return _playerHealth; }
            set
            {
                if (_playerHealth != value)
                {
                    _playerHealth = value;
                    OnPropertyChangedWithValue(value, "PlayerHealth");
                }
            }
        }

        [DataSourceProperty]
        public string MountHealth
        {
            get { return _mountHealth; }
            set
            {
                if (_mountHealth != value)
                {
                    _mountHealth = value;
                    OnPropertyChangedWithValue(value, "MountHealth");
                }
            }
        }

        [DataSourceProperty]
        public string ShieldHealth
        {
            get { return _shieldHealth; }
            set
            {
                if (_shieldHealth != value)
                {
                    _shieldHealth = value;
                    OnPropertyChangedWithValue(value, "ShieldHealth");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowPlayerHealth
        {
            get { return _showPlayerHealth; }
            set
            {
                if (_showPlayerHealth != value)
                {
                    _showPlayerHealth = value;
                    OnPropertyChangedWithValue(value, "ShowPlayerHealth");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowMountHealth
        {
            get { return _showMountHealth; }
            set
            {
                if (_showMountHealth != value)
                {
                    _showMountHealth = value;
                    OnPropertyChangedWithValue(value, "ShowMountHealth");
                }
            }
        }

        [DataSourceProperty]
        public bool ShowShieldHealth
        {
            get { return _showShieldHealth; }
            set
            {
                if (_showShieldHealth != value)
                {
                    _showShieldHealth = value;
                    OnPropertyChangedWithValue(value, "ShowShieldHealth");
                }
            }
        }

        [DataSourceProperty]
        public float PlayerHealthOffsetX
        {
            get { return _playerHealthOffsetX; }
            set
            {
                if (_playerHealthOffsetX != value)
                {
                    _playerHealthOffsetX = value;
                    OnPropertyChangedWithValue(value, "PlayerHealthOffsetX");
                }
            }
        }

        [DataSourceProperty]
        public float PlayerHealthOffsetY
        {
            get { return _playerHealthOffsetY; }
            set
            {
                if (_playerHealthOffsetY != value)
                {
                    _playerHealthOffsetY = value;
                    OnPropertyChangedWithValue(value, "PlayerHealthOffsetY");
                }
            }
        }

        [DataSourceProperty]
        public float MountHealthOffsetX
        {
            get { return _mountHealthOffsetX; }
            set
            {
                if (_mountHealthOffsetX != value)
                {
                    _mountHealthOffsetX = value;
                    OnPropertyChangedWithValue(value, "MountHealthOffsetX");
                }
            }
        }

        [DataSourceProperty]
        public float MountHealthOffsetY
        {
            get { return _mountHealthOffsetY; }
            set
            {
                if (_mountHealthOffsetY != value)
                {
                    _mountHealthOffsetY = value;
                    OnPropertyChangedWithValue(value, "MountHealthOffsetY");
                }
            }
        }

        [DataSourceProperty]
        public float ShieldHealthOffsetX
        {
            get { return _shieldHealthOffsetX; }
            set
            {
                if (_shieldHealthOffsetX != value)
                {
                    _shieldHealthOffsetX = value;
                    OnPropertyChangedWithValue(value, "ShieldHealthOffsetX");
                }
            }
        }

        [DataSourceProperty]
        public float ShieldHealthOffsetY
        {
            get { return _shieldHealthOffsetY; }
            set
            {
                if (_shieldHealthOffsetY != value)
                {
                    _shieldHealthOffsetY = value;
                    OnPropertyChangedWithValue(value, "ShieldHealthOffsetY");
                }
            }
        }

        public void ClearAll()
        {
            PlayerHealth = "";
            MountHealth = "";
            ShieldHealth = "";
        }

        public void UpdateOffsets(ElementCustomization agentHealth, ElementCustomization mountHealth, ElementCustomization shieldHealth)
        {
            PlayerHealthOffsetX = agentHealth.OffsetX;
            PlayerHealthOffsetY = agentHealth.OffsetY;
            MountHealthOffsetX = mountHealth.OffsetX;
            MountHealthOffsetY = mountHealth.OffsetY;
            ShieldHealthOffsetX = shieldHealth.OffsetX;
            ShieldHealthOffsetY = shieldHealth.OffsetY;
        }

        public void UpdateVisibility(bool showPlayer, bool showMount, bool showShield)
        {
            ShowPlayerHealth = showPlayer;
            ShowMountHealth = showMount;
            ShowShieldHealth = showShield;
        }
    }
}