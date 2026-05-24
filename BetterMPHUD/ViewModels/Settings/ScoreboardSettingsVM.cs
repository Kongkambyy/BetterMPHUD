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
        public bool SummaryEnabled
        {
            get => Settings.ScoreboardSummaryEnabled;
            set
            {
                if (Settings.ScoreboardSummaryEnabled != value)
                {
                    Settings.ScoreboardSummaryEnabled = value;
                    OnPropertyChangedWithValue(value, "SummaryEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowPing
        {
            get => Settings.ScoreboardShowPing;
            set
            {
                if (Settings.ScoreboardShowPing != value)
                {
                    Settings.ScoreboardShowPing = value;
                    OnPropertyChangedWithValue(value, "ShowPing");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowState
        {
            get => Settings.ScoreboardShowState;
            set
            {
                if (Settings.ScoreboardShowState != value)
                {
                    Settings.ScoreboardShowState = value;
                    OnPropertyChangedWithValue(value, "ShowState");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool ShowWarlordsBanners
        {
            get => Settings.ScoreboardShowWarlordsBanners;
            set
            {
                if (Settings.ScoreboardShowWarlordsBanners != value)
                {
                    Settings.ScoreboardShowWarlordsBanners = value;
                    OnPropertyChangedWithValue(value, "ShowWarlordsBanners");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool IsNativeScoreboardModeSelected => Settings.ScoreboardMode == ScoreboardMode.Native;

        [DataSourceProperty]
        public bool IsCustomScoreboardModeSelected => Settings.ScoreboardMode == ScoreboardMode.Custom;

        [DataSourceProperty]
        public string BackgroundOpacityText => (Settings.ScoreboardBackgroundOpacity * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string StripingOpacityText => (Settings.ScoreboardStripingOpacity * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string DeadPlayerOpacityText => (Settings.ScoreboardDeadPlayerOpacity * 100).ToString("F0") + "%";

        [DataSourceProperty]
        public string RowScaleText => (Settings.ScoreboardRowScale * 100).ToString("F0") + "%";

        private static readonly string[] SortColumns =
        {
            "Name", "Kills", "Score"
        };

        [DataSourceProperty]
        public string DefaultSortColumnText => Settings.ScoreboardDefaultSortColumn;

        [DataSourceProperty]
        public string DefaultSortDirectionText => Settings.ScoreboardDefaultSortAscending ? "Ascending" : "Descending";

        public void ExecuteIncreaseBackgroundOpacity() => AdjustBackgroundOpacity(0.05f);
        public void ExecuteDecreaseBackgroundOpacity() => AdjustBackgroundOpacity(-0.05f);
        public void ExecuteIncreaseBackgroundOpacityLarge() => AdjustBackgroundOpacity(0.1f);
        public void ExecuteDecreaseBackgroundOpacityLarge() => AdjustBackgroundOpacity(-0.1f);

        public void ExecuteIncreaseRowScale() => AdjustRowScale(0.05f);
        public void ExecuteDecreaseRowScale() => AdjustRowScale(-0.05f);
        public void ExecuteIncreaseRowScaleLarge() => AdjustRowScale(0.1f);
        public void ExecuteDecreaseRowScaleLarge() => AdjustRowScale(-0.1f);

        private void AdjustRowScale(float delta)
        {
            float newValue = Settings.ScoreboardRowScale + delta;
            if (newValue >= 0.75f && newValue <= 1.6f)
            {
                Settings.ScoreboardRowScale = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(RowScaleText, "RowScaleText");
                NotifyChanged();
            }
        }

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

        public void ExecuteNextScoreboardDefaultSortColumn() => AdjustDefaultSortColumn(1);
        public void ExecutePreviousScoreboardDefaultSortColumn() => AdjustDefaultSortColumn(-1);
        public void ExecuteNextScoreboardDefaultSortDirection() => ToggleDefaultSortDirection();
        public void ExecutePreviousScoreboardDefaultSortDirection() => ToggleDefaultSortDirection();
        public void ExecuteSelectNativeScoreboardMode() => SetScoreboardMode(ScoreboardMode.Native);
        public void ExecuteSelectCustomScoreboardMode() => SetScoreboardMode(ScoreboardMode.Custom);

        private void AdjustDefaultSortColumn(int direction)
        {
            int index = Array.IndexOf(SortColumns, Settings.ScoreboardDefaultSortColumn);
            if (index < 0)
                index = Array.IndexOf(SortColumns, "Score");

            index += direction;
            if (index < 0)
                index = SortColumns.Length - 1;
            if (index >= SortColumns.Length)
                index = 0;

            Settings.ScoreboardDefaultSortColumn = SortColumns[index];
            OnPropertyChangedWithValue(DefaultSortColumnText, "DefaultSortColumnText");
            NotifyChanged();
        }

        private void ToggleDefaultSortDirection()
        {
            Settings.ScoreboardDefaultSortAscending = !Settings.ScoreboardDefaultSortAscending;
            OnPropertyChangedWithValue(DefaultSortDirectionText, "DefaultSortDirectionText");
            NotifyChanged();
        }

        private void SetScoreboardMode(ScoreboardMode mode)
        {
            if (Settings.ScoreboardMode == mode)
                return;

            Settings.ScoreboardMode = mode;
            OnPropertyChangedWithValue(IsNativeScoreboardModeSelected, "IsNativeScoreboardModeSelected");
            OnPropertyChangedWithValue(IsCustomScoreboardModeSelected, "IsCustomScoreboardModeSelected");
            NotifyChanged();
        }

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
            Settings.ScoreboardSummaryEnabled = true;
            Settings.ScoreboardMode = ScoreboardMode.Custom;
            Settings.ScoreboardShowPing = true;
            Settings.ScoreboardShowState = true;
            Settings.ScoreboardShowWarlordsBanners = true;
            Settings.ScoreboardRowScale = 1f;
            Settings.ScoreboardDefaultSortColumn = "Score";
            Settings.ScoreboardDefaultSortAscending = false;
            RefreshAll();
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(BackgroundEnabled, "BackgroundEnabled");
            OnPropertyChangedWithValue(StripingEnabled, "StripingEnabled");
            OnPropertyChangedWithValue(DeadPlayerTintEnabled, "DeadPlayerTintEnabled");
            OnPropertyChangedWithValue(HideUIWhenOpen, "HideUIWhenOpen");
            OnPropertyChangedWithValue(SummaryEnabled, "SummaryEnabled");
            OnPropertyChangedWithValue(ShowPing, "ShowPing");
            OnPropertyChangedWithValue(ShowState, "ShowState");
            OnPropertyChangedWithValue(ShowWarlordsBanners, "ShowWarlordsBanners");
            OnPropertyChangedWithValue(IsNativeScoreboardModeSelected, "IsNativeScoreboardModeSelected");
            OnPropertyChangedWithValue(IsCustomScoreboardModeSelected, "IsCustomScoreboardModeSelected");
            OnPropertyChangedWithValue(BackgroundOpacityText, "BackgroundOpacityText");
            OnPropertyChangedWithValue(StripingOpacityText, "StripingOpacityText");
            OnPropertyChangedWithValue(DeadPlayerOpacityText, "DeadPlayerOpacityText");
            OnPropertyChangedWithValue(RowScaleText, "RowScaleText");
            OnPropertyChangedWithValue(DefaultSortColumnText, "DefaultSortColumnText");
            OnPropertyChangedWithValue(DefaultSortDirectionText, "DefaultSortDirectionText");
            NotifyChanged();
        }
    }
}
