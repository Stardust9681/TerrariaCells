using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Net;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.Chat;
using TerrariaCells;
using TerrariaCells.Common.GlobalNPCs;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Common.Systems;
using System.Linq;

namespace TerrariaCells.Content.Packets
{
    internal class PlayerPacketHandler() : PacketHandler(TCPacketType.PlayerPacket)
    {
        public enum PlayerSyncType : byte
        {
            ///<summary>
            ///Syncs player meta-progression (see also: <see cref="Common.ModPlayers.MetaPlayer"/>)
            ///</summary>
            ///<remarks>
            ///<b>Send/Receive:</b>
            ///<para><i>To Client:</i> <c> <see langword="params"/> <see langword="byte"/>[] indeces </c></para>
            ///<para><i>To Server:</i> <c> <see langword="params"/> <see langword="byte"/>[] indeces </c></para>
            ///</remarks>
            MetaProgress, //{ byte, byte, .. }

            ///<summary>
            ///Syncs player states (see also: <see cref="Common.ModPlayers.LifeModPlayer"/>)
            ///</summary>
            ///<remarks>
            ///<b>Send/Receive:</b>
            ///<para><i>To Client:</i> <c> <see cref="ushort"/> health </c></para>
            ///<para><i>To Server:</i> <c> <see cref="ushort"/> health </c></para>
            ///</remarks>
            StatSync, //ushort

            ///<summary>
            ///Sent from client to server on join, server responds telling player to either teleport to a living player, or to enter "spectate mode"
            ///</summary>
            ///<remarks>
            ///<b>Send/Receive:</b>
            ///<para><i>To Client:</i> <c> <see langword="bool"/> doSpectate </c></para>
            ///<para><i>To Server:</i> <c> No Params </c></para>
            ///</remarks>
            NewPlayerJoin,
        }
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            PlayerSyncType type = (PlayerSyncType)reader.ReadByte();
            switch (type)
            {
                case PlayerSyncType.MetaProgress:
                    HandleMeta(mod, reader, fromWho);
                    return;
                case PlayerSyncType.StatSync:
                    HandleStat(mod, reader, fromWho);
                    return;
                case PlayerSyncType.NewPlayerJoin:
                    HandleNewPlayerJoin(mod, reader, fromWho);
                    return;
            }
        }
        private void HandleMeta(Mod mod, BinaryReader reader, int fromWho)
        {
            Player player;
            if (fromWho == 256)
                player = Main.LocalPlayer;
            else
                player = Main.player[fromWho];
            Common.ModPlayers.MetaPlayer modPlayer = player.GetModPlayer<Common.ModPlayers.MetaPlayer>();
            modPlayer.GetSyncPlayer(reader);

            if(Main.netMode == 2)
            {
                modPlayer.SyncPlayer(-1, fromWho, false);
            }
        }
        private void HandleStat(Mod mod, BinaryReader reader, int fromWho)
        {
            byte whoAmI = reader.ReadByte();
            Player sender = Main.player[whoAmI];
            Common.ModPlayers.LifeModPlayer modPlayer = sender.GetModPlayer<Common.ModPlayers.LifeModPlayer>();
            modPlayer.extraHealth = reader.ReadUInt16();

            if(Main.netMode == 2)
            {
                modPlayer.SyncPlayer(-1, fromWho, false);
            }
        }
        private void HandleNewPlayerJoin(Mod mod, BinaryReader reader, int fromWho)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                ModPacket packet = ModNetHandler.GetPacket(mod, HandlerType);
                packet.Write((byte)PlayerSyncType.NewPlayerJoin);

                var tele = ModContent.GetInstance<TeleportTracker>();
                bool shouldDie = tele.level > 1 && tele.NextLevel.ToLower().Equals("inn");

                int activeLivingPlayers = 0;
                foreach (Player p in Main.ActivePlayers)
                    if (!p.DeadOrGhost && p.whoAmI != fromWho)
                        activeLivingPlayers++;
                if (activeLivingPlayers == 0)
                    shouldDie = false;
                packet.Write(shouldDie);
                packet.Send(fromWho);
            }
            else
            {
                bool shouldDie = reader.ReadBoolean();

                int tpTarget = -1;
                for (int i = 0; i < Main.maxNetPlayers; i++)
                {
                    Player test = Main.player[i];
                    if (!test.active)
                        continue;
                    if (test.DeadOrGhost)
                        continue;
                    if (test.whoAmI == Main.myPlayer)
                        continue;
                    tpTarget = i;
                    break;
                }
                if (tpTarget != -1)
                {
                    Main.LocalPlayer.Teleport(Main.player[tpTarget].position, TeleportationStyleID.Portal);
                }

                if (shouldDie)
                {
                    Main.LocalPlayer.ghost = true;
                }
            }
        }
    }
}