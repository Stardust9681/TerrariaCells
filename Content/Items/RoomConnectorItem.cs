using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.Items
{
	public class RoomConnectorItem : ModItem
	{
		public override void SetDefaults() {
			Item.consumable = true;
			Item.useTime = 10;
			Item.useAnimation = 10;
			Item.useStyle = 1;
			Item.maxStack = 9999;
			Item.autoReuse = true;
			Item.createTile = ModContent.TileType<Tiles.RoomConnectorTile>();
		}
	}
}
