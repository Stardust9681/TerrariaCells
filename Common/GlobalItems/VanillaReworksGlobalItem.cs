using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Map;
using Terraria.ModLoader;
using static TerrariaCells.Common.GlobalItems.ModifierSystem;

namespace TerrariaCells.Common.GlobalItems
{
    public class VanillaReworksGlobalItem : GlobalItem
    {
        public override bool NeedsAmmo(Item item, Player player)
        {
            return false;
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {

            // Iterate backwards through the list of tooltips so we can change it while we iterate through
            for (int i = tooltips.Count - 1; i >= 0; i--)
            {
                // Alter vanilla tooltips here
                switch (tooltips[i].Name)
                {
                    case "Material": // Remove the Material tag in the item tooltip
                        tooltips.Remove(tooltips[i]);
                        break;
                    case "Knockback":
                        float knockback = Main.LocalPlayer.GetWeaponKnockback(item, item.knockBack);

                        knockback = (knockback / 20) * 100;
                        knockback = MathF.Round(knockback);

                        tooltips[i].Text += " (" + knockback + "%)";
                        break;
                    case "UseMana":
                        tooltips.Remove(tooltips[i]);
                        break;
                    case "EtherianManaWarning":
                        tooltips.Remove(tooltips[i]);
                        break;
                    case "OneDropLogo":
                        tooltips.Remove(tooltips[i]);
                        break;
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
