using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader.UI;
using Terraria.ModLoader.UI.Elements;
using Terraria.ID;
using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader.IO;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Packets;

namespace TerrariaCells.Common.ModPlayers
{
    public class RewardPlayer : ModPlayer
    {
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life < 1 && target.lifeMax > 5 && !target.friendly && !NPCID.Sets.ProjectileNPC[target.type])
            {
                RewardTrackerSystem.killCount++;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    ModPacket packet = ModNetHandler.GetPacket(Mod, TCPacketType.TrackerPacket);
                    packet.Write((byte)TrackerPacketHandler.ClientNetMsg.NewKills);
                    packet.Send();
                }
            }
        }
        public override void OnEnterWorld()
        {
            RewardTrackerSystem.UpdateTracker_EnterNewWorld();
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModNetHandler.GetPacket(Mod, TCPacketType.TrackerPacket);
                packet.Write((byte)TrackerPacketHandler.ClientNetMsg.RequestSync);
                packet.Send();
            }
        }
    }
}