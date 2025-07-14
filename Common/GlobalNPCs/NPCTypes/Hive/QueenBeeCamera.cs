using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Hive
{
    public class QueenBeeCamera : ModPlayer
    {
        
        public bool NearQueenBee = false;
        public override void PreUpdate()
        {
            if (NPC.AnyNPCs(NPCID.QueenBee))
            {
                if (Main.npc[NPC.FindFirstNPC(NPCID.QueenBee)].Distance(Player.Center) < 800)
                {
                    NearQueenBee = true;
                }
            }
            if (NearQueenBee)
            {
                Systems.CameraManipulation.SetCamera(45, QueenBee.SpawnPosition - Main.ScreenSize.ToVector2()/2);
                Systems.CameraManipulation.SetZoom(45, new Vector2(95, 55) * 12);
                if ((Player.Center.X + Player.velocity.X < QueenBee.SpawnPosition.X - 613 && Player.velocity.X < 0) || (Player.Center.X + Player.velocity.X > QueenBee.SpawnPosition.X + 613 && Player.velocity.X > 0))
                {
                    Player.velocity.X = 0;
                    Player.position.X = Player.oldPosition.X;
                }
            }
            if (NPC.downedQueenBee)
            {
                NearQueenBee = false;
            }
            
            base.PostUpdate();
        }
    }
}
