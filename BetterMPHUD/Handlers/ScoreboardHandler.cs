using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.ImageIdentifiers;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using BetterMPHUD.Core;
using BetterMPHUD.Services;
using BetterMPHUD.ViewModels;

namespace BetterMPHUD.Handlers
{
    public class ScoreboardHandler
    {
        private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        private const string WarlordsPeerComponentTypeName = "MBWarlords.LastConcord.WarlordsPeerComponent";
        private const float ScoreboardRefreshInterval = 0.25f;
    
        private Dictionary<Widget, float> _originalAlphas = new Dictionary<Widget, float>();
        private readonly Dictionary<NetworkCommunicator, WarlordsBannerCacheEntry> _warlordsBannerCache = new Dictionary<NetworkCommunicator, WarlordsBannerCacheEntry>();
        private readonly Dictionary<string, WarlordsScoreboardVisuals> _warlordsVisuals = new Dictionary<string, WarlordsScoreboardVisuals>(StringComparer.OrdinalIgnoreCase);
        private GauntletLayer _summaryLayer;
        private ScoreboardSummaryVM _summaryVM;
        private Widget _nativeScoreboardWidget;
        private WidgetOriginalValues _nativeScoreboardOriginal;
        private Type _warlordsPeerComponentType;
        private float _nextSummaryRefreshTime;
        private float _currentRowScale = 1f;
        private string _lastRefreshKey = "";
        private bool _initialized;
        private bool _wasSummaryRequested;

        private class WarlordsBannerCacheEntry
        {
            public string BannerCode;
            public string FactionBannerCode;
            public BannerImageIdentifierVM Banner;
            public BannerImageIdentifierVM FactionBanner;
        }

        private class WarlordsScoreboardVisuals
        {
            public ViewModel PlayerViewModel;
            public BannerImageIdentifierVM Banner;
            public BannerImageIdentifierVM FactionBanner;
        }

        public Action OnShowCursorRequested;

        public void Initialize(MissionScreen screen)
        {
            if (screen == null || _summaryLayer != null) return;

            _summaryVM = new ScoreboardSummaryVM();
            _summaryVM.OnShowCursorRequested = RequestShowCursor;
            _summaryVM.OnToggleMuteRequested = ToggleMuteAll;
            _summaryVM.OnInspectPlayerCardRequested = InspectPlayerCard;
            _summaryVM.OnMutePermanentlyRequested = MutePlayerPermanently;
            _summaryVM.OnMuteTextRequested = MutePlayerText;
            _summaryVM.OnMuteVoiceRequested = MutePlayerVoice;
            _summaryVM.OnReportRequested = ReportPlayer;
            _summaryVM.OnScoreboardPlayerActionRequested = ExecuteScoreboardPlayerAction;
            _summaryLayer = new GauntletLayer("BetterScoreboardSummaryLayer", 65, false);
            _summaryLayer.LoadMovie("ScoreboardSummary", _summaryVM);
            screen.AddLayer(_summaryLayer);
        }

        public void Apply(HudSettings settings, Mission mission, MissionScreen screen)
        {
            if (mission == null) return;
            Initialize(screen);

            Widget scoreboardWidget;
            GauntletLayer layer = FindScoreboardLayer(mission, out scoreboardWidget);
        
            if (layer == null || layer.UIContext == null || layer.UIContext.Root == null || scoreboardWidget == null || !scoreboardWidget.IsVisible)
            {
                _initialized = false;
                _originalAlphas.Clear();
                SetSummaryVisible(false);
                return;
            }

            UpdateSummary(settings, mission);
        }

        public bool IsSummaryVisible
        {
            get { return _summaryVM != null && _summaryVM.IsVisible; }
        }

        public void SetInteractive(bool interactive)
        {
            if (_summaryLayer == null)
                return;

            if (interactive)
            {
                _summaryLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                ScreenManager.TrySetFocus(_summaryLayer);
            }
            else
            {
                _summaryLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Invalid);
                ScreenManager.TryLoseFocus(_summaryLayer);
            }
        }

        public void UpdateCustomScoreboard(HudSettings settings, Mission mission, MissionScreen screen, bool isHeld)
        {
            if (mission == null)
                return;

            if (!isHeld)
            {
                SetInteractive(false);
                SetSummaryVisible(false);
                _wasSummaryRequested = false;
                _nextSummaryRefreshTime = 0f;
                return;
            }

            Initialize(screen);
            string refreshKey = GetRefreshKey(settings);
            float currentTime = mission.CurrentTime;
            bool shouldRefresh = !_wasSummaryRequested || currentTime >= _nextSummaryRefreshTime || _lastRefreshKey != refreshKey;

            if (shouldRefresh)
            {
                UpdateSummary(settings, mission);
                _nextSummaryRefreshTime = currentTime + ScoreboardRefreshInterval;
                _lastRefreshKey = refreshKey;
            }
            else
            {
                SetSummaryVisible(true);
            }

            _wasSummaryRequested = true;
        }

        public bool IsNativeScoreboardHotkeyDown(MissionScreen screen)
        {
            bool sceneLayerDown = screen != null && screen.SceneLayer != null && screen.SceneLayer.Input.IsHotKeyDown("HoldShow");
            bool summaryLayerDown = _summaryLayer != null && _summaryLayer.Input.IsHotKeyDown("HoldShow");
            bool tabDown = Input.IsKeyDown(InputKey.Tab);
            return sceneLayerDown || summaryLayerDown || tabDown;
        }

        public bool IsNativeScoreboardHotkeyPressed(MissionScreen screen)
        {
            bool sceneLayerPressed = screen != null && screen.SceneLayer != null && screen.SceneLayer.Input.IsHotKeyPressed("HoldShow");
            bool summaryLayerPressed = _summaryLayer != null && _summaryLayer.Input.IsHotKeyPressed("HoldShow");
            bool tabPressed = Input.IsKeyPressed(InputKey.Tab);
            return sceneLayerPressed || summaryLayerPressed || tabPressed;
        }


        private GauntletLayer FindScoreboardLayer(Mission mission, out Widget scoreboardWidget)
        {
            scoreboardWidget = null;

            foreach (MissionBehavior behavior in mission.MissionBehaviors)
            {
                if (behavior.GetType().Name == "MissionGauntletMultiplayerScoreboard")
                {
                    GauntletLayer namedLayer = GetLayerFromBehavior(behavior);
                    scoreboardWidget = FindScoreboardWidget(namedLayer);
                    if (scoreboardWidget != null)
                        return namedLayer;
                }
            }

            Widget foundWidget = null;
            GauntletLayer layer = LayerFinder.FindByPredicate(mission, delegate(GauntletLayer candidate)
            {
                Widget widget = FindScoreboardWidget(candidate);
                if (widget == null)
                    return false;

                foundWidget = widget;
                return true;
            });

            scoreboardWidget = foundWidget;
            return layer;
        }

        private Widget FindScoreboardWidget(GauntletLayer layer)
        {
            if (layer == null || layer.UIContext == null || layer.UIContext.Root == null)
                return null;

            return FindScoreboardWidgetRecursive(layer.UIContext.Root, 0);
        }

        private Widget FindScoreboardWidgetRecursive(Widget widget, int depth)
        {
            if (widget == null || depth > 80)
                return null;

            string typeName = widget.GetType().Name;
            if (typeName.Contains("MultiplayerScoreboardScreenWidget"))
                return widget;

            for (int i = 0; i < widget.ChildCount; i++)
            {
                Widget found = FindScoreboardWidgetRecursive(widget.GetChild(i), depth + 1);
                if (found != null)
                    return found;
            }

            return null;
        }

        private GauntletLayer GetLayerFromBehavior(MissionBehavior behavior)
        {
            Type type = behavior.GetType();
            
            foreach (FieldInfo field in type.GetFields(Flags))
            {
                if (typeof(GauntletLayer).IsAssignableFrom(field.FieldType))
                {
                    GauntletLayer layer = field.GetValue(behavior) as GauntletLayer;
                    if (layer != null && layer.UIContext != null && layer.UIContext.Root != null)
                        return layer;
                }
            }
            return null;
        }

        private void ApplyToAllWidgets(Widget widget, HudSettings settings, int depth)
        {
            if (depth > 80) return;

            if (!_originalAlphas.ContainsKey(widget))
            {
                _originalAlphas[widget] = widget.AlphaFactor;
            }

            float originalAlpha = _originalAlphas[widget];
            bool isBackground = false;
            
            if (widget.Sprite != null && widget.Sprite.Name != null)
            {
                string spriteName = widget.Sprite.Name;
                
                if (spriteName.Contains("flat_panel"))
                {
                    isBackground = true;
                    
                    if (settings.ScoreboardBackgroundEnabled)
                        widget.AlphaFactor = settings.ScoreboardBackgroundOpacity;
                    else
                        widget.AlphaFactor = 0f;
                }
                else if (spriteName.Contains("BlankWhiteSquare"))
                {
                    isBackground = true;
                    
                    if (settings.ScoreboardStripingEnabled)
                    {
                        float targetAlpha = originalAlpha * (settings.ScoreboardStripingOpacity / 0.4f);
                        widget.AlphaFactor = Math.Min(targetAlpha, 1f);
                    }
                    else
                    {
                        widget.AlphaFactor = 0f;
                    }
                }
            }

            for (int i = 0; i < widget.ChildCount; i++)
            {
                ApplyToAllWidgets(widget.GetChild(i), settings, depth + 1);
            }
        }

        private void HideNativeScoreboard(Widget widget, int depth)
        {
            if (widget == null || depth > 80)
                return;

            widget.AlphaFactor = 0f;

            for (int i = 0; i < widget.ChildCount; i++)
                HideNativeScoreboard(widget.GetChild(i), depth + 1);
        }

        private void MoveNativeScoreboardOffscreen(Widget widget)
        {
            if (widget == null)
                return;

            if (_nativeScoreboardWidget != widget)
            {
                RestoreNativeScoreboard();
                _nativeScoreboardWidget = widget;
                _nativeScoreboardOriginal = WidgetOriginalValues.Capture(widget);
            }

            widget.PositionXOffset = _nativeScoreboardOriginal.X + 10000f;
        }

        private void RestoreNativeScoreboard()
        {
            if (_nativeScoreboardWidget == null || !_nativeScoreboardOriginal.IsValid)
                return;

            _nativeScoreboardWidget.PositionXOffset = _nativeScoreboardOriginal.X;
            _nativeScoreboardWidget.PositionYOffset = _nativeScoreboardOriginal.Y;
            _nativeScoreboardWidget = null;
            _nativeScoreboardOriginal = default(WidgetOriginalValues);
        }

        private void UpdateSummary(HudSettings settings, Mission mission)
        {
            if (_summaryVM == null)
                return;

            _summaryVM.ShowPing = settings.ScoreboardShowPing;
            _summaryVM.ShowStateColumn = settings.ScoreboardShowState;
            _summaryVM.ShowWarlordsBanners = settings.ScoreboardShowWarlordsBanners && IsWarlordsActive();
            _summaryVM.ShowWarlordsPlayerActions = IsWarlordsActive();
            _currentRowScale = settings.ScoreboardRowScale;
            _summaryVM.BackgroundVisible = settings.ScoreboardBackgroundEnabled;
            _summaryVM.BackgroundOpacity = settings.ScoreboardBackgroundOpacity;
            _summaryVM.TeamSummaryVisible = settings.ScoreboardSummaryEnabled;
            _summaryVM.ApplyDefaultSort(settings.ScoreboardDefaultSortColumn, settings.ScoreboardDefaultSortAscending);
            Dictionary<MissionPeer, NetworkCommunicator> peerMap = BuildPeerMap();
            RefreshWarlordsScoreboardVisuals();
            MissionScoreboardComponent scoreboard = mission.GetMissionBehavior<MissionScoreboardComponent>();
            if (scoreboard == null || scoreboard.Sides == null || scoreboard.Sides.Length == 0)
            {
                UpdateSummaryFromPeers(mission, peerMap);
                return;
            }

            MissionScoreboardComponent.MissionScoreboardSide first = scoreboard.Sides[0];
            MissionScoreboardComponent.MissionScoreboardSide second = scoreboard.Sides.Length > 1 ? scoreboard.Sides[1] : null;

            ApplySide(first, true, mission, peerMap);
            if (second != null)
                ApplySide(second, false, mission, peerMap);
            else
                ApplyEmptySecondSide();

            SetSummaryVisible(true);
        }

        private void UpdateSummaryFromPeers(Mission mission, Dictionary<MissionPeer, NetworkCommunicator> peerMap)
        {
            Team playerTeam = mission.PlayerTeam;
            Team enemyTeam = mission.PlayerEnemyTeam;

            ApplyPeerTeam(playerTeam, GetTeamName(playerTeam, true, mission), true, mission, peerMap);
            ApplyPeerTeam(enemyTeam, GetTeamName(enemyTeam, false, mission), false, mission, peerMap);
            SetSummaryVisible(true);
        }

        private void ApplyPeerTeam(Team team, string name, bool first, Mission mission, Dictionary<MissionPeer, NetworkCommunicator> peerMap)
        {
            int players = 0;
            int alive = 0;
            int kills = 0;
            int deaths = 0;
            int score = 0;
            int infantry = 0;
            int archers = 0;
            int cavalry = 0;
            MBBindingList<ScoreboardPlayerVM> playersList = new MBBindingList<ScoreboardPlayerVM>();

            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer == null)
                    continue;

                MissionPeer peer = networkPeer.GetComponent<MissionPeer>();
                if (peer == null || peer.Team != team)
                    continue;

                players++;
                kills += peer.KillCount;
                deaths += peer.DeathCount;
                score += peer.Score;

                if (peer.IsControlledAgentActive)
                    alive++;

                if (MatchesSearch(peer.DisplayedName, GetSearchText(first)))
                    playersList.Add(CreatePlayer(peer, networkPeer));
            }

            if (team != null)
                CountClassesForSide(mission, team.Side, ref infantry, ref archers, ref cavalry);

            int dead = Math.Max(players - alive, 0);
            string totals = "Players " + players + " | Alive " + alive + " | Dead " + dead;
            string classes = "Inf " + infantry + " | Arc " + archers + " | Cav " + cavalry;

            if (first)
            {
                _summaryVM.TeamOneName = name;
                _summaryVM.TeamOneTotals = totals;
                _summaryVM.TeamOneClasses = classes;
                _summaryVM.TeamOneRoundScore = "0";
                _summaryVM.TeamOnePlayers = SortPlayers(playersList, true);
            }
            else
            {
                _summaryVM.TeamTwoName = name;
                _summaryVM.TeamTwoTotals = totals;
                _summaryVM.TeamTwoClasses = classes;
                _summaryVM.TeamTwoRoundScore = "0";
                _summaryVM.TeamTwoPlayers = SortPlayers(playersList, false);
            }
        }

        private void ApplySide(MissionScoreboardComponent.MissionScoreboardSide side, bool first, Mission mission, Dictionary<MissionPeer, NetworkCommunicator> peerMap)
        {
            int players = 0;
            int alive = 0;
            int kills = 0;
            int deaths = 0;
            int score = 0;
            int infantry = 0;
            int archers = 0;
            int cavalry = 0;
            MBBindingList<ScoreboardPlayerVM> playersList = new MBBindingList<ScoreboardPlayerVM>();

            foreach (MissionPeer peer in side.Players)
            {
                if (peer == null) continue;

                players++;
                kills += peer.KillCount;
                deaths += peer.DeathCount;
                score += peer.Score;

                if (peer.IsControlledAgentActive)
                    alive++;

                if (MatchesSearch(peer.DisplayedName, GetSearchText(first)))
                    playersList.Add(CreatePlayer(peer, GetNetworkPeer(peerMap, peer)));
            }

            CountClassesForSide(mission, side.Side, ref infantry, ref archers, ref cavalry);

            string teamName = GetSideName(side.Side, first, mission);
            int dead = Math.Max(players - alive, 0);
            string totals = "Players " + players + " | Alive " + alive + " | Dead " + dead;
            string classes = "Inf " + infantry + " | Arc " + archers + " | Cav " + cavalry;

            if (first)
            {
                _summaryVM.TeamOneName = teamName;
                _summaryVM.TeamOneTotals = totals;
                _summaryVM.TeamOneClasses = classes;
                _summaryVM.TeamOneRoundScore = side.SideScore.ToString();
                _summaryVM.TeamOnePlayers = SortPlayers(playersList, true);
            }
            else
            {
                _summaryVM.TeamTwoName = teamName;
                _summaryVM.TeamTwoTotals = totals;
                _summaryVM.TeamTwoClasses = classes;
                _summaryVM.TeamTwoRoundScore = side.SideScore.ToString();
                _summaryVM.TeamTwoPlayers = SortPlayers(playersList, false);
            }
        }

        private ScoreboardPlayerVM CreatePlayer(MissionPeer peer, NetworkCommunicator networkPeer)
        {
            Agent agent = peer.ControlledAgent;
            string classSprite = GetScoreboardClassSprite(peer, agent);
            string rowColor = peer.IsControlledAgentActive ? "#F2E8D5FF" : "#8C8170FF";
            string name = string.IsNullOrEmpty(peer.DisplayedName) ? "Player" : peer.DisplayedName;
            int ping = networkPeer != null ? (int)Math.Round(networkPeer.AveragePingInMilliseconds) : 0;
            bool showPing = _summaryVM == null || _summaryVM.ShowPing;
            ScoreboardPlayerVM player = new ScoreboardPlayerVM(name, peer.Score, peer.KillCount, peer.DeathCount, peer.AssistCount, ping, peer.IsControlledAgentActive, showPing, classSprite, rowColor);
            player.IsLocalPlayer = networkPeer != null && networkPeer.IsMine;
            player.RowScale = _currentRowScale;
            player.OnActionsRequested = OpenPlayerActions;
            bool showWarlordsBannerColumn = _summaryVM != null && _summaryVM.ShowWarlordsBanners;
            player.ShowStateColumn = _summaryVM == null || _summaryVM.ShowStateColumn;
            player.ShowWarlordsBannerColumn = showWarlordsBannerColumn;
            player.ShowNativeNameLayout = !showWarlordsBannerColumn;
            ApplyWarlordsBanner(player, networkPeer);
            return player;
        }

        private static string GetScoreboardClassSprite(MissionPeer peer, Agent agent)
        {
            if (peer == null)
                return "";

            if (!peer.IsControlledAgentActive)
                return Constants.Sprites.Death;

            if (agent == null)
                return "";

            return AgentHelper.GetClassSprite(agent);
        }

        private void OpenPlayerActions(ScoreboardPlayerVM player)
        {
            if (_summaryVM != null && player != null && !player.IsLocalPlayer)
                _summaryVM.OpenPlayerActions(player, BuildPlayerActions(player.NameValue));
        }

        private string GetRefreshKey(HudSettings settings)
        {
            if (_summaryVM == null)
                return "";

            return settings.ScoreboardShowPing
                + "|" + settings.ScoreboardShowState
                + "|" + settings.ScoreboardShowWarlordsBanners
                + "|" + settings.ScoreboardRowScale.ToString("F2")
                + "|" + settings.ScoreboardBackgroundEnabled
                + "|" + settings.ScoreboardBackgroundOpacity.ToString("F2")
                + "|" + settings.ScoreboardSummaryEnabled
                + "|" + settings.ScoreboardDefaultSortColumn
                + "|" + settings.ScoreboardDefaultSortAscending
                + "|" + _summaryVM.TeamOneSearchText
                + "|" + _summaryVM.TeamTwoSearchText
                + "|" + _summaryVM.GetSortColumn(true)
                + "|" + _summaryVM.GetSortAscending(true)
                + "|" + _summaryVM.GetSortColumn(false)
                + "|" + _summaryVM.GetSortAscending(false);
        }

        private Dictionary<MissionPeer, NetworkCommunicator> BuildPeerMap()
        {
            Dictionary<MissionPeer, NetworkCommunicator> peerMap = new Dictionary<MissionPeer, NetworkCommunicator>();

            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer == null)
                    continue;

                MissionPeer peer = networkPeer.GetComponent<MissionPeer>();
                if (peer != null && !peerMap.ContainsKey(peer))
                    peerMap.Add(peer, networkPeer);
            }

            return peerMap;
        }

        private static NetworkCommunicator GetNetworkPeer(Dictionary<MissionPeer, NetworkCommunicator> peerMap, MissionPeer peer)
        {
            if (peerMap == null || peer == null)
                return null;

            NetworkCommunicator networkPeer;
            return peerMap.TryGetValue(peer, out networkPeer) ? networkPeer : null;
        }

        private bool IsWarlordsActive()
        {
            return GetWarlordsPeerComponentType() != null;
        }

        private Type GetWarlordsPeerComponentType()
        {
            if (_warlordsPeerComponentType == null)
                _warlordsPeerComponentType = FindType(WarlordsPeerComponentTypeName);

            return _warlordsPeerComponentType;
        }

        private static Type FindType(string typeName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(typeName, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private void ApplyWarlordsBanner(ScoreboardPlayerVM player, NetworkCommunicator networkPeer)
        {
            if (player == null)
                return;

            WarlordsScoreboardVisuals visuals;
            if (!string.IsNullOrWhiteSpace(player.NameValue) && _warlordsVisuals.TryGetValue(player.NameValue, out visuals))
            {
                ApplyWarlordsScoreboardVisuals(player, visuals);
                if (player.HasWarlordsBanner || player.HasScoreboardFactionBanner)
                    return;
            }

            if (networkPeer == null)
                return;

            Type componentType = GetWarlordsPeerComponentType();
            if (componentType == null)
                return;

            object component = GetPeerComponent(networkPeer, componentType);
            if (component == null)
                return;

            string bannerCode = InvokeStringMethod(component, "GetEffectiveAccountBannerCode");
            if (string.IsNullOrWhiteSpace(bannerCode))
                bannerCode = GetStringProperty(component, "AccountBannerCode");
            if (string.IsNullOrWhiteSpace(bannerCode))
                bannerCode = GetStringProperty(component, "DefaultAccountSigilBannerCode");

            object clan = GetProperty(component, "Clan");
            string factionBannerCode = GetStringProperty(clan, "FactionBanner");
            WarlordsBannerCacheEntry cacheEntry = GetWarlordsBannerCacheEntry(networkPeer, bannerCode, factionBannerCode);
            BannerImageIdentifierVM banner = cacheEntry != null ? cacheEntry.Banner : null;
            BannerImageIdentifierVM factionBanner = cacheEntry != null ? cacheEntry.FactionBanner : null;
            if (banner == null && factionBanner != null)
            {
                banner = factionBanner;
                factionBanner = null;
            }

            if (banner != null)
            {
                player.ScoreboardBanner = banner;
                player.HasWarlordsBanner = true;
            }

            if (factionBanner != null)
            {
                player.ScoreboardFactionBanner = factionBanner;
                player.HasScoreboardFactionBanner = true;
            }
        }

        private void ApplyWarlordsScoreboardVisuals(ScoreboardPlayerVM player, WarlordsScoreboardVisuals visuals)
        {
            if (player == null || visuals == null)
                return;

            if (visuals.PlayerViewModel != null)
            {
                player.WarlordsPlayer = visuals.PlayerViewModel;
                player.HasWarlordsBanner = true;
            }

            if (visuals.Banner != null)
            {
                player.ScoreboardBanner = visuals.Banner;
                player.HasWarlordsBanner = true;
            }
            else if (visuals.FactionBanner != null)
            {
                player.ScoreboardBanner = visuals.FactionBanner;
                player.HasWarlordsBanner = true;
                return;
            }

            if (visuals.FactionBanner != null)
            {
                player.ScoreboardFactionBanner = visuals.FactionBanner;
                player.HasScoreboardFactionBanner = true;
            }
        }

        private void RefreshWarlordsScoreboardVisuals()
        {
            _warlordsVisuals.Clear();

            if (!IsWarlordsActive() || Mission.Current == null)
                return;

            foreach (MissionBehavior behavior in Mission.Current.MissionBehaviors)
            {
                if (behavior == null || behavior.GetType().Name != "WLMissionGauntletMultiplayerScoreboard")
                    continue;

                object dataSource = GetScoreboardActionSource(GetFieldValue(behavior, "_dataSource"));
                AddWarlordsSideVisuals(GetProperty(dataSource, "AllySide"));
                AddWarlordsSideVisuals(GetProperty(dataSource, "EnemySide"));
                return;
            }
        }

        private void AddWarlordsSideVisuals(object sideVm)
        {
            if (sideVm == null)
                return;

            foreach (PropertyInfo property in sideVm.GetType().GetProperties(Flags))
            {
                if (property.PropertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    continue;

                IEnumerable entries = null;
                try
                {
                    entries = property.GetValue(sideVm, null) as IEnumerable;
                }
                catch
                {
                    entries = null;
                }

                if (entries == null)
                    continue;

                foreach (object entry in entries)
                    AddWarlordsPlayerVisual(entry);
            }
        }

        private void AddWarlordsPlayerVisual(object entry)
        {
            if (entry == null)
                return;

            string name = GetScoreboardEntryName(entry);
            if (string.IsNullOrWhiteSpace(name))
                return;

            BannerImageIdentifierVM banner = GetProperty(entry, "ScoreboardBanner") as BannerImageIdentifierVM;
            BannerImageIdentifierVM factionBanner = GetProperty(entry, "ScoreboardFactionBanner") as BannerImageIdentifierVM;
            ViewModel playerViewModel = entry as ViewModel;

            if (banner == null)
            {
                InvokeNoArg(entry, "ResolveWarlordsPeerComponent");
                InvokeNoArg(entry, "RefreshScoreboardBanner");
                banner = GetProperty(entry, "ScoreboardBanner") as BannerImageIdentifierVM;
            }

            if (banner == null || factionBanner == null)
            {
                object component = GetFieldValue(entry, "_warlordsPeerComponent");
                if (component != null)
                {
                    if (banner == null)
                        banner = CreateBannerImageIdentifier(GetWarlordsAccountBannerCode(component));

                    if (factionBanner == null)
                        factionBanner = CreateBannerImageIdentifier(GetWarlordsFactionBannerCode(component));
                }
            }

            if (playerViewModel == null && banner == null && factionBanner == null)
                return;

            _warlordsVisuals[name] = new WarlordsScoreboardVisuals
            {
                PlayerViewModel = playerViewModel,
                Banner = banner ?? factionBanner,
                FactionBanner = banner != null ? factionBanner : null,
            };
        }

        private static string GetScoreboardEntryName(object entry)
        {
            if (entry == null)
                return "";

            MissionPeer peer = GetMissionPeerFromEntry(entry);
            if (peer != null && !string.IsNullOrWhiteSpace(peer.DisplayedName))
                return peer.DisplayedName;

            string name = GetObjectText(entry, "Name");
            if (string.IsNullOrWhiteSpace(name))
                name = GetObjectText(entry, "NameText");
            if (string.IsNullOrWhiteSpace(name))
                name = GetObjectText(entry, "DisplayedName");
            if (string.IsNullOrWhiteSpace(name))
                name = GetNameFromScoreboardStats(entry);

            return name;
        }

        private static string GetWarlordsAccountBannerCode(object component)
        {
            if (component == null)
                return "";

            string bannerCode = InvokeStringMethod(component, "GetEffectiveAccountBannerCode");
            if (string.IsNullOrWhiteSpace(bannerCode))
                bannerCode = GetStringProperty(component, "AccountBannerCode");
            if (string.IsNullOrWhiteSpace(bannerCode))
                bannerCode = GetStringProperty(component, "DefaultAccountSigilBannerCode");

            return bannerCode;
        }

        private static string GetWarlordsFactionBannerCode(object component)
        {
            if (component == null)
                return "";

            object clan = GetProperty(component, "Clan");
            string factionBannerCode = GetStringProperty(clan, "FactionBanner");
            if (string.IsNullOrWhiteSpace(factionBannerCode))
                factionBannerCode = GetStringProperty(clan, "Banner");

            return factionBannerCode;
        }

        private WarlordsBannerCacheEntry GetWarlordsBannerCacheEntry(NetworkCommunicator networkPeer, string bannerCode, string factionBannerCode)
        {
            if (networkPeer == null)
                return null;

            WarlordsBannerCacheEntry cacheEntry;
            if (_warlordsBannerCache.TryGetValue(networkPeer, out cacheEntry)
                && cacheEntry.BannerCode == bannerCode
                && cacheEntry.FactionBannerCode == factionBannerCode)
            {
                return cacheEntry;
            }

            cacheEntry = new WarlordsBannerCacheEntry
            {
                BannerCode = bannerCode,
                FactionBannerCode = factionBannerCode,
                Banner = CreateBannerImageIdentifier(bannerCode),
                FactionBanner = CreateBannerImageIdentifier(factionBannerCode),
            };

            _warlordsBannerCache[networkPeer] = cacheEntry;
            return cacheEntry;
        }

        private static BannerImageIdentifierVM CreateBannerImageIdentifier(string bannerCode)
        {
            if (string.IsNullOrWhiteSpace(bannerCode))
                return null;

            try
            {
                if (!Banner.IsValidBannerCode(bannerCode))
                    return null;

                return new BannerImageIdentifierVM(new Banner(bannerCode), false);
            }
            catch
            {
                return null;
            }
        }

        private static object GetPeerComponent(NetworkCommunicator networkPeer, Type componentType)
        {
            if (networkPeer == null || componentType == null)
                return null;

            IDictionary components = GetProperty(networkPeer, "PeerComponents") as IDictionary;
            if (components == null || !components.Contains(componentType))
                return null;

            return components[componentType];
        }

        private static string InvokeStringMethod(object instance, string methodName)
        {
            if (instance == null)
                return "";

            MethodInfo method = instance.GetType().GetMethod(methodName, Flags, null, Type.EmptyTypes, null);
            return method != null ? method.Invoke(instance, null) as string ?? "" : "";
        }

        private static string GetStringProperty(object instance, string propertyName)
        {
            return GetProperty(instance, propertyName) as string ?? "";
        }

        private static string GetObjectText(object instance, string propertyName)
        {
            object value = GetProperty(instance, propertyName);
            return value != null ? value.ToString() : "";
        }

        private static bool GetBoolProperty(object instance, string propertyName, bool defaultValue)
        {
            object value = GetProperty(instance, propertyName);
            return value is bool ? (bool)value : defaultValue;
        }

        private static object GetProperty(object instance, string propertyName)
        {
            if (instance == null)
                return null;

            PropertyInfo property = instance.GetType().GetProperty(propertyName, Flags);
            return property != null ? property.GetValue(instance, null) : null;
        }

        private static object GetFieldValue(object instance, string fieldName)
        {
            if (instance == null)
                return null;

            FieldInfo field = instance.GetType().GetField(fieldName, Flags);
            return field != null ? field.GetValue(instance) : null;
        }

        private static object InvokeMethod(object instance, string methodName, params object[] args)
        {
            if (instance == null)
                return null;

            MethodInfo method = instance.GetType().GetMethod(methodName, Flags);
            return method != null ? method.Invoke(instance, args) : null;
        }

        private NetworkCommunicator FindNetworkPeer(MissionPeer peer)
        {
            if (peer == null)
                return null;

            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer == null)
                    continue;

                if (networkPeer.GetComponent<MissionPeer>() == peer)
                    return networkPeer;
            }

            return null;
        }

        private MBBindingList<ScoreboardPlayerVM> SortPlayers(MBBindingList<ScoreboardPlayerVM> players, bool first)
        {
            List<ScoreboardPlayerVM> sorted = new List<ScoreboardPlayerVM>();
            foreach (ScoreboardPlayerVM player in players)
                sorted.Add(player);

            string column = _summaryVM != null ? _summaryVM.GetSortColumn(first) : "Score";
            bool ascending = _summaryVM != null && _summaryVM.GetSortAscending(first);

            sorted.Sort(delegate(ScoreboardPlayerVM x, ScoreboardPlayerVM y)
            {
                int result = ComparePlayers(x, y, column);
                return ascending ? result : -result;
            });

            MBBindingList<ScoreboardPlayerVM> resultList = new MBBindingList<ScoreboardPlayerVM>();
            foreach (ScoreboardPlayerVM player in sorted)
                resultList.Add(player);

            return resultList;
        }

        private int ComparePlayers(ScoreboardPlayerVM x, ScoreboardPlayerVM y, string column)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            switch (column)
            {
                case "Name":
                    return StringComparer.OrdinalIgnoreCase.Compare(x.NameValue, y.NameValue);
                case "Ping":
                    return x.PingValue.CompareTo(y.PingValue);
                case "Kills":
                    return x.KillsValue.CompareTo(y.KillsValue);
                case "Deaths":
                    return x.DeathsValue.CompareTo(y.DeathsValue);
                case "Assists":
                    return x.AssistsValue.CompareTo(y.AssistsValue);
                case "Score":
                default:
                    return x.ScoreValue.CompareTo(y.ScoreValue);
            }
        }

        private void RequestShowCursor()
        {
            if (OnShowCursorRequested != null)
                OnShowCursorRequested();
        }

        private MBBindingList<ScoreboardPlayerActionVM> BuildPlayerActions(string playerName)
        {
            MBBindingList<ScoreboardPlayerActionVM> actions = TryBuildNativePlayerActions(playerName);
            if (actions.Count > 0)
                return actions;

            if (_summaryVM != null && _summaryVM.ShowWarlordsPlayerActions)
                actions.Add(new ScoreboardPlayerActionVM("inspect", "Inspect Player Card", "Inspect", true, ExecuteFallbackPlayerAction));

            bool textMuted = IsPlayerTextMuted(playerName);
            bool voiceMuted = IsPlayerVoiceMuted(playerName);
            actions.Add(new ScoreboardPlayerActionVM(textMuted && voiceMuted ? "unmute_permanent" : "mute_permanent", textMuted && voiceMuted ? "Unmute Permanently" : "Mute Permanently", textMuted && voiceMuted ? "UnmutePermanent" : "MutePermanent", true, ExecuteFallbackPlayerAction));
            actions.Add(new ScoreboardPlayerActionVM(textMuted ? "unmute_text" : "mute_text", textMuted ? "Unmute Text" : "Mute Text", textMuted ? "UnmuteText" : "MuteText", true, ExecuteFallbackPlayerAction));
            actions.Add(new ScoreboardPlayerActionVM(voiceMuted ? "unmute_voice" : "mute_voice", voiceMuted ? "Unmute Voice" : "Mute Voice", voiceMuted ? "UnmuteVoice" : "MuteVoice", true, ExecuteFallbackPlayerAction));
            actions.Add(new ScoreboardPlayerActionVM("report", "Report", "Report", true, ExecuteFallbackPlayerAction));
            return actions;
        }

        private MBBindingList<ScoreboardPlayerActionVM> TryBuildNativePlayerActions(string playerName)
        {
            MBBindingList<ScoreboardPlayerActionVM> result = new MBBindingList<ScoreboardPlayerActionVM>();
            if (string.IsNullOrWhiteSpace(playerName) || Mission.Current == null)
                return result;

            foreach (MissionBehavior behavior in Mission.Current.MissionBehaviors)
            {
                if (behavior == null)
                    continue;

                string behaviorName = behavior.GetType().Name;
                if (behaviorName != "WLMissionGauntletMultiplayerScoreboard" && behaviorName != "MissionGauntletMultiplayerScoreboard")
                    continue;

                object dataSource = GetScoreboardActionSource(GetFieldValue(behavior, "_dataSource"));
                object entry = FindScoreboardPlayerEntry(dataSource, playerName);
                if (entry == null)
                    continue;

                InvokeNoArg(entry, "ExecuteSelection");
                IEnumerable nativeActions = GetProperty(dataSource, "PlayerActionList") as IEnumerable;
                if (nativeActions == null)
                    continue;

                foreach (object nativeAction in nativeActions)
                {
                    string definition = GetObjectText(nativeAction, "Definition");
                    if (string.IsNullOrWhiteSpace(definition))
                        continue;

                    string value = GetObjectText(nativeAction, "Value");
                    if (string.IsNullOrWhiteSpace(value))
                        value = definition.Replace(" ", "");

                    result.Add(new ScoreboardPlayerActionVM("scoreboard:" + definition, definition, value, GetBoolProperty(nativeAction, "IsEnabled", true), ExecuteFallbackPlayerAction));
                }

                if (result.Count > 0)
                    return result;
            }

            return result;
        }

        private void ExecuteFallbackPlayerAction(string actionId)
        {
        }

        private void ExecuteScoreboardPlayerAction(string playerName, string actionDefinition)
        {
            if (TryExecuteScoreboardPlayerAction(playerName, actionDefinition, false))
                return;

            if (string.Equals(actionDefinition, "Unmute Permanently", StringComparison.OrdinalIgnoreCase))
            {
                SetPlayerTextMuted(playerName, false);
                SetPlayerVoiceMuted(playerName, false);
                return;
            }

            if (string.Equals(actionDefinition, "Unmute Text", StringComparison.OrdinalIgnoreCase))
            {
                SetPlayerTextMuted(playerName, false);
                return;
            }

            if (string.Equals(actionDefinition, "Unmute Voice", StringComparison.OrdinalIgnoreCase))
            {
                SetPlayerVoiceMuted(playerName, false);
                return;
            }

            ShowScoreboardActionMessage(actionDefinition + " is not available for this player right now.");
        }

        private bool IsPlayerTextMuted(string playerName)
        {
            NetworkCommunicator networkPeer = FindNetworkPeerByName(playerName);
            MissionPeer peer = networkPeer != null ? networkPeer.GetComponent<MissionPeer>() : null;
            ChatBox chatBox = Game.Current != null ? Game.Current.GetGameHandler<ChatBox>() : null;
            if (peer == null || peer.Peer == null || chatBox == null)
                return false;

            object playerId = peer.Peer.Id;
            object result = InvokeMethod(chatBox, "IsPlayerMuted", playerId);
            return result is bool && (bool)result;
        }

        private bool IsPlayerVoiceMuted(string playerName)
        {
            NetworkCommunicator networkPeer = FindNetworkPeerByName(playerName);
            MissionPeer peer = networkPeer != null ? networkPeer.GetComponent<MissionPeer>() : null;
            if (peer == null)
                return false;

            object value = GetProperty(peer, "IsMuted");
            return value is bool && (bool)value;
        }

        private void SetPlayerTextMuted(string playerName, bool muted)
        {
            NetworkCommunicator networkPeer = FindNetworkPeerByName(playerName);
            MissionPeer peer = networkPeer != null ? networkPeer.GetComponent<MissionPeer>() : null;
            ChatBox chatBox = Game.Current != null ? Game.Current.GetGameHandler<ChatBox>() : null;
            if (peer == null || peer.Peer == null || chatBox == null)
                return;

            chatBox.SetPlayerMuted(peer.Peer.Id, muted);
        }

        private void SetPlayerVoiceMuted(string playerName, bool muted)
        {
            NetworkCommunicator networkPeer = FindNetworkPeerByName(playerName);
            MissionPeer peer = networkPeer != null ? networkPeer.GetComponent<MissionPeer>() : null;
            if (peer == null)
                return;

            peer.SetMuted(muted);
        }

        private void ToggleMuteAll(bool muted)
        {
            ChatBox chatBox = Game.Current != null ? Game.Current.GetGameHandler<ChatBox>() : null;

            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer == null || networkPeer.IsMine)
                    continue;

                MissionPeer peer = networkPeer.GetComponent<MissionPeer>();
                if (peer == null || peer.Peer == null)
                    continue;

                if (chatBox != null)
                    chatBox.SetPlayerMuted(peer.Peer.Id, muted);

                peer.SetMuted(muted);
            }
        }

        private void InspectPlayerCard(string playerName)
        {
            if (TryExecuteScoreboardPlayerAction(playerName, "Inspect Player Card", true))
                return;

            ShowScoreboardActionMessage("Could not open Inspect Player Card for this player.");
        }

        private void MutePlayerPermanently(string playerName)
        {
            if (TryExecuteScoreboardPlayerAction(playerName, "Mute Permanently", false))
                return;

            MutePlayerText(playerName);
            MutePlayerVoice(playerName);
        }

        private void MutePlayerText(string playerName)
        {
            if (TryExecuteScoreboardPlayerAction(playerName, "Mute Text", false))
                return;

            SetPlayerTextMuted(playerName, true);
        }

        private void MutePlayerVoice(string playerName)
        {
            if (TryExecuteScoreboardPlayerAction(playerName, "Mute Voice", false))
                return;

            SetPlayerVoiceMuted(playerName, true);
        }

        private void ReportPlayer(string playerName)
        {
            if (TryExecuteScoreboardPlayerAction(playerName, "Report", false))
                return;

            ShowScoreboardActionMessage("Report is not available for this player right now.");
        }

        private bool TryExecuteScoreboardPlayerAction(string playerName, string actionDefinition, bool warlordsOnly)
        {
            if (string.IsNullOrWhiteSpace(playerName) || string.IsNullOrWhiteSpace(actionDefinition) || Mission.Current == null)
                return false;

            foreach (MissionBehavior behavior in Mission.Current.MissionBehaviors)
            {
                if (behavior == null)
                    continue;

                string behaviorName = behavior.GetType().Name;
                bool isWarlords = behaviorName == "WLMissionGauntletMultiplayerScoreboard";
                bool isNative = behaviorName == "MissionGauntletMultiplayerScoreboard";

                if (!isWarlords && (!isNative || warlordsOnly))
                    continue;

                if (TryExecuteScoreboardPlayerAction(behavior, playerName, actionDefinition))
                    return true;
            }

            return false;
        }

        private bool TryExecuteScoreboardPlayerAction(MissionBehavior behavior, string playerName, string actionDefinition)
        {
            object dataSource = GetScoreboardActionSource(GetFieldValue(behavior, "_dataSource"));
            object entry = FindScoreboardPlayerEntry(dataSource, playerName);
            if (entry == null)
                return false;

            InvokeNoArg(entry, "ExecuteSelection");
            IEnumerable actions = GetProperty(dataSource, "PlayerActionList") as IEnumerable;
            if (actions == null)
                return false;

            foreach (object action in actions)
            {
                string definition = GetObjectText(action, "Definition");
                if (!string.Equals(definition, actionDefinition, StringComparison.OrdinalIgnoreCase))
                    continue;

                return InvokeNoArg(action, "ExecuteAction");
            }

            return false;
        }

        private static object GetScoreboardActionSource(object dataSource)
        {
            if (dataSource == null)
                return null;

            object scoreboard = GetProperty(dataSource, "Scoreboard");
            return scoreboard ?? dataSource;
        }

        private object FindScoreboardPlayerEntry(object dataSource, string playerName)
        {
            if (dataSource == null)
                return null;

            object entry = FindScoreboardPlayerEntryInSide(GetProperty(dataSource, "AllySide"), playerName);
            if (entry != null)
                return entry;

            entry = FindScoreboardPlayerEntryInSide(GetProperty(dataSource, "EnemySide"), playerName);
            if (entry != null)
                return entry;

            return FindScoreboardPlayerEntryInSide(dataSource, playerName);
        }

        private object FindScoreboardPlayerEntryInSide(object source, string playerName)
        {
            if (source == null)
                return null;

            foreach (PropertyInfo property in source.GetType().GetProperties(Flags))
            {
                if (property.PropertyType == typeof(string) || !typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    continue;

                IEnumerable entries = null;
                try
                {
                    entries = property.GetValue(source, null) as IEnumerable;
                }
                catch
                {
                    entries = null;
                }

                if (entries == null)
                    continue;

                foreach (object entry in entries)
                {
                    if (IsScoreboardPlayerEntry(entry, playerName))
                        return entry;
                }
            }

            return null;
        }

        private static bool IsScoreboardPlayerEntry(object entry, string playerName)
        {
            if (entry == null)
                return false;

            MissionPeer peer = GetMissionPeerFromEntry(entry);
            if (peer != null && string.Equals(peer.DisplayedName, playerName, StringComparison.OrdinalIgnoreCase))
                return true;

            string name = GetObjectText(entry, "Name");
            if (string.IsNullOrWhiteSpace(name))
                name = GetObjectText(entry, "NameText");
            if (string.IsNullOrWhiteSpace(name))
                name = GetObjectText(entry, "DisplayedName");
            if (string.IsNullOrWhiteSpace(name))
                name = GetNameFromScoreboardStats(entry);

            return string.Equals(name, playerName, StringComparison.OrdinalIgnoreCase);
        }

        private static MissionPeer GetMissionPeerFromEntry(object entry)
        {
            foreach (FieldInfo field in entry.GetType().GetFields(Flags))
            {
                if (typeof(MissionPeer).IsAssignableFrom(field.FieldType))
                    return field.GetValue(entry) as MissionPeer;
            }

            foreach (PropertyInfo property in entry.GetType().GetProperties(Flags))
            {
                if (!typeof(MissionPeer).IsAssignableFrom(property.PropertyType))
                    continue;

                try
                {
                    return property.GetValue(entry, null) as MissionPeer;
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static string GetNameFromScoreboardStats(object entry)
        {
            IEnumerable stats = GetProperty(entry, "Stats") as IEnumerable;
            if (stats == null)
                return "";

            foreach (object stat in stats)
            {
                string headerId = GetObjectText(stat, "HeaderID");
                if (!string.Equals(headerId, "name", StringComparison.OrdinalIgnoreCase))
                    continue;

                string item = GetObjectText(stat, "Item");
                if (!string.IsNullOrWhiteSpace(item))
                    return item;
            }

            return "";
        }

        private NetworkCommunicator FindNetworkPeerByName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer == null)
                    continue;

                MissionPeer peer = networkPeer.GetComponent<MissionPeer>();
                if (peer != null && string.Equals(peer.DisplayedName, playerName, StringComparison.OrdinalIgnoreCase))
                    return networkPeer;
            }

            return null;
        }

        private static bool InvokeNoArg(object instance, string methodName)
        {
            if (instance == null)
                return false;

            MethodInfo method = instance.GetType().GetMethod(methodName, Flags, null, Type.EmptyTypes, null);
            if (method == null)
                return false;

            method.Invoke(instance, null);
            return true;
        }

        private static void ShowScoreboardActionMessage(string message)
        {
            InformationManager.DisplayMessage(new InformationMessage("[BetterMPHUD] " + message, Colors.Yellow));
        }

        private string GetSearchText(bool first)
        {
            if (_summaryVM == null)
                return "";

            return first ? _summaryVM.TeamOneSearchText : _summaryVM.TeamTwoSearchText;
        }

        private bool MatchesSearch(string name, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return true;

            if (string.IsNullOrWhiteSpace(name))
                return false;

            string search = NormalizeSearchToken(searchText);
            string fullName = NormalizeSearchToken(name);
            string nameWithoutTag = NormalizeSearchToken(RemoveLeadingTag(name));
            string tag = NormalizeSearchToken(ExtractLeadingTag(name));

            return fullName.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                || nameWithoutTag.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrEmpty(tag) && tag.StartsWith(search, StringComparison.OrdinalIgnoreCase));
        }

        private string NormalizeSearchToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            string normalized = value.Trim();
            if (normalized.Length >= 2 && normalized[0] == '[' && normalized[normalized.Length - 1] == ']')
                normalized = normalized.Substring(1, normalized.Length - 2).Trim();

            return normalized;
        }

        private string ExtractLeadingTag(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            string trimmed = name.TrimStart();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal))
                return "";

            int closeIndex = trimmed.IndexOf(']');
            if (closeIndex <= 1)
                return "";

            return trimmed.Substring(1, closeIndex - 1);
        }

        private string RemoveLeadingTag(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            string trimmed = name.TrimStart();
            if (!trimmed.StartsWith("[", StringComparison.Ordinal))
                return trimmed;

            int closeIndex = trimmed.IndexOf(']');
            if (closeIndex < 0 || closeIndex + 1 >= trimmed.Length)
                return trimmed;

            return trimmed.Substring(closeIndex + 1).TrimStart();
        }

        private void CountClassesForSide(Mission mission, TaleWorlds.Core.BattleSideEnum side, ref int infantry, ref int archers, ref int cavalry)
        {
            if (mission == null || mission.Teams == null)
                return;

            foreach (Team team in mission.Teams)
            {
                if (team == null || team.Side != side)
                    continue;

                foreach (Agent agent in team.ActiveAgents)
                {
                    if (agent == null || !agent.IsHuman)
                        continue;

                    if (agent.HasMount)
                        cavalry++;
                    else if (AgentHelper.HasRangedWeapon(agent))
                        archers++;
                    else
                        infantry++;
                }
            }
        }

        private string GetSideName(TaleWorlds.Core.BattleSideEnum side, bool first, Mission mission)
        {
            string warlordsName = GetWarlordsScoreboardSideName(first);
            if (!string.IsNullOrWhiteSpace(warlordsName))
                return warlordsName;

            string cultureName = GetCultureNameForSide(side);
            if (!string.IsNullOrWhiteSpace(cultureName))
                return cultureName;

            Team team = GetTeamForSide(mission, side);
            string teamName = GetTeamName(team, first, mission);
            if (!string.IsNullOrWhiteSpace(teamName))
                return teamName;

            if (side == TaleWorlds.Core.BattleSideEnum.Attacker)
                return "Attackers";
            if (side == TaleWorlds.Core.BattleSideEnum.Defender)
                return "Defenders";
            return side.ToString();
        }

        private string GetTeamName(Team team, bool first, Mission mission)
        {
            if (team != null)
            {
                string warlordsName = GetWarlordsScoreboardSideName(first);
                if (!string.IsNullOrWhiteSpace(warlordsName))
                    return warlordsName;

                string cultureName = GetCultureNameForSide(team.Side);
                if (!string.IsNullOrWhiteSpace(cultureName))
                    return cultureName;
            }

            return first ? "Allies" : "Enemies";
        }

        private string GetWarlordsScoreboardSideName(bool first)
        {
            if (!IsWarlordsActive() || Mission.Current == null)
                return "";

            foreach (MissionBehavior behavior in Mission.Current.MissionBehaviors)
            {
                if (behavior == null || behavior.GetType().Name != "WLMissionGauntletMultiplayerScoreboard")
                    continue;

                object dataSource = GetFieldValue(behavior, "_dataSource");
                object sideVm = GetProperty(dataSource, first ? "AllySide" : "EnemySide");
                string name = GetObjectText(sideVm, "Name");
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
            }

            return "";
        }

        private string GetCultureNameForSide(TaleWorlds.Core.BattleSideEnum side)
        {
            MultiplayerOptions.OptionType optionType;
            if (side == TaleWorlds.Core.BattleSideEnum.Defender)
                optionType = MultiplayerOptions.OptionType.CultureTeam1;
            else if (side == TaleWorlds.Core.BattleSideEnum.Attacker)
                optionType = MultiplayerOptions.OptionType.CultureTeam2;
            else
                return "";

            string cultureId = MultiplayerOptionsExtensions.GetStrValue(optionType, MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            if (string.IsNullOrWhiteSpace(cultureId))
                return "";

            BasicCultureObject culture = MBObjectManager.Instance != null ? MBObjectManager.Instance.GetObject<BasicCultureObject>(cultureId) : null;
            return culture != null && culture.Name != null ? culture.Name.ToString() : cultureId;
        }

        private Team GetTeamForSide(Mission mission, TaleWorlds.Core.BattleSideEnum side)
        {
            if (mission == null || mission.Teams == null)
                return null;

            foreach (Team team in mission.Teams)
            {
                if (team != null && team.Side == side)
                    return team;
            }

            return null;
        }

        private void ApplyEmptySecondSide()
        {
            _summaryVM.TeamTwoName = "";
            _summaryVM.TeamTwoTotals = "";
            _summaryVM.TeamTwoClasses = "";
            _summaryVM.TeamTwoRoundScore = "";
            _summaryVM.TeamTwoPlayers = new MBBindingList<ScoreboardPlayerVM>();
        }

        private void SetSummaryVisible(bool visible)
        {
            if (_summaryVM != null)
                _summaryVM.IsVisible = visible;
        }

        public void DebugPrintStructure()
        {
            if (Mission.Current == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("[Scoreboard] No mission", Colors.Red));
                return;
            }

            MissionBehavior scoreboardBehavior = null;
            foreach (MissionBehavior behavior in Mission.Current.MissionBehaviors)
            {
                if (behavior.GetType().Name == "MissionGauntletMultiplayerScoreboard")
                {
                    scoreboardBehavior = behavior;
                    break;
                }
            }

            if (scoreboardBehavior == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("[Scoreboard] Behavior not found", Colors.Red));
                return;
            }

            Widget scoreboardWidget;
            GauntletLayer layer = FindScoreboardLayer(Mission.Current, out scoreboardWidget);
            if (layer == null || layer.UIContext == null || layer.UIContext.Root == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("[Scoreboard] Open scoreboard first!", Colors.Yellow));
                return;
            }

            InformationManager.DisplayMessage(new InformationMessage("[Scoreboard] Printing tree...", Colors.Cyan));
            PrintTree(layer.UIContext.Root, 0, 8);
            InformationManager.DisplayMessage(new InformationMessage("[Scoreboard] Stored " + _originalAlphas.Count + " widgets", Colors.Green));
        }

        private void PrintTree(Widget widget, int indent, int maxDepth)
        {
            if (maxDepth <= 0 || indent > 6) return;

            string typeName = widget.GetType().Name;
            string spriteName = (widget.Sprite != null && widget.Sprite.Name != null) ? widget.Sprite.Name : "-";
            
            if (typeName.Length > 35) typeName = typeName.Substring(0, 35);
            if (spriteName.Length > 25) spriteName = spriteName.Substring(0, 25);

            string prefix = new string(' ', indent * 2);
            string msg = prefix + typeName + " | " + spriteName + " | A:" + widget.AlphaFactor.ToString("F2");
            
            Color color = Colors.White;
            if (spriteName.Contains("flat_panel") || spriteName.Contains("BlankWhite")) 
                color = Colors.Cyan;

            InformationManager.DisplayMessage(new InformationMessage(msg, color));

            for (int i = 0; i < widget.ChildCount; i++)
                PrintTree(widget.GetChild(i), indent + 1, maxDepth - 1);
        }

        public void Reset()
        {
            _initialized = false;
            _originalAlphas.Clear();
            _warlordsBannerCache.Clear();
            _nextSummaryRefreshTime = 0f;
            _lastRefreshKey = "";
            _wasSummaryRequested = false;
            RestoreNativeScoreboard();
            SetSummaryVisible(false);
        }

        public void Cleanup(MissionScreen screen)
        {
            if (screen != null && _summaryLayer != null)
                screen.RemoveLayer(_summaryLayer);

            _summaryLayer = null;
            _summaryVM = null;
            Reset();
        }
    }
}
