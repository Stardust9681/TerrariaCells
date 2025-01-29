using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems
{
    public class VanillaReworksGlobalItem : GlobalItem
    {

        public override void SetDefaults(Item item)
        {
            // CHANGE DEFAULT ITEM STATS HERE
            switch (item.type)
            {

                // RANGED WEAPONS
                // Guns
                case ItemID.PhoenixBlaster:
                    item.damage = 8;
                    item.useTime = 14;
                    break;
                case ItemID.Minishark:
                    // dps ~60, with reloading ~90, tapers off to 70
                    item.damage = 3;
                    break;
                case ItemID.SniperRifle:
                    item.damage = 40;
                    item.useTime = 25;
                    item.useAnimation = item.useTime;
                    break;
                case ItemID.OnyxBlaster:
                    item.damage = 5;
                    item.useTime = 48;
                    break;
                // Bows
                case ItemID.PulseBow:
                    item.damage = 8;
                    item.useTime = 23;
                    break;
                case ItemID.IceBow:
                    item.damage = 4;
                    item.useTime = 16;
                    break;
                // Launchers
                case ItemID.Toxikarp:
                    item.damage = 2;
                    item.useTime = 12;
                    break;
                case ItemID.RocketLauncher:
                    item.damage = 35;
                    item.useTime = 30;
                    break;
                case ItemID.StarCannon:
                    item.damage = 15;
                    item.useTime = 10;
                    item.useAnimation = 30;
                    item.reuseDelay = 30;
                    break;
                case ItemID.GrenadeLauncher:
                    item.damage = 18;
                    item.useTime = 20;
                    break;
                // MELEE
                // Swords
                case ItemID.FieryGreatsword:
                    item.damage = 12;
                    item.useTime = 30;
                    break;
                case ItemID.Starfury:
                    item.damage = 8;
                    item.useTime = 20;
                    break;
            }
        }

        // Prevents guns from utilizing ammo
        public override bool NeedsAmmo(Item item, Player player)
        {
            return false;
        }


        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            // Disable crits for all weapons by default
            crit = 0;

            // ADD/MODIFY CUSTOM CRIT EFFECTS HERE
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;

            // Iterate through the list of tooltips so we can change vanilla tooltips
            foreach (TooltipLine tooltip in tooltips)
            {
                // CHANGE VANILLA TOOLTIPS HERE
                switch (tooltip.Name)
                {
                    case "Material": // Remove the Material tag in the item tooltip
                        tooltip.Hide();
                        break;
                    case "Knockback":
                        float knockback = player.GetWeaponKnockback(item, item.knockBack);

                        knockback = (knockback / 20) * 100;
                        knockback = MathF.Round(knockback);

                        tooltip.Text += " (" + knockback + "%)";
                        break;
                    case "Speed":

                        int tempStat = (int)(item.useAnimation * (1 / player.GetWeaponAttackSpeed(item)));

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
                    case "UseMana":
                        tooltip.Hide();
                        break;
                    case "EtherianManaWarning":
                        tooltip.Hide();
                        break;
                    case "OneDropLogo":
                        tooltip.Hide();
                        break;
                        /*
                    case "Favorite":
                        tooltip.Hide();
                        break;
                    case "FavoriteDesc":
                        tooltip.Hide();
                        break;
                        */
                }

            }


            /*
            //// FOR TESTING
            Mod.Logger.Debug("Tooltip for " + item.Name);
            foreach (TooltipLine tooltip in tooltips)
            {
                Mod.Logger.Debug(tooltip.Name + " : " + tooltip.Text);
            }
            */

        }
    }
}
