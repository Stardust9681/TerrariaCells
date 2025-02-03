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
			On_Player.TileInteractionsUse += On_Player_TileInteractionsUse;
			On_Player.InInteractionRange += On_Player_InInteractionRange;
        }

		public override void Unload()
		{
			On_Projectile.ExplodeTiles -= On_Projectile_ExplodeTiles;
			On_Player.TileInteractionsUse -= On_Player_TileInteractionsUse;
			On_Player.InInteractionRange -= On_Player_InInteractionRange;
		}

		private bool On_Player_InInteractionRange(On_Player.orig_InInteractionRange orig, Player self, int interactX, int interactY, Terraria.DataStructures.TileReachCheckSettings settings)
		{
			if (DevConfig.Instance.BuilderMode)
			{
				return orig.Invoke(self, interactX, interactY, settings);
			}

			Tile tile = Framing.GetTileSafely(interactX, interactY);
			if (ValidTiles.Contains(tile.TileType))
			{
				if (tile.TileType == TileID.TeleportationPylon)
				{
					settings.OverrideXReach = Systems.WorldPylonSystem.MAX_PYLON_RANGE;
					settings.OverrideYReach = Systems.WorldPylonSystem.MAX_PYLON_RANGE;
				}
				return orig.Invoke(self, interactX, interactY, settings);
			}
			return false;
		}

		private readonly int[] ValidTiles = new int[] {
			TileID.TeleportationPylon,
			TileID.GemLocks,
			TileID.Containers,
			TileID.Containers2,
			TileID.Heart,
			TileID.ManaCrystal,
			ModContent.TileType<Content.Tiles.LevelExitPylon.ForestExitPylon>(),
			ModContent.TileType<Content.Tiles.LevelExitPylon.DesertExitPylon>(),
			ModContent.TileType<Content.Tiles.LevelExitPylon.HiveExitPylon>(),
			ModContent.TileType<Content.Tiles.LevelExitPylon.SnowExitPylon>(),
		};
		private void On_Player_TileInteractionsUse(On_Player.orig_TileInteractionsUse orig, Player self, int myX, int myY)
		{
			if (DevConfig.Instance.BuilderMode)
			{
				orig.Invoke(self, myX, myY);
				return;
			}

			Tile tile = Framing.GetTileSafely(myX, myY);
			if (ValidTiles.Contains(tile.TileType))
			{
				orig.Invoke(self, myX, myY);
			}
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
	}
}
