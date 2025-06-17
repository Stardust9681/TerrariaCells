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
    internal class PlayerPacketHandler() : PacketHandler(TCPacketType.PlayerPacket)
    {
        public enum PlayerSyncType : byte
        {
            MetaProgress, //{ byte, byte, .. }
            StatSync, //byte (short?)
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
            }
        }
        private void HandleMeta(Mod mod, BinaryReader reader, int fromWho)
        {
            Player player = Main.player[fromWho];
            Common.ModPlayers.MetaPlayer modPlayer = player.GetModPlayer<Common.ModPlayers.MetaPlayer>();
            modPlayer.GetSyncPlayer(reader);
        }
        private void HandleStat(Mod mod, BinaryReader reader, int fromWho)
        {

        }
    }
}
