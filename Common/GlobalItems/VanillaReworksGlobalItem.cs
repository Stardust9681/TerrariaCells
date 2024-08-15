using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems
{
    public class VanillaReworksGlobalItem : GlobalItem
    {

        // Prevents guns from utilizing ammo
        public override bool NeedsAmmo(Item item, Player player)
        {
            return false;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {

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
                        float knockback = Main.LocalPlayer.GetWeaponKnockback(item, item.knockBack);

                        knockback = (knockback / 20) * 100;
                        knockback = MathF.Round(knockback);

                        tooltip.Text += " (" + knockback + "%)";
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
