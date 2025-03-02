using System;
using System.IO;
using TerrariaCells;
using TerrariaCells.Content;
using TerrariaCells.Content.Packets;

namespace TerrariaCells.Common.Utilities
{
    internal class ModNetHandler 
    {
        internal static SpawnPacketHandler spawnHandler = new(TCPacketType.SpawnPacket);
        internal static ChestPacketHandler chestHandler = new(TCPacketType.ChestPacket);
        public static void HandlePacket(BinaryReader reader, int fromWho)
        {
            // Switch on TCPacketType, when sending a packet, this should always be written first
            switch ((TCPacketType)reader.ReadByte())
            {
                case TCPacketType.SpawnPacket:
                    spawnHandler.HandlePacket(reader, fromWho);
                    break;
                case TCPacketType.ChestPacket:
                    chestHandler.HandlePacket(reader, fromWho);
                    break;
            }
        }
    }
    public enum TCPacketType : byte
    {
        SpawnPacket,
        ChestPacket,
    }
}