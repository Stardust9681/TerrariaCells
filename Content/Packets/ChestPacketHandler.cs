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
using TerrariaCells.Common.ModPlayers;

namespace TerrariaCells.Content.Packets
{
    internal class ChestPacketHandler() : PacketHandler(TCPacketType.ChestPacket)
	{
        private readonly ChestLootSpawner spawner = ModContent.GetInstance<ChestLootSpawner>();

        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
		{
            switch ((ChestPacketType)reader.ReadByte())
            {
                case ChestPacketType.ServerOpenChest: //Asking server to open
                {
                    int chest = reader.ReadInt32();
                    int x = Main.chest[chest].x;
                    int y = Main.chest[chest].y;
                    spawner.OpenChest(x, y, chest);
                    // The only way to sync our custom chest system to clients is to send another packet, so here we go
                    if (Main.netMode == NetmodeID.MultiplayerClient) break;
                    ModPacket packet = mod.GetPacket();
                    packet.Write((byte)this.HandlerType);
                    packet.Write((byte)ChestPacketType.ClientOpenChest);
                    packet.Write(chest);
                    packet.Send();
                    break;
                }
                // This is called on each client, only the server should write to this packet
                case ChestPacketType.ClientOpenChest: //Asking client to open
                {
                    int clientChest = reader.ReadInt32();
                    spawner.lootedChests.Add(clientChest);
                    break;
                }
                case ChestPacketType.ClientCloseChests:
                    ushort count = reader.ReadUInt16();
                    for (int i = 0; i < count; i++)
                    {
                        int chest = reader.ReadUInt16();
                        spawner.lootedChests.Remove(chest);
                    }
                    break;
                case ChestPacketType.ServerJoin: //Runs on server
                {
                    if (Main.netMode != NetmodeID.Server) return;
                    int whoAmI = reader.ReadInt32();
                    ModPacket p = GetPacket((byte)ChestPacketType.ClientJoin, -1);
                    p.Write(spawner.lootedChests.Count);
                    foreach (int chestIndex in spawner.lootedChests)
                    {
                        p.Write(chestIndex);
                    }
                    p.Send(whoAmI, -1);
                    break;
                }
                case ChestPacketType.ClientJoin: //Runs on client
                {
                    int cnt = reader.ReadInt32();
                    for (int i = 0; i < cnt; i++)
                    {
                        spawner.lootedChests.Add(reader.ReadInt32());
                    }
                    break;
                }
            }
		}
	}
    public enum ChestPacketType : byte
    {
        ///<summary>
        ///Sent from server to client, tells client to consider a chest to be opened
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> <see langword="int"/> chestIndex </c></para>
        ///<para><i>To Server:</i> <c> N/A </c></para>
        ///</remarks>
        ClientOpenChest,

        ///<summary>
        ///Ask a client to close a number of chests. Used for end-of-level rewards.
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive</b>:
        ///<para><i>To Client:</i> <c><see langword="ushort"/> count, <see langword="params ushort"/>[] chests</c></para>
        ///<para><i>To Server:</i> <c>N/A</c></para>
        ///</remarks>
        ClientCloseChests,

        ///<summary>
        ///Sent from client to server, tells server to consider a chest to be opened
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> N/A </c></para>
        ///<para><i>To Server:</i> <c> <see langword="int"/> chestIndex </c></para>
        ///</remarks>
        ServerOpenChest,

        ///<summary>
        ///Sent from client to server, requests updated <see cref="ChestLootSpawner"/> data
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> N/A </c></para>
        ///<para><i>To Server:</i> <c> <see langword="int"/> whoAmI </c></para>
        ///</remarks>
        ServerJoin,

        ///<summary>
        ///Sent from server to client, in response to <see cref="ServerJoin"/>
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> <see langword="int"/> count, <see langword="params"/> <see langword="int"/>[] chestIndeces </c></para>
        ///<para><i>To Server:</i> <c> N/A </c></para>
        ///</remarks>
        ClientJoin,
    }
}