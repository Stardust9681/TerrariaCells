using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace TerrariaCells.Content.Tiles 
{
	public class PotMarkerTile : ModTile 
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.addTile(Type);
			AddMapEntry(Color.Blue);
		}
	}
}
