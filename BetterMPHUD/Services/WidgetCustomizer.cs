using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using BetterMPHUD.Core;

namespace BetterMPHUD.Services
{
    public class WidgetCustomizer
    {
        private readonly Dictionary<Widget, WidgetOriginalValues> _childOriginals = new Dictionary<Widget, WidgetOriginalValues>();
        public void StoreChildrenOriginals(Widget parent)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (!_childOriginals.ContainsKey(child))
                    _childOriginals[child] = WidgetOriginalValues.Capture(child);
                StoreChildrenOriginals(child);
            }
        }

        public void ApplyCustomization(Widget widget, WidgetOriginalValues original, 
            ElementCustomization custom, bool isVisible)
        {
            float targetX = original.X + custom.OffsetX;
            float targetY = original.Y + custom.OffsetY;

            if (!isVisible)
            {
                targetX += Constants.UI.HideOffset;
                targetY += Constants.UI.HideOffset;
            }

            widget.PositionXOffset = targetX;
            widget.PositionYOffset = targetY;

            if (custom.Scale != 1f)
            {
                ApplyScale(widget, original, custom.Scale);
                ApplyScaleToChildren(widget, custom.Scale);
            }
            else
            {
                ResetScale(widget, original);
                ResetChildrenScale(widget);
            }
        }

        public void ApplyPositionOnly(Widget widget, WidgetOriginalValues original, 
            ElementCustomization custom)
        {
            widget.PositionXOffset = original.X + custom.OffsetX;
            widget.PositionYOffset = original.Y + custom.OffsetY;
        }

        private void ApplyScale(Widget widget, WidgetOriginalValues original, float scale)
        {
            if (widget.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0)
                widget.SuggestedWidth = original.Width * scale;
            if (widget.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0)
                widget.SuggestedHeight = original.Height * scale;
        }

        private void ResetScale(Widget widget, WidgetOriginalValues original)
        {
            if (widget.WidthSizePolicy == SizePolicy.Fixed)
                widget.SuggestedWidth = original.Width;
            if (widget.HeightSizePolicy == SizePolicy.Fixed)
                widget.SuggestedHeight = original.Height;
        }

        private void ApplyScaleToChildren(Widget parent, float scale)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (_childOriginals.TryGetValue(child, out var original))
                {
                    ApplyScale(child, original, scale);
                    child.MarginTop = original.MarginTop * scale;
                    child.MarginBottom = original.MarginBottom * scale;
                    child.MarginLeft = original.MarginLeft * scale;
                    child.MarginRight = original.MarginRight * scale;
                }
                ApplyScaleToChildren(child, scale);
            }
        }

        private void ResetChildrenScale(Widget parent)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (_childOriginals.TryGetValue(child, out var original))
                {
                    ResetScale(child, original);
                    child.MarginTop = original.MarginTop;
                    child.MarginBottom = original.MarginBottom;
                    child.MarginLeft = original.MarginLeft;
                    child.MarginRight = original.MarginRight;
                }
                ResetChildrenScale(child);
            }
        }

        public void Clear() => _childOriginals.Clear();
    }
}