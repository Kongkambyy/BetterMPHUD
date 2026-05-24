using System;
using System.Reflection;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;

namespace BetterMPHUD.Services
{
    public static class LayerFinder
    {
        private static readonly string[] CommonLayerFieldNames = new string[]
        { 
            "_gauntletLayer", "_layer", "_hudLayer", "gauntletLayer", "_dataSource" 
        };

        private static readonly BindingFlags Flags = 
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

        public static GauntletLayer FindInBehavior(MissionBehavior behavior)
        {
            if (behavior == null) return null;

            for (int i = 0; i < CommonLayerFieldNames.Length; i++)
            {
                FieldInfo field = behavior.GetType().GetField(CommonLayerFieldNames[i], Flags);
                if (field != null)
                {
                    GauntletLayer layer = field.GetValue(behavior) as GauntletLayer;
                    if (layer != null) return layer;
                }
            }
            return null;
        }

        public static GauntletLayer FindByPredicate(Mission mission, Func<GauntletLayer, bool> predicate)
        {
            foreach (MissionBehavior behavior in mission.MissionBehaviors)
            {
                FieldInfo[] fields = behavior.GetType().GetFields(Flags);
                foreach (FieldInfo field in fields)
                {
                    if (!typeof(GauntletLayer).IsAssignableFrom(field.FieldType)) continue;
                    
                    GauntletLayer layer = field.GetValue(behavior) as GauntletLayer;
                    if (layer != null && layer.UIContext != null && layer.UIContext.Root != null && predicate(layer))
                        return layer;
                }
            }
            return null;
        }

        public static MissionBehavior FindBehaviorByName(Mission mission, params string[] namePatterns)
        {
            foreach (MissionBehavior behavior in mission.MissionBehaviors)
            {
                string typeName = behavior.GetType().Name;
                for (int i = 0; i < namePatterns.Length; i++)
                {
                    if (typeName.Contains(namePatterns[i])) 
                        return behavior;
                }
            }
            return null;
        }
    }
}
