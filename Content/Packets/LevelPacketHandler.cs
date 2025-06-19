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

namespace TerrariaCells.Content.Packets
{
    internal class LevelPacketHandler() : PacketHandler(TCPacketType.LevelPacket)
    {
        internal const string Key_FailedTP = "Portal_FailedTP";
        public override void HandlePacket(Mod mod, BinaryReader reader, int fromWho)
        {
            if(Main.netMode == NetmodeID.Server) //Sent by connected client
            {
                string destination = reader.ReadString();

                Main.NewText(destination);
                mod.Logger.Debug(destination);

                Player sender = Main.player[fromWho];
                foreach (Player player in Main.ActivePlayers)
                {
                    if (player.DeadOrGhost) continue;
                    if (player.DistanceSQ(sender.position) > MathF.Pow(NumberHelpers.ToTileDist(WorldPylonSystem.MAX_PYLON_RANGE), 2))
                    {
                        //ChatHelper.BroadcastChatMessage(NetworkText.FromKey(Key_FailedTP, sender.name, destination), Main.OurFavoriteColor);
                        //mod.Logger.Debug($"Player {player.name} too far from {sender.name}. Distance: {player.Distance(sender.position)} > {NumberHelpers.ToTileDist(WorldPylonSystem.MAX_PYLON_RANGE)}");
                        return;
                    }
                }

                var tele = ModContent.GetInstance<TeleportTracker>();
                tele.Update_SetVariables(destination);
                var tpTile = tele.GetTelePos(tele.GetActualDestination(destination));
                tele.Update_SetWorldConditions(destination);


                ModPacket packet = ModNetHandler.GetPacket(mod, HandlerType);
                packet.Write(tpTile.X);
                packet.Write(tpTile.Y);
                packet.Write((byte)tele.level);
                packet.Send();

                tele.Update_PostTeleport(tele.GetActualDestination(destination));
                mod.Logger.Info("Post Teleport Update");
            }
            else //Received by client (from Server)
            {
                (short X, short Y) = (reader.ReadInt16(), reader.ReadInt16());
                byte level = reader.ReadByte();
                //string dest = reader.ReadString();
                var tele = ModContent.GetInstance<TeleportTracker>();
                tele.level = level;

                Vector2 telePos = new Point(X, Y).ToWorldCoordinates();
                NetMessage.SendData(
                    MessageID.TeleportPlayerThroughPortal,
                    -1,
                    -1,
                    null,
                    Main.LocalPlayer.whoAmI,
                    (int)telePos.X,
                    (int)telePos.Y
                );

                if (Main.LocalPlayer.DeadOrGhost)
                {
                    Main.LocalPlayer.ChangeSpawn(X, Y);
                    //Main.LocalPlayer.respawnTimer = 0;
                    Main.LocalPlayer.Spawn(PlayerSpawnContext.ReviveFromDeath);
                }
            }
        }
    }
}
