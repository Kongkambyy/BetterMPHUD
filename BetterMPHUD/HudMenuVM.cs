using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class HudMenuVM : ViewModel
    {
        private bool _isConfigMenuOpen;
        public Action OnCloseConfigMenu;

        public HudMenuVM()
        {
            // Constructor code (load settings) would go here later
        }

        public void ExecuteClose()
        {
            OnCloseConfigMenu?.Invoke();
        }

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
    }
}