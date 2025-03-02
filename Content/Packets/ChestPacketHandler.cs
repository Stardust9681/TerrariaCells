using System;
using System.IO;
using System.Text.Json;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Net;
using Terraria.ModLoader.Utilities;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells;
using TerrariaCells.Common.GlobalNPCs;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Common.Systems;

namespace TerrariaCells.Content.Packets
{
    internal class ChestPacketHandler(TCPacketType handlerType) : PacketHandler(handlerType)
	{
        private readonly ChestLootSpawner spawner = ModContent.GetInstance<ChestLootSpawner>();

        public override void HandlePacket(BinaryReader reader, int fromWho)
		{
            switch ((ChestPacketType)reader.ReadByte())
            {
                case ChestPacketType.ServerOpenChest:
                    int chest = reader.ReadInt32();
                    int x = Main.chest[chest].x;
                    int y = Main.chest[chest].y;
                    spawner.OpenChest(x, y, chest);
                    // The only way to sync our custom chest system to clients is to send another packet, so here we go
                    if (Main.netMode == NetmodeID.MultiplayerClient) break;
                    ModPacket packet = ModContent.GetInstance<TerrariaCells>().GetPacket();
                    packet.Write((byte)TCPacketType.ChestPacket);
                    packet.Write((byte)ChestPacketType.ClientOpenChest);
                    packet.Write(chest);
                    packet.Send();
                    break;
                // This is called on each client, only the server should call this packet
                case ChestPacketType.ClientOpenChest:
                    int clientChest = reader.ReadInt32();
                    int newX = Main.chest[clientChest].x;
                    int newY = Main.chest[clientChest].y;
                    //spawner.OpenChest(newX, newY, clientChest);
                    break;
            }
            
		}
	}
    public enum ChestPacketType : byte
    {
        ClientOpenChest,
        ServerOpenChest,
    }
}