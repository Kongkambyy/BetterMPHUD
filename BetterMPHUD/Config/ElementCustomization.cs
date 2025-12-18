using System;

namespace BetterMPHUD
{
    [Serializable]
    public class ElementCustomization
    {
        public float OffsetX { get; set; } = 0f;
        public float OffsetY { get; set; } = 0f;
        public float Scale { get; set; } = 1f;

        public ElementCustomization() { }

        public ElementCustomization(float offsetX, float offsetY, float scale)
        {
            OffsetX = offsetX;
            OffsetY = offsetY;
            Scale = scale;
        }

        public void Reset()
        {
            OffsetX = 0f;
            OffsetY = 0f;
            Scale = 1f;
        }
    }

    public enum HudElement
    {
        TimeAndScores,
        TeamAvatars,
        Morale
    }
}