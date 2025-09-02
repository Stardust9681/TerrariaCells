using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ModLoader;

using TerrariaCells.Common.Systems;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Packets
{
    public class HeartPacketHandler() : PacketHandler(TCPacketType.HeartPacket)
    {
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            var instance = ModContent.GetInstance<ClickedHeartsTracker>();
            switch ((HeartPacketType)reader.ReadByte())
            {
                case HeartPacketType.ClientUse: //Read on server
                    instance.collectedHearts.Add(((int)reader.ReadUInt16(), (int)reader.ReadUInt16()));
                    NetMessage.SendData(Terraria.ID.MessageID.WorldData);
                    return;
            }
        }

        public enum HeartPacketType : byte
        {
            /// <summary>
            /// Sent by client when interacting with a Life Crystal. Not for use on Server.
            /// </summary>
            /// <remarks>
            ///<b>Send/Receive</b>:
            ///<para><i>To Client:</i> <c>N/A</c></para>
            ///<para><i>To Server:</i> <c><see langword="ushort"/> topLeftX, <see langword="ushort"/> topLeftY</c></para>
            /// </remarks>
            ClientUse,
        }
    }
}
