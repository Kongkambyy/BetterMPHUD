using System;

namespace BetterMPHUD
{
    [Serializable]
    public class CrosshairSettings
    {
        public bool CustomCrosshairEnabled { get; set; } = false;
        public int SizeHorizontal { get; set; } = 10;
        public int SizeVertical { get; set; } = 10;
        public int Offset { get; set; } = 10;
        public float Opacity { get; set; } = 1f;
        public string Color { get; set; } = "#FF0000FF";
        
        public bool DotEnabled { get; set; } = false;
        public string DotColor { get; set; } = "#FFFFFFFF";
        public int DotSizeWidth { get; set; } = 4;
        public int DotSizeHeight { get; set; } = 4;
        public bool DotIsCircular { get; set; } = true; // NEW: true = circular, false = square

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
            DotSizeWidth = 4;
            DotSizeHeight = 4;
            DotIsCircular = true;
        }
    }
}