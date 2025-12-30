using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class CrosshairSettingsVM : BaseSettingsVM
    {
        private MBBindingList<ColorItemVM> _colorList;

        public CrosshairSettingsVM(HudSettings settings, Action onSettingsChanged) 
            : base(settings, onSettingsChanged)
        {
            InitializeColorList();
        }

        private void InitializeColorList()
        {
            _colorList = new MBBindingList<ColorItemVM>();
            string[] colors = new string[]
            {
                "#FF0000FF", "#00FF00FF", "#0000FFFF", "#FFFFFFFF",
                "#FFFF00FF", "#00FFFFFF", "#FF00FFFF", "#FFA500FF",
                "#FF6666FF", "#66FF66FF", "#6666FFFF", "#000000FF",
                "#888888FF", "#FF1493FF", "#8B4513FF", "#4B0082FF"
            };

            foreach (string color in colors)
                _colorList.Add(new ColorItemVM(color, OnColorSelected));

            SelectCurrentColor();
        }

        private void OnColorSelected(ColorItemVM selected)
        {
            foreach (var item in _colorList)
                item.IsSelected = false;

            selected.IsSelected = true;
            Settings.CrosshairSettings.Color = selected.ColorHex;
            NotifyChanged();
        }

        private void SelectCurrentColor()
        {
            if (_colorList == null) return;
            string currentColor = Settings.CrosshairSettings.Color ?? "#FF0000FF";
            foreach (var item in _colorList)
                item.IsSelected = string.Equals(item.ColorHex, currentColor, StringComparison.OrdinalIgnoreCase);
        }

        [DataSourceProperty]
        public MBBindingList<ColorItemVM> ColorList => _colorList;

        [DataSourceProperty]
        public bool CustomCrosshairEnabled
        {
            get => Settings.CrosshairSettings.CustomCrosshairEnabled;
            set
            {
                if (Settings.CrosshairSettings.CustomCrosshairEnabled != value)
                {
                    Settings.CrosshairSettings.CustomCrosshairEnabled = value;
                    OnPropertyChangedWithValue(value, "CustomCrosshairEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public int WidthInt
        {
            get => Settings.CrosshairSettings.SizeHorizontal;
            set
            {
                if (Settings.CrosshairSettings.SizeHorizontal != value)
                {
                    Settings.CrosshairSettings.SizeHorizontal = value;
                    OnPropertyChangedWithValue(value, "WidthInt");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public int LengthInt
        {
            get => Settings.CrosshairSettings.SizeVertical;
            set
            {
                if (Settings.CrosshairSettings.SizeVertical != value)
                {
                    Settings.CrosshairSettings.SizeVertical = value;
                    OnPropertyChangedWithValue(value, "LengthInt");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public int OffsetInt
        {
            get => Settings.CrosshairSettings.Offset;
            set
            {
                if (Settings.CrosshairSettings.Offset != value)
                {
                    Settings.CrosshairSettings.Offset = value;
                    OnPropertyChangedWithValue(value, "OffsetInt");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public float OpacityFloat
        {
            get => Settings.CrosshairSettings.Opacity;
            set
            {
                float rounded = (float)Math.Round(value, 2);
                if (Settings.CrosshairSettings.Opacity != rounded)
                {
                    Settings.CrosshairSettings.Opacity = rounded;
                    OnPropertyChangedWithValue(rounded, "OpacityFloat");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string ColorText => Settings.CrosshairSettings.Color;

        public void ExecuteIncreaseWidth() => AdjustWidth(1);
        public void ExecuteDecreaseWidth() => AdjustWidth(-1);
        public void ExecuteIncreaseWidthLarge() => AdjustWidth(5);
        public void ExecuteDecreaseWidthLarge() => AdjustWidth(-5);

        private void AdjustWidth(int delta)
        {
            int newValue = Settings.CrosshairSettings.SizeHorizontal + delta;
            if (newValue >= 1 && newValue <= 75)
            {
                Settings.CrosshairSettings.SizeHorizontal = newValue;
                OnPropertyChangedWithValue(WidthInt, "WidthInt");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseLength() => AdjustLength(1);
        public void ExecuteDecreaseLength() => AdjustLength(-1);
        public void ExecuteIncreaseLengthLarge() => AdjustLength(5);
        public void ExecuteDecreaseLengthLarge() => AdjustLength(-5);

        private void AdjustLength(int delta)
        {
            int newValue = Settings.CrosshairSettings.SizeVertical + delta;
            if (newValue >= 1 && newValue <= 75)
            {
                Settings.CrosshairSettings.SizeVertical = newValue;
                OnPropertyChangedWithValue(LengthInt, "LengthInt");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseOffset() => AdjustCrosshairOffset(1);
        public void ExecuteDecreaseOffset() => AdjustCrosshairOffset(-1);
        public void ExecuteIncreaseOffsetLarge() => AdjustCrosshairOffset(5);
        public void ExecuteDecreaseOffsetLarge() => AdjustCrosshairOffset(-5);

        private void AdjustCrosshairOffset(int delta)
        {
            int newValue = Settings.CrosshairSettings.Offset + delta;
            if (newValue >= 0 && newValue <= 15)
            {
                Settings.CrosshairSettings.Offset = newValue;
                OnPropertyChangedWithValue(OffsetInt, "OffsetInt");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseOpacity() => AdjustOpacity(0.05f);
        public void ExecuteDecreaseOpacity() => AdjustOpacity(-0.05f);
        public void ExecuteIncreaseOpacityLarge() => AdjustOpacity(0.1f);
        public void ExecuteDecreaseOpacityLarge() => AdjustOpacity(-0.1f);

        private void AdjustOpacity(float delta)
        {
            float newValue = Settings.CrosshairSettings.Opacity + delta;
            if (newValue >= 0f && newValue <= 1f)
            {
                Settings.CrosshairSettings.Opacity = (float)Math.Round(newValue, 2);
                OnPropertyChangedWithValue(OpacityFloat, "OpacityFloat");
                NotifyChanged();
            }
        }

        public void ExecuteSetColorRed() => SetColor("#FF0000FF");
        public void ExecuteSetColorGreen() => SetColor("#00FF00FF");
        public void ExecuteSetColorBlue() => SetColor("#0000FFFF");
        public void ExecuteSetColorWhite() => SetColor("#FFFFFFFF");
        public void ExecuteSetColorYellow() => SetColor("#FFFF00FF");
        public void ExecuteSetColorCyan() => SetColor("#00FFFFFF");
        public void ExecuteSetColorMagenta() => SetColor("#FF00FFFF");
        public void ExecuteSetColorOrange() => SetColor("#FFA500FF");

        private void SetColor(string color)
        {
            Settings.CrosshairSettings.Color = color;
            SelectCurrentColor();
            NotifyChanged();
        }

        public void ExecuteReset()
        {
            Settings.CrosshairSettings.CustomCrosshairEnabled = false;
            Settings.CrosshairSettings.SizeHorizontal = 30;
            Settings.CrosshairSettings.SizeVertical = 30;
            Settings.CrosshairSettings.Offset = 30;
            Settings.CrosshairSettings.Opacity = 1f;
            Settings.CrosshairSettings.Color = "#FF0000FF";
            SelectCurrentColor();
            RefreshAll();
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(CustomCrosshairEnabled, "CustomCrosshairEnabled");
            OnPropertyChangedWithValue(WidthInt, "WidthInt");
            OnPropertyChangedWithValue(LengthInt, "LengthInt");
            OnPropertyChangedWithValue(OffsetInt, "OffsetInt");
            OnPropertyChangedWithValue(OpacityFloat, "OpacityFloat");
            SelectCurrentColor();
            NotifyChanged();
        }
    }
}