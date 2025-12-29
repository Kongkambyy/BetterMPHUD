using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BetterMPHUD.Handlers
{
    public class ScoreboardHandler
    {
        private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
        
        private Dictionary<Widget, float> _originalAlphas = new Dictionary<Widget, float>();
        private bool _initialized;

        public void Apply(HudSettings settings, Mission mission)
        {
            if (mission == null) return;

            GauntletLayer layer = FindScoreboardLayer(mission);
            
            if (layer == null || layer.UIContext == null || layer.UIContext.Root == null)
            {
                // Scoreboard closed - reset for next time
                _initialized = false;
                _originalAlphas.Clear();
                return;
            }

            // Apply every frame while scoreboard is open
            ApplyToAllWidgets(layer.UIContext.Root, settings, 0);
        }

        private GauntletLayer FindScoreboardLayer(Mission mission)
        {
            foreach (MissionBehavior behavior in mission.MissionBehaviors)
            {
                if (behavior.GetType().Name == "MissionGauntletMultiplayerScoreboard")
                {
                    return GetLayerFromBehavior(behavior);
                }
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

            // Store original alpha on first encounter
            if (!_originalAlphas.ContainsKey(widget))
            {
                _originalAlphas[widget] = widget.AlphaFactor;
            }

            float originalAlpha = _originalAlphas[widget];
            bool isBackground = false;
            
            // Check if this is a background widget
            if (widget.Sprite != null && widget.Sprite.Name != null)
            {
                string spriteName = widget.Sprite.Name;
                
                // Main panel background
                if (spriteName.Contains("flat_panel"))
                {
                    isBackground = true;
                    
                    if (settings.ScoreboardBackgroundEnabled)
                        widget.AlphaFactor = settings.ScoreboardBackgroundOpacity;
                    else
                        widget.AlphaFactor = 0f;
                }
                // Other backgrounds (BlankWhiteSquare)
                else if (spriteName.Contains("BlankWhiteSquare"))
                {
                    isBackground = true;
                    
                    if (settings.ScoreboardStripingEnabled)
                    {
                        // Scale original alpha by our setting
                        float targetAlpha = originalAlpha * (settings.ScoreboardStripingOpacity / 0.4f);
                        widget.AlphaFactor = Math.Min(targetAlpha, 1f);
                    }
                    else
                    {
                        widget.AlphaFactor = 0f;
                    }
                }
            }

            // Recurse to children
            for (int i = 0; i < widget.ChildCount; i++)
            {
                ApplyToAllWidgets(widget.GetChild(i), settings, depth + 1);
            }
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

            GauntletLayer layer = GetLayerFromBehavior(scoreboardBehavior);
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
            // Restore original alphas if widgets still exist
            foreach (var kvp in _originalAlphas)
            {
                try
                {
                    if (kvp.Key != null)
                        kvp.Key.AlphaFactor = kvp.Value;
                }
                catch { }
            }

            _initialized = false;
            _originalAlphas.Clear();
        }
    }
}