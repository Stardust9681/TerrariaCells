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
using Terraria.GameContent;
//using static TerrariaCells.Common.Utilities.JsonUtil;

namespace TerrariaCells.Common.GlobalNPCs
{
    class VanillaNPCShop : GlobalNPC
    {
		public override bool InstancePerEntity => true;

		private static int[] Weapons; //Arms Dealer
		private static int[] Accessories; //Goblin Tinkerer
		private static int[] Skills; //Merchant (Wizard? Plz)
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
				const int MinCount = 3;
				List<int> items = new List<int>();
				foreach (int itemType in Accessories)
				{
					if (items.Count > MinCount && Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(itemType);
				}
				selectedItems = items.ToArray();
			}
			if (npc.type == NPCID.Merchant)
			{
				if (playtesting)
				{
					selectedItems = Skills;
					return;
				}
				const int MinCount = 1;
				List<int> items = new List<int>();
				foreach (int itemType in Skills)
				{
					if (items.Count > MinCount && Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(itemType);
				}
				selectedItems = items.ToArray();
			}
		}
		public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
		{
			int shopCustomPrice = 10;
			switch (npc.type) {
				case NPCID.ArmsDealer:
					shopCustomPrice = 2000;
					break;
				case NPCID.GoblinTinkerer:
					shopCustomPrice = 10000;
					break;
				case NPCID.Merchant:
					shopCustomPrice = 30000;
					break;
			}
			if (npc.type is NPCID.ArmsDealer or NPCID.GoblinTinkerer or NPCID.Merchant)
			{
				for (int i = 0; i < items.Length; i++)
				{
					if (i < selectedItems.Length)
						items[i] = new Item(selectedItems[i]) { shopCustomPrice = shopCustomPrice };
					else
						items[i] = null;
				}
			}
		}
		public override void ModifyShop(NPCShop shop)
        {
			if (shop.NpcType is NPCID.ArmsDealer or NPCID.GoblinTinkerer or NPCID.Merchant)
            {
                // Remove all existing entries from the shop
                foreach (var entry in shop.Entries)
                {
                    entry.Disable();
                }
            }
        }
    }

	class NPCShopDetours : ModSystem {
        public override void Load()
        {
			On_ShopHelper.GetShoppingSettings += GetShoppingSettings;
        }

        public override void Unload()
        {
			On_ShopHelper.GetShoppingSettings -= GetShoppingSettings;
        }

        private ShoppingSettings GetShoppingSettings(On_ShopHelper.orig_GetShoppingSettings orig, ShopHelper self, Player player, NPC npc)
        {
            ShoppingSettings shoppingSettings = orig(self, player, npc);
			shoppingSettings.PriceAdjustment = 1.0;
            return shoppingSettings;
        }
    }
}
