using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace ModTesting.Content.Items
{
    public class TierSystemGlobalItem : GlobalItem
    {

        public static float damageLevelScaling = 1.2f;
        public static float critLevelScaling = 1.05f;
        public static float knockbackLevelScaling = 1.125f;

        public int itemLevel = 1;

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0;
        }

        public override void SetDefaults(Item item)
        {
            SetNameWithTier(item);
        }

        public void GainLevels(Item item, int xp)
        {
            itemLevel += xp;
        }

        // Modify overrides to set weapon stats based on item level
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            damage *= MathF.Pow(damageLevelScaling, itemLevel);
        }

        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            crit *= MathF.Pow(critLevelScaling, itemLevel);
        }

        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            knockback *= MathF.Pow(knockbackLevelScaling, itemLevel);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (item.damage > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "Tier", "[Tier " + itemLevel.ToString() + "]"));
            }
        }

        // Occurs immediately following a reforge, currently increases item level when reforged
        public override void PostReforge(Item item)
        {
            GainLevels(item, 1);
            SetDefaults(item);
        }

        /// <summary>
        /// Sets the name of the item with the current tier as a suffix
        /// </summary>
        /// <param name="item"></param>
        public void SetNameWithTier(Item item)
        {
            item.ClearNameOverride();
            item.SetNameOverride(item.Name + " [Tier " + itemLevel.ToString() + "]");
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(itemLevel);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            itemLevel = 0;
            GainLevels(item, reader.ReadInt32());

            SetNameWithTier(item);
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            tag["level"] = itemLevel;//Save experience tag
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            itemLevel = 0;
            GainLevels(item, tag.Get<int>("level")); //Load experience tag

            SetNameWithTier(item);
        }

        // I thought this was supposed to help with keeping values, according to the Calamity source code, but it didn't seem to help- will revisit if other issues with values transferring over are found.
        /*
        public override GlobalItem Clone(Item item, Item itemClone)
        {
            ModTesting.Instance.Logger.Debug(item.Name + " cloned with tier " + itemLevel.ToString());

            TierSystemGlobalItem myClone = (TierSystemGlobalItem)base.Clone(item, itemClone);

            myClone.itemLevel = itemLevel;

            return myClone;
        }
        */

    }

}
