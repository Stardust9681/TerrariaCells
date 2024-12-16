using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Casters : GlobalNPC
    {
        public bool DesertSpiritDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            for (int i = 0; i < npc.oldPos.Length; i++)
            {
                spriteBatch.Draw(t.Value, npc.oldPos[i] - screenPos + new Vector2(npc.width, npc.height)/2, new Rectangle(0, npc.frame.Y, npc.frame.Width, npc.frame.Height), drawColor * 0.5f * (1 - (float)i / npc.oldPos.Length), npc.rotation, new Vector2(npc.frame.Width, npc.frame.Height) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
            spriteBatch.Draw(t.Value, npc.position - screenPos + new Vector2(npc.width, npc.height) / 2, new Rectangle(0, CustomFrameY, npc.frame.Width, npc.frame.Height), drawColor * 0.9f, npc.rotation, new Vector2(npc.frame.Width, npc.frame.Height) / 2, npc.scale, npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            return false;
        }
        public void DesertSpiritFrame(NPC npc)
        {
            int min = npc.ai[3] > 60 && npc.ai[3] < 220 ? 8 : 0;
            CustomFrameCounter++;
            if (CustomFrameCounter >= 5)
            {
                CustomFrameCounter = 0;
                CustomFrameY += 64;
                if (CustomFrameY >= 64 * (min + 8))
                {
                    
                    CustomFrameY = min * 64;
                }
            }
        }
        public bool DesertSpiritAI(NPC npc, Player target)
        {
            int timeRotating = 300;
            if (npc.HasValidTarget)
            {
                npc.direction = npc.Center.X > target.Center.X ? -1 : 1;
                npc.spriteDirection = npc.direction;
            }
            if (npc.ai[3] <= 0)
            {
                Vector2 tpos = npc.Center;
                if (npc.HasValidTarget)
                {
                    tpos = target.Center;

                }

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame);
                }

                Teleport(npc, tpos, 200);
                SoundEngine.PlaySound(SoundID.Item8, npc.Center);
                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Shadowflame);
                }
                Vector2 rotpos = npc.Center - new Vector2(0, 15);
                npc.ai[1] = rotpos.X;
                npc.ai[2] = rotpos.Y;
                npc.ai[3] = timeRotating;
                
                 
            }

            if (npc.ai[3] > 100 && npc.ai[3] % 10 == 0 && npc.HasValidTarget && npc.ai[3] < timeRotating - 100)
            {
                Projectile.NewProjectileDirect(npc.GetSource_FromAI(), target.Center + new Vector2(Main.rand.Next(20, 100), 0).RotatedByRandom(MathHelper.TwoPi), Vector2.Zero, ProjectileID.DesertDjinnCurse, 0, 1, -1, npc.whoAmI, target.whoAmI);
            }
            npc.Center = new Vector2(npc.ai[1], npc.ai[2]) + new Vector2((float)Math.Sin(MathHelper.ToRadians(npc.ai[3] * 2)) * 50, (float)Math.Sin(MathHelper.ToRadians(npc.ai[3]*5))*10);
           
            npc.ai[3]--;
            

            return false;
        }
        
    }
}
