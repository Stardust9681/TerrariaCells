using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

using static TerrariaCells.Common.Utilities.JsonUtil;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.Systems
{
	public class NPCRoomSpawner : ModSystem
	{
		/// <summary> Add entries to this list during generation. </summary>
		internal static List<RoomMarker> RoomMarkers = new List<RoomMarker>();
		internal static IReadOnlyDictionary<string, RoomSpawnInfo> RoomInfo;
		public override void SetupContent()
		{
			SetupJson();
		}
		private void SetupJson()
		{
			Dictionary<string, RoomSpawnInfo> info = new Dictionary<string, RoomSpawnInfo>();
			const string path = "SpawnInfo.json";
			using (StreamReader stream = new StreamReader(Mod.GetFileStream(path)))
			{
				string json = stream.ReadToEnd();
				JObject Root = (JObject)JsonConvert.DeserializeObject(json); //Get json contents in whole

				JArray Biomes = Root.GetItem<JArray>("Biomes", new JArray()); //Get biomes from root
				foreach (JToken biome in Biomes)
				{
					int roomCount = 0;
					string biomeName = biome.GetItem<string>("BiomeName"); //name from biome

					JArray Rooms = biome.GetItem<JArray>("Rooms", new JArray()); //Gets rooms from biome
					foreach (JToken room in Rooms)
					{
						string roomName = room.GetItem<string>("Name"); //name from room

						if (!string.IsNullOrEmpty(roomName)) //In case no name provided
						{
							if(!roomName.StartsWith(biomeName))
								roomName = $"{biomeName}_{roomName}";
						}
						else
						{
							roomName = $"{biomeName}_roomNo{roomCount}";
							Mod.Logger.Warn($"JSON: No room name was provided for Biome:{biomeName} Room:#{roomCount}, one has been automatically created for you: {roomName}");
						}
						roomCount++;

						List<NPCSpawnInfo> spawnInfo = new List<NPCSpawnInfo>();

						JArray NPCSpawnInfoArray = room.GetItem<JArray>("SpawnInfo", new JArray());
						foreach (JToken npcSpawnInfo in NPCSpawnInfoArray)
						{
							string nameOrType = npcSpawnInfo.GetItem<string>("NameOrType", "0");
							ushort offsetX = npcSpawnInfo.GetItem<ushort>("OffsetX", 0);
							ushort offsetY = npcSpawnInfo.GetItem<ushort>("OffsetY", 0);
							spawnInfo.Add(new NPCSpawnInfo(nameOrType, offsetX, offsetY));
						}
						info.Add(roomName, new RoomSpawnInfo(roomName, spawnInfo.ToArray()));
					}
				}
			}

			RoomInfo = info;
		}
		public override void PostUpdateNPCs()
		{
			for (int i = 0; i < Main.maxPlayers; i++)
			{
				if (!Main.player[i].active) continue;
				foreach (RoomMarker marker in RoomMarkers) marker.Update(i);
			}
		}

		public override void SaveWorldData(TagCompound tag)
		{
			tag.Add($"{nameof(RoomMarkers)}_Count", RoomMarkers.Count);
			for(int i = 0; i < RoomMarkers.Count; i++)
			{
				RoomMarker marker = RoomMarkers[i];
				tag.Add($"Room{i}_XY", marker.Anchor);
				tag.Add($"Room{i}_Name", marker.RoomName);
			}
		}
		public override void LoadWorldData(TagCompound tag)
		{
			RoomMarkers = new List<RoomMarker>();
			int roomMarkersCount = tag.Get<int>($"{nameof(RoomMarkers)}_Count");
			if (roomMarkersCount <= 0)
			{
				Mod.Logger.Warn("No Room data found for world.");
				return;
			}
			for (int i = 0; i < roomMarkersCount; i++)
			{
				Point anchor = tag.Get<Point>($"Room{i}_XY");
				string name = tag.Get<string>($"Room{i}_Name");
				RoomMarkers.Add(new RoomMarker(anchor, name));
			}
		}
	}

	public class RoomMarker
	{
		/// <summary> 32 Tiles </summary>
		public const float LOAD_RANGE = 512;

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
		public Point16 Size()
		{
			Point16 result = new Point16(0);
			StructureHelper.Generator.GetDimensions(RoomName, ModContent.GetInstance<TerrariaCells>(), ref result);
			return result;
		}
		public readonly string RoomName;
		private bool didSpawns;

		//Maybe should be methods? Can't set readonly fields...
		public Point Center => new Point(Anchor.X + (Width / 2), Anchor.Y + (Height / 2));
		public int Left => Anchor.X;
		public int Top => Anchor.Y;
		public int Right => Anchor.X + Width;
		public int Bottom => Anchor.Y + Height;

		public short Width => Size().X;
		public short Height => Size().Y;

		public RoomSpawnInfo GetNPCSpawns() => NPCRoomSpawner.RoomInfo[RoomName];
		public bool TryGetNPCSpawns(out RoomSpawnInfo info) => NPCRoomSpawner.RoomInfo.TryGetValue(RoomName, out info);

		//Update the RoomMarker
		//General update tasks here
		internal void Update(int playerIndex)
		{
			if (InRange(Main.player[playerIndex].Center))
			{
				//Any other room load behaviours to add here?
				HandleSpawns();
			}
		}
		//Returns true if player is within a specified distance of all edges (see const: LOAD_RANGE)
		//Return false otherwise
		//Used for determining when to spawn room enemies
		private bool InRange(Vector2 pos)
		{
			return
				(Left - LOAD_RANGE) < pos.X
				&& pos.X < (Right + LOAD_RANGE)
				&& (Top - LOAD_RANGE) < pos.Y
				&& pos.Y < (Bottom + LOAD_RANGE);
		}
		//Called when Player is InRange(..) to handle enemy spawns. Runs once per room
		private void HandleSpawns()
		{
			if (didSpawns) return;
			foreach (NPCSpawnInfo info in GetNPCSpawns().NPCs)
			{
				NPC.NewNPC(Entity.GetSource_NaturalSpawn(), (Left + info.OffsetX)*16, (Top + info.OffsetY)*16, info.NPCType);
			}
			didSpawns = true;
		}
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
		public readonly ushort OffsetX;
		public readonly ushort OffsetY;
		public readonly string NameOrType;
		private int? npcType;
		/// <exception cref="ArgumentException"></exception>
		public int NPCType
		{
			get
			{
				if (npcType != null) //if npcType has already been established, use that
					return npcType.Value;
				if (int.TryParse(NameOrType, out int result1)) //Try to parse as number first, in case we use constant ID
					return (int)(npcType = result1);
				if (NPCID.Search.TryGetId(NameOrType, out int result2)) //Check for Vanilla NPC with name
					return (int)(npcType = result2);
				if (ModContent.GetInstance<TerrariaCells>().TryFind<ModNPC>(NameOrType, out ModNPC modNPC)) //Check for ModNPC with name
					return (int)(npcType = modNPC.Type);
				throw new ArgumentException($"NPC Type or Name: '{NameOrType}' was not found");
			}
		}
	}
}
