using System;
using System.IO;
using TerrariaCells;
using TerrariaCells.Content;
using TerrariaCells.Content.Packets;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;
using Terraria.Localization;
using TerrariaCells.Common.GlobalNPCs;

namespace TerrariaCells.Common.Utilities
{
    internal class ModNetHandler : ModSystem
    {
        static ModNetHandler()
        {
            Handlers = new Dictionary<TCPacketType, PacketHandler>()
            {
                [TCPacketType.SpawnPacket] = new SpawnPacketHandler(),
                [TCPacketType.ChestPacket] = new ChestPacketHandler(),
                [TCPacketType.PylonPacket] = new PylonPacketHandler(),
                [TCPacketType.LevelPacket] = new LevelPacketHandler(),
                [TCPacketType.PlayerPacket] = new PlayerPacketHandler(),
                [TCPacketType.ShopPacket] = new ShopPacketHandler(),
                [TCPacketType.BuffPacket] = new BuffPacketHandler(),
            };
        }
        internal static Dictionary<TCPacketType, PacketHandler> Handlers;
        public static void HandlePacket(Terraria.ModLoader.Mod mod, BinaryReader reader, int fromWho)
        {
            // Switch on TCPacketType, when sending a packet, this should always be written first
            TCPacketType type = (TCPacketType)reader.ReadByte();
            if (Handlers.TryGetValue(type, out var handler)) handler.HandlePacket(mod, reader, fromWho);
        }
        internal static Terraria.ModLoader.ModPacket GetPacket(Terraria.ModLoader.Mod mod, TCPacketType type, ushort len = 256)
        {
            Terraria.ModLoader.ModPacket packet = mod.GetPacket(len);
            packet.Write((byte)type);
            return packet;
        }
    }
    public enum TCPacketType : byte
    {
        SpawnPacket,
        ChestPacket,
        PylonPacket,
        LevelPacket,
        PlayerPacket,
        ShopPacket,
        BuffPacket,
    }
}