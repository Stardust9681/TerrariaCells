using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

using static TerrariaCells.Common.Utilities.JsonUtil;

namespace TerrariaCells.Common.Systems
{
	public class NPCRoomSpawner : ModSystem
	{
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
