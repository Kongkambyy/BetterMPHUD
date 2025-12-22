using System;
using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;
using BetterMPHUD.Services;
using TaleWorlds.Engine.GauntletUI;

namespace BetterMPHUD.Handlers
{
    public class TopBarHandler
    {
        private readonly WidgetCustomizer _customizer = new WidgetCustomizer();
        
        private TrackedWidget _timeAndScores;
        private TrackedWidget _avatars;
        private TrackedWidget _morale;
        private TrackedWidget _controlPoints;
        
        private Widget _enemyScoreWidget;
        private readonly List<Widget> _bannerWidgets = new List<Widget>();
        
        private bool _cached;

        public TopBarHandler()
        {
            ResetTrackedWidgets();
        }

        private void ResetTrackedWidgets()
        {
            _timeAndScores = new TrackedWidget { Element = HudElement.TimeAndScores };
            _avatars = new TrackedWidget { Element = HudElement.TeamAvatars };
            _morale = new TrackedWidget { Element = HudElement.Morale };
            _controlPoints = new TrackedWidget { Element = HudElement.Morale };
        }

        public void Apply(HudSettings settings, Mission mission)
        {
            MissionBehavior behavior = LayerFinder.FindBehaviorByName(mission, "MissionMultiplayerHUDExtension", "HUDExtension");
            if (behavior == null) return;

            GauntletLayer layer = LayerFinder.FindInBehavior(behavior);
            if (layer == null || layer.UIContext == null || layer.UIContext.Root == null) return;

            if (!_cached)
            {
                CacheWidgets(layer.UIContext.Root);
                _cached = true;
            }

            ApplyVisibility(settings);
            ApplyCustomization(settings);
        }

        private void CacheWidgets(Widget root)
        {
            _bannerWidgets.Clear();
            SearchWidgets(root, 0);
            
            if (_timeAndScores.Widget != null) _customizer.StoreChildrenOriginals(_timeAndScores.Widget);
            if (_avatars.Widget != null) _customizer.StoreChildrenOriginals(_avatars.Widget);
            if (_morale.Widget != null) _customizer.StoreChildrenOriginals(_morale.Widget);
            if (_controlPoints.Widget != null) _customizer.StoreChildrenOriginals(_controlPoints.Widget);
        }

        private void SearchWidgets(Widget widget, int depth)
        {
            if (depth > Constants.UI.MaxWidgetSearchDepth) return;

            TryCacheTimeAndScores(widget);
            TryCacheAvatars(widget);
            TryCacheEnemyScore(widget);
            TryCacheBanner(widget);
            TryCacheMorale(widget);
            TryCacheControlPoints(widget);

            for (int i = 0; i < widget.ChildCount; i++)
                SearchWidgets(widget.GetChild(i), depth + 1);
        }

        private void TryCacheTimeAndScores(Widget widget)
        {
            if (_timeAndScores.Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel != null && 
                widget.HorizontalAlignment == HorizontalAlignment.Center &&
                widget.VerticalAlignment == VerticalAlignment.Top &&
                widget.MarginTop >= 4 && widget.MarginTop <= 6)
            {
                _timeAndScores.Cache(widget);
            }
        }

        private void TryCacheAvatars(Widget widget)
        {
            if (_avatars.Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel != null && 
                widget.HeightSizePolicy == SizePolicy.Fixed &&
                widget.SuggestedHeight >= 74 && widget.SuggestedHeight <= 76 &&
                widget.ChildCount == 3)
            {
                _avatars.Cache(widget);
            }
        }

        private void TryCacheEnemyScore(Widget widget)
        {
            if (_enemyScoreWidget != null) return;
            if (widget.WidthSizePolicy == SizePolicy.Fixed &&
                widget.SuggestedWidth >= 9 && widget.SuggestedWidth <= 11 &&
                widget.MarginLeft >= 4 && widget.MarginLeft <= 6)
            {
                _enemyScoreWidget = widget;
            }
        }

        private void TryCacheBanner(Widget widget)
        {
            if (widget.WidthSizePolicy != SizePolicy.Fixed || widget.HeightSizePolicy != SizePolicy.Fixed) return;
            if (widget.SuggestedWidth < 49 || widget.SuggestedWidth > 51) return;
            if (widget.SuggestedHeight < 49 || widget.SuggestedHeight > 51) return;
            
            if (WidgetFinder.HasChildOfType(widget, "MaskedTextureWidget") && !_bannerWidgets.Contains(widget))
                _bannerWidgets.Add(widget);
        }

        private void TryCacheMorale(Widget widget)
        {
            if (_morale.Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel == null) return;
            
            bool isMorale = (widget.Sprite != null && widget.Sprite.Name != null && widget.Sprite.Name.Contains("morale")) ||
                           WidgetFinder.HasChildOfType(widget, "MoraleWidget");
            
            if (isMorale) _morale.Cache(widget);
        }

        private void TryCacheControlPoints(Widget widget)
        {
            if (_controlPoints.Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel != null &&
                widget.HorizontalAlignment == HorizontalAlignment.Center &&
                Math.Abs(widget.MarginTop - 80f) < 1f &&
                widget.ChildCount == 3)
            {
                _controlPoints.Cache(widget);
            }
        }

        private void ApplyVisibility(HudSettings settings)
        {
            if (_timeAndScores.Widget != null) _timeAndScores.Widget.IsVisible = settings.ShowTimeAndScores;
            if (_avatars.Widget != null) _avatars.Widget.IsVisible = settings.ShowAvatars;
            if (_enemyScoreWidget != null) _enemyScoreWidget.IsVisible = settings.ShowEnemyScore;
            foreach (Widget banner in _bannerWidgets) banner.IsVisible = settings.ShowBanners;
            if (_morale.Widget != null) _morale.Widget.IsVisible = settings.ShowMorale;
            if (_controlPoints.Widget != null) _controlPoints.Widget.IsVisible = settings.ShowMorale;
        }

        private void ApplyCustomization(HudSettings settings)
        {
            ApplyIfReady(_timeAndScores, settings.TimeAndScoresCustom);
            ApplyIfReady(_avatars, settings.TeamAvatarsCustom);
            ApplyIfReady(_morale, settings.MoraleCustom);
            ApplyIfReady(_controlPoints, settings.MoraleCustom);
        }

        private void ApplyIfReady(TrackedWidget tracked, ElementCustomization custom)
        {
            if (tracked.IsReady)
                _customizer.ApplyCustomization(tracked.Widget, tracked.Original, custom, true);
        }

        public void Reset()
        {
            _cached = false;
            ResetTrackedWidgets();
            _enemyScoreWidget = null;
            _bannerWidgets.Clear();
            _customizer.Clear();
        }
    }
}