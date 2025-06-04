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
using TerrariaCells.Content.Packets;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.ModPlayers
{
	public class ChestPlayer : ModPlayer
    {
        private int lastChestState = -1;
        readonly ChestLootSpawner spawner = ModContent.GetInstance<ChestLootSpawner>();

        public override void OnEnterWorld()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            ModPacket p = Mod.GetPacket();
            p.Write((byte)TCPacketType.ChestPacket);
            p.Write((byte)ChestPacketType.ServerJoin);
            p.Write(Player.whoAmI);
            p.Send();
        }
        public override void PostUpdate()
        {
            // We have to relegate to checking every tick because On_Player.OpenChest doesn't work on clients
            if (Player.chest != -1)
            {
                lastChestState = Player.chest;
                Player.chest = -1;
                if(Main.netMode == NetmodeID.MultiplayerClient)
                    ClientOpenChest(lastChestState);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) return;
            foreach (int chest in spawner.lootedChests)
            {
                if (Main.chest[chest] == null)
                {
                    continue;
                }
                Main.chest[chest].frame = 2;
                Main.chest[chest].frameCounter = 10;
            }
        }

        public void ClientOpenChest(int chest)
        {
            // Ask server to open the chest
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)TCPacketType.ChestPacket);
            packet.Write((byte)ChestPacketType.ServerOpenChest);
            packet.Write(chest);
            packet.Send();
        }
    }
}