using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;
using Terraria.Chat;
using Terraria.Localization;
using TerrariaCells.Common.GlobalNPCs;

namespace TerrariaCells.Common.ModPlayers
{
    public class MetaPlayer : ModPlayer
    {
        public override void Load()
        {
            for(int i = 0; i < ProgressionCount; i++)
            {
                _ = Mod.GetLocalization("ui.metaprogress.entry_"+i, () => "Undefined");
            }
        }

        #region Major Flags
        
        //Flags for progression. Literally just add whatever and it should work <3
        public bool CloudJump { get => this[0]; set => this[0] = value; }
        public bool Goblin { get => this[1]; set => this[1] = value; }
        public bool DownedBoC { get => this[2]; set => this[2] = value; }
        public bool DownedEoW { get => this[3]; set => this[3] = value; }
        public bool DownedQB { get => this[4]; set => this[4] = value; }
        public bool DownedSkele { get => this[5]; set => this[5] = value; }
        public bool DownedWoF { get => this[6]; set => this[6] = value; }

        internal const int ProgressionCount = 7;
        
        private void SaveFlags(TagCompound tag)
        {
            if(metaProgression?.HasAnySet() == true)
                tag.Add("TerraCells:MetaProgress", metaProgression);
        }
        
        private void LoadFlags(TagCompound tag)
        {
            if (tag.TryGet<BitArray>("TerraCells:MetaProgress", out metaProgression))
            {
                if (metaProgression.Length < ProgressionCount)
                {
                    BitArray oldBits = new BitArray(metaProgression);
                    metaProgression = new BitArray(ProgressionCount);
                    for (int i = 0; i < oldBits.Length; i++)
                    {
                        metaProgression[i] = oldBits[i];
                    }
                }
            }
            else
            {
                metaProgression = new BitArray(ProgressionCount);
            }
            //overrideMeta = new BitArray(metaProgression);
        }
        
        #endregion

        public void DoUnlockText(LocalizedText text, Color? colour = null, int overheadTime = 360)
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                Main.NewText(text.Value, colour);
                Player.chatOverhead.NewMessage(text.Value, overheadTime);
            }
        }
        
        #region Item Unlocks

        private const string _ITEM_KEYS = "Keys:Item";
        private const string _ITEM_VALS = "Vals:Item";
        public Dictionary<int, bool> ItemUnlocks { get; private set; } = new Dictionary<int, bool>();
        private void SaveItems(TagCompound tag)
        {
            tag[_ITEM_KEYS] = ItemUnlocks.Keys.ToList();
            tag[_ITEM_VALS] = ItemUnlocks.Values.ToList();
        }
        private void LoadItems(TagCompound tag)
        {
            List<int> itemKeys = tag.Get<List<int>>(_ITEM_KEYS);
            List<bool> itemVals = tag.Get<List<bool>>(_ITEM_VALS);
            ItemUnlocks = itemKeys.Zip(itemVals, (k, v) => new KeyValuePair<int, bool>(k, v)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        #endregion

        public Content.UI.UnlockState CheckUnlocks(Item item)
        {
            return CheckUnlocks(item.type);
        }
        public Content.UI.UnlockState CheckUnlocks(int itemType)
        {
            if(!ItemUnlocks.TryGetValue(itemType, out bool result))
                return Content.UI.UnlockState.Locked;
            return result ? Content.UI.UnlockState.Found : Content.UI.UnlockState.Unlocked;
        }

        #region Backing Functionality
        public bool HasFlag(int index)
        {
            if(index < 0 || index > ProgressionCount)
            {
#if DEBUG
                throw new InvalidOperationException();
#else
                return false;
#endif
            }
            return metaProgression[index];
        }
        //private BitArray overrideMeta = new BitArray(ProgressionCount);
        private BitArray metaProgression = new BitArray(ProgressionCount);
        internal bool this[int index]
        {
            get => metaProgression[index] /*&& overrideMeta[index]*/;
            set
            {
                if(value && !metaProgression[index])
                {
                    metaProgression[index] = value;
                    //overrideMeta[index] = value;
                }
                else
                {
                    //overrideMeta[index] = value;
                }
                if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                    SyncPlayer(-1, Main.myPlayer, false);
            }
        }
        public override void SaveData(TagCompound tag)
        {
            SaveFlags(tag);

            SaveItems(tag);
        }
        public override void LoadData(TagCompound tag)
        {
            SaveFlags(tag);

            LoadItems(tag);
        }

        public override void OnEnterWorld()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
            {
                SyncPlayer(-1, Main.myPlayer, true);
            }
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            //Common.Utilities.ModNetHandler.Handlers[Utilities.TCPacketType.PlayerPacket]
            ModPacket packet = Common.Utilities.ModNetHandler.GetPacket(Mod, Utilities.TCPacketType.PlayerPacket);
            packet.Write((byte)Content.Packets.PlayerPacketHandler.PlayerSyncType.MetaProgress);
            byte[] arr = new byte[1 + (ProgressionCount/sizeof(byte))];
            //overrideMeta.CopyTo(arr, 0);
            metaProgression.CopyTo(arr, 0);
            packet.Write(arr, 0, arr.Length);

            packet.Write(newPlayer);
            if(newPlayer)
            {
                packet.Write((ushort)ItemUnlocks.Count);
                foreach(int key in ItemUnlocks.Keys)
                {
                    packet.Write7BitEncodedInt(key);
                }
            }
            
            packet.Send(toWho, fromWho);
        }
        public void GetSyncPlayer(System.IO.BinaryReader reader)
        {
            byte[] arr = new byte[1 + (ProgressionCount/sizeof(byte))];
            reader.Read(arr, 0, arr.Length);
            BitArray temp = new BitArray(arr);
            for(int i = 0; i < temp.Length; i++)
            {
                this[i] = temp[i];
            }

            if(reader.ReadBoolean())
            {
                ItemUnlocks = new Dictionary<int, bool>();
                ushort len = reader.ReadUInt16();
                for(ushort i = 0; i < len; i++)
                {
                    ItemUnlocks[reader.Read7BitEncodedInt()] = false;
                }
            }
        }
        public override void CopyClientState(ModPlayer targetCopy)
        {
            MetaPlayer copy = (MetaPlayer)targetCopy;
            copy.metaProgression = this.metaProgression;
            //copy.overrideMeta = this.overrideMeta;
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            MetaPlayer client = (MetaPlayer)clientPlayer;
            if (!metaProgression.Equals(client.metaProgression) /*|| !overrideMeta.Equals(client.overrideMeta)*/)
            {
                SyncPlayer(-1, Main.myPlayer, false);
            }
        }
        #endregion
    }
    //TagCompound doesn't handle BitArray normally :/
    public class BitArraySerializer : TagSerializer<BitArray, TagCompound>
    {
        internal static FieldInfo BitArray_ArrayINT_m_array = typeof(BitArray).GetField("m_array", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static FieldInfo BitArray_Int_m_length = typeof(BitArray).GetField("m_length", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static FieldInfo BitArray_Int_version = typeof(BitArray).GetField("_version", BindingFlags.NonPublic | BindingFlags.Instance);

        public override BitArray Deserialize(TagCompound tag)
        {
            int[] arrs = tag.Get<int[]>("m_array");
            return new BitArray(arrs);
        }

        public override TagCompound Serialize(BitArray value)
        {
            return new TagCompound()
            {
                ["m_array"] = (int[])BitArray_ArrayINT_m_array.GetValue(value)
            };
        }
    }

    public class UnlockItem : GlobalItem
    {
        public override bool AppliesToEntity(Item entity, bool lateInstantiation)
        {
            return
                VanillaNPCShop.Weapons.Contains(entity.type)
                || VanillaNPCShop.Accessories.Contains(entity.type)
                || VanillaNPCShop.Armors.Contains(entity.type)
                || VanillaNPCShop.Skills.Contains(entity.type);
        }

        public override void UpdateInventory(Item item, Player player)
        {
            player.GetModPlayer<MetaPlayer>().ItemUnlocks[item.type] = true;
        }
    }
}
