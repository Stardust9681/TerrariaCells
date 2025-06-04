using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers
{
    public class BaseStatsPlayer : ModPlayer
    {
        public override void PostUpdateMiscEffects()
        {
			/*Player.manaRegenBonus = -80;
            if (Player.velocity.Length() > 0.01f)
            {
                Player.manaRegenBonus = -34;
            }*/

			//These values ripped from Terraria.Player.UpdateManaRegen()
			Player.manaRegenBonus -= Player.statManaMax2 / 3 + 1;
			if (Player.IsStandingStillForSpecialEffects || Player.grappling[0] > -1 || Player.manaRegenBuff)
			{
				Player.manaRegenBonus -= Player.statManaMax2 / 3;
			}
			if (Player.usedArcaneCrystal)
			{
				Player.manaRegenBonus -= Player.statManaMax2 / 50;
			}
			Player.manaRegenBonus += Player.statManaMax2 / (int)MathHelper.Lerp(33, 66, (float)Player.statMana/(float)Player.statManaMax2);
        }

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
		{
			base.ModifyMaxStats(out health, out mana);
			mana.Flat += 180;
		}
	}
}
