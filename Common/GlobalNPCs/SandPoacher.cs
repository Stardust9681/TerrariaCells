using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fighters
    {
        public bool DrawSandPoacher(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.ai[3] == 1)
            {
                int timeDigging = 100;
                float x = npc.ai[2] / (timeDigging / 2f);
                float lerp = (float)Math.Sin((x * Math.PI) / 2);
                if (npc.ai[2] > timeDigging / 2)
                {
                    x = (npc.ai[2] - timeDigging / 2) / (timeDigging / 2f);
                    lerp = 1 - (float)Math.Sin((x * Math.PI) / 2);
                }

                Asset<Texture2D> poacher = TextureAssets.Npc[NPCID.DesertScorpionWall];
                Main.instance.LoadNPC(NPCID.DesertScorpionWall);
                spriteBatch.Draw(poacher.Value, npc.Center - screenPos + new Vector2(0, MathHelper.Lerp(0, 30, lerp)), new Rectangle(0, CustomFrameY, poacher.Width(), poacher.Height() / 4), drawColor * npc.Opacity, npc.rotation, new Vector2(poacher.Width() / 2, poacher.Height() / 4 / 2), npc.scale, SpriteEffects.None, 1);
                return false;
            }
            return true;
        }
        public void SandPoacherFrame(NPC npc)
        {
            if (npc.ai[3] == 1)
            {
                CustomFrameCounter++;
                if (CustomFrameCounter >= 5)
                {
                    CustomFrameY += 80;
                    if (CustomFrameY > 80 * 3) CustomFrameY = 0;
                    CustomFrameCounter = 0;
                }
            }
        }
        public void SandPoacherAI(NPC npc, Player target)
        {
            int timeWalking = 200;
            int timeDigging = 100;
            npc.ai[2]++;
            //during dig
            if (npc.ai[3] == 1)
            {
                //dont walk
                ShouldWalk = false;

                //change opacity over time
                float x = npc.ai[2] / (timeDigging / 2f);
                float dx = (npc.ai[2] - timeDigging / 2) / (timeDigging / 2f);
                npc.velocity = Vector2.Zero;
                npc.Opacity = npc.ai[2] < timeDigging / 2 ? MathHelper.Lerp(1, 0, x) : MathHelper.Lerp(0, 1, dx);
                //rotate down during dig, up during undig
                npc.rotation = npc.ai[2] < timeDigging / 2 ? MathHelper.Pi : 0;

                //teleport try to find ground
                if (npc.ai[2] == timeDigging / 2)
                {
                    Vector2 position = target.Center;
                    float randomChange = Main.rand.NextFloat(30, 100);
                    position.X += -150 * target.direction;
                    int attempts = 100;
                    while (!WorldGen.SolidTile2(Main.tile[(position + new Vector2(0, 1)).ToTileCoordinates()]) && attempts > 0)
                    {
                        attempts--;
                        position.Y++;
                    }
                    npc.position = position + new Vector2(0, -npc.height);
                }
                Dust.NewDustDirect(npc.BottomLeft, npc.width, 0, DustID.Sand, 0, -4);
            }

            //start dig
            if (npc.ai[2] >= timeWalking && npc.ai[3] == 0 && npc.collideY && target != null)
            {
                npc.hide = true;
                npc.ai[2] = 0;
                npc.ai[3] = 1;
            }
            //end dig
            else if (npc.ai[2] >= timeDigging && npc.ai[3] == 1)
            {
                npc.hide = false;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
            }
        }
    }
}
