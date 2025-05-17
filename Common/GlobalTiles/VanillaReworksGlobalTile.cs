using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.GlobalTiles
{
    public class VanillaReworksGlobalTile : GlobalTile
    {
        public override void Load()
        {
            On_Projectile.ExplodeTiles += On_Projectile_ExplodeTiles;
			
        }

        public override void Unload()
		{
			On_Projectile.ExplodeTiles -= On_Projectile_ExplodeTiles;
		}

		// Prevent explosions from destroying tiles
		private void On_Projectile_ExplodeTiles(On_Projectile.orig_ExplodeTiles orig, Projectile self, Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wallSplode)
        {
			if (!DevConfig.Instance.PreventExplosionDamage)
			{
				orig.Invoke(self, compareSpot, radius, minI, maxI, minJ, maxJ, wallSplode);
			}
        }

		// Stop tiles from dropping any items when destroyed
		public override bool CanDrop(int i, int j, int type)
        {
			if (DevConfig.Instance.BuilderMode)
			{
				return base.CanDrop(i, j, type);
			}

            return false;
        }

		public override bool CanPlace(int i, int j, int type)
		{
			if (DevConfig.Instance.BuilderMode)
			{
				return base.CanPlace(i, j, type);
			}

			return false;
		}

		public override void KillTile(int i, int j, int typeT, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
            if (typeT >= 481 && typeT <= 483)
            {
                noItem = true;
                Tile tile2 = Framing.GetTileSafely(i, j);
                tile2.ClearTile();
                int fallType = (int)(typeT - 481 + 736);
                if (Main.netMode == 0)
                {
                    Projectile.NewProjectile(null, (float)(i * 16 + 8), (float)(j * 16 + 8), 0f, 0.41f, fallType, 0, 0f, Main.myPlayer, 0f, 0f, 0f);
                }
                else if (Main.netMode == 2)
                {
                    int num24 = Projectile.NewProjectile(null, (float)(i * 16 + 8), (float)(j * 16 + 8), 0f, 0.41f, fallType, 0, 0f, Main.myPlayer, 0f, 0f, 0f);
                    Main.projectile[num24].netUpdate = true;
                }

                for (int nearbyCracked = 0; nearbyCracked < 8; nearbyCracked++)
                {
                    int nearbyCrackedX = i;
                    int nearbyCrackedY = j;
                    if (nearbyCracked == 0)
                    {
                        nearbyCrackedX--;
                    }
                    else if (nearbyCracked == 1)
                    {
                        nearbyCrackedX++;
                    }
                    else if (nearbyCracked == 2)
                    {
                        nearbyCrackedY--;
                    }
                    else if (nearbyCracked == 3)
                    {
                        nearbyCrackedY++;
                    }
                    else if (nearbyCracked == 4)
                    {
                        nearbyCrackedX--;
                        nearbyCrackedY--;
                    }
                    else if (nearbyCracked == 5)
                    {
                        nearbyCrackedX++;
                        nearbyCrackedY--;
                    }
                    else if (nearbyCracked == 6)
                    {
                        nearbyCrackedX--;
                        nearbyCrackedY++;
                    }
                    else if (nearbyCracked == 7)
                    {
                        nearbyCrackedX++;
                        nearbyCrackedY++;
                    }
                    Tile tile3 = Framing.GetTileSafely(nearbyCrackedX, nearbyCrackedY);
                    
                    if (tile3.TileType >= TileID.CrackedBlueDungeonBrick && tile3.TileType <= TileID.CrackedPinkDungeonBrick)
                    {
                        WorldGen.KillTile(nearbyCrackedX, nearbyCrackedY, false, false, true);
                    }
                }
            }
        }
	}
}