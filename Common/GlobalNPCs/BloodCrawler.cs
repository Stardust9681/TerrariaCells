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
        const int bloodCrawlerChargeUpTime = 120;
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
                    new Rectangle(0, (int)CustomFrameY * 40, 66, 38),
                    drawColor,
                    npc.rotation,
                    Vector2.Zero,
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
                CustomFrameY = Math.Min(CustomFrameCounter / 4, 5);
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
                    Main.NewText("end attack");
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


        //private void VanillaBloodCrawlerAI()
        //{
        //    if (Main.player[target].position.Y + (float)Main.player[target].height == position.Y + (float)height)
        //    {
        //        directionY = -1;
        //    }
        //    bool flag5 = false;
        //    bool flag6 = false;
        //    if (velocity.X == 0f)
        //    {
        //        flag6 = true;
        //    }
        //    if (justHit)
        //    {
        //        flag6 = false;
        //    }
        //    int num56 = 60;
        //    bool flag7 = false;
        //    bool flag8 = true;
        //    flag8 = false;
        //    int num64 = type;
        //    if (velocity.Y == 0f && ((velocity.X > 0f && direction < 0) || (velocity.X < 0f && direction > 0)))
        //    {
        //        flag7 = true;
        //    }
        //    if (position.X == oldPosition.X || ai[3] >= (float)num56 || flag7)
        //    {
        //        ai[3] += 1f;
        //    }
        //    else if ((double)Math.Abs(velocity.X) > 0.9 && ai[3] > 0f)
        //    {
        //        ai[3] -= 1f;
        //    }
        //    if (ai[3] > (float)(num56 * 10))
        //    {
        //        ai[3] = 0f;
        //    }
        //    if (justHit)
        //    {
        //        ai[3] = 0f;
        //    }
        //    if (ai[3] == (float)num56)
        //    {
        //        netUpdate = true;
        //    }
        //    if (Main.player[target].Hitbox.Intersects(base.Hitbox))
        //    {
        //        ai[3] = 0f;
        //    }

        //    if (ai[3] < (float)num56 && DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(type, position, this))
        //    {
        //        if (shimmerTransparency < 1f)
        //        {
        //            if ((type == 3 || type == 591 || type == 590 || type == 331 || type == 332 || type == 21 || (type >= 449 && type <= 452) || type == 31 || type == 294 || type == 295 || type == 296 || type == 77 || type == 110 || type == 132 || type == 167 || type == 161 || type == 162 || type == 186 || type == 187 || type == 188 || type == 189 || type == 197 || type == 200 || type == 201 || type == 202 || type == 203 || type == 223 || type == 291 || type == 292 || type == 293 || type == 320 || type == 321 || type == 319 || type == 481 || type == 632 || type == 635) && Main.rand.Next(1000) == 0)
        //            {
        //                SoundEngine.PlaySound(14, (int)position.X, (int)position.Y);
        //            }
        //            if ((type == 489 || type == 586) && Main.rand.Next(800) == 0)
        //            {
        //                SoundEngine.PlaySound(14, (int)position.X, (int)position.Y, type);
        //            }
        //            if ((type == 78 || type == 79 || type == 80 || type == 630) && Main.rand.Next(500) == 0)
        //            {
        //                SoundEngine.PlaySound(26, (int)position.X, (int)position.Y);
        //            }
        //            if (type == 159 && Main.rand.Next(500) == 0)
        //            {
        //                SoundEngine.PlaySound(29, (int)position.X, (int)position.Y, 7);
        //            }
        //            if (type == 162 && Main.rand.Next(500) == 0)
        //            {
        //                SoundEngine.PlaySound(29, (int)position.X, (int)position.Y, 6);
        //            }
        //            if (type == 181 && Main.rand.Next(500) == 0)
        //            {
        //                SoundEngine.PlaySound(29, (int)position.X, (int)position.Y, 8);
        //            }
        //            if (type >= 269 && type <= 280 && Main.rand.Next(1000) == 0)
        //            {
        //                SoundEngine.PlaySound(14, (int)position.X, (int)position.Y);
        //            }
        //        }
        //        TargetClosest();
        //        if (directionY > 0 && Main.player[target].Center.Y <= base.Bottom.Y)
        //        {
        //            directionY = -1;
        //        }
        //    }
        //    else if (!(ai[2] > 0f) || !DespawnEncouragement_AIStyle3_Fighters_CanBeBusyWithAction(type))
        //    {
        //        if (Main.IsItDay() && (double)(position.Y / 16f) < Main.worldSurface && type != 624 && type != 631)
        //        {
        //            EncourageDespawn(10);
        //        }
        //        if (velocity.X == 0f)
        //        {
        //            if (velocity.Y == 0f)
        //            {
        //                ai[0] += 1f;
        //                if (ai[0] >= 2f)
        //                {
        //                    direction *= -1;
        //                    spriteDirection = direction;
        //                    ai[0] = 0f;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ai[0] = 0f;
        //        }
        //        if (direction == 0)
        //        {
        //            direction = 1;
        //        }
        //    }

        //    float num80 = 1.5f;
        //    if (velocity.X < 0f - num80 || velocity.X > num80)
        //    {
        //        if (velocity.Y == 0f)
        //        {
        //            velocity *= 0.8f;
        //        }
        //    }
        //    else if (velocity.X < num80 && direction == 1)
        //    {
        //        if (type == 466 && velocity.X < -2f)
        //        {
        //            velocity.X *= 0.9f;
        //        }
        //        if (type == 586 && velocity.Y == 0f && velocity.X < -1f)
        //        {
        //            velocity.X *= 0.9f;
        //        }
        //        velocity.X += 0.07f;
        //        if (velocity.X > num80)
        //        {
        //            velocity.X = num80;
        //        }
        //    }
        //    else if (velocity.X > 0f - num80 && direction == -1)
        //    {
        //        if (type == 466 && velocity.X > 2f)
        //        {
        //            velocity.X *= 0.9f;
        //        }
        //        if (type == 586 && velocity.Y == 0f && velocity.X > 1f)
        //        {
        //            velocity.X *= 0.9f;
        //        }
        //        velocity.X -= 0.07f;
        //        if (velocity.X < 0f - num80)
        //        {
        //            velocity.X = 0f - num80;
        //        }
        //    }

        //    if (Main.netMode != 1)
        //    {
        //        if (Main.expertMode && target >= 0 && (type == 163 || type == 238 || type == 236 || type == 237) && Collision.CanHit(base.Center, 1, 1, Main.player[target].Center, 1, 1))
        //        {
        //            localAI[0] += 1f;
        //            if (justHit)
        //            {
        //                localAI[0] -= Main.rand.Next(20, 60);
        //                if (localAI[0] < 0f)
        //                {
        //                    localAI[0] = 0f;
        //                }
        //            }
        //            if (localAI[0] > (float)Main.rand.Next(180, 900))
        //            {
        //                localAI[0] = 0f;
        //                Vector2 vector32 = Main.player[target].Center - base.Center;
        //                vector32.Normalize();
        //                vector32 *= 8f;
        //                int attackDamage_ForProjectiles2 = GetAttackDamage_ForProjectiles(18f, 18f);
        //                Projectile.NewProjectile(GetSpawnSource_ForProjectile(), base.Center.X, base.Center.Y, vector32.X, vector32.Y, 472, attackDamage_ForProjectiles2, 0f, Main.myPlayer);
        //            }
        //        }
        //        if (velocity.Y == 0f)
        //        {
        //            int num131 = -1;
        //            switch (type)
        //            {
        //                case 164:
        //                    num131 = 165;
        //                    break;
        //                case 236:
        //                    num131 = 237;
        //                    break;
        //                case 163:
        //                    num131 = 238;
        //                    break;
        //                case 239:
        //                    num131 = 240;
        //                    break;
        //                case 530:
        //                    num131 = 531;
        //                    break;
        //            }
        //            if (num131 != -1 && NPCCanStickToWalls())
        //            {
        //                Transform(num131);
        //            }
        //        }
        //    }
        //    if (velocity.Y == 0f)
        //    {
        //        int num181 = (int)(position.Y + (float)height + 7f) / 16;
        //        int num182 = (int)(position.Y - 9f) / 16;
        //        int num183 = (int)position.X / 16;
        //        int num184 = (int)(position.X + (float)width) / 16;
        //        int num185 = (int)(position.X + 8f) / 16;
        //        int num186 = (int)(position.X + (float)width - 8f) / 16;
        //        bool flag22 = false;
        //        for (int num187 = num185; num187 <= num186; num187++)
        //        {
        //            if (num187 >= num183 && num187 <= num184 && Main.tile[num187, num181] == null)
        //            {
        //                flag22 = true;
        //                continue;
        //            }
        //            if (Main.tile[num187, num182] != null && Main.tile[num187, num182].nactive() && Main.tileSolid[Main.tile[num187, num182].type])
        //            {
        //                flag5 = false;
        //                break;
        //            }
        //            if (!flag22 && num187 >= num183 && num187 <= num184 && Main.tile[num187, num181].nactive() && Main.tileSolid[Main.tile[num187, num181].type])
        //            {
        //                flag5 = true;
        //            }
        //        }
        //        if (!flag5 && velocity.Y < 0f)
        //        {
        //            velocity.Y = 0f;
        //        }
        //        if (flag22)
        //        {
        //            return;
        //        }
        //    }
        //    if (velocity.Y >= 0f && (type != 580 || directionY != 1))
        //    {
        //        int num188 = 0;
        //        if (velocity.X < 0f)
        //        {
        //            num188 = -1;
        //        }
        //        if (velocity.X > 0f)
        //        {
        //            num188 = 1;
        //        }
        //        Vector2 vector39 = position;
        //        vector39.X += velocity.X;
        //        int num189 = (int)((vector39.X + (float)(width / 2) + (float)((width / 2 + 1) * num188)) / 16f);
        //        int num190 = (int)((vector39.Y + (float)height - 1f) / 16f);
        //        if (WorldGen.InWorld(num189, num190, 4))
        //        {
        //            if (Main.tile[num189, num190] == null)
        //            {
        //                Main.tile[num189, num190] = new Tile();
        //            }
        //            if (Main.tile[num189, num190 - 1] == null)
        //            {
        //                Main.tile[num189, num190 - 1] = new Tile();
        //            }
        //            if (Main.tile[num189, num190 - 2] == null)
        //            {
        //                Main.tile[num189, num190 - 2] = new Tile();
        //            }
        //            if (Main.tile[num189, num190 - 3] == null)
        //            {
        //                Main.tile[num189, num190 - 3] = new Tile();
        //            }
        //            if (Main.tile[num189, num190 + 1] == null)
        //            {
        //                Main.tile[num189, num190 + 1] = new Tile();
        //            }
        //            if (Main.tile[num189 - num188, num190 - 3] == null)
        //            {
        //                Main.tile[num189 - num188, num190 - 3] = new Tile();
        //            }
        //            if ((float)(num189 * 16) < vector39.X + (float)width && (float)(num189 * 16 + 16) > vector39.X && ((Main.tile[num189, num190].nactive() && !Main.tile[num189, num190].topSlope() && !Main.tile[num189, num190 - 1].topSlope() && Main.tileSolid[Main.tile[num189, num190].type] && !Main.tileSolidTop[Main.tile[num189, num190].type]) || (Main.tile[num189, num190 - 1].halfBrick() && Main.tile[num189, num190 - 1].nactive())) && (!Main.tile[num189, num190 - 1].nactive() || !Main.tileSolid[Main.tile[num189, num190 - 1].type] || Main.tileSolidTop[Main.tile[num189, num190 - 1].type] || (Main.tile[num189, num190 - 1].halfBrick() && (!Main.tile[num189, num190 - 4].nactive() || !Main.tileSolid[Main.tile[num189, num190 - 4].type] || Main.tileSolidTop[Main.tile[num189, num190 - 4].type]))) && (!Main.tile[num189, num190 - 2].nactive() || !Main.tileSolid[Main.tile[num189, num190 - 2].type] || Main.tileSolidTop[Main.tile[num189, num190 - 2].type]) && (!Main.tile[num189, num190 - 3].nactive() || !Main.tileSolid[Main.tile[num189, num190 - 3].type] || Main.tileSolidTop[Main.tile[num189, num190 - 3].type]) && (!Main.tile[num189 - num188, num190 - 3].nactive() || !Main.tileSolid[Main.tile[num189 - num188, num190 - 3].type]))
        //            {
        //                float num191 = num190 * 16;
        //                if (Main.tile[num189, num190].halfBrick())
        //                {
        //                    num191 += 8f;
        //                }
        //                if (Main.tile[num189, num190 - 1].halfBrick())
        //                {
        //                    num191 -= 8f;
        //                }
        //                if (num191 < vector39.Y + (float)height)
        //                {
        //                    float num192 = vector39.Y + (float)height - num191;
        //                    float num193 = 16.1f;
        //                    if (type == 163 || type == 164 || type == 236 || type == 239 || type == 530)
        //                    {
        //                        num193 += 8f;
        //                    }
        //                    if (num192 <= num193)
        //                    {
        //                        gfxOffY += position.Y + (float)height - num191;
        //                        position.Y = num191 - (float)height;
        //                        if (num192 < 9f)
        //                        {
        //                            stepSpeed = 1f;
        //                        }
        //                        else
        //                        {
        //                            stepSpeed = 2f;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (flag5)
        //    {
        //        int num194 = (int)((position.X + (float)(width / 2) + (float)(15 * direction)) / 16f);
        //        int num195 = (int)((position.Y + (float)height - 15f) / 16f);
        //        if (type == 109 || type == 163 || type == 164 || type == 199 || type == 236 || type == 239 || type == 257 || type == 258 || type == 290 || type == 391 || type == 425 || type == 427 || type == 426 || type == 580 || type == 508 || type == 415 || type == 530 || type == 532 || type == 582)
        //        {
        //            num194 = (int)((position.X + (float)(width / 2) + (float)((width / 2 + 16) * direction)) / 16f);
        //        }
        //        if (Main.tile[num194, num195] == null)
        //        {
        //            Main.tile[num194, num195] = new Tile();
        //        }
        //        if (Main.tile[num194, num195 - 1] == null)
        //        {
        //            Main.tile[num194, num195 - 1] = new Tile();
        //        }
        //        if (Main.tile[num194, num195 - 2] == null)
        //        {
        //            Main.tile[num194, num195 - 2] = new Tile();
        //        }
        //        if (Main.tile[num194, num195 - 3] == null)
        //        {
        //            Main.tile[num194, num195 - 3] = new Tile();
        //        }
        //        if (Main.tile[num194, num195 + 1] == null)
        //        {
        //            Main.tile[num194, num195 + 1] = new Tile();
        //        }
        //        if (Main.tile[num194 + direction, num195 - 1] == null)
        //        {
        //            Main.tile[num194 + direction, num195 - 1] = new Tile();
        //        }
        //        if (Main.tile[num194 + direction, num195 + 1] == null)
        //        {
        //            Main.tile[num194 + direction, num195 + 1] = new Tile();
        //        }
        //        if (Main.tile[num194 - direction, num195 + 1] == null)
        //        {
        //            Main.tile[num194 - direction, num195 + 1] = new Tile();
        //        }
        //        Main.tile[num194, num195 + 1].halfBrick();
        //        if (Main.tile[num194, num195 - 1].nactive() && (Main.tile[num194, num195 - 1].type == 10 || Main.tile[num194, num195 - 1].type == 388) && flag8)
        //        {
        //            ai[2] += 1f;
        //            ai[3] = 0f;
        //            if (ai[2] >= 60f)
        //            {
        //                bool flag23 = type == 3 || type == 430 || type == 590 || type == 331 || type == 332 || type == 132 || type == 161 || type == 186 || type == 187 || type == 188 || type == 189 || type == 200 || type == 223 || type == 320 || type == 321 || type == 319 || type == 21 || type == 324 || type == 323 || type == 322 || type == 44 || type == 196 || type == 167 || type == 77 || type == 197 || type == 202 || type == 203 || type == 449 || type == 450 || type == 451 || type == 452 || type == 481 || type == 201 || type == 635;
        //                bool flag24 = Main.player[target].ZoneGraveyard && Main.rand.Next(60) == 0;
        //                if ((!Main.bloodMoon || Main.getGoodWorld) && !flag24 && flag23)
        //                {
        //                    ai[1] = 0f;
        //                }
        //                velocity.X = 0.5f * (float)(-direction);
        //                int num196 = 5;
        //                if (Main.tile[num194, num195 - 1].type == 388)
        //                {
        //                    num196 = 2;
        //                }
        //                ai[1] += num196;
        //                if (type == 27)
        //                {
        //                    ai[1] += 1f;
        //                }
        //                if (type == 31 || type == 294 || type == 295 || type == 296)
        //                {
        //                    ai[1] += 6f;
        //                }
        //                ai[2] = 0f;
        //                bool flag25 = false;
        //                if (ai[1] >= 10f)
        //                {
        //                    flag25 = true;
        //                    ai[1] = 10f;
        //                }
        //                if (type == 460)
        //                {
        //                    flag25 = true;
        //                }
        //                WorldGen.KillTile(num194, num195 - 1, fail: true);
        //                if ((Main.netMode != 1 || !flag25) && flag25 && Main.netMode != 1)
        //                {
        //                    if (type == 26)
        //                    {
        //                        WorldGen.KillTile(num194, num195 - 1);
        //                        if (Main.netMode == 2)
        //                        {
        //                            NetMessage.SendData(17, -1, -1, null, 0, num194, num195 - 1);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (Main.tile[num194, num195 - 1].type == 10)
        //                        {
        //                            bool flag26 = WorldGen.OpenDoor(num194, num195 - 1, direction);
        //                            if (!flag26)
        //                            {
        //                                ai[3] = num56;
        //                                netUpdate = true;
        //                            }
        //                            if (Main.netMode == 2 && flag26)
        //                            {
        //                                NetMessage.SendData(19, -1, -1, null, 0, num194, num195 - 1, direction);
        //                            }
        //                        }
        //                        if (Main.tile[num194, num195 - 1].type == 388)
        //                        {
        //                            bool flag27 = WorldGen.ShiftTallGate(num194, num195 - 1, closing: false);
        //                            if (!flag27)
        //                            {
        //                                ai[3] = num56;
        //                                netUpdate = true;
        //                            }
        //                            if (Main.netMode == 2 && flag27)
        //                            {
        //                                NetMessage.SendData(19, -1, -1, null, 4, num194, num195 - 1);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            int num197 = spriteDirection;
        //            if (type == 425)
        //            {
        //                num197 *= -1;
        //            }
        //            if ((velocity.X < 0f && num197 == -1) || (velocity.X > 0f && num197 == 1))
        //            {
        //                if (height >= 32 && Main.tile[num194, num195 - 2].nactive() && Main.tileSolid[Main.tile[num194, num195 - 2].type])
        //                {
        //                    if (Main.tile[num194, num195 - 3].nactive() && Main.tileSolid[Main.tile[num194, num195 - 3].type])
        //                    {
        //                        velocity.Y = -8f;
        //                        netUpdate = true;
        //                    }
        //                    else
        //                    {
        //                        velocity.Y = -7f;
        //                        netUpdate = true;
        //                    }
        //                }
        //                else if (Main.tile[num194, num195 - 1].nactive() && Main.tileSolid[Main.tile[num194, num195 - 1].type])
        //                {
        //                    if (type == 624)
        //                    {
        //                        velocity.Y = -8f;
        //                        int num198 = (int)(position.Y + (float)height) / 16;
        //                        if (WorldGen.SolidTile((int)base.Center.X / 16, num198 - 8))
        //                        {
        //                            direction *= -1;
        //                            spriteDirection = direction;
        //                            velocity.X = 3 * direction;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        velocity.Y = -6f;
        //                    }
        //                    netUpdate = true;
        //                }
        //                else if (position.Y + (float)height - (float)(num195 * 16) > 20f && Main.tile[num194, num195].nactive() && !Main.tile[num194, num195].topSlope() && Main.tileSolid[Main.tile[num194, num195].type])
        //                {
        //                    velocity.Y = -5f;
        //                    netUpdate = true;
        //                }
        //                else if (directionY < 0 && type != 67 && (!Main.tile[num194, num195 + 1].nactive() || !Main.tileSolid[Main.tile[num194, num195 + 1].type]) && (!Main.tile[num194 + direction, num195 + 1].nactive() || !Main.tileSolid[Main.tile[num194 + direction, num195 + 1].type]))
        //                {
        //                    velocity.Y = -8f;
        //                    velocity.X *= 1.5f;
        //                    netUpdate = true;
        //                }
        //                else if (flag8)
        //                {
        //                    ai[1] = 0f;
        //                    ai[2] = 0f;
        //                }
        //                if (velocity.Y == 0f && flag6 && ai[3] == 1f)
        //                {
        //                    velocity.Y = -5f;
        //                }
        //                if (velocity.Y == 0f && (Main.expertMode || type == 586) && Main.player[target].Bottom.Y < base.Top.Y && Math.Abs(base.Center.X - Main.player[target].Center.X) < (float)(Main.player[target].width * 3) && Collision.CanHit(this, Main.player[target]))
        //                {
        //                    if (type == 586)
        //                    {
        //                        int num199 = (int)((base.Bottom.Y - 16f - Main.player[target].Bottom.Y) / 16f);
        //                        if (num199 < 14 && Collision.CanHit(this, Main.player[target]))
        //                        {
        //                            if (num199 < 7)
        //                            {
        //                                velocity.Y = -8.8f;
        //                            }
        //                            else if (num199 < 8)
        //                            {
        //                                velocity.Y = -9.2f;
        //                            }
        //                            else if (num199 < 9)
        //                            {
        //                                velocity.Y = -9.7f;
        //                            }
        //                            else if (num199 < 10)
        //                            {
        //                                velocity.Y = -10.3f;
        //                            }
        //                            else if (num199 < 11)
        //                            {
        //                                velocity.Y = -10.6f;
        //                            }
        //                            else
        //                            {
        //                                velocity.Y = -11f;
        //                            }
        //                        }
        //                    }
        //                    if (velocity.Y == 0f)
        //                    {
        //                        int num200 = 6;
        //                        if (Main.player[target].Bottom.Y > base.Top.Y - (float)(num200 * 16))
        //                        {
        //                            velocity.Y = -7.9f;
        //                        }
        //                        else
        //                        {
        //                            int num201 = (int)(base.Center.X / 16f);
        //                            int num202 = (int)(base.Bottom.Y / 16f) - 1;
        //                            for (int num203 = num202; num203 > num202 - num200; num203--)
        //                            {
        //                                if (Main.tile[num201, num203].nactive() && TileID.Sets.Platforms[Main.tile[num201, num203].type])
        //                                {
        //                                    velocity.Y = -7.9f;
        //                                    break;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if ((type == 31 || type == 294 || type == 295 || type == 296 || type == 47 || type == 77 || type == 104 || type == 168 || type == 196 || type == 385 || type == 389 || type == 464 || type == 470 || (type >= 524 && type <= 527)) && velocity.Y == 0f)
        //            {
        //                int num204 = 100;
        //                int num205 = 50;
        //                if (type == 586)
        //                {
        //                    num204 = 150;
        //                    num205 = 150;
        //                }
        //                if (Math.Abs(position.X + (float)(width / 2) - (Main.player[target].position.X + (float)(Main.player[target].width / 2))) < (float)num204 && Math.Abs(position.Y + (float)(height / 2) - (Main.player[target].position.Y + (float)(Main.player[target].height / 2))) < (float)num205 && ((direction > 0 && velocity.X >= 1f) || (direction < 0 && velocity.X <= -1f)))
        //                {
        //                    if (type == 586)
        //                    {
        //                        velocity.X += direction;
        //                        velocity.X *= 2f;
        //                        if (velocity.X > 8f)
        //                        {
        //                            velocity.X = 8f;
        //                        }
        //                        if (velocity.X < -8f)
        //                        {
        //                            velocity.X = -8f;
        //                        }
        //                        velocity.Y = -4.5f;
        //                        if (position.Y > Main.player[target].position.Y + 40f)
        //                        {
        //                            velocity.Y -= 2f;
        //                        }
        //                        if (position.Y > Main.player[target].position.Y + 80f)
        //                        {
        //                            velocity.Y -= 2f;
        //                        }
        //                        if (position.Y > Main.player[target].position.Y + 120f)
        //                        {
        //                            velocity.Y -= 2f;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        velocity.X *= 2f;
        //                        if (velocity.X > 3f)
        //                        {
        //                            velocity.X = 3f;
        //                        }
        //                        if (velocity.X < -3f)
        //                        {
        //                            velocity.X = -3f;
        //                        }
        //                        velocity.Y = -4f;
        //                    }
        //                    netUpdate = true;
        //                }
        //            }
        //            if (type == 120 && velocity.Y < 0f)
        //            {
        //                velocity.Y *= 1.1f;
        //            }
        //            if (type == 287 && velocity.Y == 0f && Math.Abs(position.X + (float)(width / 2) - (Main.player[target].position.X + (float)(Main.player[target].width / 2))) < 150f && Math.Abs(position.Y + (float)(height / 2) - (Main.player[target].position.Y + (float)(Main.player[target].height / 2))) < 50f && ((direction > 0 && velocity.X >= 1f) || (direction < 0 && velocity.X <= -1f)))
        //            {
        //                velocity.X = 8 * direction;
        //                velocity.Y = -4f;
        //                netUpdate = true;
        //            }
        //            if (type == 287 && velocity.Y < 0f)
        //            {
        //                velocity.X *= 1.2f;
        //                velocity.Y *= 1.1f;
        //            }
        //            if (type == 460 && velocity.Y < 0f)
        //            {
        //                velocity.X *= 1.3f;
        //                velocity.Y *= 1.1f;
        //            }
        //        }
        //    }
        //    else if (flag8)
        //    {
        //        ai[1] = 0f;
        //        ai[2] = 0f;
        //    }
        //}

    }
}
