using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Packets
{
    internal class PylonPacketHandler() : PacketHandler(TCPacketType.PylonPacket)
    {
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            switch((PylonPacketType)reader.ReadByte())
            {
                case PylonPacketType.PylonDiscovery:
                {
                    short pylonX = reader.ReadInt16();
                    short pylonY = reader.ReadInt16();
                    WorldPylonSystem.MarkDiscovery(new Point16(pylonX, pylonY));
                    // We only want the server to send this back to clients
                    if (Main.netMode == NetmodeID.MultiplayerClient) return;
                    ModPacket p = GetPacket((byte)PylonPacketType.PylonDiscovery, -1);
                    p.Write(pylonX);
                    p.Write(pylonY);
                    p.Send();
                    break;
                }
                case PylonPacketType.ServerPlayerEnter:
                {
                    if (Main.netMode != NetmodeID.Server) return;
                    int playerIndex = reader.ReadInt32();
                    // Write a new packet to send to clients
                    ModPacket p = GetPacket((byte)PylonPacketType.ClientPlayerEnter, -1);
                    int pylonCount = WorldPylonSystem.GetDiscoveredPylons().Count;
                    p.Write(pylonCount);
                    foreach (Point16 point in WorldPylonSystem.GetDiscoveredPylons().Keys)
                    {
                        WorldPylonSystem.GetDiscoveredPylons().TryGetValue(point, out bool val);
                        if (!val) continue;
                        p.Write(point.X);
                        p.Write(point.Y);
                    }
                    p.Send(playerIndex, -1);
                    //ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral("" + playerIndex), Color.White);
                    break;
                }
                case PylonPacketType.ClientPlayerEnter:
                    int pyCnt = reader.ReadInt32();
                    for (int i = 0; i < pyCnt; i++)
                    {
                        WorldPylonSystem.MarkDiscovery(
                            new Point16(reader.ReadInt16(), 
                                        reader.ReadInt16()));
                    }
                    break;
            }
        }
    }
    public enum PylonPacketType
    {
        ///<summary>
        ///Syncs found pylons from client to server, and from server to client
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> <see langword="short"/> pylonX, <see langword="short"/> pylonY </c></para>
        ///<para><i>To Server:</i> <c> <see langword="short"/> pylonX, <see langword="short"/> pylonY </c></para>
        ///</remarks>
        PylonDiscovery,

        ///<summary>
        ///Sent from client to server, requests updated <see cref="PylonSystem"/>/<see cref="WorldPylonSystem"/> data.
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> N/A </c></para>
        ///<para><i>To Server:</i> <c> <see langword="int"/> whoAmI </c></para>
        ///</remarks>
        ServerPlayerEnter,

        ///<summary>
        ///Sent from server to client, in response to <see cref="ServerPlayerEnter"/>
        ///</summary>
        ///<remarks>
        ///<b>Send/Receive:</b>
        ///<para><i>To Client:</i> <c> <see langword="int"/> foundPylonCount, <see langword="params"/> (<see langword="short"/>, <see langword="short"/>)[] foundPylons </c></para>
        ///<para><i>To Server:</i> <c> N/A </c></para>
        ///</remarks>
        ClientPlayerEnter,
    }
}