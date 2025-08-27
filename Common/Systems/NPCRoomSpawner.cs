using System;
using System.Linq;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaCells.Common.Systems
{
	public class NPCRoomSpawner : ModSystem
	{
		public static void ResetSpawns()
		{
            Mod mod = ModLoader.GetMod("TerrariaCells");

			NPCRespawnHandler.RespawnMarkers?.Clear();
			if (RoomMarkers is null) RoomMarkers = new List<RoomMarker>();
			else RoomMarkers.Clear();
            foreach (NPC npc in Main.ActiveNPCs)
            {
                npc.netSkip = -1;
                npc.active = false; //Disable all current NPCs
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
                }
            }

            BasicWorldGenData data = ModContent
                .GetInstance<BasicWorldGeneration>()
                .BasicWorldGenData;

            if (data is null)
            {
                mod.Logger.Error("Could not get BasicWorldGenData");
                return;
            }

            if (data.LevelPositions.Count == 0)
            {
                mod.Logger.Warn("No levels found!");
            }

            foreach (var (levelName, variation) in data.LevelVariations)
            {
                Point16 pos = data.LevelPositions[levelName];
                LevelStructure levelStructure = BasicWorldGeneration
                    .StaticLevelData.Find(x => x.Name == levelName)
                    .Structures[variation];
                string roomName = levelStructure.Name;
                ushort width = (ushort)
                    StructureHelper.API.Generator.GetStructureData(levelStructure.Path, mod).width;
                ushort height = (ushort)
                    StructureHelper.API.Generator.GetStructureData(levelStructure.Path, mod).height;
                Point position = pos.ToPoint();
                RoomMarker marker = new RoomMarker(position, levelStructure, width, height);
                RoomMarkers.Add(marker);
				var worldCoords = position.ToWorldCoordinates();
                mod.Logger.Info(
                    $"Added marker for {levelName} at world coordinates {worldCoords} tile coordinates {pos}"
                );
            }
		}

		public static void ResetSpawnsForStructure(LevelStructure levelStructure, Point levelPosition)
		{
            Mod mod = ModLoader.GetMod(nameof(TerrariaCells));

			BasicWorldGenData data = ModContent
                .GetInstance<BasicWorldGeneration>()
                .BasicWorldGenData;

            if (data is null)
			{
				mod.Logger.Error("Could not get BasicWorldGenData");
				return;
			}

			foreach (StructureSpawnInfo spawnInfo in levelStructure.SpawnInfo)
			{
				spawnInfo.SpawnedNPC.active = false; //Disable all current NPCs in structure
			}

			RoomMarkers.RemoveAll(x => x.Structure == levelStructure);

			ushort width = (ushort)
				StructureHelper.API.Generator.GetStructureData(levelStructure.Path, mod).width;
			ushort height = (ushort)
				StructureHelper.API.Generator.GetStructureData(levelStructure.Path, mod).height;
			RoomMarker marker = new RoomMarker(levelPosition, levelStructure, width, height);
			RoomMarkers.Add(marker);
			var worldCoords = levelPosition.ToWorldCoordinates();
			mod.Logger.Info(
				$"Added marker for {levelStructure.Path} at world coordinates {worldCoords} tile coordinates {levelPosition}"
			);
		}

		/// <summary> Add entries to this list during biome generation. </summary>
		public static List<RoomMarker> RoomMarkers = new List<RoomMarker>();
		/// <summary>
		/// <para>Keys are room name with biome ( formatted as <c>Biome_room_name</c> ).</para>
		/// <para>Values are SpawnInfo for corresponding room.</para>
		/// </summary>
		// internal static Dictionary<string, RoomSpawnInfo> RoomInfo = [];

		// deferred call to access worldgen data after loaded
		// called @ TerrariaCells.Common.Systems.BasicWorldGen.LoadWorldData
		public new void OnWorldLoad()
		{
			// RoomInfo.Clear();

			SpawnInfoDeterminer determiner = ModContent.GetInstance<SpawnInfoDeterminer>();

			BasicWorldGenData worldGenData = StaticFileAccess.Instance.WorldGenData;

			foreach (Level level in BasicWorldGenData.LevelData)
			{
				LevelStructure structure = level.GetGeneratedStructure(worldGenData);

				if (structure.SpawnInfo == null)
				{
					Main.NewText($"SpawnInfo for {structure.Name} failed to load. Check client.log for more info");
					continue;
				}

				// if (spawnInfo.Where(x => x.OffsetX == 0 & x.OffsetY == 0).Count() != 0)
				// {
				// 	throw new Exception(structure.Name);
				// }

				// RoomSpawnInfo roomInfo = new(structure.Name, structure.SpawnInfo);
				// RoomInfo.Add($"{level.Name}_{structure.Name}", roomInfo);
				// Mod.Logger.Info($"Inserted {spawnInfo.Length} spawns for {level.Name}_{structure.Name}");
			}
		}

		public override void PostUpdateNPCs()
		{
			if (Configs.DevConfig.Instance.DisableSpawns) return;
            if(Main.netMode == NetmodeID.MultiplayerClient) return;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (!Main.player[i].active) continue;
                if(Main.player[i].DeadOrGhost) continue;
				foreach (RoomMarker marker in RoomMarkers) marker.a_Update(i);
			}
		}
	}

	public class RoomMarker
	{
		/// <summary> 30 Tiles </summary>
		public const float LOAD_RANGE = 480;

		/// <param name="position">Position in TILE COORDINATES (xy/16)</param>
		/// <param name="size">Size in TILES (xy/16)</param>
		/// <param name="name">Room name for dictionary access</param>
		public RoomMarker(Point position, LevelStructure structure)
		{
			Anchor = position;
			Structure = structure;
		}
		/// <param name="i">Position X in TILE COORDINATES (x/16)</param>
		/// <param name="j">Position Y in TILE COORDINATES (y/16)</param>
		/// <param name="size">Size in TILES (xy/16)</param>
		/// <param name="name">Room name for dictionary access</param>
		public RoomMarker(int i, int j, LevelStructure structure)
		{
			Anchor = new Point(i, j);
			Structure = structure;
		}

		public readonly Point Anchor; //Considering making this Point16
		public readonly LevelStructure Structure;
		private bool didSpawns = false;
		public bool DidSpawns
		{
			get => didSpawns;
			internal init => didSpawns = value;
		}

		public Point16 Size()
		{
			Point16 result = new Point16(0);
            try
            {
				// StructureHelper.Generator.GetDimensions(RoomName, ModContent.GetInstance<TerrariaCells>(), ref result);
			}
			catch (Exception)
            {
				// ModContent.GetInstance<TerrariaCells>().Logger.Error($"Room: {LevelStructure.} was not found/did not exist");
			}
			return result;
		}

		public short Width => Size().X;
		public short Height => Size().Y;

		public Point Center => new Point(Anchor.X + (Width / 2), Anchor.Y + (Height / 2));
		public int Left => Anchor.X;
		public int Top => Anchor.Y;
		public int Right => Anchor.X + Width;
		public int Bottom => Anchor.Y + Height;

		// public RoomSpawnInfo GetNPCSpawns() => NPCRoomSpawner.RoomInfo[RoomName];
		// public bool TryGetNPCSpawns(out RoomSpawnInfo info) => NPCRoomSpawner.RoomInfo.TryGetValue(RoomName, out info);

		//General update tasks here
		internal void Update(int playerIndex)
		{
			if (InRange(Main.player[playerIndex].Center))
			{
				//Any other room load behaviours to add here?
				HandleSpawns();
			}
		}
		//Returns true if player is within a specified distance of all edges
		//Used for determining when to spawn room enemies
		private bool InRange(Vector2 pos)
		{
			return
				((Left*16) - LOAD_RANGE) < pos.X
				&& pos.X < ((Right*16) + LOAD_RANGE)
				&& ((Top*16) - LOAD_RANGE) < pos.Y
				&& pos.Y < ((Bottom*16) + LOAD_RANGE);
		}
		//Called when Player is InRange(..) to handle enemy spawns. Runs once per room
		private void HandleSpawns()
		{
			if (didSpawns) return;
			// foreach (NPCSpawnInfo info in GetNPCSpawns().NPCs)
			// {
			// 	int whoAmI = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (Left + info.OffsetX)*16, (Top + info.OffsetY)*16, info.NPCType);
            //     NPCRespawnHandler.HandleSpecialSpawn(Main.npc[whoAmI], Left + info.OffsetX, Top + info.OffsetY);
            // }
			didSpawns = true;
		}


        //You know what they say...
        //Temporary solutions make for the most permanent solutions.
        //Doesn't really matter, just an amusing perspective.
		#region Alpha Testing Hax
		public RoomMarker(Point position, LevelStructure structure, ushort tileWidth, ushort tileHeight) : this(position, structure)
		{
			// ModContent.GetInstance<TerrariaCells>().Logger.Info(roomName);
			a_width = tileWidth;
			a_height = tileHeight;
		}

		/// <summary>
		/// Width in tiles
		/// </summary>
		public ushort a_width = 0;
		/// <summary>
		/// Height in tiles
		/// </summary>
		public ushort a_height = 0;

		internal void a_Update(int playerIndex)
		{
            if (didSpawns)
                return;

            if (a_InRange(Main.player[playerIndex].Center))
			{
				//Any other room load behaviours to add here?
				a_HandleSpawns();
			}
		}

		private bool a_InRange(Vector2 pos)
		{
			return
				((Left * 16) - LOAD_RANGE) < pos.X
				&& pos.X < (((Left + a_width) * 16) + LOAD_RANGE)
				&& ((Top * 16) - LOAD_RANGE) < pos.Y
				&& pos.Y < (((Top + a_height) * 16) + LOAD_RANGE);
		}

		private void a_HandleSpawns()
		{
			foreach (var info in Structure.SpawnInfo)
			{
				int whoAmI = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (Left + info.X) * 16 + 8, (Top + a_height - info.Y) * 16 + 8, info.SetID);
                NPCRespawnHandler.HandleSpecialSpawn(info.SpawnedNPC, Left + info.X, Top + info.Y);
                info.SpawnedNPC = Main.npc[whoAmI];

                Main.npc[whoAmI].EncourageDespawn(-1);
                Main.npc[whoAmI].CheckActive();

                if (Main.npc[whoAmI].active && Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, whoAmI);
                }
            }
			didSpawns = true;
			ModContent.GetInstance<TerrariaCells>().Logger.Info($"Spawned entites for {Structure.SpawnInfoPath}");
		}
		#endregion
	}
}
