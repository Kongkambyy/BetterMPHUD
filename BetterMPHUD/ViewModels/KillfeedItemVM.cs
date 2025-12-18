using System;
using TaleWorlds.Library;

namespace BetterMPHUD
{
    public class KillfeedItemVM : ViewModel
    {
        private string _killerName;
        private string _victimName;
        private string _rowColor;
        private readonly Action<KillfeedItemVM> _onRemove;

        public float ExpireTime { get; set; }

        public KillfeedItemVM(string killerName, string victimName, string rowColor, float expireTime, Action<KillfeedItemVM> onRemove)
        {
            _killerName = killerName;
            _victimName = victimName;
            _rowColor = rowColor;
            ExpireTime = expireTime;
            _onRemove = onRemove;
        }

        public void ExecuteRemove() => _onRemove?.Invoke(this);

        [DataSourceProperty]
        public string KillerName { get => _killerName; set { if (_killerName != value) { _killerName = value; OnPropertyChangedWithValue(value, "KillerName"); } } }

        [DataSourceProperty]
        public string VictimName { get => _victimName; set { if (_victimName != value) { _victimName = value; OnPropertyChangedWithValue(value, "VictimName"); } } }

        [DataSourceProperty]
        public string RowColor { get => _rowColor; set { if (_rowColor != value) { _rowColor = value; OnPropertyChangedWithValue(value, "RowColor"); } } }
    }
}