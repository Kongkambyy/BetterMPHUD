using System;
using TaleWorlds.GauntletUI.BaseTypes;
using BetterMPHUD.Core;

namespace BetterMPHUD.Services
{
    public static class WidgetFinder
    {
        public static Widget FindById(Widget root, string id)
        {
            if (root.Id == id) return root;
            for (int i = 0; i < root.ChildCount; i++)
            {
                Widget result = FindById(root.GetChild(i), id);
                if (result != null) return result;
            }
            return null;
        }

        public static Widget FindByType(Widget root, string typeName)
        {
            if (root.GetType().Name == typeName) return root;
            for (int i = 0; i < root.ChildCount; i++)
            {
                Widget result = FindByType(root.GetChild(i), typeName);
                if (result != null) return result;
            }
            return null;
        }

        public static Widget FindBySprite(Widget root, string spriteNameContains)
        {
            if (root.Sprite != null && root.Sprite.Name != null && root.Sprite.Name.Contains(spriteNameContains))
                return root;
            for (int i = 0; i < root.ChildCount; i++)
            {
                Widget result = FindBySprite(root.GetChild(i), spriteNameContains);
                if (result != null) return result;
            }
            return null;
        }

        public static Widget FindByPredicate(Widget root, Func<Widget, bool> predicate, int maxDepth = 50)
        {
            return SearchRecursive(root, predicate, 0, maxDepth);
        }

        private static Widget SearchRecursive(Widget widget, Func<Widget, bool> predicate, int depth, int maxDepth)
        {
            if (depth > maxDepth) return null;
            if (predicate(widget)) return widget;
            
            for (int i = 0; i < widget.ChildCount; i++)
            {
                Widget result = SearchRecursive(widget.GetChild(i), predicate, depth + 1, maxDepth);
                if (result != null) return result;
            }
            return null;
        }

        public static string GetBrushName(Widget widget)
        {
            try
            {
                var brushProperty = widget.GetType().GetProperty("Brush");
                if (brushProperty != null)
                {
                    object brush = brushProperty.GetValue(widget);
                    if (brush != null) return brush.ToString();
                }
            }
            catch { }
            return null;
        }

        public static bool HasChildOfType(Widget widget, string typeName)
        {
            for (int i = 0; i < widget.ChildCount; i++)
            {
                if (widget.GetChild(i).GetType().Name == typeName) 
                    return true;
            }
            return false;
        }

        public static bool HasChildWithSprite(Widget widget, string spriteContains)
        {
            for (int i = 0; i < widget.ChildCount; i++)
            {
                Widget child = widget.GetChild(i);
                if (child.Sprite != null && child.Sprite.Name != null && child.Sprite.Name.Contains(spriteContains)) 
                    return true;
            }
            return false;
        }
    }
}