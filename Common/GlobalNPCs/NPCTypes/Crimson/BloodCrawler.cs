using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    public partial class Fighters
    {
        private static Asset<Texture2D> BloodCrawler_Pounce;
        const float bloodCrawlerAttackMaxBlockDistanceX = 20f;
        const float bloodCrawlerAttackMaxBlockDistanceY = 4f;
        const int bloodCrawlerChargeUpTime = 60;
        const int bloodCrawlerDelayBetweenAttacks = 30;

        private static int[] BloodCrawlers { get; set; } = { NPCID.BloodCrawler, NPCID.BloodCrawlerWall };

        public bool DrawBloodCrawler(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (ExtraAI[0] > 0)
            {
                spriteBatch.Draw(
                    BloodCrawler_Pounce.Value,
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
            /*if (ExtraAI[1] == 1 && npc.NPCCanStickToWalls())
            {
                npc.Transform(NPCID.BloodCrawlerWall);
                ExtraAI[1] = 0;
                return;
            }*/
            ExtraAI[1] = 0;

            if (target == null)
            {
                ShouldWalk = false;
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

            ShouldWalk = xBlockDistance < 20;

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

                return;
            }
            void BloodCrawlerAttackingAI()
            {
                ExtraAI[0]++;

                if (ExtraAI[0] == 1)
                {
                    if (npc.TryGetTarget(out Entity target))
                    {
                        npc.ai[2] = MathF.Sign(target.position.X - npc.position.X);
                    }
                    else if (MathF.Abs(npc.ai[2]) != 1)
                    {
                        npc.ai[2] = npc.spriteDirection;
                    }
                }

                if (ExtraAI[0] < bloodCrawlerChargeUpTime)
                {
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
                    npc.velocity = Vector2.Zero;
                    ShouldWalk = false;
                    npc.direction = MathF.Sign(npc.ai[2]);
                    npc.spriteDirection = npc.direction;
                }
                else if (ExtraAI[0] == bloodCrawlerChargeUpTime)
                {
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = true;
                    npc.velocity = new Vector2(npc.ai[2] * 10, -5);
                    npc.direction = MathF.Sign(npc.ai[2]);
                    npc.spriteDirection = npc.direction;
                }
                else if (ExtraAI[0] > bloodCrawlerChargeUpTime + 10 && npc.collideY)
                {
                    npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
                    ExtraAI[0] = -bloodCrawlerDelayBetweenAttacks;
                }
            }
        }

        void BloodCrawler_OnHit(NPC npc, Player player, NPC.HitInfo hit, int dmg)
        {
            if (hit.DamageType.CountsAsClass(DamageClass.Melee))
            {
                if (ExtraAI[0] > 0 && ExtraAI[0] < bloodCrawlerChargeUpTime)
                {
                    ExtraAI[0] = Math.Max(ExtraAI[0] - 5, 0);
                }
            }
        }
    }
}