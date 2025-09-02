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
                [TCPacketType.TrackerPacket] = new TrackerPacketHandler(),
                [TCPacketType.HeartPacket] = new HeartPacketHandler(),
            };
        }
        internal static Dictionary<TCPacketType, PacketHandler> Handlers;
        public static void HandlePacket(Terraria.ModLoader.Mod mod, BinaryReader reader, int fromWho)
        {
            // Switch on TCPacketType, when sending a packet, this should always be written first
            TCPacketType type = (TCPacketType)reader.ReadByte();
            if (Handlers.TryGetValue(type, out var handler))
                handler.HandlePacket(mod, reader, fromWho);
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
        /// <summary>
        /// See <see cref="SpawnPacketType"/> for more details
        /// </summary>
        SpawnPacket,
        /// <summary>
        /// See <see cref="ChestPacketType"/> for more details
        /// </summary>
        ChestPacket,
        /// <summary>
        /// See <see cref="PylonPacketType"/> for more details
        /// </summary>
        PylonPacket,
        ///<summary>
        ///Update level info on teleport
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> <see langword="short"/> X, <see langword="short"/> Y, <see langword="byte"/> level, <see langword="string"/> nextDestination </c></para>
        ///<para><i>To Server:</i> <c> <see langword="string"/> destination </c></para>
        ///</remarks>
        LevelPacket,
        /// <summary>
        /// See <see cref="PlayerPacketHandler.PlayerSyncType"/> for more details
        /// </summary>
        PlayerPacket,
        /// <summary>
        /// Use <see cref="VanillaNPCShop.NetSendShop(NPC, ModPacket, int, int)"/> and <see cref="VanillaNPCShop.NetReceiveShop(NPC, BinaryReader)"/>
        /// </summary>
        ShopPacket,
        /// <summary>
        /// See <see cref="BuffPacketHandler.BuffPacketType"/> for more details
        /// </summary>
        BuffPacket,
        /// <summary>
        /// See <see cref="TrackerPacketHandler.ClientNetMsg"/> for more details
        /// </summary>
        TrackerPacket,
        /// <summary>
        /// See <see cref="HeartPacketHandler.HeartPacketType"/> for more details
        /// </summary>
        HeartPacket,
    }

    //Template XML docs for specific message types:

    ///<summary>
    ///?
    ///</summary>
    ///<remarks>
    ///<b>Send/Receive:</b>
    ///<para><i>To Client:</i> <c> ? </c></para>
    ///<para><i>To Server:</i> <c> ? </c></para>
    ///</remarks>
}