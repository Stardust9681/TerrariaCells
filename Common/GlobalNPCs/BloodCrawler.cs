using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fighters
    {
        const float bloodCrawlerAttackMaxBlockDistanceX = 20f;
        const float bloodCrawlerAttackMaxBlockDistanceY = 4f;
        const int bloodCrawlerChargeUpTime = 70;
        const int bloodCrawlerDelayBetweenAttacks = 30;

        int[] BloodCrawlers = { NPCID.BloodCrawler, NPCID.BloodCrawlerWall };

        public bool DrawBloodCrawler(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ExtraAI[0] > 0)
            {
                Asset<Texture2D> pounce = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/BloodcrawlerPounce");
                spriteBatch.Draw(
                    pounce.Value,
                    npc.position - screenPos,
                    new Rectangle(0, (int)CustomFrameY * 40 + 2, 66, 38),
                    drawColor,
                    npc.rotation,
                    new Vector2(5, 16),
                    Vector2.One,
                    npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0);

                return false;
            }

            return true;
        }

        public void BloodCrawlerFrame(NPC npc)
        {
            if (ExtraAI[0] > 0)
            {
                CustomFrameCounter++;
                CustomFrameY = Math.Min(CustomFrameCounter / 8, 5);
                if (CustomFrameCounter > bloodCrawlerChargeUpTime)
                {
                    CustomFrameY = CustomFrameCounter % 8 <= 1 ? 6 : 7;
                }
            }
            else
            {
                CustomFrameY = -1;
                CustomFrameCounter = 0;
            }
        }

        public void BloodCrawlerAI(NPC npc, Player target)
        {

            if (target == null)
            {
                if (ExtraAI[0] <= 0)
                {
                    BloodCrawlerWalkingAI();
                }
                else
                {
                    BloodCrawlerAttackingAI();
                }
                return;
            }

            Vector2 npcToTarget = (target.Center - npc.Center) / 16f;

            float xBlockDistance = Math.Abs(npcToTarget.X);
            float yBlockDistance = Math.Abs(npcToTarget.Y);
            float heightDifference = target.position.Y + target.height - npc.position.Y - npc.height;

            if (ExtraAI[0] < 0) //wait for delay before next attack
            {
                ExtraAI[0]++;
            }

            if (ExtraAI[0] > 0 || ( //already started charging
                    xBlockDistance < bloodCrawlerAttackMaxBlockDistanceX && //close enough horizontally
                    yBlockDistance < bloodCrawlerAttackMaxBlockDistanceY && //close enough vertically
                    heightDifference > -1f && //target not below
                    npc.collideY && //not falling
                    ExtraAI[0] >= 0)) //not waiting for delay
            {
                BloodCrawlerAttackingAI();
            }
            else
            {
                BloodCrawlerWalkingAI();
            }

            void BloodCrawlerWalkingAI()
            {
                ExtraAI[0] = Math.Max(0, ExtraAI[0]);

                npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;

                if (npc.NPCCanStickToWalls())
                {
                    npc.Transform(NPCID.BloodCrawlerWall);
                }
                return;
            }
            void BloodCrawlerAttackingAI()
            {
                ExtraAI[0]++;

                if (ExtraAI[0] < bloodCrawlerChargeUpTime)
                {
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
                    npc.velocity = Vector2.Zero;
                    ShouldWalk = false;
                }
                else if (ExtraAI[0] == bloodCrawlerChargeUpTime)
                {
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = true;
                    npc.velocity = new Vector2(npc.direction * 10, -5);
                }
                else if (ExtraAI[0] > bloodCrawlerChargeUpTime + 10 && npc.collideY)
                {
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
                    ExtraAI[0] = -bloodCrawlerDelayBetweenAttacks;
                }
            }

        }

        public void BloodCrawlerWallAI(NPC npc)
        {
            const float bloodCrawlerSpeedFactor = 1.5f;

            //make sure npc is real
            if (npc == null || !npc.active) return;

            npc.GetGlobalNPC<CombatNPC>().allowContactDamage = true;

            npc.oldVelocity /= bloodCrawlerSpeedFactor;
            npc.velocity /= bloodCrawlerSpeedFactor;

            VanillaBloodCrawlerWallAI(npc);

            npc.oldVelocity *= bloodCrawlerSpeedFactor;
            npc.velocity *= bloodCrawlerSpeedFactor;
        }

        //copied and adjusted from terraria source code
        void VanillaBloodCrawlerWallAI(NPC npc)
        {
            if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
            {
                npc.TargetClosest();
            }
            Vector2 npcCenter = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
            Vector2 npcToTarget = new Vector2(
                Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2),
                Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2)
                );
            npcToTarget.X = (int)(npcToTarget.X / 8f) * 8;
            npcToTarget.Y = (int)(npcToTarget.Y / 8f) * 8;
            npcCenter.X = (int)(npcCenter.X / 8f) * 8;
            npcCenter.Y = (int)(npcCenter.Y / 8f) * 8;
            npcToTarget.X -= npcCenter.X;
            npcToTarget.Y -= npcCenter.Y;
            if (npc.confused)
            {
                npcToTarget.X *= -2f;
                npcToTarget.Y *= -2f;
            }
            float npcToTargetLength = (float)Math.Sqrt(npcToTarget.X * npcToTarget.X + npcToTarget.Y * npcToTarget.Y);
            if (npcToTargetLength == 0f)
            {
                npcToTarget.X = npc.velocity.X;
                npcToTarget.Y = npc.velocity.Y;
            }
            else
            {
                npcToTarget *= 2f / npcToTargetLength;
            }
            if (Main.player[npc.target].dead)
            {
                npcToTarget.X = (float)npc.direction;
                npcToTarget.Y = -1f;
            }
            npc.spriteDirection = -1;
            if (!Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height))
            {
                npc.ai[0] += 1f;
                if (npc.ai[0] > 0f)
                {
                    npc.velocity.Y += 0.023f;
                }
                else
                {
                    npc.velocity.Y -= 0.023f;
                }
                if (npc.ai[0] < -100f || npc.ai[0] > 100f)
                {
                    npc.velocity.X += 0.023f;
                }
                else
                {
                    npc.velocity.X -= 0.023f;
                }
                if (npc.ai[0] > 200f)
                {
                    npc.ai[0] = -200f;
                }
                npc.velocity += npcToTarget * 0.007f;
                npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X);

                if (npc.velocity.X > 1.5 || npc.velocity.X < -1.5)
                {
                    npc.velocity.X *= 0.9f;
                }
                if (npc.velocity.Y > 1.5 || npc.velocity.Y < -1.5)
                {
                    npc.velocity.Y *= 0.9f;
                }

                if (npc.velocity.X > 3f || npc.velocity.X < -3f)
                {
                    npc.velocity.X = 3f;
                }
                if (npc.velocity.Y > 3f || npc.velocity.Y < -3f)
                {
                    npc.velocity.Y = 3f;
                }
            }
            else
            {
                if (npc.velocity.X < npcToTarget.X)
                {
                    npc.velocity.X += 0.08f;
                    if (npc.velocity.X < 0f && npcToTarget.X > 0f)
                    {
                        npc.velocity.X += 0.08f;
                    }
                }
                else if (npc.velocity.X > npcToTarget.X)
                {
                    npc.velocity.X -= 0.08f;
                    if (npc.velocity.X > 0f && npcToTarget.X < 0f)
                    {
                        npc.velocity.X -= 0.08f;
                    }
                }
                if (npc.velocity.Y < npcToTarget.Y)
                {
                    npc.velocity.Y += 0.08f;
                    if (npc.velocity.Y < 0f && npcToTarget.Y > 0f)
                    {
                        npc.velocity.Y += 0.08f;
                    }
                }
                else if (npc.velocity.Y > npcToTarget.Y)
                {
                    npc.velocity.Y -= 0.08f;
                    if (npc.velocity.Y > 0f && npcToTarget.Y < 0f)
                    {
                        npc.velocity.Y -= 0.08f;
                    }
                }
                npc.rotation = (float)Math.Atan2(npcToTarget.Y, npcToTarget.X);
            }
            if (npc.collideX)
            {
                npc.netUpdate = true;
                npc.velocity.X = -0.5f * npc.oldVelocity.X;
                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                {
                    npc.velocity.X = 2f;
                }
                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                {
                    npc.velocity.X = -2f;
                }
            }
            if (npc.collideY)
            {
                npc.netUpdate = true;
                npc.velocity.Y = -0.5f * npc.oldVelocity.Y;
                if (npc.velocity.Y > 0f && (double)npc.velocity.Y < 1.5)
                {
                    npc.velocity.Y = 2f;
                }
                if (npc.velocity.Y < 0f && (double)npc.velocity.Y > -1.5)
                {
                    npc.velocity.Y = -2f;
                }
            }

            if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
            {
                npc.netUpdate = true;
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                return;
            }
            if (!npc.NPCCanStickToWalls())
            {
                npc.Transform(239);
            }
        }
    }
}
