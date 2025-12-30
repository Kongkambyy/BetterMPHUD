using System;
using TaleWorlds.Library;

namespace BetterMPHUD.ViewModels.Settings
{
    public class DotSettingsVM : BaseSettingsVM
    {
        private MBBindingList<ColorItemVM> _colorList;

        public DotSettingsVM(HudSettings settings, Action onSettingsChanged) 
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
            Settings.CrosshairSettings.DotColor = selected.ColorHex;
            NotifyChanged();
        }

        private void SelectCurrentColor()
        {
            if (_colorList == null) return;
            string currentColor = Settings.CrosshairSettings.DotColor ?? "#FFFFFFFF";
            foreach (var item in _colorList)
                item.IsSelected = string.Equals(item.ColorHex, currentColor, StringComparison.OrdinalIgnoreCase);
        }

        [DataSourceProperty]
        public MBBindingList<ColorItemVM> ColorList => _colorList;

        [DataSourceProperty]
        public bool DotEnabled
        {
            get => Settings.CrosshairSettings.DotEnabled;
            set
            {
                if (Settings.CrosshairSettings.DotEnabled != value)
                {
                    Settings.CrosshairSettings.DotEnabled = value;
                    OnPropertyChangedWithValue(value, "DotEnabled");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public bool DotIsCircular
        {
            get => Settings.CrosshairSettings.DotIsCircular;
            set
            {
                if (Settings.CrosshairSettings.DotIsCircular != value)
                {
                    Settings.CrosshairSettings.DotIsCircular = value;
                    OnPropertyChangedWithValue(value, "DotIsCircular");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public int WidthInt
        {
            get => Settings.CrosshairSettings.DotSizeWidth;
            set
            {
                if (Settings.CrosshairSettings.DotSizeWidth != value)
                {
                    Settings.CrosshairSettings.DotSizeWidth = value;
                    OnPropertyChangedWithValue(value, "WidthInt");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public int HeightInt
        {
            get => Settings.CrosshairSettings.DotSizeHeight;
            set
            {
                if (Settings.CrosshairSettings.DotSizeHeight != value)
                {
                    Settings.CrosshairSettings.DotSizeHeight = value;
                    OnPropertyChangedWithValue(value, "HeightInt");
                    NotifyChanged();
                }
            }
        }

        [DataSourceProperty]
        public string WidthText => Settings.CrosshairSettings.DotSizeWidth.ToString();

        [DataSourceProperty]
        public string HeightText => Settings.CrosshairSettings.DotSizeHeight.ToString();

        [DataSourceProperty]
        public string ColorText => Settings.CrosshairSettings.DotColor;

        [DataSourceProperty]
        public string OffsetXText => Settings.CrosshairSettings.DotOffsetX.ToString();

        [DataSourceProperty]
        public string OffsetYText => Settings.CrosshairSettings.DotOffsetY.ToString();

        public void ExecuteIncreaseWidth() => AdjustWidth(1);
        public void ExecuteDecreaseWidth() => AdjustWidth(-1);
        public void ExecuteIncreaseWidthLarge() => AdjustWidth(2);
        public void ExecuteDecreaseWidthLarge() => AdjustWidth(-2);

        private void AdjustWidth(int delta)
        {
            int newValue = Settings.CrosshairSettings.DotSizeWidth + delta;
            if (newValue >= 1 && newValue <= 20)
            {
                Settings.CrosshairSettings.DotSizeWidth = newValue;
                OnPropertyChangedWithValue(WidthInt, "WidthInt");
                OnPropertyChangedWithValue(WidthText, "WidthText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseHeight() => AdjustHeight(1);
        public void ExecuteDecreaseHeight() => AdjustHeight(-1);
        public void ExecuteIncreaseHeightLarge() => AdjustHeight(2);
        public void ExecuteDecreaseHeightLarge() => AdjustHeight(-2);

        private void AdjustHeight(int delta)
        {
            int newValue = Settings.CrosshairSettings.DotSizeHeight + delta;
            if (newValue >= 1 && newValue <= 20)
            {
                Settings.CrosshairSettings.DotSizeHeight = newValue;
                OnPropertyChangedWithValue(HeightInt, "HeightInt");
                OnPropertyChangedWithValue(HeightText, "HeightText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseOffsetX() => AdjustOffsetX(1);
        public void ExecuteDecreaseOffsetX() => AdjustOffsetX(-1);
        public void ExecuteIncreaseOffsetXLarge() => AdjustOffsetX(5);
        public void ExecuteDecreaseOffsetXLarge() => AdjustOffsetX(-5);

        private void AdjustOffsetX(int delta)
        {
            int newValue = Settings.CrosshairSettings.DotOffsetX + delta;
            if (newValue >= -200 && newValue <= 200)
            {
                Settings.CrosshairSettings.DotOffsetX = newValue;
                OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
                NotifyChanged();
            }
        }

        public void ExecuteIncreaseOffsetY() => AdjustOffsetY(1);
        public void ExecuteDecreaseOffsetY() => AdjustOffsetY(-1);
        public void ExecuteIncreaseOffsetYLarge() => AdjustOffsetY(5);
        public void ExecuteDecreaseOffsetYLarge() => AdjustOffsetY(-5);

        private void AdjustOffsetY(int delta)
        {
            int newValue = Settings.CrosshairSettings.DotOffsetY + delta;
            if (newValue >= -200 && newValue <= 200)
            {
                Settings.CrosshairSettings.DotOffsetY = newValue;
                OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
                NotifyChanged();
            }
        }

        public void ExecuteSetCircular()
        {
            DotIsCircular = true;
        }

        public void ExecuteSetSquare()
        {
            DotIsCircular = false;
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
            Settings.CrosshairSettings.DotColor = color;
            SelectCurrentColor();
            OnPropertyChangedWithValue(ColorText, "ColorText");
            NotifyChanged();
        }

        public void ExecuteReset()
        {
            Settings.CrosshairSettings.DotEnabled = false;
            Settings.CrosshairSettings.DotColor = "#FFFFFFFF";
            Settings.CrosshairSettings.DotSizeWidth = 6;
            Settings.CrosshairSettings.DotSizeHeight = 6;
            Settings.CrosshairSettings.DotIsCircular = true;
            Settings.CrosshairSettings.DotOffsetX = 0;
            Settings.CrosshairSettings.DotOffsetY = 0;
            SelectCurrentColor();
            RefreshAll();
        }

        public override void RefreshAll()
        {
            OnPropertyChangedWithValue(DotEnabled, "DotEnabled");
            OnPropertyChangedWithValue(DotIsCircular, "DotIsCircular");
            OnPropertyChangedWithValue(WidthInt, "WidthInt");
            OnPropertyChangedWithValue(HeightInt, "HeightInt");
            OnPropertyChangedWithValue(WidthText, "WidthText");
            OnPropertyChangedWithValue(HeightText, "HeightText");
            OnPropertyChangedWithValue(ColorText, "ColorText");
            OnPropertyChangedWithValue(OffsetXText, "OffsetXText");
            OnPropertyChangedWithValue(OffsetYText, "OffsetYText");
            SelectCurrentColor();
            NotifyChanged();
        }
    }
}