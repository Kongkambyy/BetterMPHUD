using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class WarlordsSettingsVM : BaseSettingsVM
    {
        public WarlordsSettingsVM(HudSettings settings, Action onSettingsChanged)
            : base(settings, onSettingsChanged)
        {
        }

        [DataSourceProperty]
        public bool OnlyShowRoundGoldTotal
        {
            get => Settings.WarlordsOnlyShowRoundGoldTotal;
            set
            {
                if (Settings.WarlordsOnlyShowRoundGoldTotal != value)
                {
                    Settings.WarlordsOnlyShowRoundGoldTotal = value;
                    OnPropertyChangedWithValue(value, "OnlyShowRoundGoldTotal");
                    NotifyChanged();
                }
            }
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(OnlyShowRoundGoldTotal, "OnlyShowRoundGoldTotal");
        }
    }
}
