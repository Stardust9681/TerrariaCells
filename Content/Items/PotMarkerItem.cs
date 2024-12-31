using Terraria;
using Terraria.ModLoader;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.Items 
{
	public class PotMarkerItem : ModItem 
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<PotMarkerTile>());
		}
	}
}
