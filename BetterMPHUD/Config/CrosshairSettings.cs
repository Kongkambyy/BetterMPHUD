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

        public CrosshairSettings() { }

        public void Reset()
        {
            CustomCrosshairEnabled = false;
            SizeHorizontal = 10;
            SizeVertical = 10;
            Offset = 10;
            Opacity = 1f;
            Color = "#FF0000FF";
        }
    }
}