using System;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    public class KillfeedItemVM : ViewModel
    {
        private string _killerName;
        private string _victimName;
        private string _killerColor;
        private string _victimColor;
        private readonly Action<KillfeedItemVM> _onRemove;

        public float ExpireTime { get; set; }

        // Removed backgroundColor from constructor
        public KillfeedItemVM(string killerName, string victimName, string killerColor, string victimColor, float expireTime, Action<KillfeedItemVM> onRemove)
        {
            _killerName = killerName;
            _victimName = victimName;
            _killerColor = killerColor;
            _victimColor = victimColor;
            ExpireTime = expireTime;
            _onRemove = onRemove;
        }

        public void ExecuteRemove()
        {
            _onRemove?.Invoke(this);
        }

        [DataSourceProperty]
        public string KillerName
        {
            get => _killerName;
            set
            {
                if (_killerName != value)
                {
                    _killerName = value;
                    OnPropertyChangedWithValue(value, "KillerName");
                }
            }
        }

        [DataSourceProperty]
        public string VictimName
        {
            get => _victimName;
            set
            {
                if (_victimName != value)
                {
                    _victimName = value;
                    OnPropertyChangedWithValue(value, "VictimName");
                }
            }
        }

        [DataSourceProperty]
        public string KillerColor
        {
            get => _killerColor;
            set
            {
                if (_killerColor != value)
                {
                    _killerColor = value;
                    OnPropertyChangedWithValue(value, "KillerColor");
                }
            }
        }

        [DataSourceProperty]
        public string VictimColor
        {
            get => _victimColor;
            set
            {
                if (_victimColor != value)
                {
                    _victimColor = value;
                    OnPropertyChangedWithValue(value, "VictimColor");
                }
            }
        }
    }
}