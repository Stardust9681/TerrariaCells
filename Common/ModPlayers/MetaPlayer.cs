using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.IO;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;

namespace TerrariaCells.Common.ModPlayers
{
    public class MetaPlayer : ModPlayer
    {
        //Flags for progression. Literally just add whatever and it should work <3
        //Net Sync supports up to 255 flags. If you need more than that, update netcode below
        public bool CloudJump { get => this[0]; set => this[0] = value; }
        public bool Goblin { get => this[1]; set => this[1] = value; }

        //-1 because this is also a property
        internal static int ProgressionCount => typeof(MetaPlayer).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length - 1;

        #region Backing Functionality
        private BitArray metaProgression = new BitArray(ProgressionCount);
        internal bool this[int index]
        {
            get => metaProgression[index];
            set
            {
                metaProgression[index] = value;
                if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
                    SyncPlayer(-1, Main.myPlayer, false);
            }
        }
        public override void SaveData(TagCompound tag)
        {
            if(metaProgression?.HasAnySet() == true)
                tag.Add("TerraCells:MetaProgress", metaProgression);
        }
        public override void LoadData(TagCompound tag)
        {
            int expectedLength = ProgressionCount;
            if (tag.TryGet<BitArray>("TerraCells:MetaProgress", out metaProgression))
            {
                if (metaProgression.Length < expectedLength)
                {
                    BitArray oldBits = new BitArray(metaProgression);
                    metaProgression = new BitArray(expectedLength);
                    for (int i = 0; i < oldBits.Length; i++)
                    {
                        metaProgression[i] = oldBits[i];
                    }
                }
            }
            else
            {
                metaProgression = new BitArray(expectedLength);
            }
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
            byte len = 0;
            for (byte i = 0; i < ProgressionCount; i++)
            {
                if (metaProgression[i])
                    len++;
            }
            packet.Write(len);
            for (byte i = 0; i < ProgressionCount; i++)
            {
                if (metaProgression[i])
                    packet.Write(i);
            }
            packet.Send();
        }
        public void GetSyncPlayer(System.IO.BinaryReader reader)
        {
            byte len = reader.ReadByte();
            for (int i = 0; i < len; i++)
            {
                byte index = reader.ReadByte();
                this.metaProgression[index] = true;
            }
            Mod.Logger.Info($"{Player.name} has Cloud: {CloudJump}");
            Mod.Logger.Info($"{Player.name} has Goblin: {Goblin}");
        }
        public override void CopyClientState(ModPlayer targetCopy)
        {
            MetaPlayer copy = (MetaPlayer)targetCopy;
            copy.metaProgression = this.metaProgression;
        }
        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            MetaPlayer client = (MetaPlayer)clientPlayer;
            if (!metaProgression.Equals(client.metaProgression))
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
}
