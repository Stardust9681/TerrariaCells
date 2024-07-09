using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TerrariaCells.Common.ModPlayers
{
	//I suspect we're going to want different modplayers:
	//This can (for now) handle the responsibilities of handling all the new weapon types
	public class WeaponsManager : ModPlayer
	{
		public Content.Items.CellsWeapon UseItem { get; internal set; }
	}
}
