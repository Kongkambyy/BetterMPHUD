using TaleWorlds.Library;

namespace BetterMPHUD.Core
{
    public static class Constants
    {
        public static class Colors
        {
            public static readonly Color FriendlyKill = new Color(0.27f, 1f, 0.27f, 1f);
            public static readonly Color EnemyKill = new Color(1f, 0.27f, 0.27f, 1f);
            public static readonly Color Neutral = new Color(1f, 1f, 1f, 1f);
        }

        public static class Sprites
        {
            public const string Sword = "icon_sword";
            public const string Bow = "icon_bow";
            public const string Horse = "icon_horse";
            public const string Death = "icon_death";
        }

        public static class Adjustment
        {
            public const float PositionStep = 10f;
            public const float PositionStepLarge = 50f;
            public const float ScaleStep = 0.1f;
            public const float ScaleStepLarge = 0.25f;
            public const float MinScale = 0.5f;
            public const float MaxScale = 2.0f;
            public const float FadeoutStep = 1f;
            public const float MinFadeout = 1f;
            public const float MaxFadeout = 30f;
            public const float DefaultFadeout = 8f;
        }

        public static class Killfeed
        {
            public const int MaxEntries = 15;
            public const int BaseFont = 20;
            public const int BaseIcon = 24;
            public const int BaseSkull = 34;
            public const int BaseRow = 32;
            public const float MinScale = 0.1f;
        }

        public static class UI
        {
            public const float HideOffset = 10000f;
            public const float SettingsEnforceInterval = 1.0f;
            public const int MaxWidgetSearchDepth = 50;
        }
    }
}