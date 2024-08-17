using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        int[] Mummies = { NPCID.Mummy, NPCID.DarkMummy, NPCID.BloodMummy, NPCID.LightMummy };
        public bool DrawMummy(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];

            spriteBatch.Draw(t.Value, npc.Center - screenPos + new Vector2(0, npc.height/2 + 5), new Rectangle(npc.frame.X, npc.frame.Y, npc.frame.Width, npc.frame.Height), drawColor, npc.rotation, new Vector2(t.Width()/2, t.Height() / Main.npcFrameCount[npc.type]), new Vector2(npc.scale * 1.1f, npc.scale + npc.localAI[0]), npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

            return false;
        }
        public void MummyFrame(NPC npc)
        {
        }
        public void MummyAI(NPC npc, Player target)
        {
            int walkTime = 300;
            int jumpTime = 100;

            int stretchUpTime = 10;
            int squishUpTime = 40;
            int stretchDownTime = 10;
            int squishDownTime = 40;

            npc.ai[2]++;
            npc.localAI[0] = 0;


            if (npc.ai[2] >= walkTime && npc.ai[3] == 0)
            {
                npc.ai[2] = 0;
                npc.ai[3] = 1;
            }
            if (npc.ai[2] >= jumpTime && npc.ai[3] == 1)
            {
                npc.ai[2] = 0;
                npc.ai[3] = 0;
            }
            if (npc.ai[3] == 1)
            {
                npc.velocity.X = 0;
                if (npc.ai[2] == 1)
                {
                    npc.velocity.Y = -10;
                }
                Main.NewText(npc.velocity.Y);
                npc.localAI[0] = MathHelper.Lerp(-0.3f, 0.3f, Math.Abs(npc.velocity.Y) / 10f);
                //if (npc.ai[2] < stretchUpTime)
                //{
                //    float x = npc.ai[2] / stretchUpTime;
                //    float lerper = 1 - (float)Math.Cos((x * Math.PI) / 2);
                //    npc.localAI[0] = npc.scale + MathHelper.Lerp(0, 0.3f, lerper);
                //}
                //else if (npc.ai[2] < squishUpTime + stretchUpTime)
                //{
                //    float x = (npc.ai[2] - stretchUpTime) / squishUpTime;
                //    float lerper = (float)Math.Sin((x * Math.PI) / 2);
                //    npc.localAI[0] = npc.scale + MathHelper.Lerp(0.3f, -0.3f, lerper);
                //}
                //else if (npc.ai[2] < squishUpTime + stretchUpTime + stretchDownTime)
                //{
                //    float x = (npc.ai[2] - stretchUpTime - squishUpTime) / stretchDownTime;
                //    float lerper = 1 - (float)Math.Cos((x * Math.PI) / 2);
                //    npc.localAI[0] = npc.scale + MathHelper.Lerp(-0.3f, 0.3f, lerper);
                //}
                //else if (npc.ai[2] < squishUpTime + stretchUpTime + stretchDownTime + squishDownTime)
                //{
                //    float x = (npc.ai[2] - stretchUpTime - squishUpTime - stretchDownTime) / squishDownTime;
                //    float lerper = (float)Math.Sin((x * Math.PI) / 2);
                //    npc.localAI[0] = npc.scale + MathHelper.Lerp(0.3f, -0.3f, lerper);
                //}
            }
        }
    }
}
