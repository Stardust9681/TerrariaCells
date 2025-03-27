using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.Map;
using TerrariaCells.Common.Configs;
using TerrariaCells.Content.Packets;
using TerrariaCells.Common.Utilities;
using Terraria.ID;
using Terraria.Chat;
using Terraria.Localization;
using TerrariaCells.Content.TileEntities;
using Terraria.GameContent.Tile_Entities;
using Terraria.ModLoader.Default;

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
		public static Point16 NearestPylonToPlayer(Player player, out int appxDistanceInTiles)
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
			appxDistanceInTiles = appxDist;
			return pylonPos;
		}

		public override void ClearWorld()
		{
			_pylonDiscoveries = null;
		}
		public static Dictionary<Point16, bool> GetDiscoveredPylons()
		{
			return _PylonDiscoveries;
		}
	}
	public class PylonSystem : GlobalPylon
	{
        public override void SetStaticDefaults()
        {
			On_TeleportPylonsSystem.HandleTeleportRequest += HandleTeleportRequest;
        }
		public override void Unload()
        {
            On_TeleportPylonsSystem.HandleTeleportRequest -= HandleTeleportRequest;
        }
		// Multiplayer only
		// We have to use a separate handle for multiplayer because our other teleport 
		// checks have no way of accessing the player that is trying to teleport
		/// <summary>
		/// 
		/// </summary>
		private void HandleTeleportRequest(On_TeleportPylonsSystem.orig_HandleTeleportRequest orig, TeleportPylonsSystem system, TeleportPylonInfo destinationPylonInfo, int playerIndex)
		{
			if (!DevConfig.Instance.DoPylonDiscoveries)
			{
				orig.Invoke(system, destinationPylonInfo, playerIndex);
				return;
			}
			Player player = Main.player[playerIndex];
			// ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(
			// 	"Unmodified HandleTeleport Pylon Position" + info.PositionInTiles.ToWorldCoordinates().ToString()), Color.SkyBlue);
			Point16 nearbyPylonPos = WorldPylonSystem.NearestPylonToPlayer(player, out int compareDist);
			// Check distance of player to nearby pylon
			if (compareDist > WorldPylonSystem.MAX_PYLON_RANGE)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Not close enough to interact with pylon"), Color.Yellow);
				return;
			}
			TileEntity nearbyPylon = TileEntity.ByPosition[nearbyPylonPos];
			// Is nearby pylon modded
			if (nearbyPylon is TEModdedPylon modEntity)
			{
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Nearby pylon is modded"), Color.Red);
				modEntity.TryGetModPylon(out ModPylon modPylon);
				if (modPylon.PylonType == destinationPylonInfo.TypeOfPylon)
				{
					orig.Invoke(system, destinationPylonInfo, playerIndex);
					return;
				}
				else
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Pylon type mismatch"), Color.Yellow);
					return;
				}
			}
			// Is pylon vanilla
			foreach (TeleportPylonInfo info in Main.PylonSystem.Pylons)
			{
				if (info.PositionInTiles == nearbyPylonPos)
				{
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Nearby pylon vanilla type : " + info.TypeOfPylon), Color.Red);
					if (info.TypeOfPylon == destinationPylonInfo.TypeOfPylon)
					{
						orig.Invoke(system, destinationPylonInfo, playerIndex);
						return;
					}
					ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("Pylon type mismatch"), Color.Yellow);
					break;
				}
			}
			return;
		}
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
		/// <summary>
		/// Don't override this, nearbyPylonInfo does not contain the right data, 
		/// and it's impossible to find the correct player that's teleporting on the server, modify HandleTeleportRequest instead
		/// </summary>
		/// <param name="destinationPylonInfo"></param>
		/// <param name="nearbyPylonInfo">For some godforsaken reason, this.PositionInTiles is always (0, 0)</param>
		/// <param name="destinationPylonValid"></param>
		/// <param name="validNearbyPylonFound"></param>
		/// <param name="errorKey"></param>
		public override void PostValidTeleportCheck(TeleportPylonInfo destinationPylonInfo, TeleportPylonInfo nearbyPylonInfo, ref bool destinationPylonValid, ref bool validNearbyPylonFound, ref string errorKey)
		{
			destinationPylonValid = true;
			validNearbyPylonFound = true;
			base.PostValidTeleportCheck(
				destinationPylonInfo, 
				nearbyPylonInfo, 
				ref destinationPylonValid,
				ref validNearbyPylonFound,
				ref errorKey);
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
	}
	public class PylonPlayer : ModPlayer
	{
        public override void OnEnterWorld()
        {
			// Server needs to update discovered pylons on client
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				ModPacket p = Mod.GetPacket();
				p.Write((byte)TCPacketType.PylonPacket);
				p.Write((byte)PylonPacketType.ServerPlayerEnter);
				p.Write(Main.LocalPlayer.whoAmI);
				p.Send(255, -1); // 255 is the server
			}
		}
        public override void PostUpdate()
		{
			Point16 nearestPylon = WorldPylonSystem.NearestPylonToPlayer(Player, out int appxDist);
			if (appxDist > WorldPylonSystem.MAX_PYLON_RANGE) return;
			if (WorldPylonSystem.PylonFound(nearestPylon)) return;
			// Do the normal thing if there's no server
			if (Main.netMode == NetmodeID.SinglePlayer) 
			{
				WorldPylonSystem.MarkDiscovery(nearestPylon);
				return;
			}
			// Otherwise check if we're the server, if not we send a packet to tell the server we've discovered a pylon
			if (Main.netMode == NetmodeID.Server) return;
			ModPacket p = Mod.GetPacket();
			p.Write((byte)TCPacketType.PylonPacket);
			p.Write((byte)PylonPacketType.PylonDiscovery);
			p.Write(nearestPylon.X);
			p.Write(nearestPylon.Y);
			p.Send();
		}
	}
}
