using System;
using System.Collections.Generic;
using System.Linq;
using TerrariaCells;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Net;
using Terraria.DataStructures;
using TerrariaCells.Common.Systems;
using Terraria.Chat;
using Terraria.Localization;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Packets;

namespace TerrariaCells.Common.ModPlayers
{
	public class SpawnPlayer : ModPlayer
    {
        private int deadTimer = 0;

        public override void OnRespawn()
        {
            deadTimer = 0;
            base.OnRespawn();
        }
        public override void UpdateDead()
        {
            // If we're in singleplayer we want to do normal respawning
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                if (Player.respawnTimer > 60 * 5)
                    Player.respawnTimer = 60 * 5;
                return;
            }
            Player.respawnTimer = 60 * 5;
            deadTimer++;
            // Wait two seconds because we don't want instant respawn
            if (deadTimer == 60 * 2)
            {
                // Ask the server to respawn players, only if we're on an actual server
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)TCPacketType.SpawnPacket);
                packet.Write((byte)SpawnPacketType.SpawnDead);
                packet.Send();
            }
        }
        public static bool AreAllPlayersDead()
        {
            foreach (Player player in Main.player)
            {
                if (!player.active) continue;
                if (!player.dead) return true;
            }
            return false;
        }
    }
}