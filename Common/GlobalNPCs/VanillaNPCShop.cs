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
		private static int[] Armors; //Merchant
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
			//Will be replaced with chest loot table entry like the other buyable items
			Armors = new int[]{
                ItemID.NinjaHood,
				ItemID.NinjaShirt,
				ItemID.NinjaPants,
				ItemID.JungleHat,
				ItemID.JungleShirt,
				ItemID.JunglePants,
				ItemID.NecroHelmet,
				ItemID.NecroBreastplate,
				ItemID.NecroGreaves,
				ItemID.MoltenHelmet,
				ItemID.MoltenBreastplate,
				ItemID.MoltenGreaves
			};
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
				const int MinCount = 3;
				List<int> items = new List<int>();
				for(int i = 0; i < Weapons.Length; i++)
				{
					if (items.Count > MinCount && !Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(Main.rand.Next(Weapons.Where(x => !items.Contains(x)).ToArray()));
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
				for (int i = 0; i < Accessories.Length; i++)
				{
					if (items.Count > MinCount && !Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(Main.rand.Next(Accessories.Where(x => !items.Contains(x)).ToArray()));
				}
				selectedItems = items.ToArray();
			}
			if (npc.type == NPCID.Merchant)
			{
				int[] SkillsAndArmors = new int[Skills.Length + Armors.Length];
				for (int i = 0; i < Skills.Length; i++)
				{
					SkillsAndArmors[i] = Skills[i];
				}
				for (int i = 0; i < Armors.Length; i++)
				{
					SkillsAndArmors[Skills.Length + i] = Armors[i];
				}

                if (playtesting)
                {
                    selectedItems = SkillsAndArmors;
                    return;
                }
                const int MinCount = 2;
                List<int> items = new List<int>();
				for (int i = 0; i < SkillsAndArmors.Length; i++)
				{
					if (items.Count > MinCount && !Main.rand.NextBool(items.Count - MinCount)) continue;

					items.Add(Main.rand.Next(SkillsAndArmors.Where(x => !items.Contains(x)).ToArray()));
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
