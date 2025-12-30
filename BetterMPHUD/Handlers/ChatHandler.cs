using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;
using BetterMPHUD.Services;

namespace BetterMPHUD.Handlers
{
    public class ChatHandler
    {
        private Widget _chatLogWidget;
        private Widget _chatLogParent;
        
        private Widget _chatLogInnerPanel;
        private Widget _chatTextBackground;
        private Widget _chatTextInputParent;
        private Widget _combatLogButton;
        private Widget _shoutsButton;
        private Widget _cycleChannelsTextContainer;
        private Widget _verticalScrollbar;

        
        private Dictionary<Widget, WidgetOriginalValues> _childOriginals = new Dictionary<Widget, WidgetOriginalValues>();
        
        private bool _cached;
        private bool _lastShowChat = true;
        private bool _lastMinimalMode = false;
        
        private static WidgetOriginalValues _originalValues;
        private static bool _originalsCaptured = false;

        public void Apply(HudSettings settings, Mission mission)
        {
            if (!_cached)
            {
                GauntletLayer layer = FindChatLayer();
                if (layer == null || layer.UIContext == null || layer.UIContext.Root == null)
                    return;

                CacheWidgets(layer.UIContext.Root);
                if (_chatLogWidget == null)
                    return;
                    
                _cached = true;
            }

            if (_chatLogWidget == null)
                return;

            ApplyVisibility(settings);
            ApplyMinimalMode(settings);
            ApplyCustomization(settings);
        }

        private GauntletLayer FindChatLayer()
        {
            try
            {
                Type chatLogViewType = null;
                
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        chatLogViewType = assembly.GetType("TaleWorlds.MountAndBlade.GauntletUI.GauntletChatLogView");
                        if (chatLogViewType != null)
                            break;
                    }
                    catch { }
                }
                
                if (chatLogViewType == null)
                    return null;

                PropertyInfo currentProp = chatLogViewType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static);
                if (currentProp == null)
                    return null;

                object chatLogView = currentProp.GetValue(null);
                if (chatLogView == null)
                    return null;

                PropertyInfo layerProp = chatLogViewType.GetProperty("Layer", BindingFlags.Public | BindingFlags.Instance);
                if (layerProp == null)
                {
                    Type baseType = chatLogViewType.BaseType;
                    while (baseType != null)
                    {
                        layerProp = baseType.GetProperty("Layer", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                        if (layerProp != null)
                            break;
                        baseType = baseType.BaseType;
                    }
                }

                if (layerProp != null)
                {
                    GauntletLayer layer = layerProp.GetValue(chatLogView) as GauntletLayer;
                    if (layer != null)
                        return layer;
                }

                BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                foreach (FieldInfo field in chatLogViewType.GetFields(flags))
                {
                    if (typeof(GauntletLayer).IsAssignableFrom(field.FieldType))
                    {
                        GauntletLayer layer = field.GetValue(chatLogView) as GauntletLayer;
                        if (layer != null)
                            return layer;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private Widget FindChatWidgetInTree(Widget root)
        {
            if (root == null) return null;
            
            string typeName = root.GetType().Name;
            if (typeName == "ChatLogWidget")
                return root;
            
            if (root.Id == "ChatLogWidget")
                return root;

            for (int i = 0; i < root.ChildCount; i++)
            {
                Widget found = FindChatWidgetInTree(root.GetChild(i));
                if (found != null)
                    return found;
            }
            
            return null;
        }

        private Widget FindWidgetById(Widget root, string id, int maxDepth = 10)
        {
            if (root == null || maxDepth <= 0) return null;
            
            if (root.Id == id)
                return root;

            for (int i = 0; i < root.ChildCount; i++)
            {
                Widget found = FindWidgetById(root.GetChild(i), id, maxDepth - 1);
                if (found != null)
                    return found;
            }
            
            return null;
        }

        private void CacheWidgets(Widget root)
        {
            _chatLogWidget = FindChatWidgetInTree(root);
    
            if (_chatLogWidget == null)
                return;

            if (!_originalsCaptured)
            {
                _originalValues = WidgetOriginalValues.Capture(_chatLogWidget);
                _originalsCaptured = true;
            }
    
            _chatLogParent = _chatLogWidget.ParentWidget;
    
            _chatLogInnerPanel = FindWidgetById(_chatLogWidget, "ChatLogInnerPanel");
            _chatTextBackground = FindWidgetById(_chatLogWidget, "ChatTextBackground");
            _chatTextInputParent = FindWidgetById(_chatLogWidget, "ChatTextInputParent");
    
            Widget scrollContainer = FindWidgetById(_chatLogWidget, "ScrollablePanelContainer");
            if (scrollContainer != null)
            {
                _verticalScrollbar = FindWidgetById(scrollContainer, "VerticalScrollbar");
            }
    
            Widget chatInputParent = FindWidgetById(_chatLogWidget, "ChatInputParent");
            if (chatInputParent != null)
                FindButtonsRecursive(chatInputParent, 0);
    
            FindCycleTextContainer();
        }

        private void FindButtonsRecursive(Widget widget, int depth)
        {
            if (depth > 5) return;
            
            string typeName = widget.GetType().Name;
            
            if (typeName == "ButtonWidget" && 
                widget.SuggestedWidth == 100 && 
                widget.SuggestedHeight == 30)
            {
                if (_combatLogButton == null)
                    _combatLogButton = widget;
                else if (_shoutsButton == null)
                    _shoutsButton = widget;
            }
            
            for (int i = 0; i < widget.ChildCount; i++)
                FindButtonsRecursive(widget.GetChild(i), depth + 1);
        }

        private void FindCycleTextContainer()
        {
            if (_chatLogWidget == null) return;
            
            for (int i = 0; i < _chatLogWidget.ChildCount; i++)
            {
                Widget child = _chatLogWidget.GetChild(i);
                
                if (!string.IsNullOrEmpty(child.Id))
                    continue;
                
                bool hasTextChild = false;
                for (int j = 0; j < child.ChildCount; j++)
                {
                    if (child.GetChild(j) is TextWidget)
                    {
                        hasTextChild = true;
                        break;
                    }
                }
                
                if (hasTextChild)
                {
                    _cycleChannelsTextContainer = child;
                    break;
                }
            }
        }

        private void ApplyVisibility(HudSettings settings)
        {
            if (settings.ShowChat != _lastShowChat)
            {
                SetChatVisible(settings.ShowChat);
            }
        }

        private void ApplyMinimalMode(HudSettings settings)
        {
            if (settings.ChatMinimalMode != _lastMinimalMode)
            {
                _lastMinimalMode = settings.ChatMinimalMode;
                SetMinimalMode(settings.ChatMinimalMode);
            }
        }

        private void SetMinimalMode(bool minimal)
        {
            float alpha = minimal ? 0f : 1f;
    
            if (_chatLogInnerPanel != null)
                _chatLogInnerPanel.AlphaFactor = alpha;
    
            if (_chatTextBackground != null)
                _chatTextBackground.AlphaFactor = alpha;
    
            if (_chatTextInputParent != null)
                _chatTextInputParent.AlphaFactor = alpha;
            
            if (_verticalScrollbar != null)
                _verticalScrollbar.AlphaFactor = alpha;
            
            if (_verticalScrollbar != null)
                _verticalScrollbar.SetGlobalAlphaRecursively(alpha);
    
            if (_combatLogButton != null)
                _combatLogButton.SetGlobalAlphaRecursively(alpha);
    
            if (_shoutsButton != null)
                _shoutsButton.SetGlobalAlphaRecursively(alpha);
    
            if (_cycleChannelsTextContainer != null)
                _cycleChannelsTextContainer.SetGlobalAlphaRecursively(alpha);
        }

        private void ApplyCustomization(HudSettings settings)
        {
            if (_chatLogWidget == null || !_originalsCaptured) return;

            ElementCustomization custom = settings.ChatCustom;
            if (custom == null) return;
            
            _chatLogWidget.PositionXOffset = _originalValues.X + custom.OffsetX;
            _chatLogWidget.PositionYOffset = _originalValues.Y + custom.OffsetY;

            if (custom.Scale != 1f)
            {
                if (_originalValues.Width > 0)
                    _chatLogWidget.SuggestedWidth = _originalValues.Width * custom.Scale;
                if (_originalValues.Height > 0)
                    _chatLogWidget.SuggestedHeight = _originalValues.Height * custom.Scale;
            }
            else
            {
                if (_originalValues.Width > 0)
                    _chatLogWidget.SuggestedWidth = _originalValues.Width;
                if (_originalValues.Height > 0)
                    _chatLogWidget.SuggestedHeight = _originalValues.Height;
            }
        }

        public void SetChatVisible(bool visible)
        {
            _lastShowChat = visible;
            
            if (_chatLogWidget != null)
                _chatLogWidget.IsVisible = visible;
        }

        public void DebugPrintStructure()
        {
            GauntletLayer layer = FindChatLayer();
            
            if (layer == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[Chat] Could not find chat layer", Colors.Red));
                return;
            }
            
            if (layer.UIContext?.Root == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    "[Chat] Layer found but no UIContext/Root", Colors.Red));
                return;
            }

            InformationManager.DisplayMessage(new InformationMessage(
                "[Chat] Cached widgets:", Colors.Green));
            InformationManager.DisplayMessage(new InformationMessage(
                "  InnerPanel: " + (_chatLogInnerPanel != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "  TextBackground: " + (_chatTextBackground != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "  CombatLogBtn: " + (_combatLogButton != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "  ShoutsBtn: " + (_shoutsButton != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "  CycleText: " + (_cycleChannelsTextContainer != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "  TextInputParent: " + (_chatTextInputParent != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "  VerticalScrollbar: " + (_verticalScrollbar != null), Colors.Cyan));
            InformationManager.DisplayMessage(new InformationMessage(
                "[Chat] Full tree:", Colors.Green));
            PrintTree(layer.UIContext.Root, 0, 10);
        }

        private void PrintTree(Widget widget, int indent, int maxDepth)
        {
            if (maxDepth <= 0 || indent > 8) return;

            string typeName = widget.GetType().Name;
            string extra = "";

            if (!string.IsNullOrEmpty(widget.Id))
                extra += " Id:" + widget.Id;

            if (widget.WidthSizePolicy == SizePolicy.Fixed)
                extra += " [" + (int)widget.SuggestedWidth + "x" + (int)widget.SuggestedHeight + "]";

            string prefix = new string(' ', indent * 2);
            InformationManager.DisplayMessage(new InformationMessage(
                prefix + typeName + extra, Colors.White));

            for (int i = 0; i < widget.ChildCount; i++)
                PrintTree(widget.GetChild(i), indent + 1, maxDepth - 1);
        }

        public void Reset()
        {
            _cached = false;
            _chatLogWidget = null;
            _chatLogParent = null;
            _chatLogInnerPanel = null;
            _chatTextInputParent = null;
            _chatTextBackground = null;
            _combatLogButton = null;
            _shoutsButton = null;
            _cycleChannelsTextContainer = null;
            _childOriginals.Clear();
            _lastMinimalMode = false;
            _verticalScrollbar = null;
        }
    }
}