using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using Terraria.DataStructures;
//using static TerrariaCells.Common.Utilities.JsonUtil;

namespace TerrariaCells.Common.GlobalNPCs
{
    class VanillaNPCShop : GlobalNPC
    {
		public override bool InstancePerEntity => true;

		private static int[] Weapons; //Arms Dealer
		private static int[] Accessories; //Goblin Tinkerer
		private static int[] Skills; //Wizard?
		public override void Load()
		{
			const string path = "chest loot tables.json";
			using (StreamReader stream = new StreamReader(Mod.GetFileStream(path)))
			{
				string json = stream.ReadToEnd();
				JObject Root = (JObject)JsonConvert.DeserializeObject(json); //Get json contents in whole

				Weapons = Root.GetItem<int[]>("1");
				Accessories = Root.GetItem<int[]>("20");
				Skills = Root.GetItem<int[]>("19");
			}
		}

		private int[] selectedItems;
		public override void SetDefaults(NPC npc)
		{
			bool playtesting = Configs.DevConfig.Instance.PlaytesterShops;
			if (npc.type == NPCID.ArmsDealer)
			{
				if (playtesting)
				{
					selectedItems = Weapons;
					return;
				}
				const int MinCount = 4;
				List<int> items = new List<int>();
				foreach (int itemType in Weapons)
				{
					if (items.Count > MinCount && !Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(itemType);
				}
				selectedItems = items.ToArray();
			}
			if (npc.type == NPCID.GoblinTinkerer)
			{
				if (playtesting)
				{
					selectedItems = Accessories;
					return;
				}
				const int MinCount = 2;
				List<int> items = new List<int>();
				foreach (int itemType in Accessories)
				{
					if (items.Count > MinCount && Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(itemType);
				}
				selectedItems = items.ToArray();
			}
		}
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			if (npc.type is NPCID.ArmsDealer or NPCID.GoblinTinkerer)
			{
				for (int i = 0; i < items.Length; i++)
				{
					if (i < selectedItems.Length)
						items[i] = new Item(selectedItems[i]) { shopCustomPrice = 10 };
					else
						items[i] = null;
				}
			}
		}
		public override void ModifyShop(NPCShop shop)
        {
			if (shop.NpcType is NPCID.ArmsDealer or NPCID.GoblinTinkerer)
            {
                // Remove all existing entries from the shop
                foreach (var entry in shop.Entries)
                {
                    entry.Disable();
                }
            }
        }
    }
}
