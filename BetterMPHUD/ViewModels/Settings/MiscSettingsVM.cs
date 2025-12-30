using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class MiscSettingsVM : BaseSettingsVM
    {
        public MiscSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
        }

        [DataSourceProperty]
        public bool CameraSnapbackEnabled
        {
            get => Settings.CameraSnapbackEnabled;
            set
            {
                if (Settings.CameraSnapbackEnabled != value)
                {
                    Settings.CameraSnapbackEnabled = value;
                    OnPropertyChangedWithValue(value, "CameraSnapbackEnabled");
                    NotifyChanged();
                }
            }
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(CameraSnapbackEnabled, "CameraSnapbackEnabled");
            NotifyChanged();
        }
    }
}