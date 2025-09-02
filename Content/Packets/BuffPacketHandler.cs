using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using TerrariaCells.Common.GlobalNPCs;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Packets
{
    public class BuffPacketHandler() : PacketHandler(TCPacketType.BuffPacket)
    {
        public enum BuffPacketType : byte
        {
            ///<summary>
            ///Syncs adding buff to NPC
            ///</summary>
            ///<remarks>
            ///<b>Send/Receive:</b>
            ///<para><i>To Client:</i> <c> <see langword="byte"/> NPC.whoAmI, <see langword="byte"/> buffIndex, 7bit <see langword="int"/> buffOrigTime, 7bit <see langword="int"/> buffStacks </c></para>
            ///<para><i>To Server:</i> <c> <see langword="byte"/> NPC.whoAmI, <see langword="byte"/> buffIndex, 7bit <see langword="int"/> buffOrigTime, 7bit <see langword="int"/> buffStacks </c></para>
            ///</remarks>
            AddBuff,

            ///<summary>
            ///Syncs buff orig time and stack count.
            ///</summary>
            ///<remarks>
            ///<b>Send/Receive:</b>
            ///<para><i>To Client:</i> <c> <see cref="Tuple{int, int}"/>[<see cref="NPC.maxBuffs"/>] data </c></para>
            ///<para><i>To Server:</i> <c> ? </c></para>
            ///</remarks>
            Buffs
        }
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            //MessageID.AddNPCBuff = 53
            //MessageID.NPCBuffs = 54
            BuffPacketType type = (BuffPacketType)reader.ReadByte();
            //Buff Applied: { NPCID, Type, iTime, Stacks }
            //

            //For SOME reason GetGlobalNPC<BuffNPC> sometimes throws "Key not found"
            //Even though the GlobalNPC applies to ALL NPCs. But I digress
            //So in case it doesn't exist for some NPC, but somehow that NPC sent this packet
            //We need to "consume" the rest of the packet somehow.
            //Hence, discard, BinaryReader.ReadBytes(..)

            byte npcWhoAmI = reader.ReadByte();
            if(Main.npc[npcWhoAmI].TryGetGlobalNPC<BuffNPC>(out BuffNPC buffNPC))
                buffNPC.NetReceieve(Main.npc[npcWhoAmI], type, reader, fromWho);
            else
                _ = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
        }
    }
}
