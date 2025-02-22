using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TerrariaCells.Content.Items.Placeable
{
	public class CrimsonPylon : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.TeleportationPylonVictory}";

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Content.Tiles.CrimsonPylon>());
		}
	}
}
