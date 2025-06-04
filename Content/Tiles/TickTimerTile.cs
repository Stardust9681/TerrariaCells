using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Content.Tiles {
    internal class TickTimerTile : ModTile {
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new(
                ModContent.GetInstance<TickTimerTileEntity>().Hook_AfterPlacement,
                -1,
                0,
                true
            );
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);
        }
        public override bool RightClick(int i, int j) {
            var tile = Main.tile[i, j];
            tile.TileFrameY = (short) (tile.TileFrameY == 0 ? 18 : 0);
            return true;
        }

        public override void HitWire(int i, int j) {
            RightClick(i, j);
        }
    }
}
