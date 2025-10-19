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
using Terraria.Localization;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.Items;
using TerrariaCells.Content.UI;

//using static TerrariaCells.Common.Utilities.JsonUtil;

namespace TerrariaCells.Common.GlobalNPCs
{
    class VanillaNPCShop : GlobalNPC
    {
		public override bool InstancePerEntity => true;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.townNPC;
        }

        private ItemDef[] selectedItems = Array.Empty<ItemDef>();
        public bool nurse_HasHealed = false;
        public override void SetDefaults(NPC entity)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Systems.TeleportTracker system = ModContent.GetInstance<Systems.TeleportTracker>();
                UpdateTeleport(entity, system.level, system.NextLevel);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModNetHandler.GetPacket(Mod, TCPacketType.ShopPacket).Send();
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
                    {
                        ItemDef def = selectedItems[i];
                        items[i] = new Item(def.Type);
                        if (items[i].TryGetGlobalItem<GlobalItems.TierSystemGlobalItem>(out var tierItem))
                            tierItem.SetLevel(items[i], def.Level);
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
            
            if(shop.NpcType == NPCID.GoblinTinkerer)
            {
                var items = ItemsJson.Instance.Loot[ItemsJson.ItemCategory.Accessories];
                foreach(var type in items)
                {
                    shop.Add(type, new Condition(LocalizedText.Empty, () => Main.LocalPlayer.GetModPlayer<ModPlayers.MetaPlayer>().CheckUnlocks(type) != UnlockState.Locked));
                }
            }
        }

        public static void UpdateTeleport(int level, string? levelName = null, bool net = false)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if(npc.TryGetGlobalNPC<VanillaNPCShop>(out var shopNPC))
                    shopNPC.UpdateTeleport(npc, level, levelName);
            }
        }
        public void UpdateTeleport(NPC npc, int level, string? levelName = null, bool net = false)
        {
            switch (npc.type)
            {
                case NPCID.ArmsDealer:
                    UpdateNPCShop(npc, ItemsJson.Instance.Loot[ItemsJson.ItemCategory.Weapons], level, 3);
                    break;
                case NPCID.Merchant:
                    UpdateNPCShop(npc, (int[])[.. ItemsJson.Instance.Loot[ItemsJson.ItemCategory.Armor], .. ItemsJson.Instance.Loot[ItemsJson.ItemCategory.Abilities]], level, 2);
                    break;

                case NPCID.Nurse:
                    nurse_HasHealed = false;
                    break;

                default:
                    return;
            }

            if (net)
            {
                NetSendShop(npc, ModNetHandler.GetPacket(Mod, TCPacketType.ShopPacket));
                Mod.Logger.Info($"Send shop for NPC: {npc.FullName}");
            }
        }
        private void UpdateNPCShop(NPC npc, IReadOnlyList<int> itemTypes, int itemLevel, int min = 1, int max = 40)
        {
            IReadOnlyList<int> selection = new List<int>(itemTypes);
            if (Configs.DevConfig.Instance.PlaytesterShops || min >= itemTypes.Count)
            {
                selectedItems = selection.Select(x => new ItemDef(x, itemLevel)).ToArray();
                return;
            }
            else
            {
                foreach(Player player in Main.ActivePlayers)
                {
                    Common.ModPlayers.MetaPlayer metaPlayer = player.GetModPlayer<ModPlayers.MetaPlayer>();
                    selection = metaPlayer.GetDropOptions(selection).ToArray();
                }
            }
            List<int> items = new List<int>();
            for (int i = 0; i < selection.Count; i++)
            {
                if (items.Count > min && !Main.rand.NextBool(items.Count - min)) continue;

                items.Add(Main.rand.Next(selection.Where(x => !items.Contains(x)).ToArray()));

                if (items.Count > max) break;
            }
            selectedItems = items.Select(x => new ItemDef(x, itemLevel)).ToArray();
        }

        public void NetSendShop(NPC npc, ModPacket packet, int toClient = -1, int ignoreClient = -1)
        {
            packet.Write((byte)npc.whoAmI);
            packet.Write((byte)selectedItems.Length);
            //O(n^3) is terrible I know
            //But I opted to do it this way so as to send less data over network
            for (int i = 0; i < selectedItems.Length; i++)
            {
                ItemDef shopItem = selectedItems[i];
                packet.Write7BitEncodedInt(shopItem.Type);
                packet.Write((byte)shopItem.Level);
                packet.Write((byte)shopItem.Mods.Length);
                if (shopItem.Mods.Length > 0)
                {
                    FunkyModifier[] modifiers = FunkyModifierItemModifier.GetModPool(shopItem.Type);
                    for (int j = 0; j < shopItem.Mods.Length; j++)
                    {
                        FunkyModifier itemMod = shopItem.Mods[j];
                        //Most significant bit used to indicate valid or invalid modifier
                        //If received value has that flag set, the result will be discarded
                        byte modIndex = 0b1_0000000;
                        for (int k = 0; k < modifiers.Length; k++)
                        {
                            if (modifiers[k].Equals(itemMod))
                            {
                                modIndex = (byte)k;
                                break;
                            }
                        }
                        packet.Write(modIndex);
                    }
                }
            }
            packet.Send(toClient, ignoreClient);
        }
        public void NetReceiveShop(NPC npc, BinaryReader reader)
        {
            //npc.whoAmI is read in packet handler, and the NPC is passed in here
            byte shopSize = reader.ReadByte();
            //Mod.Logger.Info($"Shop Size: {shopSize}");
            selectedItems = new ItemDef[shopSize];
            for (int i = 0; i < shopSize; i++)
            {
                int itemType = reader.Read7BitEncodedInt();
                byte level = reader.ReadByte();
                byte modCount = reader.ReadByte();
                //Mod.Logger.Info($"Shop Entry: {{ Type: {itemType}, Level: {level}, Mods: {modCount} }}");
                if (modCount > 0)
                {
                    List<FunkyModifier> mods = new List<FunkyModifier>();
                    FunkyModifier[] modPool = FunkyModifierItemModifier.GetModPool(itemType);
                    for (int j = 0; j < modCount; j++)
                    {
                        byte flagAndModType = reader.ReadByte();
                        if ((flagAndModType & 0b1_0000000) != 0)
                            continue;
                        mods.Add(modPool[flagAndModType]);
                    }
                    selectedItems[i] = new ItemDef(itemType, level, mods.ToArray());
                }
                else
                {
                    selectedItems[i] = new ItemDef(itemType, level, Array.Empty<FunkyModifier>());
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