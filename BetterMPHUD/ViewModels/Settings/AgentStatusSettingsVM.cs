using System;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels.Settings
{
    public class AgentStatusSettingsVM : BaseSettingsVM
    {
        private int _selectedIndex;
        private string _selectedName;
        private ElementCustomization _currentCustomization;

        public AgentStatusSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
            SelectAgentHealth();
        }

        private void SelectElement(int index, string name, ElementCustomization customization)
        {
            _selectedIndex = index;
            _selectedName = name;
            _currentCustomization = customization;
            RefreshDisplay();
        }

        public void SelectAgentHealth() => SelectElement(0, "Agent Health", Settings.AgentHealthCustom);
        public void SelectMountHealth() => SelectElement(1, "Mount Health", Settings.MountHealthCustom);
        public void SelectShieldHealth() => SelectElement(2, "Shield Health", Settings.ShieldHealthCustom);
        public void SelectWeaponInfo() => SelectElement(3, "Weapon Info", Settings.WeaponInfoCustom);
        public void SelectGoldAmount() => SelectElement(4, "Gold Amount", Settings.GoldAmountCustom);
        public void SelectTroopCount() => SelectElement(5, "Troop Count", Settings.TroopCountCustom);
        public void SelectDamageFeed() => SelectElement(6, "Damage Feed", Settings.DamageFeedCustom);

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
            Settings.AgentHealthCustom.Reset();
            Settings.MountHealthCustom.Reset();
            Settings.ShieldHealthCustom.Reset();
            Settings.WeaponInfoCustom.Reset();
            Settings.GoldAmountCustom.Reset();
            Settings.TroopCountCustom.Reset();
            Settings.DamageFeedCustom.Reset();
            SelectAgentHealth();
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