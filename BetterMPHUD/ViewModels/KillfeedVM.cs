using System;
using System.Linq; // Required for ToList() fix
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels
{
    public class KillfeedVM : ViewModel
    {
        private MBBindingList<KillfeedItemVM> _killList;
        private bool _isVisible;
        
        // Base pixel sizes (100% scale)
        private const int BASE_FONT = 20;
        private const int BASE_ICON = 24;
        private const int BASE_SKULL = 34;
        private const int BASE_ROW = 32;

        public KillfeedVM()
        {
            _killList = new MBBindingList<KillfeedItemVM>();
            _isVisible = false;
        }

        public void UpdateScale(float scale)
        {
            // Prevent scale from being too small (crash protection)
            if (scale < 0.1f) scale = 0.1f;

            int newFont = Math.Max(1, (int)(BASE_FONT * scale));
            int newIcon = Math.Max(1, (int)(BASE_ICON * scale));
            int newSkull = Math.Max(1, (int)(BASE_SKULL * scale));
            int newRow = Math.Max(1, (int)(BASE_ROW * scale));

            // CRITICAL FIX: .ToList() creates a safe copy for the loop
            // This prevents "Collection was modified" crashes
            foreach (var item in _killList.ToList())
            {
                item.UpdateSizes(newFont, newIcon, newSkull, newRow);
            }
        }
        
        public void GetScaledSizes(float scale, out int font, out int icon, out int skull, out int row)
        {
            if (scale < 0.1f) scale = 0.1f;
            
            font = Math.Max(1, (int)(BASE_FONT * scale));
            icon = Math.Max(1, (int)(BASE_ICON * scale));
            skull = Math.Max(1, (int)(BASE_SKULL * scale));
            row = Math.Max(1, (int)(BASE_ROW * scale));
        }

        public void AddKill(KillfeedItemVM item)
        {
            if (_killList.Count >= 15)
            {
                _killList.RemoveAt(0);
            }
            _killList.Add(item);
        }

        public void RemoveKill(KillfeedItemVM item)
        {
            if (_killList.Contains(item))
            {
                _killList.Remove(item);
            }
        }

        public void Clear()
        {
            _killList.Clear();
        }

        [DataSourceProperty]
        public MBBindingList<KillfeedItemVM> KillList
        {
            get => _killList;
            set
            {
                if (_killList != value)
                {
                    _killList = value;
                    OnPropertyChangedWithValue(value, "KillList");
                }
            }
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChangedWithValue(value, "IsVisible");
                }
            }
        }
    }
}