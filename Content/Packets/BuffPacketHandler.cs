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
            AddBuff,
            Buffs
        }
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            //MessageID.AddNPCBuff = 53
            //MessageID.NPCBuffs = 54
            BuffPacketType type = (BuffPacketType)reader.ReadByte();
            //Buff Applied: { NPCID, Type, iTime, Stacks }
            //
            byte npcWhoAmI = reader.ReadByte();
            Main.npc[npcWhoAmI].GetGlobalNPC<BuffNPC>().NetReceieve(Main.npc[npcWhoAmI], type, reader, fromWho);
        }
    }
}
