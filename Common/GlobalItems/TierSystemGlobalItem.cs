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
        public static float damageLevelScaling = 1.30f;
        public static float knockbackLevelScaling = 1.125f;

        public int itemLevel = 1;

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0 || lateInstantiation && entity.shoot != ItemID.None;
        }

        public override void SetDefaults(Item item)
        {
            // all values recorded at tier 1 with no buffs and point blank
            // tried to make all weapons balanced around 30 dps
            switch (item.type)
            {
                case ItemID.PulseBow:
                    // with bounces: dps ~27, with charging ~60
                    // without bounces: dps ~21, with charing ~50
                    item.damage = 7;
                    break;
                case ItemID.QuadBarrelShotgun:
                    // dps ~40, with reloading ~48
                    item.damage = 5;
                    break;
                case ItemID.OnyxBlaster:
                    // dps ~28, with reloading ~32
                    item.damage = 4;
                    break;
                case ItemID.SniperRifle:
                    // dps: ~19, with reloading: ~40
                    item.damage = 18;
                    break;
                case ItemID.PhoenixBlaster:
                    // dps: ~27, with reloading: ~28
                    item.damage = 7;
                    break;
            }
            item.crit = -100;
            SetNameWithTier(item);
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
            // Equation is found using Sorbet's example values.
            // Graph of tiers vs damage values: https://www.desmos.com/calculator/mz89u5adai
            damage += MathF.Pow(damageLevelScaling, itemLevel) - 1.3f;
        }

        public override float UseSpeedMultiplier(Item item, Player player)
        {
            return 1;
            //return 1 + (MathF.Sqrt(itemLevel - 1) * attackSpeedLevelScaling);
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
        }
        /// <summary>
        /// Resets the name of the item and appends the tier as a suffix
        /// </summary>
        /// <param name="item"></param>
        public void SetNameWithTier(Item item)
        {
            item.ClearNameOverride();
            item.SetNameOverride(item.Name + " [Tier " + itemLevel.ToString() + "]");
            //tooltips.Add(new TooltipLine(Mod, "Tier", "[Tier " + itemLevel.ToString() + "]"));
        }

        public void SetRarityWithTier(Item item)
        {
            item.rare = itemLevel;
        }

        public void SetLevelAndRefreshItem(Item item, int level)
        {
            SetLevel(item, level);
            SetNameWithTier(item);
            SetRarityWithTier(item);
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
