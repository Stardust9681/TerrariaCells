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
			const string PATH = "chest loot tables.json";
			using (StreamReader stream = new StreamReader(Mod.GetFileStream(PATH)))
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

        private ItemDef[] new_SelectedItems = Array.Empty<ItemDef>();
        public bool nurse_HasHealed = false;
		public override void SetDefaults(NPC npc)
		{
            UpdateTeleport(npc, 1, "Forest");
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
                    if (i < new_SelectedItems.Length)
                    {
                        ItemDef def = new_SelectedItems[i];
                        items[i] = new Item(def.Type);
                        if (items[i].TryGetGlobalItem<GlobalItems.TierSystemGlobalItem>(out var tierItem))
                            tierItem.itemLevel = def.Level;
                        if (items[i].TryGetGlobalItem<GlobalItems.FunkyModifierItemModifier>(out var modsItem))
                            modsItem.modifiers = def.Mods;
                    }
                    else
                    {
                        items[i] = null;
                    }
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

        public static void UpdateTeleport(int level, string? levelName = null)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                npc.GetGlobalNPC<VanillaNPCShop>().UpdateTeleport(npc, level, levelName);
            }
        }
        public void UpdateTeleport(NPC npc, int level, string? levelName = null)
        {
            switch (npc.type)
            {
                case NPCID.ArmsDealer:
                    UpdateNPCShop(npc, Weapons, level, 3);
                    break;
                case NPCID.Merchant:
                    UpdateNPCShop(npc, (int[])[.. Armors, .. Skills], level, 2);
                    break;
                case NPCID.GoblinTinkerer:
                    UpdateNPCShop(npc, Accessories, level, 2);
                    break;

                case NPCID.Nurse:
                    nurse_HasHealed = false;
                    break;
            }
        }
        private void UpdateNPCShop(NPC npc, int[] itemTypes, int itemLevel, int min = 1, int max = 40)
        {
            if (Configs.DevConfig.Instance.PlaytesterShops || min > itemTypes.Length)
            {
                new_SelectedItems = itemTypes.Select(x => new ItemDef(x, itemLevel)).ToArray();
                return;
            }
            List<int> items = new List<int>();
            for (int i = 0; i < itemTypes.Length; i++)
            {
                if (items.Count > min && !Main.rand.NextBool(items.Count - min)) continue;

                items.Add(Main.rand.Next(itemTypes.Where(x => !items.Contains(x)).ToArray()));

                if (items.Count > max) break;
            }
            new_SelectedItems = items.Select(x => new ItemDef(x, itemLevel)).ToArray();
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

    struct ItemDef
    {
        public int Type;
        public int Level;
        public GlobalItems.FunkyModifier[] Mods;

        public ItemDef(int type, int level, params GlobalItems.FunkyModifier[] mods)
        {
            Type = type;
            Level = level;
            Mods = mods;
        }
        public ItemDef(int type, int level)
        {
            Type = type;
            Level = level;

            Item temp = new Item(Type);
            //temp.GetGlobalItem<GlobalItems.TierSystemGlobalItem>().itemLevel = Level;
            //GlobalItems.FunkyModifierItemModifier.Reforge(temp, Level);
            //Mods = temp.GetGlobalItem<GlobalItems.FunkyModifierItemModifier>().modifiers;
            if (GlobalItems.FunkyModifierItemModifier.CanReceiveMods(Type))
                Mods = GlobalItems.FunkyModifierItemModifier.PickMods(Type, Level);
            else
                Mods = Array.Empty<GlobalItems.FunkyModifier>();
        }
        public ItemDef(int type)
        {
            Type = type;
            Level = ModContent.GetInstance<Systems.TeleportTracker>().level;

            //temp.GetGlobalItem<GlobalItems.TierSystemGlobalItem>().itemLevel = Level;
            //GlobalItems.FunkyModifierItemModifier.Reforge(temp, Level);
            //Mods = temp.GetGlobalItem<GlobalItems.FunkyModifierItemModifier>().modifiers;
            if (GlobalItems.FunkyModifierItemModifier.CanReceiveMods(Type))
                Mods = GlobalItems.FunkyModifierItemModifier.PickMods(Type, Level);
            else
                Mods = Array.Empty<GlobalItems.FunkyModifier>();
        }
        public ItemDef()
        {
            Type = ItemID.None;
            Level = 0;
            Mods = Array.Empty<GlobalItems.FunkyModifier>();
        }
    }
}