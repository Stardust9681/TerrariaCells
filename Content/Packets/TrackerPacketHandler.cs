using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using TerrariaCells.Common.Systems;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Packets
{
    /// <summary>
    /// 
    /// </summary>
    public class TrackerPacketHandler() : PacketHandler(Common.Utilities.TCPacketType.TrackerPacket)
    {
        public enum ClientNetMsg
        {
            /// <summary>
            /// Unused
            /// </summary>
            NewPlayer,

            /// <summary>
            /// Sent from Client to Server to request <see cref="RewardTrackerSystem"/> data.
            /// </summary>
            /// <remarks>
            ///<b>Send/Receive</b>:
            /// <para><i>To Client:</i> <c>N/A</c> </para>
            /// <para><i>To Server:</i> <c>No Params</c></para>
            /// </remarks>
            RequestSync,

            /// <summary>
            /// Sent from Client to Server to inform of an enemy being killed.
            /// </summary>
            /// <remarks>
            ///<b>Send/Receive</b>:
            /// <para><i>To Client:</i> <c>N/A</c> </para>
            /// <para><i>To Server:</i> <c>No Params</c></para>
            /// </remarks>
            NewKills,
        }
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            switch (Main.netMode)
            {
                case NetmodeID.Server:
                    ClientNetMsg clientMsg = (ClientNetMsg)reader.ReadByte();
                    if (clientMsg == ClientNetMsg.RequestSync)
                    {
                        ModPacket packet = ModNetHandler.GetPacket(mod, TCPacketType.TrackerPacket);
                        packet.Write(RewardTrackerSystem.levelTimer);
                        packet.Write(RewardTrackerSystem.killCount);
                        packet.Write((byte)(RewardTrackerSystem.trackerState));
                        packet.Send(fromWho);
                    }
                    else if (clientMsg == ClientNetMsg.NewKills)
                    {
                        RewardTrackerSystem.killCount++;
                        ModPacket packet = ModNetHandler.GetPacket(mod, TCPacketType.TrackerPacket);
                        packet.Write(RewardTrackerSystem.levelTimer);
                        packet.Write(RewardTrackerSystem.killCount);
                        packet.Write((byte)RewardTrackerSystem.trackerState);
                        packet.Send();
                    }
                    break;
                case NetmodeID.MultiplayerClient:
                    uint timer = reader.ReadUInt32();
                    byte kills = reader.ReadByte();
                    RewardTrackerSystem.TrackerAction timerAction = (RewardTrackerSystem.TrackerAction)reader.ReadByte();
                    RewardTrackerSystem.UpdateTracker(timerAction);
                    RewardTrackerSystem.levelTimer = timer;
                    RewardTrackerSystem.killCount = kills;
                    break;
            }
        }
    }
}
