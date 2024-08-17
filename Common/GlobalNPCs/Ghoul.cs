using Microsoft.Xna.Framework;
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

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fighters
    {
        int[] Ghouls = { NPCID.DesertGhoul, NPCID.DesertGhoulCorruption, NPCID.DesertGhoulCrimson, NPCID.DesertGhoulHallow };
        public bool DrawGhoul(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            int max = (int)npc.localAI[1];

            if (max > npc.oldPos.Length) max = npc.oldPos.Length;
            for (int i = 0; i < max; i++)
            {
                spriteBatch.Draw(t.Value, npc.oldPos[i] + npc.Size/2 - screenPos, new Rectangle(npc.frame.X, npc.frame.Y, npc.frame.Width, npc.frame.Height), new Color(200, 100, 100) * MathHelper.Lerp(0.7f, 0f, (float)i / max), npc.rotation, new Vector2(t.Width() / 2, 25), npc.scale, npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 1);
            }

            return true;
        }
        public void GhoulFrame(NPC npc)
        {
            if (npc.ai[3] == 1)
            {
                
            }
        }
        public void GhoulAI(NPC npc, Player target)
        {
            int timeWalking = 200;
            int timeDashing = 30;
            npc.ai[2]++;

            //increase/decrease afterimage smoothly
            if (npc.localAI[1] < npc.localAI[0])
            {
                npc.localAI[1]++;
            }else if (npc.localAI[1] > npc.localAI[0])
            {
                npc.localAI[1]--;
            }

            //dont walk during dash, cancel dash if ram into wall
            if (npc.ai[3] == 1)
            {
                ShouldWalk = false;
                if (npc.collideX)
                {
                    npc.ai[2] = 0;
                    npc.ai[3] = 0;
                    npc.localAI[0] = 0;
                }   
            }

            //telegraph
            if (npc.ai[2] == timeWalking - 40 && target != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust d = Dust.NewDustDirect(npc.Center + new Vector2(5 * npc.direction, -10), 0, 0, DustID.SilverCoin, 0, -5);
                    d.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.NPCHit37, npc.Center);
            }

            //start dash
            if (npc.ai[2] >= timeWalking && npc.ai[3] == 0 && target != null)
            {
                int dir = target.Center.X > npc.Center.X ? 1 : -1;
                npc.velocity = new Vector2(15 * dir, -5);

                npc.ai[2] = 0;
                npc.ai[3] = 1;
                npc.localAI[0] = 10;
                SoundEngine.PlaySound(SoundID.NPCDeath40, npc.Center);
            }

            //end dash
            else if (npc.ai[2] >= timeDashing && npc.ai[3] == 1)
            {
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                npc.localAI[0] = 0;
            }
        }
    }
}
