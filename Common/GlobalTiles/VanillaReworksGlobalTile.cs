using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalTiles
{
    public class VanillaReworksGlobalTile : GlobalTile
    {
        public override void Load()
        {
            On_Projectile.ExplodeTiles += On_Projectile_ExplodeTiles;
        }

        // Prevent explosions from destroying tiles
        private void On_Projectile_ExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode)
        {
            return;
        }

        // Stop tiles from dropping any items when destroyed
        public override bool CanDrop(int i, int j, int type)
        {
            return false;
        }

    }
}
