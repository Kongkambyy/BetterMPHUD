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
        
        private int _fontSize;
        private float _iconSize;
        private float _skullSize;
        private float _rowHeight;
        
        private bool _showBackground;
        private string _backgroundColor;
        private float _backgroundOpacity;

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
            
            // Default background values
            _showBackground = true;
            _backgroundColor = "#000000FF";
            _backgroundOpacity = 0.7f;
        }

        public void UpdateSizes(int fontSize, float iconSize, float skullSize, float rowHeight)
        {
            FontSize = fontSize;
            IconSize = iconSize;
            SkullSize = skullSize;
            RowHeight = rowHeight;
        }
        
        // Update background settings
        public void UpdateBackground(bool show, string color, float opacity)
        {
            ShowBackground = show;
            BackgroundColor = color;
            BackgroundOpacity = opacity;
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

        [DataSourceProperty]
        public int FontSize 
        { 
            get => _fontSize; 
            set { if (_fontSize != value) { _fontSize = value; OnPropertyChangedWithValue(value, "FontSize"); } } 
        }

        [DataSourceProperty]
        public float IconSize 
        { 
            get => _iconSize; 
            set { if (_iconSize != value) { _iconSize = value; OnPropertyChangedWithValue(value, "IconSize"); } } 
        }

        [DataSourceProperty]
        public float SkullSize 
        { 
            get => _skullSize; 
            set { if (_skullSize != value) { _skullSize = value; OnPropertyChangedWithValue(value, "SkullSize"); } } 
        }

        [DataSourceProperty]
        public float RowHeight 
        { 
            get => _rowHeight; 
            set { if (_rowHeight != value) { _rowHeight = value; OnPropertyChangedWithValue(value, "RowHeight"); } } 
        }
        
        // Background properties
        [DataSourceProperty]
        public bool ShowBackground 
        { 
            get => _showBackground; 
            set { if (_showBackground != value) { _showBackground = value; OnPropertyChangedWithValue(value, "ShowBackground"); } } 
        }

        [DataSourceProperty]
        public string BackgroundColor 
        { 
            get => _backgroundColor; 
            set { if (_backgroundColor != value) { _backgroundColor = value; OnPropertyChangedWithValue(value, "BackgroundColor"); } } 
        }

        [DataSourceProperty]
        public float BackgroundOpacity 
        { 
            get => _backgroundOpacity; 
            set { if (_backgroundOpacity != value) { _backgroundOpacity = value; OnPropertyChangedWithValue(value, "BackgroundOpacity"); } } 
        }
    }
}