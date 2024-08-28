using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fliers
    {

        public bool DrawVulture(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            return true;
        }
        public void VultureFrame(NPC npc)
        {

        }
        
        public void VultureAI(NPC npc)
        {
            Player target = null;
            npc.ai[0] = 1;
            npc.noGravity = true;

            Vector2 targetPos = npc.Center;
            if (npc.HasValidTarget)
            {
               
                target = Main.player[npc.target];
                targetPos = target.Center + new Vector2(0, -200);
            }

            if (npc.Center.Y > targetPos.Y && npc.velocity.Y > -5)
            {
                npc.velocity.Y -= 0.1f;
                if (npc.velocity.Y > 2)
                {
                    npc.velocity.Y -= 0.1f;
                }
            }

            if (npc.Center.Y < targetPos.Y && npc.velocity.Y < 5)
            {
                npc.velocity.Y += 0.1f;
                if (npc.velocity.Y < -2)
                {
                    npc.velocity.Y += 0.1f;
                }
            }

            if ((npc.Center.X > targetPos.X && npc.velocity.X > -4 && Math.Abs(npc.Center.X - targetPos.X) > 100) || (npc.velocity.X < 0 && npc.velocity.X > -4 && npc.Center.X > targetPos.X))
            {
                npc.velocity.X -= 0.2f;
                
            }

            
            if ((npc.Center.X < targetPos.X && npc.velocity.X < 4 && Math.Abs(npc.Center.X - targetPos.X) > 100) || (npc.velocity.X > 0 && npc.velocity.X < 4 && npc.Center.X < targetPos.X))
            {
                npc.velocity.X += 0.2f;
                
            }


        }
    }
}
