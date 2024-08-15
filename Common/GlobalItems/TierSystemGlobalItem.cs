using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.GlobalItems
{
    /// <summary>
    /// GlobalItem that applies a level to every damage-dealing item, handling relevant weapon scaling and 
    /// </summary>
    public class TierSystemGlobalItem : GlobalItem
    {

        public static float damageLevelScaling = 2.2f;
        public static float knockbackLevelScaling = 0.35f;
        public static float attackSpeedLevelScaling = 0.125f;

        public int itemLevel = 1;

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0 || lateInstantiation && entity.shoot != ItemID.None;
        }

        public override void SetDefaults(Item item)
        {
            item.rare = itemLevel;
            Math.Clamp(item.rare, 0, 10);
        }

        public void AddLevels(Item item, int level)
        {
            itemLevel += level;
            SetDefaults(item);
        }

        public void SetLevel(Item item, int level)
        {
            itemLevel = level;
            SetDefaults(item);
        }

        // Modify overrides to set weapon stats based on item level
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            damage *= 1 + (MathF.Sqrt(itemLevel - 1) * damageLevelScaling);
        }

        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            knockback *= 1 + (MathF.Sqrt(itemLevel - 1) * knockbackLevelScaling);
        }

        public override float UseSpeedMultiplier(Item item, Player player)
        {
            return 1 + (MathF.Sqrt(itemLevel - 1) * attackSpeedLevelScaling);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {

            // Iterate through the list of tooltips so we can change vanilla tooltips
            foreach (TooltipLine tooltip in tooltips)
            {
                // Alter vanilla tooltips here
                switch (tooltip.Name)
                {
                    case "ItemName":
                        tooltip.Text += " [Tier " + itemLevel.ToString() + "]";
                        break;
                    case "Speed": // Only works here because this is where the UseSpeedMultiplier override is
                        int tempStat = (int)(item.useAnimation * (1 / UseSpeedMultiplier(item, Main.LocalPlayer)));

                        if (tempStat <= 8)
                            tooltip.Text = Lang.tip[6].Value;
                        else if (tempStat <= 20)
                            tooltip.Text = Lang.tip[7].Value;
                        else if (tempStat <= 25)
                            tooltip.Text = Lang.tip[8].Value;
                        else if (tempStat <= 30)
                            tooltip.Text = Lang.tip[9].Value;
                        else if (tempStat <= 35)
                            tooltip.Text = Lang.tip[10].Value;
                        else if (tempStat <= 45)
                            tooltip.Text = Lang.tip[11].Value;
                        else if (tempStat <= 55)
                            tooltip.Text = Lang.tip[12].Value;
                        else
                            tooltip.Text = Lang.tip[13].Value;

                        float attacksPerSecond = MathF.Round(60 / (float)tempStat, 2);
                        tooltip.Text += Mod.GetLocalization("Tooltips.AttacksPerSecond").Format(attacksPerSecond);
                        break;
                }

            }

            // Also add the tier at the end of the tooltip
            tooltips.Add(new TooltipLine(Mod, "Tier", "[Tier " + itemLevel.ToString() + "]"));
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            writer.Write(itemLevel);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            itemLevel = 0;
            AddLevels(item, reader.ReadInt32());
        }

        public override void SaveData(Item item, TagCompound tag)
        {
            tag["level"] = itemLevel;
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            itemLevel = 0;
            AddLevels(item, tag.Get<int>("level"));
        }

        public override GlobalItem Clone(Item item, Item itemClone)
        {
            TierSystemGlobalItem myClone = (TierSystemGlobalItem)base.Clone(item, itemClone);

            myClone.itemLevel = itemLevel;

            return myClone;
        }


    }

}
