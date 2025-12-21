using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews; 
using BetterMPHUD.ViewModels;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem; 

namespace BetterMPHUD
{
    public class HudBehavior : MissionBehavior
    {
        private GauntletLayer _configLayer;
        private GauntletLayer _killfeedLayer;
        private HudMenuVM _dataSource;
        private KillfeedVM _killfeedVM;
        private bool _initialized = false;
        private float _enforceSettingsTimer = 0f;
        
        // Top Bar Widgets
        private Widget _timeAndScoresWidget;
        private Widget _avatarsWidget;
        private Widget _enemyScoreWidget;
        private List<Widget> _bannerWidgets = new List<Widget>();
        private Widget _moraleWidget;
        private Widget _controlPointsWidget;
        private Widget _killfeedRootWidget;
        private bool _widgetsCached = false;

        // Agent Status Widgets
        private Widget _agentHealthWidget;
        private Widget _mountHealthWidget;
        private Widget _shieldHealthWidget;
        private Widget _weaponInfoWidget;
        private Widget _goldAmountWidget;
        private Widget _troopCountWidget;
        private Widget _couchLanceWidget;
        private Widget _spearBraceWidget;
        private Widget _damageFeedWidget;
        private Widget _agentStatusParentWidget;
        private bool _agentStatusWidgetsCached = false;

        private WidgetOriginalValues _timeAndScoresOriginal;
        private WidgetOriginalValues _avatarsOriginal;
        private WidgetOriginalValues _moraleOriginal;
        private WidgetOriginalValues _controlPointsOriginal;
        private WidgetOriginalValues _killfeedOriginal;
        
        // Agent Status Originals
        private WidgetOriginalValues _agentHealthOriginal;
        private WidgetOriginalValues _mountHealthOriginal;
        private WidgetOriginalValues _shieldHealthOriginal;
        private WidgetOriginalValues _weaponInfoOriginal;
        private WidgetOriginalValues _goldAmountOriginal;
        private WidgetOriginalValues _troopCountOriginal;
        private WidgetOriginalValues _damageFeedOriginal;
        
        private Dictionary<Widget, WidgetOriginalValues> _childOriginals = new Dictionary<Widget, WidgetOriginalValues>();

        // Camera Snapback Fields
        private FieldInfo _bearingDelta, _elevationDelta, _stBear, _scBear, _stElev, _scElev;
        private bool _cameraFieldsCached = false;

        private static readonly Color FriendlyKillColor = new Color(0.27f, 1f, 0.27f, 1f); 
        private static readonly Color EnemyKillColor = new Color(1f, 0.27f, 0.27f, 1f);     
        private static readonly Color NeutralColor = new Color(1f, 1f, 1f, 1f);         
        
        private const string SPRITE_SWORD = "icon_sword";
        private const string SPRITE_BOW = "icon_bow";
        private const string SPRITE_HORSE = "icon_horse";
        private const string SPRITE_DEATH = "icon_death";    

        private struct WidgetOriginalValues
        {
            public float X; public float Y; public float Width; public float Height;
            public float MarginTop; public float MarginBottom; public float MarginLeft; public float MarginRight;
            public float FontSize;
            public bool IsValid;
        }

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (!_initialized) TryInitializeUI();
            if (Input.IsKeyPressed(InputKey.F10)) ToggleMenu();

            _enforceSettingsTimer += dt;
            if (_enforceSettingsTimer > 1.0f)
            {
                ApplyAllSettings();
                _enforceSettingsTimer = 0f;
            }

            if (_killfeedVM != null && _killfeedVM.KillList.Count > 0)
            {
                float currentTime = Mission.Current?.CurrentTime ?? 0f;
                var toRemove = _killfeedVM.KillList.Where(item => item.ExpireTime <= currentTime).ToList();
                foreach (var item in toRemove) _killfeedVM.RemoveKill(item);
            }

            if (_dataSource != null && _dataSource.CameraSnapbackEnabled)
            {
                if (ScreenManager.TopScreen is MissionScreen ms)
                {
                    if (!_cameraFieldsCached) CacheCameraFields(ms);
                    if (ms.SceneLayer.Input.IsGameKeyReleased(25)) 
                    {
                        try
                        {
                            _bearingDelta?.SetValue(ms, 0f);
                            _elevationDelta?.SetValue(ms, 0f);
                            _stBear?.SetValue(ms, 0f);
                            _scBear?.SetValue(ms, 0f);
                            _stElev?.SetValue(ms, 0f);
                            _scElev?.SetValue(ms, 0f);
                        }
                        catch { }
                    }
                }
            }
        }
        
        private void CacheCameraFields(MissionScreen screen)
        {
            var type = screen.GetType();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            _bearingDelta = type.GetField("_cameraBearingDelta", flags);
            _elevationDelta = type.GetField("_cameraElevationDelta", flags);
            _stBear = type.GetField("_cameraSpecialTargetAddedBearing", flags);
            _scBear = type.GetField("_cameraSpecialCurrentAddedBearing", flags);
            _stElev = type.GetField("_cameraSpecialTargetAddedElevation", flags);
            _scElev = type.GetField("_cameraSpecialCurrentAddedElevation", flags);
            _cameraFieldsCached = true;
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (_killfeedVM != null && _dataSource != null && _dataSource.WarbandKillfeedEnabled 
                && affectedAgent != null && affectorAgent != null && affectedAgent.IsHuman)
            {
                Color rowColor = NeutralColor;
                string killIconSprite = SPRITE_DEATH;
                Team playerTeam = Mission.Current?.PlayerTeam;

                if (playerTeam != null)
                    rowColor = (affectedAgent.Team == playerTeam) ? EnemyKillColor : FriendlyKillColor;

                string killerClassSprite = GetAgentClassSprite(affectorAgent);
                string victimClassSprite = GetAgentClassSprite(affectedAgent);

                float fadeoutTime = _dataSource.GetSettings().KillfeedFadeoutTime;
                float expireTime = (Mission.Current?.CurrentTime ?? 0f) + fadeoutTime;

                var item = new KillfeedItemVM(
                    affectorAgent.Name, affectedAgent.Name, rowColor,
                    killerClassSprite, victimClassSprite, killIconSprite,
                    expireTime, (vm) => _killfeedVM.RemoveKill(vm));

                float currentScale = _dataSource.GetSettings().KillfeedCustom.Scale;
                _killfeedVM.GetScaledSizes(currentScale, out int font, out int icon, out int skull, out int row);
                item.UpdateSizes(font, icon, skull, row);

                _killfeedVM.AddKill(item);
            }
        }

        private string GetAgentClassSprite(Agent agent)
        {
            if (agent == null) return SPRITE_SWORD;
            if (agent.HasMount) return SPRITE_HORSE;
            if (HasRangedWeapon(agent)) return SPRITE_BOW;
            return SPRITE_SWORD;
        }

        private bool HasRangedWeapon(Agent agent)
        {
            if (agent?.Equipment == null) return false;
            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                var item = agent.Equipment[i];
                if (!item.IsEmpty && item.Item?.PrimaryWeapon != null)
                {
                    var weaponClass = item.Item.PrimaryWeapon.WeaponClass;
                    if (weaponClass == WeaponClass.Bow || weaponClass == WeaponClass.Crossbow)
                        return true;
                }
            }
            return false;
        }

        private void TryInitializeUI()
        {
            try
            {
                var missionScreen = ScreenManager.TopScreen as MissionScreen;
                if (missionScreen == null) return;

                _dataSource = new HudMenuVM();
                _dataSource.OnCloseConfigMenu = CloseMenu;
                _dataSource.OnWarbandKillfeedToggled = OnWarbandKillfeedToggled;
                _dataSource.OnHudSettingsChanged = ApplyAllSettings;

                _configLayer = new GauntletLayer("GauntletLayer", 50, false);
                _configLayer.LoadMovie("HudConfig", _dataSource);
                missionScreen.AddLayer(_configLayer);

                _killfeedVM = new KillfeedVM();
                _killfeedVM.IsVisible = _dataSource.WarbandKillfeedEnabled;

                _killfeedLayer = new GauntletLayer("GauntletLayer", 25, false); 
                _killfeedLayer.LoadMovie("WarbandKillfeed", _killfeedVM);
                missionScreen.AddLayer(_killfeedLayer);

                if (_killfeedLayer.UIContext?.Root != null && _killfeedLayer.UIContext.Root.ChildCount > 0)
                    _killfeedRootWidget = _killfeedLayer.UIContext.Root.GetChild(0);

                _initialized = true;
                ApplyAllSettings(); 
            }
            catch (Exception ex)
            {
                InformationManager.DisplayMessage(new InformationMessage($"UI Init Error: {ex.Message}", Colors.Red));
            }
        }

        private void ApplyAllSettings()
        {
            if (_dataSource == null || Mission.Current == null) return;
            
            if (_killfeedVM != null)
                _killfeedVM.IsVisible = _dataSource.WarbandKillfeedEnabled;

            ApplyKillfeedCustomization();
            ApplyTopBarSettings();
            ApplyAgentStatusSettings();
        }

        private void ApplyTopBarSettings()
        {
            var hudBehavior = Mission.Current.MissionBehaviors
                .FirstOrDefault(mb => mb.GetType().Name == "MissionMultiplayerHUDExtension" || mb.GetType().Name.Contains("HUDExtension"));

            if (hudBehavior == null) return;

            GauntletLayer nativeLayer = null;
            string[] possibleFieldNames = { "_gauntletLayer", "_layer", "_hudLayer", "gauntletLayer" };
            foreach (var fieldName in possibleFieldNames)
            {
                var fieldInfo = hudBehavior.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo != null)
                {
                    nativeLayer = fieldInfo.GetValue(hudBehavior) as GauntletLayer;
                    if (nativeLayer != null) break;
                }
            }

            if (nativeLayer == null || nativeLayer.UIContext?.Root == null) return;

            if (!_widgetsCached)
            {
                CacheWidgets(nativeLayer.UIContext.Root);
                _widgetsCached = true;
                StoreOriginalValues();
            }

            ApplyVisibilitySettings();
            ApplyCustomizationSettings();
        }

        private void ApplyAgentStatusSettings()
        {
            var agentStatusBehavior = Mission.Current.MissionBehaviors
                .FirstOrDefault(mb => mb.GetType().Name.Contains("AgentStatus") || mb.GetType().Name.Contains("MissionMainAgentEquipmentControllerView"));

            GauntletLayer agentStatusLayer = null;

            if (agentStatusBehavior != null)
            {
                var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
                string[] fieldNames = { "_gauntletLayer", "_layer", "_dataSource" };
                
                foreach (var fieldName in fieldNames)
                {
                    var fieldInfo = agentStatusBehavior.GetType().GetField(fieldName, flags);
                    if (fieldInfo != null)
                    {
                        agentStatusLayer = fieldInfo.GetValue(agentStatusBehavior) as GauntletLayer;
                        if (agentStatusLayer != null) break;
                    }
                }
            }

            if (agentStatusLayer == null)
            {
                foreach (var behavior in Mission.Current.MissionBehaviors)
                {
                    var fields = behavior.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    foreach (var field in fields)
                    {
                        if (field.FieldType == typeof(GauntletLayer))
                        {
                            var layer = field.GetValue(behavior) as GauntletLayer;
                            if (layer?.UIContext?.Root != null)
                            {
                                if (HasAgentStatusWidgets(layer.UIContext.Root))
                                {
                                    agentStatusLayer = layer;
                                    break;
                                }
                            }
                        }
                    }
                    if (agentStatusLayer != null) break;
                }
            }

            if (agentStatusLayer == null || agentStatusLayer.UIContext?.Root == null) return;

            if (!_agentStatusWidgetsCached)
            {
                CacheAgentStatusWidgets(agentStatusLayer.UIContext.Root);
                _agentStatusWidgetsCached = true;
                StoreAgentStatusOriginalValues();
            }

            ApplyAgentStatusVisibility();
            ApplyAgentStatusCustomization();
        }

        private bool HasAgentStatusWidgets(Widget root)
        {
            return FindWidgetByType(root, "AgentHealthWidget") != null || 
                   FindWidgetById(root, "HeroHealthWidget") != null;
        }

        private void CacheAgentStatusWidgets(Widget root)
        {
            _agentHealthWidget = FindWidgetById(root, "HeroHealthWidget") ?? FindWidgetByType(root, "AgentHealthWidget");
            _mountHealthWidget = FindWidgetById(root, "HorseHealthWidget");
            _shieldHealthWidget = FindWidgetById(root, "ShieldHealthWidget");
            _goldAmountWidget = FindWidgetBySprite(root, "personal_killfeed_notification");
            _damageFeedWidget = FindWidgetByType(root, "MissionAgentDamageFeedWidget");
            
            SearchAgentStatusWidgetsRecursively(root, 0);
        }

        private void SearchAgentStatusWidgetsRecursively(Widget widget, int depth)
        {
            if (depth > 50) return;
            
            string typeName = widget.GetType().Name;
            
            if (_agentHealthWidget == null && typeName == "AgentHealthWidget")
            {
                var parent = widget.ParentWidget;
                if (parent != null)
                {
                    for (int i = 0; i < parent.ChildCount; i++)
                    {
                        var sibling = parent.GetChild(i);
                        if (sibling.GetType().Name == "BrushWidget")
                        {
                            var brush = GetWidgetBrush(sibling);
                            if (brush != null && brush.Contains("HeroHealthBar"))
                            {
                                _agentHealthWidget = widget;
                                break;
                            }
                        }
                    }
                }
                if (_agentHealthWidget == null) _agentHealthWidget = widget;
            }
            
            if (_mountHealthWidget == null && typeName == "AgentHealthWidget" && widget != _agentHealthWidget)
            {
                var brush = GetWidgetBrush(widget);
                if (brush != null && brush.Contains("MountHealthBar"))
                    _mountHealthWidget = widget;
            }
            
            if (_shieldHealthWidget == null && typeName == "AgentHealthWidget" && 
                widget != _agentHealthWidget && widget != _mountHealthWidget)
            {
                var brush = GetWidgetBrush(widget);
                if (brush != null && brush.Contains("ShieldHealthBar"))
                    _shieldHealthWidget = widget;
            }
            
            if (_weaponInfoWidget == null && widget is ListPanel)
            {
                bool hasImageIdentifier = false;
                for (int i = 0; i < widget.ChildCount; i++)
                {
                    if (widget.GetChild(i).GetType().Name == "ImageIdentifierWidget")
                    {
                        hasImageIdentifier = true;
                        break;
                    }
                }
                if (hasImageIdentifier && widget.HorizontalAlignment == HorizontalAlignment.Right)
                    _weaponInfoWidget = widget;
            }
            
            if (_troopCountWidget == null && widget is ListPanel)
            {
                bool hasTroopSprite = false;
                for (int i = 0; i < widget.ChildCount; i++)
                {
                    var child = widget.GetChild(i);
                    if (child.Sprite != null && child.Sprite.Name != null && 
                        child.Sprite.Name.Contains("troop_count"))
                    {
                        hasTroopSprite = true;
                        break;
                    }
                }
                if (hasTroopSprite) _troopCountWidget = widget;
            }
            
            if (_couchLanceWidget == null && typeName == "AgentWeaponPassiveUsageVisualBrushWidget")
            {
                var brush = GetWidgetBrush(widget);
                if (brush != null && brush.Contains("CouchLance"))
                    _couchLanceWidget = widget;
            }
            
            if (_spearBraceWidget == null && typeName == "AgentWeaponPassiveUsageVisualBrushWidget" && widget != _couchLanceWidget)
            {
                var brush = GetWidgetBrush(widget);
                if (brush != null && brush.Contains("SpearBrace"))
                    _spearBraceWidget = widget;
            }

            for (int i = 0; i < widget.ChildCount; i++)
                SearchAgentStatusWidgetsRecursively(widget.GetChild(i), depth + 1);
        }

        private string GetWidgetBrush(Widget widget)
        {
            try
            {
                var brushProperty = widget.GetType().GetProperty("Brush");
                if (brushProperty != null)
                {
                    var brush = brushProperty.GetValue(widget);
                    if (brush != null) return brush.ToString();
                }
            }
            catch { }
            return null;
        }

        private Widget FindWidgetById(Widget root, string id)
        {
            if (root.Id == id) return root;
            for (int i = 0; i < root.ChildCount; i++)
            {
                var result = FindWidgetById(root.GetChild(i), id);
                if (result != null) return result;
            }
            return null;
        }

        private Widget FindWidgetByType(Widget root, string typeName)
        {
            if (root.GetType().Name == typeName) return root;
            for (int i = 0; i < root.ChildCount; i++)
            {
                var result = FindWidgetByType(root.GetChild(i), typeName);
                if (result != null) return result;
            }
            return null;
        }

        private Widget FindWidgetBySprite(Widget root, string spriteNameContains)
        {
            if (root.Sprite != null && root.Sprite.Name != null && root.Sprite.Name.Contains(spriteNameContains))
                return root;
            for (int i = 0; i < root.ChildCount; i++)
            {
                var result = FindWidgetBySprite(root.GetChild(i), spriteNameContains);
                if (result != null) return result;
            }
            return null;
        }

        private void StoreAgentStatusOriginalValues()
        {
            if (_agentHealthWidget != null) _agentHealthOriginal = CaptureWidgetValues(_agentHealthWidget);
            if (_mountHealthWidget != null) _mountHealthOriginal = CaptureWidgetValues(_mountHealthWidget);
            if (_shieldHealthWidget != null) _shieldHealthOriginal = CaptureWidgetValues(_shieldHealthWidget);
            if (_weaponInfoWidget != null) _weaponInfoOriginal = CaptureWidgetValues(_weaponInfoWidget);
            if (_goldAmountWidget != null) _goldAmountOriginal = CaptureWidgetValues(_goldAmountWidget);
            if (_troopCountWidget != null) _troopCountOriginal = CaptureWidgetValues(_troopCountWidget);
            if (_damageFeedWidget != null) _damageFeedOriginal = CaptureWidgetValues(_damageFeedWidget);
        }

        private void ApplyAgentStatusVisibility()
        {
            var settings = _dataSource.GetSettings();
            
            if (_agentHealthWidget != null) 
                SetWidgetVisibility(_agentHealthWidget, settings.ShowAgentHealth);
            if (_mountHealthWidget != null) 
                SetWidgetVisibility(_mountHealthWidget, settings.ShowMountHealth);
            if (_shieldHealthWidget != null) 
                SetWidgetVisibility(_shieldHealthWidget, settings.ShowShieldHealth);
            if (_weaponInfoWidget != null) 
                SetWidgetVisibility(_weaponInfoWidget, settings.ShowWeaponInfo);
            if (_goldAmountWidget != null) 
                SetWidgetVisibility(_goldAmountWidget, settings.ShowGoldAmount);
            if (_troopCountWidget != null) 
                SetWidgetVisibility(_troopCountWidget, settings.ShowTroopCount);
            if (_couchLanceWidget != null) 
                SetWidgetVisibility(_couchLanceWidget, settings.ShowCouchLanceState);
            if (_spearBraceWidget != null) 
                SetWidgetVisibility(_spearBraceWidget, settings.ShowCouchLanceState);
            if (_damageFeedWidget != null) 
                SetWidgetVisibility(_damageFeedWidget, settings.ShowDamageFeed);
        }

        private void SetWidgetVisibility(Widget widget, bool visible)
        {
            if (widget == null) return;
            widget.IsVisible = visible;
        }

        private void ApplyAgentStatusCustomization()
        {
            var settings = _dataSource.GetSettings();
            
            if (_agentHealthWidget != null && _agentHealthOriginal.IsValid)
                ApplyWidgetCustomization(_agentHealthWidget, _agentHealthOriginal, settings.AgentHealthCustom);
            if (_mountHealthWidget != null && _mountHealthOriginal.IsValid)
                ApplyWidgetCustomization(_mountHealthWidget, _mountHealthOriginal, settings.MountHealthCustom);
            if (_shieldHealthWidget != null && _shieldHealthOriginal.IsValid)
                ApplyWidgetCustomization(_shieldHealthWidget, _shieldHealthOriginal, settings.ShieldHealthCustom);
            if (_weaponInfoWidget != null && _weaponInfoOriginal.IsValid)
                ApplyWidgetCustomization(_weaponInfoWidget, _weaponInfoOriginal, settings.WeaponInfoCustom);
            if (_goldAmountWidget != null && _goldAmountOriginal.IsValid)
                ApplyWidgetCustomization(_goldAmountWidget, _goldAmountOriginal, settings.GoldAmountCustom);
            if (_troopCountWidget != null && _troopCountOriginal.IsValid)
                ApplyWidgetCustomization(_troopCountWidget, _troopCountOriginal, settings.TroopCountCustom);
            if (_damageFeedWidget != null && _damageFeedOriginal.IsValid)
                ApplyWidgetCustomization(_damageFeedWidget, _damageFeedOriginal, settings.DamageFeedCustom);
        }

        private void ApplyKillfeedCustomization()
        {
            if (_killfeedRootWidget == null || _killfeedVM == null) return;

            if (!_killfeedOriginal.IsValid)
                _killfeedOriginal = CaptureWidgetValues(_killfeedRootWidget);

            var settings = _dataSource.GetSettings();
            var custom = settings.KillfeedCustom;

            _killfeedRootWidget.PositionXOffset = _killfeedOriginal.X + custom.OffsetX;
            _killfeedRootWidget.PositionYOffset = _killfeedOriginal.Y + custom.OffsetY;
            _killfeedVM.UpdateScale(custom.Scale);
        }

        private WidgetOriginalValues CaptureWidgetValues(Widget widget)
        {
            float fontSize = 0f;
            if (widget is TextWidget textWidget)
                fontSize = textWidget.Brush?.FontSize ?? 0f;
            
            return new WidgetOriginalValues {
                X = widget.PositionXOffset, Y = widget.PositionYOffset,
                Width = widget.SuggestedWidth, Height = widget.SuggestedHeight,
                MarginTop = widget.MarginTop, MarginBottom = widget.MarginBottom,
                MarginLeft = widget.MarginLeft, MarginRight = widget.MarginRight,
                FontSize = fontSize, IsValid = true
            };
        }

        private void StoreOriginalValues()
        {
            _childOriginals.Clear();
            if (_timeAndScoresWidget != null) { _timeAndScoresOriginal = CaptureWidgetValues(_timeAndScoresWidget); StoreChildrenOriginals(_timeAndScoresWidget); }
            if (_avatarsWidget != null) { _avatarsOriginal = CaptureWidgetValues(_avatarsWidget); StoreChildrenOriginals(_avatarsWidget); }
            if (_moraleWidget != null) { _moraleOriginal = CaptureWidgetValues(_moraleWidget); StoreChildrenOriginals(_moraleWidget); }
            if (_controlPointsWidget != null) { _controlPointsOriginal = CaptureWidgetValues(_controlPointsWidget); StoreChildrenOriginals(_controlPointsWidget); }
        }

        private void StoreChildrenOriginals(Widget parent)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (!_childOriginals.ContainsKey(child)) _childOriginals[child] = CaptureWidgetValues(child);
                StoreChildrenOriginals(child);
            }
        }

        private void ApplyVisibilitySettings()
        {
            if (_timeAndScoresWidget != null) _timeAndScoresWidget.IsVisible = _dataSource.ShowTimeAndScores;
            if (_avatarsWidget != null) _avatarsWidget.IsVisible = _dataSource.ShowAvatars;
            if (_enemyScoreWidget != null) _enemyScoreWidget.IsVisible = _dataSource.ShowEnemyScore;
            foreach (var banner in _bannerWidgets) banner.IsVisible = _dataSource.ShowBanners;
            if (_moraleWidget != null) _moraleWidget.IsVisible = _dataSource.ShowMorale;
            if (_controlPointsWidget != null) _controlPointsWidget.IsVisible = _dataSource.ShowMorale;
        }

        private void ApplyCustomizationSettings()
        {
            var settings = _dataSource.GetSettings();
            if (_timeAndScoresWidget != null && _timeAndScoresOriginal.IsValid) ApplyWidgetCustomization(_timeAndScoresWidget, _timeAndScoresOriginal, settings.TimeAndScoresCustom);
            if (_avatarsWidget != null && _avatarsOriginal.IsValid) ApplyWidgetCustomization(_avatarsWidget, _avatarsOriginal, settings.TeamAvatarsCustom);
            if (_moraleWidget != null && _moraleOriginal.IsValid) ApplyWidgetCustomization(_moraleWidget, _moraleOriginal, settings.MoraleCustom);
            if (_controlPointsWidget != null && _controlPointsOriginal.IsValid) ApplyWidgetCustomization(_controlPointsWidget, _controlPointsOriginal, settings.MoraleCustom);
        }

        private void ApplyWidgetCustomization(Widget widget, WidgetOriginalValues original, ElementCustomization custom)
        {
            widget.PositionXOffset = original.X + custom.OffsetX;
            widget.PositionYOffset = original.Y + custom.OffsetY;
            if (custom.Scale != 1f)
            {
                if (widget.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0) widget.SuggestedWidth = original.Width * custom.Scale;
                if (widget.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0) widget.SuggestedHeight = original.Height * custom.Scale;
                ApplyScaleToChildren(widget, custom.Scale);
            }
            else
            {
                if (widget.WidthSizePolicy == SizePolicy.Fixed) widget.SuggestedWidth = original.Width;
                if (widget.HeightSizePolicy == SizePolicy.Fixed) widget.SuggestedHeight = original.Height;
                ResetChildrenToOriginal(widget);
            }
        }

        private void ApplyScaleToChildren(Widget parent, float scale)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (_childOriginals.TryGetValue(child, out WidgetOriginalValues original))
                {
                    if (child.WidthSizePolicy == SizePolicy.Fixed && original.Width > 0) child.SuggestedWidth = original.Width * scale;
                    if (child.HeightSizePolicy == SizePolicy.Fixed && original.Height > 0) child.SuggestedHeight = original.Height * scale;
                    child.MarginTop = original.MarginTop * scale; child.MarginBottom = original.MarginBottom * scale;
                    child.MarginLeft = original.MarginLeft * scale; child.MarginRight = original.MarginRight * scale;
                }
                ApplyScaleToChildren(child, scale);
            }
        }

        private void ResetChildrenToOriginal(Widget parent)
        {
            for (int i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChild(i);
                if (_childOriginals.TryGetValue(child, out WidgetOriginalValues original))
                {
                    if (child.WidthSizePolicy == SizePolicy.Fixed) child.SuggestedWidth = original.Width;
                    if (child.HeightSizePolicy == SizePolicy.Fixed) child.SuggestedHeight = original.Height;
                    child.MarginTop = original.MarginTop; child.MarginBottom = original.MarginBottom;
                    child.MarginLeft = original.MarginLeft; child.MarginRight = original.MarginRight;
                }
                ResetChildrenToOriginal(child);
            }
        }

        private void CacheWidgets(Widget root)
        {
            _bannerWidgets.Clear();
            _controlPointsWidget = null;
            SearchWidgetsRecursively(root, 0, null);
        }

        private void SearchWidgetsRecursively(Widget widget, int depth, Widget parent)
        {
            if (depth > 50) return;

            if (_timeAndScoresWidget == null && widget is ListPanel && widget.HorizontalAlignment == HorizontalAlignment.Center && widget.VerticalAlignment == VerticalAlignment.Top && widget.MarginTop >= 4 && widget.MarginTop <= 6)
                _timeAndScoresWidget = widget;

            if (_avatarsWidget == null && widget is ListPanel && widget.HeightSizePolicy == SizePolicy.Fixed && widget.SuggestedHeight >= 74 && widget.SuggestedHeight <= 76 && widget.ChildCount == 3)
                _avatarsWidget = widget;

            if (_enemyScoreWidget == null && widget.WidthSizePolicy == SizePolicy.Fixed && widget.SuggestedWidth >= 9 && widget.SuggestedWidth <= 11 && widget.MarginLeft >= 4 && widget.MarginLeft <= 6)
                _enemyScoreWidget = widget;

            if (widget.WidthSizePolicy == SizePolicy.Fixed && widget.HeightSizePolicy == SizePolicy.Fixed && widget.SuggestedWidth >= 49 && widget.SuggestedWidth <= 51 && widget.SuggestedHeight >= 49 && widget.SuggestedHeight <= 51)
            {
                bool hasBannerChild = false;
                for (int i = 0; i < widget.ChildCount; i++)
                    if (widget.GetChild(i).GetType().Name.Contains("MaskedTextureWidget")) { hasBannerChild = true; break; }
                if (hasBannerChild && !_bannerWidgets.Contains(widget)) _bannerWidgets.Add(widget);
            }

            if (_moraleWidget == null && widget is ListPanel)
            {
                bool isMoraleBySprite = widget.Sprite != null && (widget.Sprite.Name.Contains("morale_canvas") || widget.Sprite.Name.Contains("morale"));
                bool hasMoraleChild = false;
                for (int i = 0; i < widget.ChildCount; i++) { if (widget.GetChild(i).GetType().Name.Contains("MoraleWidget")) { hasMoraleChild = true; break; } }
                if (isMoraleBySprite || hasMoraleChild) _moraleWidget = widget;
            }
            
            if (_controlPointsWidget == null && widget is ListPanel && widget.HorizontalAlignment == HorizontalAlignment.Center && Math.Abs(widget.MarginTop - 80f) < 1f && widget.ChildCount == 3)
                _controlPointsWidget = widget;

            for (int i = 0; i < widget.ChildCount; i++) SearchWidgetsRecursively(widget.GetChild(i), depth + 1, widget);
        }

        private void OnWarbandKillfeedToggled(bool enabled)
        {
            if (_killfeedVM != null) { _killfeedVM.IsVisible = enabled; if (!enabled) _killfeedVM.Clear(); }
        }

        private void ToggleMenu() { if (_dataSource == null) return; if (_dataSource.IsConfigMenuOpen) CloseMenu(); else OpenMenu(); }

        private void OpenMenu()
        {
            if (_dataSource == null || _configLayer == null) return;
            _dataSource.IsConfigMenuOpen = true;
            _configLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            ScreenManager.TrySetFocus(_configLayer);
        }

        private void CloseMenu()
        {
            if (_dataSource == null || _configLayer == null) return;
            _dataSource.IsConfigMenuOpen = false;
            _configLayer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.Invalid);
            ScreenManager.TryLoseFocus(_configLayer);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            ManagedOptions.SetConfig(ManagedOptions.ManagedOptionsType.ReportCasualtiesType, 0f);
            var missionScreen = ScreenManager.TopScreen as MissionScreen;
            if (missionScreen != null) 
            { 
                if (_configLayer != null) missionScreen.RemoveLayer(_configLayer); 
                if (_killfeedLayer != null) missionScreen.RemoveLayer(_killfeedLayer); 
            }
            _dataSource = null; _killfeedVM = null; 
            _initialized = false; _widgetsCached = false; _agentStatusWidgetsCached = false;
            _childOriginals.Clear(); 
        }
    }
}