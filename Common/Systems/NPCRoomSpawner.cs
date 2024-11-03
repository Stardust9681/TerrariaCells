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
using System.IO;

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
				string json = stream.ReadToEnd(); //Is this lazy and bad? Maybe. Is someone's computer gonna cry out in pain when we do this in prod? Probably. Do I care right now? No.
				//Also, apparently dynamic type is black magic. I do quite like fucking around and finding out though.
				dynamic Root = JsonConvert.DeserializeObject(json); //Get json contents in whole
				dynamic Biomes = Root.Biomes; //Get biomes from root
				foreach (dynamic biome in Biomes)
				{
					int roomCount = 0;
					string biomeName = biome.BiomeName; //name from biome
					dynamic Rooms = biome.Rooms; //Gets rooms from biome
					foreach (dynamic room in Rooms)
					{
						string roomName = room.Name; //name from room

						if (!string.IsNullOrEmpty(roomName)) //In case no name provided
							roomName = $"{biomeName}_{room.Name}"; //Room names will be unique, hopefully
						else
						{
							Mod.Logger.Warn($"No Room Name was provided for Biome:{biomeName} Room:#{roomCount}, one has been automatically created for you.");
							roomName = $"{biomeName}_roomNo{roomCount}";
						}
						roomCount++;

						//Null checks galore here to have default values.
						dynamic NPCSpawnInfoArray = room.SpawnInfo??Array.Empty<NPCSpawnInfo>();
						if (NPCSpawnInfoArray != null) //In case no SpawnInfo provided
						{
							List<NPCSpawnInfo> spawnInfo = new List<NPCSpawnInfo>();
							foreach (dynamic npcSpawnInfo in NPCSpawnInfoArray)
							{
								string nameOrType = npcSpawnInfo.NameOrType??"0"; //Name or type is string or int, store as string, parse to int if possible
								ushort offsetX = npcSpawnInfo.OffsetX??"0";
								ushort offsetY = npcSpawnInfo.OffsetY??"0";
								spawnInfo.Add(new NPCSpawnInfo(nameOrType, offsetX, offsetY));
							}
							info.Add(roomName, new RoomSpawnInfo(roomName, spawnInfo.ToArray()));
						}
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
