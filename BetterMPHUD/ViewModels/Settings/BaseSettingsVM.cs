using System;
using TaleWorlds.Library;
using BetterMPHUD.Core;

namespace BetterMPHUD.ViewModels.Settings
{
    public abstract class BaseSettingsVM : ViewModel
    {
        protected readonly HudSettings Settings;
        protected readonly Action OnSettingsChanged;
        protected Action OnPropertyRefresh;

        protected BaseSettingsVM(HudSettings settings, Action onSettingsChanged)
        {
            Settings = settings;
            OnSettingsChanged = onSettingsChanged;
        }

        public void SetOnPropertyRefresh(Action onPropertyRefresh)
        {
            OnPropertyRefresh = onPropertyRefresh;
        }

        protected void NotifyChanged()
        {
            ConfigManager.SaveSettings(Settings);
            OnSettingsChanged?.Invoke();
            OnPropertyRefresh?.Invoke();
        }

        protected void NotifyPropertyRefresh()
        {
            OnPropertyRefresh?.Invoke();
        }

        protected float ClampAndRound(float value, float min, float max, int decimals = 2)
        {
            return (float)Math.Round(Math.Max(min, Math.Min(max, value)), decimals);
        }

        protected int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        protected void AdjustOffset(ElementCustomization custom, float dx, float dy, Action refresh)
        {
            custom.OffsetX += dx;
            custom.OffsetY += dy;
            refresh?.Invoke();
        }

        protected void AdjustScale(ElementCustomization custom, float delta, Action refresh)
        {
            float newScale = custom.Scale + delta;
            if (newScale >= Constants.Adjustment.MinScale && newScale <= Constants.Adjustment.MaxScale)
            {
                custom.Scale = (float)Math.Round(newScale, 2);
                refresh?.Invoke();
            }
        }

        public abstract void RefreshAll();
    }
}