using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private bool _sortingEnabled = false;
        
        private bool _betterAvatarsApplied = false;
        
        private Widget _enemyScoreWidget;
        private readonly List<Widget> _bannerWidgets = new List<Widget>();
        
        private bool _cached;

        public TopBarHandler()
        {
            ResetTrackedWidgets();
        }

        public void ApplyAvatarSorting(bool enabled)
        {
            _sortingEnabled = enabled;
    
            if (enabled)
            {
                SortAvatarSide(_allyAvatarsSide);
                SortAvatarSide(_enemyAvatarsSide);
            }
        }
        
        private void SortAvatarSide(Widget avatarSide)
        {
            if (avatarSide == null || avatarSide.ChildCount <= 1) return;

            var avatarData = new List<(Widget widget, HudSettings.AvatarClassType classType)>();
    
            for (int i = 0; i < avatarSide.ChildCount; i++)
            {
                Widget avatar = avatarSide.GetChild(i);
                HudSettings.AvatarClassType classType = GetAvatarClassType(avatar);
                avatarData.Add((avatar, classType));
            }

            avatarData.Sort((a, b) => a.classType.CompareTo(b.classType));
            
            var sortedWidgets = avatarData.Select(x => x.widget).ToList();
            while (avatarSide.ChildCount > 0)
            {
                avatarSide.RemoveChild(avatarSide.GetChild(0));
            }
    
            foreach (var widget in sortedWidgets)
            {
                avatarSide.AddChild(widget);
            }
        }
        
        private HudSettings.AvatarClassType GetAvatarClassType(Widget avatarWidget)
        {
            string spriteName = FindClassSpriteNameRecursive(avatarWidget);
    
            if (string.IsNullOrEmpty(spriteName))
                return HudSettings.AvatarClassType.Unknown;

            string lower = spriteName.ToLower();

            if (lower.Contains("cavalry") || lower.Contains("horsearcher"))
                return HudSettings.AvatarClassType.Cavalry;
    
            if (lower.Contains("archer") || lower.Contains("crossbow"))
                return HudSettings.AvatarClassType.Archer;
    
            if (lower.Contains("infantry"))
                return HudSettings.AvatarClassType.Infantry;

            return HudSettings.AvatarClassType.Unknown;
        }
        
        private string FindClassSpriteNameRecursive(Widget widget)
        {
            if (widget.Sprite != null && widget.Sprite.Name != null)
            {
                if (widget.Sprite.Name.Contains("TroopIcons"))
                    return widget.Sprite.Name;
            }

            string typeName = widget.GetType().Name;
            if (typeName.Contains("TroopType") || typeName.Contains("TroopIcon"))
            {
                if (widget.Sprite != null && widget.Sprite.Name != null)
                    return widget.Sprite.Name;
            }

            for (int i = 0; i < widget.ChildCount; i++)
            {
                string result = FindClassSpriteNameRecursive(widget.GetChild(i));
                if (!string.IsNullOrEmpty(result))
                    return result;
            }

            return null;
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

            if (settings.AvatarSortingEnabled)
            {
                SortAvatarSide(_allyAvatarsSide);
                SortAvatarSide(_enemyAvatarsSide);
            }
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

            if (widget is ListPanel listPanel)
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

        private HashSet<string> GetConnectedPlayerNames()
        {
            var names = new HashSet<string>();

            if (Mission.Current == null) return names;

            try
            {
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    if (networkPeer == null) continue;
            
                    MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
                    if (missionPeer != null && !string.IsNullOrEmpty(missionPeer.DisplayedName))
                    {
                        names.Add(missionPeer.DisplayedName);
                    }
                }
            }
            catch { }

            if (names.Count == 0)
            {
                if (Mission.Current.PlayerTeam != null)
                {
                    foreach (Agent agent in Mission.Current.PlayerTeam.ActiveAgents)
                    {
                        if (agent != null && agent.IsHuman && agent.MissionPeer != null)
                        {
                            if (!string.IsNullOrEmpty(agent.MissionPeer.DisplayedName))
                                names.Add(agent.MissionPeer.DisplayedName);
                        }
                    }
                }

                if (Mission.Current.PlayerEnemyTeam != null)
                {
                    foreach (Agent agent in Mission.Current.PlayerEnemyTeam.ActiveAgents)
                    {
                        if (agent != null && agent.IsHuman && agent.MissionPeer != null)
                        {
                            if (!string.IsNullOrEmpty(agent.MissionPeer.DisplayedName))
                                names.Add(agent.MissionPeer.DisplayedName);
                        }
                    }
                }
            }

            return names;
        }
        
        public void RestoreAllAvatars()
        {
            RestoreAvatarSide(_allyAvatarsSide);
            RestoreAvatarSide(_enemyAvatarsSide);
        }

        private void RestoreAvatarSide(Widget avatarSide)
        {
            if (avatarSide == null) return;

            for (int i = 0; i < avatarSide.ChildCount; i++)
            {
                avatarSide.GetChild(i).IsVisible = true;
            }
        }

        public void CleanupDisconnectedAvatars()
        {
            var connectedNames = GetConnectedPlayerNames();
            RemoveDisconnectedFromSide(_allyAvatarsSide, connectedNames);
            RemoveDisconnectedFromSide(_enemyAvatarsSide, connectedNames);
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
        
        public void ApplyBetterAvatars(bool enabled)
        {
            if (enabled)
                EnableBetterAvatars();
            else
                DisableBetterAvatars();
    
            _betterAvatarsApplied = enabled;
        }

        private void EnableBetterAvatars()
        {
            ProcessAvatarSide(_allyAvatarsSide, true);
            ProcessAvatarSide(_enemyAvatarsSide, true);
        }

        private void DisableBetterAvatars()
        {
            ProcessAvatarSide(_allyAvatarsSide, false);
            ProcessAvatarSide(_enemyAvatarsSide, false);
        }

        private void ProcessAvatarSide(Widget avatarSide, bool simplify)
        {
            if (avatarSide == null) return;
    
            for (int i = 0; i < avatarSide.ChildCount; i++)
            {
                Widget avatarWidget = avatarSide.GetChild(i);
                ProcessSingleAvatar(avatarWidget, simplify);
            }
        }

        private Dictionary<Widget, WidgetOriginalValues> _avatarOriginals = new Dictionary<Widget, WidgetOriginalValues>();

        private void ProcessSingleAvatar(Widget avatarWidget, bool simplify)
        {
            Widget steamAvatar = null;
            Widget circleBackground = null;
            Widget troopIcon = null;
            Widget iconForeground = null;
            Widget compassElement = null;
            Widget smallAvatarImage = null;

            FindAvatarComponentsRecursive(avatarWidget, ref steamAvatar, ref circleBackground, ref troopIcon,
                ref iconForeground, ref compassElement, ref smallAvatarImage);

            if (simplify)
            {
                if (troopIcon != null && !_avatarOriginals.ContainsKey(troopIcon))
                    _avatarOriginals[troopIcon] = WidgetOriginalValues.Capture(troopIcon);
                if (iconForeground != null && !_avatarOriginals.ContainsKey(iconForeground))
                    _avatarOriginals[iconForeground] = WidgetOriginalValues.Capture(iconForeground);
                if (compassElement != null && !_avatarOriginals.ContainsKey(compassElement))
                    _avatarOriginals[compassElement] = WidgetOriginalValues.Capture(compassElement);

                if (steamAvatar != null)
                    steamAvatar.IsVisible = false;

                if (circleBackground != null)
                    circleBackground.IsVisible = false;

                if (smallAvatarImage != null)
                    smallAvatarImage.IsVisible = false;

                if (compassElement != null)
                {
                    if (compassElement.WidthSizePolicy == SizePolicy.Fixed)
                        compassElement.SuggestedWidth = 60;
                    if (compassElement.HeightSizePolicy == SizePolicy.Fixed)
                        compassElement.SuggestedHeight = 60;
                }

                if (troopIcon != null)
                {
                    if (troopIcon.WidthSizePolicy == SizePolicy.Fixed)
                        troopIcon.SuggestedWidth = 55;
                    if (troopIcon.HeightSizePolicy == SizePolicy.Fixed)
                        troopIcon.SuggestedHeight = 55;
                    troopIcon.PositionXOffset = 5;
                    troopIcon.PositionYOffset = 0;
                    troopIcon.HorizontalAlignment = HorizontalAlignment.Center;
                    troopIcon.VerticalAlignment = VerticalAlignment.Center;
                }

                if (iconForeground != null)
                {
                    if (iconForeground.WidthSizePolicy == SizePolicy.Fixed)
                        iconForeground.SuggestedWidth = 50;
                    if (iconForeground.HeightSizePolicy == SizePolicy.Fixed)
                        iconForeground.SuggestedHeight = 50;
                }
            }
            else
            {
                if (steamAvatar != null)
                    steamAvatar.IsVisible = true;

                if (circleBackground != null)
                    circleBackground.IsVisible = true;

                if (smallAvatarImage != null)
                    smallAvatarImage.IsVisible = true;

                if (compassElement != null && _avatarOriginals.TryGetValue(compassElement, out var compassOrig))
                {
                    if (compassElement.WidthSizePolicy == SizePolicy.Fixed)
                        compassElement.SuggestedWidth = compassOrig.Width;
                    if (compassElement.HeightSizePolicy == SizePolicy.Fixed)
                        compassElement.SuggestedHeight = compassOrig.Height;
                }

                if (troopIcon != null && _avatarOriginals.TryGetValue(troopIcon, out var troopOrig))
                {
                    if (troopIcon.WidthSizePolicy == SizePolicy.Fixed)
                        troopIcon.SuggestedWidth = troopOrig.Width;
                    if (troopIcon.HeightSizePolicy == SizePolicy.Fixed)
                        troopIcon.SuggestedHeight = troopOrig.Height;
                    troopIcon.PositionXOffset = troopOrig.X;
                    troopIcon.PositionYOffset = troopOrig.Y;
                }

                if (iconForeground != null && _avatarOriginals.TryGetValue(iconForeground, out var fgOrig))
                {
                    if (iconForeground.WidthSizePolicy == SizePolicy.Fixed)
                        iconForeground.SuggestedWidth = fgOrig.Width;
                    if (iconForeground.HeightSizePolicy == SizePolicy.Fixed)
                        iconForeground.SuggestedHeight = fgOrig.Height;
                }
            }
        }

        private void FindAvatarComponentsRecursive(Widget widget, ref Widget steamAvatar, ref Widget circleBackground, ref Widget troopIcon, ref Widget iconForeground, ref Widget compassElement, ref Widget smallAvatarImage)
        {
            string typeName = widget.GetType().Name;
    
            if (typeName.Contains("CompassElement") || typeName.Contains("DependentPrefab"))
                compassElement = widget;
    
            if (typeName == "ImageIdentifierWidget")
            {
                if (widget.Id == "AvatarImage")
                {
                    smallAvatarImage = widget;
                }
                else
                {
                    Widget parent = widget.ParentWidget;
                    if (parent != null && !parent.GetType().Name.Contains("TroopType"))
                        steamAvatar = widget;
                }
            }
    
            if (widget.Sprite != null && widget.Sprite.Name != null)
            {
                if (widget.Sprite.Name.Contains("BlankWhiteCircle"))
                    circleBackground = widget;
            }
    
            if (typeName.Contains("MultiplayerTroopTypeIconWidget"))
                troopIcon = widget;
    
            if (widget.Id == "IconForeground")
                iconForeground = widget;
    
            for (int i = 0; i < widget.ChildCount; i++)
                FindAvatarComponentsRecursive(widget.GetChild(i), ref steamAvatar, ref circleBackground, ref troopIcon, ref iconForeground, ref compassElement, ref smallAvatarImage);
        }

        public void Reset()
        {
            _cached = false;
            _betterAvatarsApplied = false;
    
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
            _avatarOriginals?.Clear();
        }
    }
}