using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaCells.Common.ModPlayers;

namespace TerrariaCells.Content.Tiles;

public class ClickableCrystalTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.addTile(Type);
        AddMapEntry(Color.Red);
    }

    public override bool RightClick(int i, int j)
    {
        WorldGen.KillTile(i, j);
        Main.player[Main.myPlayer].GetModPlayer<LifeModPlayer>().extraHealth += 20;
        return true;
    }

    public override bool CanDrop(int i, int j)
    {
        return false;
    }
}
