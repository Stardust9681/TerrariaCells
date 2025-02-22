using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Map;
using Terraria.ModLoader.IO;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.Systems
{
    public class WorldPylonSystem : ModSystem
	{
		private static Dictionary<Point16, bool>? _pylonDiscoveries;
		private static Dictionary<Point16, bool> _PylonDiscoveries
		{
			get
			{
				return _pylonDiscoveries ??= new Dictionary<Point16, bool>();
			}
		}
		public const int MAX_PYLON_RANGE = 7;

		public static bool PylonFound(Point16 pylonPos)
		{
			if (!WorldPylonSystem._PylonDiscoveries.TryGetValue(pylonPos, out bool found))
				return false;
			return found;
		}
		public static void MarkDiscovery(Point16 tilePos)
		{
			if (!PylonFound(tilePos))
			{
				ReloadPylons();
			}
			if (!WorldPylonSystem._PylonDiscoveries.ContainsKey(tilePos))
			{
				return; //Error
			}
			WorldPylonSystem._PylonDiscoveries[tilePos] = true;
		}
		//Clear discovered pylons, and populate it with all the pylons in the world (effectively resetting it to "default" state)
		public static void ResetPylons()
		{
			WorldPylonSystem._PylonDiscoveries.Clear();
			IEnumerable<Point16> pylonData = Main.PylonSystem.Pylons.Select(x => x.PositionInTiles);
			foreach (Point16 pos in pylonData)
			{
				WorldPylonSystem._PylonDiscoveries.Add(pos, false);
			}
		}
		//Updates discovered pylons, removes pylons that no longer exist, adds ones that do (reloading it with updated state)
		public static void ReloadPylons()
		{
			IEnumerable<Point16> keys = WorldPylonSystem._PylonDiscoveries.Keys;
			IEnumerable<Point16> pylonData = Main.PylonSystem.Pylons.Select(x => x.PositionInTiles);
			foreach (Point16 pos in pylonData)
			{
				if (!keys.Contains(pos))
					WorldPylonSystem._PylonDiscoveries.Add(pos, false);
			}
			keys = WorldPylonSystem._PylonDiscoveries.Keys;
			foreach (Point16 pos in keys)
			{
				if (!pylonData.Contains(pos))
					WorldPylonSystem._PylonDiscoveries.Remove(pos);
			}
		}
		public static Point16 NearestPylonToPlayer(Player player, out int approximateDistance)
		{
			Point16 playerPosition = Terraria.Utils.ToTileCoordinates16(player.Center);
			int appxDist = -1;
			Point16 pylonPos = Point16.NegativeOne;
			foreach (Point16 pos in Main.PylonSystem.Pylons.Select(x => x.PositionInTiles))
			{
				int compareDist = Math.Abs(pos.X - playerPosition.X) + Math.Abs(pos.Y - playerPosition.Y);
				if (compareDist < appxDist || appxDist == -1)
				{
					appxDist = compareDist;
					pylonPos = pos;
				}
			}
			approximateDistance = appxDist;
			return pylonPos;
		}

		public override void ClearWorld()
		{
			_pylonDiscoveries = null;
		}
	}
	public class PylonSystem : GlobalPylon
	{
		public override bool? ValidTeleportCheck_PreNPCCount(TeleportPylonInfo pylonInfo, ref int defaultNecessaryNPCCount)
		{
			defaultNecessaryNPCCount = 0;
			return true;
		}
		public override bool? ValidTeleportCheck_PreBiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return true;
		}
		public override bool? PreCanPlacePylon(int x, int y, int tileType, TeleportPylonType pylonType)
		{
			return true;
		}

		public override void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey)
		{
			if (!DevConfig.Instance.DoPylonDiscoveries)
			{
				base.PostValidTeleportCheck(destinationPylonInfo, nearbyPylonInfo, ref destinationPylonValid, ref validNearbyPylonFound, ref errorKey);
				return;
			}

			Point16 pos = WorldPylonSystem.NearestPylonToPlayer(Main.LocalPlayer, out int compareDist);

			if (compareDist <= WorldPylonSystem.MAX_PYLON_RANGE)
			{
				validNearbyPylonFound = true;
				WorldPylonSystem.MarkDiscovery(pos);
			}
			else
			{
				validNearbyPylonFound = false;
				errorKey = "Not close enough to interact with pylon.";
			}

			if (nearbyPylonInfo.TypeOfPylon != destinationPylonInfo.TypeOfPylon)
			{
				destinationPylonValid = false;
				errorKey = "Pylon Type Mismatch.";
			}

			if (!WorldPylonSystem.PylonFound(destinationPylonInfo.PositionInTiles))
			{
				destinationPylonValid = false;
				errorKey = "Pylon not yet discovered!";
			}
		}
		public override bool PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, ref TeleportPylonInfo pylonInfo, ref bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale)
		{
			if (!DevConfig.Instance.DoPylonDiscoveries)
			{
				return base.PreDrawMapIcon(ref context, ref mouseOverText, ref pylonInfo, ref isNearPylon, ref drawColor, ref deselectedScale, ref selectedScale);
			}

			isNearPylon = true;
			drawColor = Color.White;
			if (WorldPylonSystem.PylonFound(pylonInfo.PositionInTiles))
			{
				return true;
			}
			else
			{
				Player mapDrawPlayer = Main.LocalPlayer;
				Point16 tilePos = Terraria.Utils.ToTileCoordinates16(mapDrawPlayer.Center);
				int compareDist = Math.Abs(pylonInfo.PositionInTiles.X - tilePos.X) + Math.Abs(pylonInfo.PositionInTiles.Y - tilePos.Y);

				if (compareDist <= WorldPylonSystem.MAX_PYLON_RANGE)
				{
					WorldPylonSystem.MarkDiscovery(pylonInfo.PositionInTiles);
					return true;
				}
				return false;
			}
		}
		public override bool? ValidTeleportCheck_PreAnyDanger(TeleportPylonInfo pylonInfo)
		{
			return true;
		}
	}
	public class PylonWorldTile : GlobalTile
	{
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (type.Equals(Terraria.ID.TileID.TeleportationPylon))
				WorldPylonSystem.ReloadPylons();
			if (TileLoader.GetTile(type) is ModTile modTileType and not null && modTileType is ModPylon)
				WorldPylonSystem.ReloadPylons();
			base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
		}
		public override void ModifyLight(int i, int j, int type, ref float r, ref float g, ref float b)
		{
			if (!WorldPylonSystem.PylonFound(new Point16(i, j)))
				WorldPylonSystem.MarkDiscovery(new Point16(i, j));
			base.ModifyLight(i, j, type, ref r, ref g, ref b);
		}
	}
}
