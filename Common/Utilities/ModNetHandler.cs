using System;
using System.IO;
using TerrariaCells;
using TerrariaCells.Content;
using TerrariaCells.Content.Packets;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria;

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

        public override bool HijackGetData(ref byte messageType, ref BinaryReader reader, int playerNumber)
        {
            switch (messageType)
            {
                case MessageID.SyncNPC:
                    
                    break;
            }
            return base.HijackGetData(ref messageType, ref reader, playerNumber);
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
    }
}