using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaCells.Common
{
	//We SURELY don't need more than one of this
	//I REALLY don't feel like making a separate directory for it
	//You may disagree, at which point, it becomes your responsibility to move this if you so choose
    public class TCellsGlobalBuff : GlobalBuff
    {
		public override void Update(int type, Player player, ref int buffIndex)
		{
			switch (type)
			{
				case BuffID.Swiftness:
					player.moveSpeed += 0.05f; //.25 + 0.05 => 0.3
					break;
				case BuffID.Wrath:
					player.GetDamage(DamageClass.Generic) += 0.15f; //0.1 + 0.15 => 0.25
					break;
				case BuffID.MagicPower:
					player.GetDamage(DamageClass.Magic) -= 0.2f; //0.2 - 0.2 => 0
					player.manaCost -= 0.5f;
					break;
			}
		}
	}
}
