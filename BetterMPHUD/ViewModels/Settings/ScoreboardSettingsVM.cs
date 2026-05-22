using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class ScoreboardSettingsVM : BaseSettingsVM
    {
        public Action OnDebugScoreboardStructure;

        public ScoreboardSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
        }

        [DataSourceProperty]
        public bool BackgroundEnabled
        {
            get => Settings.ScoreboardBackgroundEnabled;
            set
            {
                if (Settings.ScoreboardBackgroundEnabled != value)
                {
                    Settings.ScoreboardBackgroundEnabled = value;
                    OnPropertyChangedWithValue(value, "BackgroundEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool StripingEnabled
        {
            get => Settings.ScoreboardStripingEnabled;
            set
            {
                if (Settings.ScoreboardStripingEnabled != value)
                {
                    Settings.ScoreboardStripingEnabled = value;
                    OnPropertyChangedWithValue(value, "StripingEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool DeadPlayerTintEnabled
        {
            get => Settings.ScoreboardDeadPlayerTintEnabled;
            set
            {
                if (Settings.ScoreboardDeadPlayerTintEnabled != value)
                {
                    Settings.ScoreboardDeadPlayerTintEnabled = value;
                    OnPropertyChangedWithValue(value, "DeadPlayerTintEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool HideUIWhenOpen
        {
            get => Settings.HideUIWhenScoreboardOpen;
            set
            {
                if (Settings.HideUIWhenScoreboardOpen != value)
                {
                    Settings.HideUIWhenScoreboardOpen = value;
                    OnPropertyChangedWithValue(value, "HideUIWhenOpen");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string BackgroundOpacityText => (Settings.ScoreboardBackgroundOpacity * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string StripingOpacityText => (Settings.ScoreboardStripingOpacity * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string DeadPlayerOpacityText => (Settings.ScoreboardDeadPlayerOpacity * 100).ToString("F0") + "%";

        public void ExecuteIncreaseBackgroundOpacity() => AdjustBackgroundOpacity(0.05f);
        public void ExecuteDecreaseBackgroundOpacity() => AdjustBackgroundOpacity(-0.05f);
        public void ExecuteIncreaseBackgroundOpacityLarge() => AdjustBackgroundOpacity(0.1f);
        public void ExecuteDecreaseBackgroundOpacityLarge() => AdjustBackgroundOpacity(-0.1f);

        private void AdjustBackgroundOpacity(float delta)
        {
            float newValue = Settings.ScoreboardBackgroundOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                Settings.ScoreboardBackgroundOpacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(BackgroundOpacityText, "BackgroundOpacityText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseStripingOpacity() => AdjustStripingOpacity(0.05f);
        public void ExecuteDecreaseStripingOpacity() => AdjustStripingOpacity(-0.05f);
        public void ExecuteIncreaseStripingOpacityLarge() => AdjustStripingOpacity(0.1f);
        public void ExecuteDecreaseStripingOpacityLarge() => AdjustStripingOpacity(-0.1f);

        private void AdjustStripingOpacity(float delta)
        {
            float newValue = Settings.ScoreboardStripingOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                Settings.ScoreboardStripingOpacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(StripingOpacityText, "StripingOpacityText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseDeadPlayerOpacity() => AdjustDeadPlayerOpacity(0.05f);
        public void ExecuteDecreaseDeadPlayerOpacity() => AdjustDeadPlayerOpacity(-0.05f);
        public void ExecuteIncreaseDeadPlayerOpacityLarge() => AdjustDeadPlayerOpacity(0.1f);
        public void ExecuteDecreaseDeadPlayerOpacityLarge() => AdjustDeadPlayerOpacity(-0.1f);

        private void AdjustDeadPlayerOpacity(float delta)
        {
            float newValue = Settings.ScoreboardDeadPlayerOpacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                Settings.ScoreboardDeadPlayerOpacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(DeadPlayerOpacityText, "DeadPlayerOpacityText");
                NotifyChanged();
            }
        }

        public void ExecuteSetDeadPlayerColorRed() => SetDeadPlayerColor("#FF4444FF");
        public void ExecuteSetDeadPlayerColorGray() => SetDeadPlayerColor("#888888FF");
        public void ExecuteSetDeadPlayerColorDark() => SetDeadPlayerColor("#444444FF");
        public void ExecuteSetDeadPlayerColorWhite() => SetDeadPlayerColor("#CCCCCCFF");
        public void ExecuteSetDeadPlayerColorOrange() => SetDeadPlayerColor("#FF8800FF");
        public void ExecuteSetDeadPlayerColorPurple() => SetDeadPlayerColor("#8844FFFF");

        private void SetDeadPlayerColor(string color)
        {
            Settings.ScoreboardDeadPlayerColor = color;
            NotifyChanged();
        }

        public void ExecuteReset()
        {
            Settings.ScoreboardBackgroundEnabled = true;
            Settings.ScoreboardBackgroundOpacity = 0.8f;
            Settings.ScoreboardStripingEnabled = true;
            Settings.ScoreboardStripingOpacity = 0.2f;
            Settings.ScoreboardDeadPlayerOpacity = 0.3f;
            Settings.ScoreboardDeadPlayerColor = "";
            Settings.ScoreboardDeadPlayerTintEnabled = false;
            Settings.HideUIWhenScoreboardOpen = false;
            RefreshAll();
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(BackgroundEnabled, "BackgroundEnabled");
            OnPropertyChangedWithValue(StripingEnabled, "StripingEnabled");
            OnPropertyChangedWithValue(DeadPlayerTintEnabled, "DeadPlayerTintEnabled");
            OnPropertyChangedWithValue(HideUIWhenOpen, "HideUIWhenOpen");
            OnPropertyChangedWithValue(BackgroundOpacityText, "BackgroundOpacityText");
            OnPropertyChangedWithValue(StripingOpacityText, "StripingOpacityText");
            OnPropertyChangedWithValue(DeadPlayerOpacityText, "DeadPlayerOpacityText");
            NotifyChanged();
        }
    }
}