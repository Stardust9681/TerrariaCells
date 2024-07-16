using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerrariaCells.Content.Tiles
{
	public class PylonMarkerTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.addTile(Type);
			AddMapEntry(Color.DarkRed);
		}
	}
}
