using Terraria;
using Terraria.ModLoader;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.Items 
{
	public class PylonMarkerItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<PylonMarkerTile>());
		}
	}
}
