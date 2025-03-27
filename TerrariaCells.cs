global using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells
{
    //Contributions already present are by no means absolute, conventions are negotiable.
    public class TerrariaCells : Mod 
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI) 
        {
            ModNetHandler.HandlePacket(reader, whoAmI);
        }
    }
    /// <summary>
    /// Base class for handling net packets, 
    /// see TerrariaCells/Content/Packets for implementations
    /// </summary>
    internal abstract class PacketHandler
	{
		internal TCPacketType HandlerType { get; set; }
		/// <summary>
        /// Override this class to read through certain packets
        /// </summary>
        /// <param name="reader">the reader</param>
        /// <param name="fromWho">the owner player that sent this packet, equals 255 if server</param>
		public abstract void HandlePacket(BinaryReader reader, int fromWho);

		protected PacketHandler(TCPacketType handlerType)
		{
			HandlerType = handlerType;
		}
        /// <summary>
        /// Get a ModPacket with some written data. Write into a ModPacket with the HandlerType of the class, the sub packetType and the sender
        /// </summary>
        /// <param name="packetType">Type of packet, for most packets this should be 0</param>
        /// <param name="fromWho"> The sender of the packet, if it's lower than 0, it won't be written into the packet</param>
        /// <returns>A ModPacket that contains in this order a HandlerType, the given packetType, and the sender</returns>
		protected ModPacket GetPacket(byte packetType, int fromWho)
		{
			ModPacket p = ModContent.GetInstance<TerrariaCells>().GetPacket();
			p.Write((byte)HandlerType);
			p.Write(packetType);
            if (fromWho > -1)
            {
                p.Write((byte)fromWho);
            }
			return p;
		}
	}
}
