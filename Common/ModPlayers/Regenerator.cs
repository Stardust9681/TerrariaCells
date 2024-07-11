using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers
{
	public class Regenerator : ModPlayer
	{
		public override void NaturalLifeRegen(ref float regen)
		{
			regen = 0;
		}
	}
}
