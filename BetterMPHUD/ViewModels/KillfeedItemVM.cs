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
    }
}