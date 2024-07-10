using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TerrariaCells.Content.Tiles
{
    public class Connector : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileBlockLight[Type] = true;
            DustType = DustID.Stone;
            AddMapEntry(new Color(255, 0, 255));
        }
    }
}
