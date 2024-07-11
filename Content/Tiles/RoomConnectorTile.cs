using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Tiles 
{
	public class RoomConnectorTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = false;
		}
	}	
}
