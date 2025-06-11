using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    public partial class Fighters
    {
        const float bloodCrawlerAttackMaxBlockDistanceX = 20f;
        const float bloodCrawlerAttackMaxBlockDistanceY = 4f;
        const int bloodCrawlerChargeUpTime = 70;
        const int bloodCrawlerDelayBetweenAttacks = 30;

        int[] BloodCrawlers { get; set; } = { NPCID.BloodCrawler, NPCID.BloodCrawlerWall };

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
    }
}
