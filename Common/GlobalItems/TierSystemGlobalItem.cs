using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.GlobalItems
{
    /// <summary>
    /// GlobalItem that applies a level to every damage-dealing item, handling relevant weapon scaling and 
    /// </summary>
    public class TierSystemGlobalItem : GlobalItem
    {

        public static float damageLevelScaling = 1.2f;
        public static float critLevelScaling = 1.05f;
        public static float knockbackLevelScaling = 1.05f;
        public static float attackSpeedLevelScaling = 0.02f;

        public int itemLevel = 1;

        public override bool InstancePerEntity => true;

        // Only apply item levels to weapons
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return lateInstantiation && entity.damage > 0 || lateInstantiation && entity.shoot > 0;
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
            damage *= MathF.Sqrt(itemLevel * damageLevelScaling);
        }

        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            crit *= MathF.Sqrt(itemLevel * critLevelScaling);
        }

        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            knockback *= MathF.Sqrt(itemLevel * knockbackLevelScaling);
        }

        public override float UseSpeedMultiplier(Item item, Player player)
        {
            return 1 + MathF.Sqrt((itemLevel - 1) * attackSpeedLevelScaling);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {

            // Iterate backwards through the list of tooltips so we can change it while we iterate through
            for (int i = tooltips.Count - 1; i >= 0; i--)
            {
                // Alter vanilla tooltips here
                switch (tooltips[i].Name)
                {
                    case "ItemName":
                        tooltips[i].Text += " [Tier " + itemLevel.ToString() + "]";
                        break;
                    case "Speed": // Only works here because this is where the speed multipler calculation is

                        int tempStat = (int)(item.useAnimation * (1 / UseSpeedMultiplier(item, Main.LocalPlayer)));

                        if (tempStat <= 8)
                            tooltips[i].Text = Lang.tip[6].Value;
                        else if (tempStat <= 20)
                            tooltips[i].Text = Lang.tip[7].Value;
                        else if (tempStat <= 25)
                            tooltips[i].Text = Lang.tip[8].Value;
                        else if (tempStat <= 30)
                            tooltips[i].Text = Lang.tip[9].Value;
                        else if (tempStat <= 35)
                            tooltips[i].Text = Lang.tip[10].Value;
                        else if (tempStat <= 45)
                            tooltips[i].Text = Lang.tip[11].Value;
                        else if (tempStat <= 55)
                            tooltips[i].Text = Lang.tip[12].Value;
                        else
                            tooltips[i].Text = Lang.tip[13].Value;

                        tooltips[i].Text += " (" + 60 / tempStat + " " + Mod.GetLocalization("Tooltips.AttacksPerSecond").Value + ")";
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

        // I thought this was supposed to help with keeping values, according to the Calamity source code, but it didn't seem to help- will revisit if other issues with values transferring over are found.
        public override GlobalItem Clone(Item item, Item itemClone)
        {
            TierSystemGlobalItem myClone = (TierSystemGlobalItem)base.Clone(item, itemClone);

            myClone.itemLevel = itemLevel;

            return myClone;
        }


    }

}
