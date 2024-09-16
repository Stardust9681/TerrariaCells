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
                case ItemID.Minishark:
                    // dps ~60, with reloading ~90, tapers off to 70
                    item.damage = 3;
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
