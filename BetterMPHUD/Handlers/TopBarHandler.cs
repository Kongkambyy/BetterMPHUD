using System;
using System.Collections.Generic;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;
using BetterMPHUD.Services;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Layout;
using TaleWorlds.Library;

namespace BetterMPHUD.Handlers
{
    public class TopBarHandler
    {
        private readonly WidgetCustomizer _customizer = new WidgetCustomizer();
        
        private TrackedWidget _timeAndScores;
        private TrackedWidget _morale;
        private TrackedWidget _controlPoints;
        private TrackedWidget _powerLevel;
        private TrackedWidget _avatarsContainer;
        
        private Widget _allyAvatarsSide;
        private Widget _enemyAvatarsSide;
        private WidgetOriginalValues _allyAvatarsOriginal;
        private WidgetOriginalValues _enemyAvatarsOriginal;
        private Dictionary<Widget, WidgetOriginalValues> _allyAvatarChildOriginals = new Dictionary<Widget, WidgetOriginalValues>();
        private Dictionary<Widget, WidgetOriginalValues> _enemyAvatarChildOriginals = new Dictionary<Widget, WidgetOriginalValues>();
        
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
            _morale = new TrackedWidget { Element = HudElement.Morale };
            _controlPoints = new TrackedWidget { Element = HudElement.Morale };
            _powerLevel = new TrackedWidget { Element = HudElement.Morale };
            _avatarsContainer = new TrackedWidget { Element = HudElement.TeamAvatars };
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
            if (_morale.Widget != null) _customizer.StoreChildrenOriginals(_morale.Widget);
            if (_controlPoints.Widget != null) _customizer.StoreChildrenOriginals(_controlPoints.Widget);
            if (_powerLevel.Widget != null) _customizer.StoreChildrenOriginals(_powerLevel.Widget);
        }

        private void SearchWidgets(Widget widget, int depth)
        {
            if (depth > Constants.UI.MaxWidgetSearchDepth) return;

            TryCacheTimeAndScores(widget);
            TryCacheAvatarsContainer(widget);
            TryCacheAvatarSides(widget);
            TryCacheEnemyScore(widget);
            TryCacheBanner(widget);
            TryCacheMorale(widget);
            TryCacheControlPoints(widget);
            TryCachePowerLevel(widget);

            for (int i = 0; i < widget.ChildCount; i++)
                SearchWidgets(widget.GetChild(i), depth + 1);
        }
        
        private void TryCacheAvatarsContainer(Widget widget)
        {
            if (_avatarsContainer.Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel != null && 
                widget.HeightSizePolicy == SizePolicy.Fixed &&
                widget.SuggestedHeight >= 74 && widget.SuggestedHeight <= 76 &&
                widget.ChildCount == 3)
            {
                _avatarsContainer.Cache(widget);
            }
        }

        private void TryCacheAvatarSides(Widget widget)
        {
            string typeName = widget.GetType().Name;
            if (!typeName.Contains("AvatarsSide") && !typeName.Contains("MultiplayerTeamAvatars"))
                return;
    
            ListPanel listPanel = widget as ListPanel;
            if (listPanel == null) return;

            if (widget.HorizontalAlignment == HorizontalAlignment.Right)
            {
                if (_allyAvatarsSide == null)
                {
                    _allyAvatarsSide = widget;
                    _allyAvatarsOriginal = WidgetOriginalValues.Capture(widget);
                }
            }
            else if (widget.HorizontalAlignment == HorizontalAlignment.Left)
            {
                if (_enemyAvatarsSide == null)
                {
                    _enemyAvatarsSide = widget;
                    _enemyAvatarsOriginal = WidgetOriginalValues.Capture(widget);
                }
            }
        }

        private void TryCachePowerLevel(Widget widget)
        {
            if (_powerLevel.Widget != null) return;
    
            if (widget.HeightSizePolicy == SizePolicy.Fixed &&
                widget.SuggestedHeight >= 29 && widget.SuggestedHeight <= 31 &&
                widget.MarginTop >= 19 && widget.MarginTop <= 21 &&
                widget.HorizontalAlignment == HorizontalAlignment.Center)
            {
                if (WidgetFinder.FindByType(widget, "FillBarWidget") != null)
                {
                    _powerLevel.Cache(widget);
                    _customizer.StoreChildrenOriginals(widget);
                }
            }
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
            if (_avatarsContainer.Widget != null) _avatarsContainer.Widget.IsVisible = settings.ShowAvatars;
            if (_enemyScoreWidget != null) _enemyScoreWidget.IsVisible = settings.ShowEnemyScore;
            foreach (Widget banner in _bannerWidgets) banner.IsVisible = settings.ShowBanners;
    
            bool showMorale = settings.ShowMorale && !IsInWarmup();
            if (_morale.Widget != null) _morale.Widget.IsVisible = showMorale;
            if (_controlPoints.Widget != null) _controlPoints.Widget.IsVisible = showMorale;
        }
        
        private bool IsInWarmup()
        {
            if (Mission.Current == null) return false;
    
            try
            {
                var warmupComponent = Mission.Current.GetMissionBehavior<TaleWorlds.MountAndBlade.MultiplayerWarmupComponent>();
                if (warmupComponent != null)
                {
                    return warmupComponent.IsInWarmup;
                }
            }
            catch { }
    
            return false;
        }

        private void ApplyCustomization(HudSettings settings)
        {
            ApplyIfReady(_timeAndScores, settings.TimeAndScoresCustom);
            ApplyIfReady(_morale, settings.MoraleCustom);
            ApplyIfReady(_controlPoints, settings.MoraleCustom);
            ApplyIfReady(_powerLevel, settings.PowerLevelCustom);
    
            ApplyAvatarSide(_allyAvatarsSide, _allyAvatarsOriginal, _allyAvatarChildOriginals, 
                settings.AllyAvatarsCustom, settings.AllyAvatarsVertical, settings.AllyAvatarsSpacing);
            ApplyAvatarSide(_enemyAvatarsSide, _enemyAvatarsOriginal, _enemyAvatarChildOriginals, 
                settings.EnemyAvatarsCustom, settings.EnemyAvatarsVertical, settings.EnemyAvatarsSpacing);
        }

        private void ApplyAvatarSide(Widget widget, WidgetOriginalValues original, 
            Dictionary<Widget, WidgetOriginalValues> childOriginals,
            ElementCustomization custom, bool isVertical, float spacing)
        {
            if (widget == null) return;
    
            if (!original.IsValid)
                original = WidgetOriginalValues.Capture(widget);

            widget.PositionXOffset = original.X + custom.OffsetX;
            widget.PositionYOffset = original.Y + custom.OffsetY;

            ListPanel listPanel = widget as ListPanel;
            if (listPanel != null)
            {
                if (isVertical)
                    listPanel.StackLayout.LayoutMethod = LayoutMethod.VerticalBottomToTop;
                else
                    listPanel.StackLayout.LayoutMethod = LayoutMethod.HorizontalLeftToRight;
            }

            ApplyScaleToWidget(widget, childOriginals, custom.Scale);
    
            for (int i = 0; i < widget.ChildCount; i++)
                ApplyScaleRecursive(widget.GetChild(i), childOriginals, custom.Scale, isVertical, spacing, true);
        }
        
        private void ApplyScaleToWidget(Widget widget, Dictionary<Widget, WidgetOriginalValues> childOriginals, float scale)
        {
            if (!childOriginals.ContainsKey(widget))
                childOriginals[widget] = WidgetOriginalValues.Capture(widget);

            WidgetOriginalValues original = childOriginals[widget];

            if (widget.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0)
                widget.SuggestedWidth = original.Width * scale;
            if (widget.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0)
                widget.SuggestedHeight = original.Height * scale;

            widget.MarginTop = original.MarginTop * scale;
            widget.MarginBottom = original.MarginBottom * scale;
            widget.MarginLeft = original.MarginLeft * scale;
            widget.MarginRight = original.MarginRight * scale;
        }


        private void ApplyScaleRecursive(Widget widget, Dictionary<Widget, WidgetOriginalValues> childOriginals, 
            float scale, bool isVertical, float spacing, bool isDirectChild = false)
        {
            if (!childOriginals.ContainsKey(widget))
                childOriginals[widget] = WidgetOriginalValues.Capture(widget);

            WidgetOriginalValues original = childOriginals[widget];

            if (widget.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0)
                widget.SuggestedWidth = original.Width * scale;
            if (widget.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0)
                widget.SuggestedHeight = original.Height * scale;

            if (isDirectChild)
            {
                if (isVertical)
                {
                    widget.MarginTop = (original.MarginTop * scale) + spacing;
                    widget.MarginBottom = (original.MarginBottom * scale) + spacing;
                    widget.MarginLeft = original.MarginLeft * scale;
                    widget.MarginRight = original.MarginRight * scale;
                }
                else
                {
                    widget.MarginLeft = (original.MarginLeft * scale) + spacing;
                    widget.MarginRight = (original.MarginRight * scale) + spacing;
                    widget.MarginTop = original.MarginTop * scale;
                    widget.MarginBottom = original.MarginBottom * scale;
                }
            }
            else
            {
                widget.MarginTop = original.MarginTop * scale;
                widget.MarginBottom = original.MarginBottom * scale;
                widget.MarginLeft = original.MarginLeft * scale;
                widget.MarginRight = original.MarginRight * scale;
            }

            TextWidget textWidget = widget as TextWidget;
            if (textWidget != null && textWidget.Brush != null && original.FontSize > 0)
                textWidget.Brush.FontSize = (int)(original.FontSize * scale);

            for (int i = 0; i < widget.ChildCount; i++)
                ApplyScaleRecursive(widget.GetChild(i), childOriginals, scale, isVertical, spacing, false);
        }


        private void ApplyIfReady(TrackedWidget tracked, ElementCustomization custom)
        {
            if (tracked.IsReady)
                _customizer.ApplyCustomization(tracked.Widget, tracked.Original, custom, true);
        }
        
        public void CleanupDisconnectedAvatars()
        {
            var connectedNames = GetConnectedPlayerNames();
            RemoveDisconnectedFromSide(_allyAvatarsSide, connectedNames);
            RemoveDisconnectedFromSide(_enemyAvatarsSide, connectedNames);
        }

        private HashSet<string> GetConnectedPlayerNames()
        {
            var names = new HashSet<string>();
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.IsSynchronized)
                {
                    MissionPeer mp = peer.GetComponent<MissionPeer>();
                    if (mp != null && mp.Team != null && mp.Team.Side != BattleSideEnum.None)
                        names.Add(mp.DisplayedName);
                }
            }
            return names;
        }

        private void RemoveDisconnectedFromSide(Widget avatarSide, HashSet<string> connectedNames)
        {
            if (avatarSide == null) return;
    
            for (int i = 0; i < avatarSide.ChildCount; i++)
            {
                Widget avatarWidget = avatarSide.GetChild(i);
                string playerName = FindPlayerNameInWidget(avatarWidget);
        
                if (!string.IsNullOrEmpty(playerName) && !connectedNames.Contains(playerName))
                    avatarWidget.IsVisible = false;
            }
        }

        private string FindPlayerNameInWidget(Widget widget)
        {
            RichTextWidget richText = widget as RichTextWidget;
            if (richText != null && !string.IsNullOrEmpty(richText.Text))
                return richText.Text;
    
            TextWidget textWidget = widget as TextWidget;
            if (textWidget != null && !string.IsNullOrEmpty(textWidget.Text))
                return textWidget.Text;
    
            for (int i = 0; i < widget.ChildCount; i++)
            {
                string result = FindPlayerNameInWidget(widget.GetChild(i));
                if (!string.IsNullOrEmpty(result))
                    return result;
            }
    
            return null;
        }

        public void Reset()
        {
            _cached = false;
            ResetTrackedWidgets();
            _enemyScoreWidget = null;
            _bannerWidgets.Clear();
            _customizer.Clear();
            _allyAvatarsSide = null;
            _enemyAvatarsSide = null;
            _allyAvatarsOriginal = default(WidgetOriginalValues);
            _enemyAvatarsOriginal = default(WidgetOriginalValues);
            _allyAvatarChildOriginals?.Clear();
            _enemyAvatarChildOriginals?.Clear();
        }
    }
}