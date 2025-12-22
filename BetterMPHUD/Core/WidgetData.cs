using TaleWorlds.GauntletUI.BaseTypes;

namespace BetterMPHUD.Core
{
    public struct WidgetOriginalValues
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public float MarginTop;
        public float MarginBottom;
        public float MarginLeft;
        public float MarginRight;
        public float FontSize;
        public bool IsValid;

        public static WidgetOriginalValues Capture(Widget widget)
        {
            float fontSize = 0f;
            TextWidget textWidget = widget as TextWidget;
            if (textWidget != null && textWidget.Brush != null)
                fontSize = textWidget.Brush.FontSize;

            return new WidgetOriginalValues
            {
                X = widget.PositionXOffset,
                Y = widget.PositionYOffset,
                Width = widget.SuggestedWidth,
                Height = widget.SuggestedHeight,
                MarginTop = widget.MarginTop,
                MarginBottom = widget.MarginBottom,
                MarginLeft = widget.MarginLeft,
                MarginRight = widget.MarginRight,
                FontSize = fontSize,
                IsValid = true
            };
        }
    }

    public class TrackedWidget
    {
        public Widget Widget { get; set; }
        public WidgetOriginalValues Original { get; set; }
        public HudElement Element { get; set; }

        public bool IsReady
        {
            get { return Widget != null && Original.IsValid; }
        }

        public void Cache(Widget widget)
        {
            Widget = widget;
            if (widget != null)
                Original = WidgetOriginalValues.Capture(widget);
        }
    }
}