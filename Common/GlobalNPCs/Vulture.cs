using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

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

            



            //movement
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

            //projectile
            if (npc.HasValidTarget && npc.Distance(target.Center) < 500 && Collision.CanHitLine(npc.Center, 1, 1, target.Center, 1, 1))
            {
                npc.ai[2]++;
            }
            if (npc.ai[2] >= 120 && npc.HasValidTarget)
            {
                Vector2 pos = npc.Center + new Vector2(5 * npc.direction, -20);
                Vector2 vec = (target.Center - pos).SafeNormalize(Vector2.Zero) ;
                Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), pos, vec * 5 + target.velocity * 0.2f, ModContent.ProjectileType<VultureBone>(), TCellsUtils.ScaledHostileDamage(20), 1);
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDustDirect(pos, 0, 0, DustID.Bone, vec.X*2, vec.Y*2).noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.NPCDeath9, npc.Center);
                npc.velocity += -vec * 3;
                npc.ai[2] = 0;
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
