using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using BetterMPHUD.Core;

namespace BetterMPHUD.Services
{
    public static class AgentHelper
    {
        public static string GetClassSprite(Agent agent)
        {
            if (agent == null) return Constants.Sprites.Sword;
            if (agent.HasMount) return Constants.Sprites.Horse;
            if (HasRangedWeapon(agent)) return Constants.Sprites.Bow;
            return Constants.Sprites.Sword;
        }

        public static bool HasRangedWeapon(Agent agent)
        {
            if (agent == null || agent.Equipment == null) return false;
            
            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i++)
            {
                MissionWeapon item = agent.Equipment[i];
                if (item.IsEmpty) continue;
                if (item.Item == null || item.Item.PrimaryWeapon == null) continue;
                
                WeaponClass weaponClass = item.Item.PrimaryWeapon.WeaponClass;
                if (weaponClass == WeaponClass.Bow || weaponClass == WeaponClass.Crossbow)
                    return true;
            }
            return false;
        }
    }
}