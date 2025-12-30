using System;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels.Settings
{
    public class TopBarSettingsVM : BaseSettingsVM
    {
        private int _selectedIndex;
        private string _selectedName;
        private ElementCustomization _currentCustomization;

        public TopBarSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
            SelectTimeAndScores();
        }

        private void SelectElement(int index, string name, ElementCustomization customization)
        {
            _selectedIndex = index;
            _selectedName = name;
            _currentCustomization = customization;
            RefreshDisplay();
        }

        public void SelectTimeAndScores() => SelectElement(0, "Time & Scores", Settings.TimeAndScoresCustom);
        public void SelectTeamAvatars() => SelectElement(1, "Team Avatars", Settings.TeamAvatarsCustom);
        public void SelectMorale() => SelectElement(2, "Morale", Settings.MoraleCustom);
        public void SelectPowerLevel() => SelectElement(3, "Power Level", Settings.PowerLevelCustom);

        [DataSourceProperty]
        public int SelectedIndex => _selectedIndex;

        [DataSourceProperty]
        public string SelectedName => _selectedName;

        [DataSourceProperty]
        public string OffsetXText => _currentCustomization?.OffsetX.ToString("F0") ?? "0";

        [DataSourceProperty]
        public string OffsetYText => _currentCustomization?.OffsetY.ToString("F0") ?? "0";

        [DataSourceProperty]
        public string ScaleText => _currentCustomization != null 
            ? (_currentCustomization.Scale * 100).ToString("F0") + "%" 
            : "100%";

        public void ExecuteIncreaseOffsetX() => AdjustCurrentOffset(Constants.Adjustment.PositionStep, 0);
        public void ExecuteDecreaseOffsetX() => AdjustCurrentOffset(-Constants.Adjustment.PositionStep, 0);
        public void ExecuteIncreaseOffsetY() => AdjustCurrentOffset(0, Constants.Adjustment.PositionStep);
        public void ExecuteDecreaseOffsetY() => AdjustCurrentOffset(0, -Constants.Adjustment.PositionStep);
        public void ExecuteIncreaseOffsetXLarge() => AdjustCurrentOffset(Constants.Adjustment.PositionStepLarge, 0);
        public void ExecuteDecreaseOffsetXLarge() => AdjustCurrentOffset(-Constants.Adjustment.PositionStepLarge, 0);
        public void ExecuteIncreaseOffsetYLarge() => AdjustCurrentOffset(0, Constants.Adjustment.PositionStepLarge);
        public void ExecuteDecreaseOffsetYLarge() => AdjustCurrentOffset(0, -Constants.Adjustment.PositionStepLarge);

        private void AdjustCurrentOffset(float dx, float dy)
        {
            if (_currentCustomization == null) return;
            AdjustOffset(_currentCustomization, dx, dy, RefreshDisplay);
        }

        public void ExecuteIncreaseScale() => AdjustCurrentScale(Constants.Adjustment.ScaleStep);
        public void ExecuteDecreaseScale() => AdjustCurrentScale(-Constants.Adjustment.ScaleStep);
        public void ExecuteIncreaseScaleLarge() => AdjustCurrentScale(Constants.Adjustment.ScaleStepLarge);
        public void ExecuteDecreaseScaleLarge() => AdjustCurrentScale(-Constants.Adjustment.ScaleStepLarge);

        private void AdjustCurrentScale(float delta)
        {
            if (_currentCustomization == null) return;
            AdjustScale(_currentCustomization, delta, RefreshDisplay);
        }

        public void ExecuteResetElement()
        {
            _currentCustomization?.Reset();
            RefreshDisplay();
        }

        public void ExecuteResetAll()
        {
            Settings.TimeAndScoresCustom.Reset();
            Settings.TeamAvatarsCustom.Reset();
            Settings.MoraleCustom.Reset();
            Settings.PowerLevelCustom.Reset();
            SelectTimeAndScores();
            NotifyChanged();
        }

        private void RefreshDisplay()
        {
            OnPropertyChangedWithValue(SelectedIndex, "SelectedIndex");
            OnPropertyChangedWithValue(SelectedName, "SelectedName");
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            OnPropertyChangedWithValue(ScaleText, "ScaleText");
            NotifyChanged();
        }

        public override void RefreshAll()
        {
            RefreshDisplay();
        }
    }
}