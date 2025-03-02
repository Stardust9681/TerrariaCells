global using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Common.Configs;
using Terraria.Chat;
using Terraria.Localization;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Systems;
using System.Collections.Generic;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Packets;

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
    /// see SpawnPacketHandler.cs for implementations
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
        /// Get a ModPacket
        /// </summary>
        /// <param name="packetType">Type of packet, for most packets this should be 0</param>
        /// <param name="fromWho"></param>
        /// <returns>A ModPacket that contains a HandlerType, the given packetType, and the sender</returns>
		protected ModPacket GetPacket(byte packetType, int fromWho)
		{
			ModPacket p = GetPacket(packetType, fromWho);
			p.Write((byte)HandlerType);
			p.Write(packetType);
            p.Write((byte)fromWho);
			return p;
		}
        // This currently remains unused, but it can be helpful
	}
}
