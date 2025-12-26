using System;

namespace BetterMPHUD
{
    [Serializable]
    public class CrosshairSettings
    {
        public bool CustomCrosshairEnabled { get; set; } = false;
        public int SizeHorizontal { get; set; } = 30;
        public int SizeVertical { get; set; } = 30;
        public int Offset { get; set; } = 30;
        public float Opacity { get; set; } = 1f;
        public string Color { get; set; } = "#FF0000FF";
        
        public bool DotEnabled { get; set; } = false;
        public string DotColor { get; set; } = "#FFFFFFFF";
        public int DotSizeWidth { get; set; } = 6;
        public int DotSizeHeight { get; set; } = 6;
        public bool DotIsCircular { get; set; } = true;

        public CrosshairSettings() { }

        public void Reset()
        {
            CustomCrosshairEnabled = false;
            SizeHorizontal = 10;
            SizeVertical = 10;
            Offset = 10;
            Opacity = 1f;
            Color = "#FF0000FF";
            DotEnabled = false;
            DotColor = "#FFFFFFFF";
            DotSizeWidth = 6;
            DotSizeHeight = 6;
            DotIsCircular = true;
        }
    }
}