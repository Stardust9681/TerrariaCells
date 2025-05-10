using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.TileEntities {
    internal class TickTimerTileEntity : ModTileEntity {
        public override void Update() {
            var x = Position.X;
            var y = Position.Y;
            if (Main.tile[x, y].TileFrameY != 0) {
                Wiring.TripWire(x, y, 1, 1);
            }
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 1, 1);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
            }
            return Place(i, j);
        }

        public override bool IsTileValidForEntity(int x, int y) {
            var tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<TickTimerTile>();
        }
    }
}
