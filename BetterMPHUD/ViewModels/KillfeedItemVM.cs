using System;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    public class KillfeedItemVM : ViewModel
    {
        private string _killerName;
        private string _victimName;
        private Color _rowColor;
        private string _killerClassSprite;
        private string _victimClassSprite;
        private string _killIconSprite;
        
        // NEW: Scaling Properties
        private int _fontSize;
        private int _iconSize;
        private int _skullSize;
        private int _rowHeight;

        private readonly Action<KillfeedItemVM> _onRemove;
        public float ExpireTime { get; set; }

        public KillfeedItemVM(
            string killerName, 
            string victimName, 
            Color rowColor,
            string killerClassSprite,
            string victimClassSprite,
            string killIconSprite,
            float expireTime, 
            Action<KillfeedItemVM> onRemove)
        {
            _killerName = killerName;
            _victimName = victimName;
            _rowColor = rowColor;
            _killerClassSprite = killerClassSprite;
            _victimClassSprite = victimClassSprite;
            _killIconSprite = killIconSprite;
            ExpireTime = expireTime;
            _onRemove = onRemove;
        }

        // NEW: Method to update sizes dynamically
        public void UpdateSizes(int fontSize, int iconSize, int skullSize, int rowHeight)
        {
            FontSize = fontSize;
            IconSize = iconSize;
            SkullSize = skullSize;
            RowHeight = rowHeight;
        }

        public void ExecuteRemove() => _onRemove?.Invoke(this);

        [DataSourceProperty]
        public string KillerName 
        { 
            get => _killerName; 
            set { if (_killerName != value) { _killerName = value; OnPropertyChangedWithValue(value, "KillerName"); } } 
        }

        [DataSourceProperty]
        public string VictimName 
        { 
            get => _victimName; 
            set { if (_victimName != value) { _victimName = value; OnPropertyChangedWithValue(value, "VictimName"); } } 
        }

        [DataSourceProperty]
        public Color RowColor 
        { 
            get => _rowColor; 
            set { if (_rowColor != value) { _rowColor = value; OnPropertyChangedWithValue(value, "RowColor"); } } 
        }

        [DataSourceProperty]
        public string KillerClassSprite 
        { 
            get => _killerClassSprite; 
            set { if (_killerClassSprite != value) { _killerClassSprite = value; OnPropertyChangedWithValue(value, "KillerClassSprite"); } } 
        }

        [DataSourceProperty]
        public string VictimClassSprite 
        { 
            get => _victimClassSprite; 
            set { if (_victimClassSprite != value) { _victimClassSprite = value; OnPropertyChangedWithValue(value, "VictimClassSprite"); } } 
        }

        [DataSourceProperty]
        public string KillIconSprite 
        { 
            get => _killIconSprite; 
            set { if (_killIconSprite != value) { _killIconSprite = value; OnPropertyChangedWithValue(value, "KillIconSprite"); } } 
        }

        // NEW: Bindable Size Properties
        [DataSourceProperty]
        public int FontSize 
        { 
            get => _fontSize; 
            set { if (_fontSize != value) { _fontSize = value; OnPropertyChangedWithValue(value, "FontSize"); } } 
        }

        [DataSourceProperty]
        public int IconSize 
        { 
            get => _iconSize; 
            set { if (_iconSize != value) { _iconSize = value; OnPropertyChangedWithValue(value, "IconSize"); } } 
        }

        [DataSourceProperty]
        public int SkullSize 
        { 
            get => _skullSize; 
            set { if (_skullSize != value) { _skullSize = value; OnPropertyChangedWithValue(value, "SkullSize"); } } 
        }

        [DataSourceProperty]
        public int RowHeight 
        { 
            get => _rowHeight; 
            set { if (_rowHeight != value) { _rowHeight = value; OnPropertyChangedWithValue(value, "RowHeight"); } } 
        }
    }
}