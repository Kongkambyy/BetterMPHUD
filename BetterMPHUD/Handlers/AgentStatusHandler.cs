using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;
using BetterMPHUD.Services;

namespace BetterMPHUD.Handlers
{
    public class AgentStatusHandler
    {
        private readonly WidgetCustomizer _customizer = new WidgetCustomizer();
        private readonly Dictionary<HudElement, TrackedWidget> _widgets = new Dictionary<HudElement, TrackedWidget>();
        private bool _cached;

        public AgentStatusHandler()
        {
            InitializeWidgets();
        }

        private void InitializeWidgets()
        {
            _widgets[HudElement.AgentHealth] = new TrackedWidget { Element = HudElement.AgentHealth };
            _widgets[HudElement.MountHealth] = new TrackedWidget { Element = HudElement.MountHealth };
            _widgets[HudElement.ShieldHealth] = new TrackedWidget { Element = HudElement.ShieldHealth };
            _widgets[HudElement.WeaponInfo] = new TrackedWidget { Element = HudElement.WeaponInfo };
            _widgets[HudElement.GoldAmount] = new TrackedWidget { Element = HudElement.GoldAmount };
            _widgets[HudElement.TroopCount] = new TrackedWidget { Element = HudElement.TroopCount };
            _widgets[HudElement.DamageFeed] = new TrackedWidget { Element = HudElement.DamageFeed };
        }

        public void Apply(HudSettings settings, Mission mission)
        {
            GauntletLayer layer = FindAgentStatusLayer(mission);
            if (layer == null || layer.UIContext == null || layer.UIContext.Root == null) return;

            if (!_cached)
            {
                CacheWidgets(layer.UIContext.Root);
                _cached = true;
            }

            ApplyCustomization(settings);
        }

        private GauntletLayer FindAgentStatusLayer(Mission mission)
        {
            MissionBehavior behavior = LayerFinder.FindBehaviorByName(mission, "AgentStatus", "MissionMainAgentEquipmentControllerView");
            if (behavior != null)
            {
                GauntletLayer layer = LayerFinder.FindInBehavior(behavior);
                if (layer != null) return layer;
            }

            return LayerFinder.FindByPredicate(mission, delegate(GauntletLayer layer) 
            {
                return WidgetFinder.FindByType(layer.UIContext.Root, "AgentHealthWidget") != null ||
                       WidgetFinder.FindById(layer.UIContext.Root, "HeroHealthWidget") != null;
            });
        }

        private void CacheWidgets(Widget root)
        {
            Widget agentHealth = WidgetFinder.FindById(root, "HeroHealthWidget");
            if (agentHealth == null)
                agentHealth = WidgetFinder.FindByType(root, "AgentHealthWidget");
            _widgets[HudElement.AgentHealth].Cache(agentHealth);
            
            _widgets[HudElement.MountHealth].Cache(WidgetFinder.FindById(root, "HorseHealthWidget"));
            _widgets[HudElement.ShieldHealth].Cache(WidgetFinder.FindById(root, "ShieldHealthWidget"));
            _widgets[HudElement.GoldAmount].Cache(WidgetFinder.FindBySprite(root, "personal_killfeed_notification"));
            _widgets[HudElement.DamageFeed].Cache(WidgetFinder.FindByType(root, "MissionAgentDamageFeedWidget"));
            
            SearchAdditionalWidgets(root, 0);
        }

        private void SearchAdditionalWidgets(Widget widget, int depth)
        {
            if (depth > Constants.UI.MaxWidgetSearchDepth) return;

            TryCacheWeaponInfo(widget);
            TryCacheTroopCount(widget);

            for (int i = 0; i < widget.ChildCount; i++)
                SearchAdditionalWidgets(widget.GetChild(i), depth + 1);
        }

        private void TryCacheWeaponInfo(Widget widget)
        {
            if (_widgets[HudElement.WeaponInfo].Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel == null) return;
            if (widget.HorizontalAlignment != HorizontalAlignment.Right) return;
            
            if (WidgetFinder.HasChildOfType(widget, "ImageIdentifierWidget"))
                _widgets[HudElement.WeaponInfo].Cache(widget);
        }

        private void TryCacheTroopCount(Widget widget)
        {
            if (_widgets[HudElement.TroopCount].Widget != null) return;
            ListPanel listPanel = widget as ListPanel;
            if (listPanel == null) return;
            
            if (WidgetFinder.HasChildWithSprite(widget, "troop_count"))
                _widgets[HudElement.TroopCount].Cache(widget);
        }

        private void ApplyCustomization(HudSettings settings)
        {
            ApplyElement(HudElement.AgentHealth, settings.AgentHealthCustom, settings.ShowAgentHealth);
            ApplyElement(HudElement.MountHealth, settings.MountHealthCustom, settings.ShowMountHealth);
            ApplyElement(HudElement.ShieldHealth, settings.ShieldHealthCustom, settings.ShowShieldHealth);
            ApplyElement(HudElement.WeaponInfo, settings.WeaponInfoCustom, settings.ShowWeaponInfo);
            ApplyElement(HudElement.GoldAmount, settings.GoldAmountCustom, settings.ShowGoldAmount);
            ApplyElement(HudElement.TroopCount, settings.TroopCountCustom, settings.ShowTroopCount);
            ApplyElement(HudElement.DamageFeed, settings.DamageFeedCustom, settings.ShowDamageFeed);
        }

        private void ApplyElement(HudElement element, ElementCustomization custom, bool isVisible)
        {
            TrackedWidget tracked = _widgets[element];
            if (!tracked.IsReady) return;
            
            tracked.Widget.AlphaFactor = 1.0f;
            tracked.Widget.DoNotAcceptEvents = false;
            _customizer.ApplyCustomization(tracked.Widget, tracked.Original, custom, isVisible);
        }

        public void Reset()
        {
            _cached = false;
            _widgets.Clear();
            InitializeWidgets();
            _customizer.Clear();
        }
    }
}