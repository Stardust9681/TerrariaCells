using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalTiles {
    internal class BastStatueReworkGlobalTile : GlobalTile {
        public override void RightClick(int i, int j, int type) {
            if (type != TileID.CatBast) {
                return;
            }
            var tile = Framing.GetTileSafely(i, j);
            var frameX = tile.TileFrameX;
            if (frameX >= 72) {
                // The player has used this statue
                return;
            }
            if (frameX % 36 == 18) {
                i -= 1;
            }
            j -= tile.TileFrameY / 18;
            for (var y = j; y < j + 3; y++) {
                for (var x = i; x < i + 2; x++) {
                    // technically this could cause 
                    // the transmutation glitch :P
                    Main.tile[x, y].TileFrameX += 72;
                }
            }
            // todo: netcode to tell the server that the statue is deactivated
            Main.LocalPlayer.AddBuff(BuffID.CatBast, 1080000, false);
        }

        public override void NearbyEffects(int i, int j, int type, bool closer) {
            var tile = Framing.GetTileSafely(i, j);
            if (tile.TileType != TileID.CatBast) {
                return;
            }
            var frameX = tile.TileFrameX;
            if (frameX >= 72) {
                // The player has used this statue
                return;
            }
            Main.LocalPlayer.ClearBuff(BuffID.CatBast);
        }
    }
}
