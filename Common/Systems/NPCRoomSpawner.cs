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
			foreach (NPC npc in Main.ActiveNPCs) npc.active = false; //Disable all current NPCs

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
                string name = RoomMarker.GetInternalRoomName(levelName, roomName);
                Point position = pos.ToPoint();
                RoomMarker marker = new RoomMarker(position, name, width, height);
                RoomMarkers.Add(marker);
				var worldCoords = position.ToWorldCoordinates();
                mod.Logger.Info(
                    $"Added marker for {levelName} at world coordinates {worldCoords} tile coordinates {pos}"
                );
            }
		}

		/// <summary> Add entries to this list during biome generation. </summary>
		public static List<RoomMarker> RoomMarkers = new List<RoomMarker>();
		/// <summary>
		/// <para>Keys are room name with biome ( formatted as <c>Biome_room_name</c> ).</para>
		/// <para>Values are SpawnInfo for corresponding room.</para>
		/// </summary>
		internal static Dictionary<string, RoomSpawnInfo> RoomInfo = [];

		// deferred call to access worldgen data after loaded
		// called @ TerrariaCells.Common.Systems.BasicWorldGen.LoadWorldData
		public new void OnWorldLoad()
		{
			RoomInfo.Clear();

			SpawnInfoDeterminer determiner = ModContent.GetInstance<SpawnInfoDeterminer>();

			BasicWorldGenData worldGenData = StaticFileAccess.Instance.WorldGenData;

			foreach (Level level in BasicWorldGenData.LevelData)
			{
				LevelStructure structure = level.GetGeneratedStructure(worldGenData);

				NPCSpawnInfo[] spawnInfo = structure.SpawnInfo
					.Select(x => new NPCSpawnInfo(x.SetID, (ushort)x.X, (ushort)x.Y))
					.ToArray();

				// if (spawnInfo.Where(x => x.OffsetX == 0 & x.OffsetY == 0).Count() != 0)
				// {
				// 	throw new Exception(structure.Name);
				// }

				RoomSpawnInfo roomInfo = new(structure.Name, spawnInfo);
				RoomInfo.Add($"{level.Name}_{structure.Name}", roomInfo);
				Mod.Logger.Info($"Inserted {spawnInfo.Length} spawns for {level.Name}_{structure.Name}");
			}
		}

		public override void PostUpdateNPCs()
		{
			if (Configs.DevConfig.Instance.DisableSpawns) return;

			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (!Main.player[i].active) continue;
				foreach (RoomMarker marker in RoomMarkers) marker.a_Update(i);
			}
		}
	}

	public class RoomMarker
	{
		public static string GetInternalRoomName(string biome, string roomName) => $"{biome}_{roomName}";
		/// <summary> 30 Tiles </summary>
		public const float LOAD_RANGE = 480;

		/// <param name="position">Position in TILE COORDINATES (xy/16)</param>
		/// <param name="size">Size in TILES (xy/16)</param>
		/// <param name="name">Room name for dictionary access</param>
		public RoomMarker(Point position, string name)
		{
			Anchor = position;
			RoomName = name;
		}
		/// <param name="i">Position X in TILE COORDINATES (x/16)</param>
		/// <param name="j">Position Y in TILE COORDINATES (y/16)</param>
		/// <param name="size">Size in TILES (xy/16)</param>
		/// <param name="name">Room name for dictionary access</param>
		public RoomMarker(int i, int j, string name)
		{
			Anchor = new Point(i, j);
			RoomName = name;
		}

		public readonly Point Anchor; //Considering making this Point16
		public readonly string RoomName;
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
				ModContent.GetInstance<TerrariaCells>().Logger.Error($"Room: {RoomName} was not found/did not exist");
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

		public RoomSpawnInfo GetNPCSpawns() => NPCRoomSpawner.RoomInfo[RoomName];
		public bool TryGetNPCSpawns(out RoomSpawnInfo info) => NPCRoomSpawner.RoomInfo.TryGetValue(RoomName, out info);

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
			foreach (NPCSpawnInfo info in GetNPCSpawns().NPCs)
			{
				int whoAmI = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (Left + info.OffsetX)*16, (Top + info.OffsetY)*16, info.NPCType);
                NPCRespawnHandler.HandleSpecialSpawn(Main.npc[whoAmI], (Left + info.OffsetX), (Top + info.OffsetY));
            }
			didSpawns = true;
		}

		#region Alpha Testing Hax
		public RoomMarker(Point position, string roomName, ushort tileWidth, ushort tileHeight) : this(position, roomName)
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
			if (didSpawns) return;
			foreach (NPCSpawnInfo info in GetNPCSpawns().NPCs)
			{
				int whoAmI = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (Left + info.OffsetX) * 16 + 8, (Top + a_height - info.OffsetY) * 16 + 8, info.NPCType);
                NPCRespawnHandler.HandleSpecialSpawn(Main.npc[whoAmI], (Left + info.OffsetX), (Top + a_height - info.OffsetY));
			}
			didSpawns = true;
		}
		#endregion
	}

	public struct RoomSpawnInfo
	{
		public RoomSpawnInfo(string name, NPCSpawnInfo[] info)
		{
			RoomName = name;
			NPCs = info;
		}
		public readonly string RoomName;
		public readonly NPCSpawnInfo[] NPCs;
	}

	public struct NPCSpawnInfo
	{
		public NPCSpawnInfo(string name, ushort x, ushort y)
		{
			NameOrType = name;
			OffsetX = x;
			OffsetY = y;
		}
		public NPCSpawnInfo(int id, ushort x, ushort y)
		{
			npcType = id;
			OffsetX = x;
			OffsetY = y;
		}
		public readonly ushort OffsetX;
		public readonly ushort OffsetY;
		public readonly string NameOrType;
		private int? npcType;
        /// <exception cref="ArgumentException"/>
        public int NPCType
        {
            get
            {
                return GetNPCType();
            }
        }

        private int GetNPCType()
        {
            if (npcType != null) //if npcType has already been established, use that
                return npcType.Value;
            if (int.TryParse(NameOrType, out int result1)) //Try to parse as number first, in case we use constant ID
                return (int)(npcType = result1);
            if (NPCID.Search.TryGetId(NameOrType, out int result2)) //Check for Vanilla NPC with name
                return (int)(npcType = result2);
            if (ModContent.GetInstance<TerrariaCells>().TryFind<ModNPC>(NameOrType, out ModNPC modNPC)) //Check for ModNPC with name
                return (int)(npcType = modNPC.Type);
            ModContent.GetInstance<TerrariaCells>().Logger.Warn($"TerraCells NPC Spawning Error: NPC Type or Name: '{NameOrType}' was not found.");
            return NPCID.FairyCritterPink;
        }
    }
}
