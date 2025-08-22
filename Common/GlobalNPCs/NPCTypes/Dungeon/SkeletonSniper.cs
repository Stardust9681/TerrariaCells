using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using TerrariaCells.Common.Utilities;
using static TerrariaCells.Common.Utilities.NPCHelpers;
using static TerrariaCells.Common.Utilities.PlayerHelpers;
using Terraria.DataStructures;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Dungeon
{
    public class SkeletonSniper : GlobalNPC, Common.GlobalNPCs.PreFindFrame.IGlobal
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.SkeletonSniper;

        const int Idle = 0;
        const int FollowPlayer = 1;
        const int Shoot = 2;
        const int Reload = 3;

        public override void OnSpawn(NPC npc, IEntitySource source) => CombatNPC.ToggleContactDamage(npc, false);

        public override bool PreAI(NPC npc)
        {
            switch ((int)npc.ai[1])
            {
                case Idle:
                    IdleAI(npc);
                    break;
                case FollowPlayer:
                    FollowPlayerAI(npc);
                    break;
                case Shoot:
                    ShootAI(npc);
                    break;
                case Reload:
                    ReloadAI(npc);
                    break;
            }
            return false;
        }

        const float MaxSpeed = 1.4f;
        const float Accel = 0.1f;
        private void IdleAI(NPC npc)
        {
            npc.TargetClosest(false);
            if (npc.TargetInAggroRange(NumberHelpers.ToTileDist(22)))
            {
                npc.ai[0] = 0;
                npc.ai[1] = FollowPlayer;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }

            npc.direction = MathF.Sign(npc.velocity.X);

            float newVel = npc.velocity.X + npc.direction * Accel;
            if (npc.direction == 0)
            {
                newVel = npc.velocity.X + npc.spriteDirection * Accel;
                npc.direction = MathF.Sign(newVel);
            }
            if (MathF.Abs(newVel) < MaxSpeed)
                npc.velocity.X = newVel;
            else
                npc.velocity.X = MathF.Sign(npc.velocity.X) * MaxSpeed;

            if (npc.collideX)
            {
                Vector2 oldPos = npc.position;
                Vector2 oldVel = npc.velocity;
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
                if (npc.position.Equals(oldPos))
                {
                    npc.position -= npc.oldVelocity * 2;
                    npc.ai[0] = 0;
                    npc.ai[1] = Idle;
                    npc.ai[2] = -npc.direction;
                    npc.ai[3] = 0;
                    return;
                }
                else
                {
                    if (MathF.Abs(npc.velocity.X) < MathF.Abs(oldVel.X))
                    {
                        npc.velocity.X = (npc.velocity.X + oldVel.X) * 0.5f;
                    }
                    npc.ai[0] -= 3;
                }
            }

            Vector2 nextGround = npc.FindGroundInFront();
            if (npc.Grounded() && nextGround.Y > npc.Bottom.Y + npc.height)
            {
                npc.velocity.X *= 0.5f;
                npc.ai[0] = 0;
                npc.ai[1] = Idle;
                npc.ai[2] = -npc.direction;
                npc.ai[3] = 0;
                return;
            }

            if (npc.ai[0] > 240)
            {
                npc.ai[0] = 0;
                npc.ai[1] = Idle;
                npc.ai[2] = -npc.direction;
                npc.ai[3] = 0;
                return;
            }

            npc.velocity.Y += 0.036f; //Apply gravity
            npc.ai[0]++;
        }
        private void FollowPlayerAI(NPC npc)
        {
            npc.TargetClosest(true);
            if (!npc.TryGetTarget(out Entity target))
            {
                npc.ai[0] = 0;
                npc.ai[1] = Idle;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }
            if (npc.TargetInAggroRange(target, 640f, true, false))
            {
                npc.ai[0] = 0;
                npc.ai[1] = Shoot;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }
            Vector2 nextGround = npc.FindGroundInFront();
            if (npc.Grounded() && nextGround.Y > npc.Bottom.Y + npc.height)
            {
                npc.position.X -= npc.velocity.X;
                npc.velocity.X = 0;
                return;
            }

            if (npc.TargetInAggroRange(target, 700, false))
            {
                int direction = target.position.X < npc.position.X ? -1 : 1;

                float newVel = npc.velocity.X + direction * Accel;
                if (MathF.Abs(newVel) < MaxSpeed)
                    npc.velocity.X = newVel;
                else
                    npc.velocity.X = MathF.Sign(npc.velocity.X) * MaxSpeed;
            }

            npc.velocity.Y += 0.036f;
        }
        private void ShootAI(NPC npc)
        {
            if (!npc.TryGetTarget(out Entity target))
            {
                npc.ai[0] = 0;
                npc.ai[1] = Idle;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }
            npc.direction = target.position.X < npc.position.X ? -1 : 1;
            npc.spriteDirection = npc.direction;
            if (npc.TargetInAggroRange(target, 640f, true, false) && npc.ai[0] < 101)
            {
                npc.ai[0]++;

                float rotation = (target.Center - npc.Center).ToRotation();
                npc.ai[3] = rotation;
            }
            else if (npc.ai[0] > 80)
            {
                npc.ai[0]++;
            }
            else if (npc.ai[0] > 0)
            {
                npc.ai[0] -= 0.4f;
            }
            else
            {
                npc.ai[0] = 0;
                npc.ai[1] = FollowPlayer;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }

            if (npc.ai[0] > 105)
            {
                if (Main.netMode != 1)
                {
                    Vector2 vel = Vector2.UnitX.RotatedBy(npc.ai[3]) * 14f;
                    Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, vel, ProjectileID.SniperBullet, TCellsUtils.ScaledHostileDamage(npc.damage), 1f, Main.myPlayer);
                }

                npc.ai[0] = 0;
                npc.ai[1] = Reload;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }

            npc.velocity.X *= 0.9f;
            npc.velocity.Y += 0.036f;
        }
        private void ReloadAI(NPC npc)
        {
            npc.ai[0]++;
            if (npc.ai[0] > 45)
            {
                npc.ai[0] = 0;
                npc.ai[1] = Idle;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
            }

            npc.velocity.X *= 0.9f;
            npc.velocity.Y += 0.036f;
        }

        public bool PreFindFrame(NPC npc, int frameHeight)
        {
            int frameNum = npc.frame.Y / frameHeight;
            switch ((int)npc.ai[1])
            {
                //Mostly borrowed framing code from Goblin Archer, actually
                case Idle:
                case FollowPlayer:
                    npc.frameCounter += (int)(npc.velocity.X);
                    if (Math.Abs(npc.frameCounter) > 4)
                    {
                        const int Offset = 7;
                        int frameCount = Main.npcFrameCount[npc.type];
                        int cFrame = npc.frame.Y / frameHeight;
                        frameNum = cFrame + Math.Sign(npc.frameCounter * npc.spriteDirection);
                        frameNum = Offset + (((frameNum - Offset) + (frameCount - Offset)) % (frameCount - Offset));
                        npc.frameCounter = 0;
                    }
                    break;
                case Shoot:
                    float rotation = npc.ai[3];
                    rotation = (rotation + MathHelper.TwoPi) % MathHelper.TwoPi;

                    if (rotation > MathHelper.PiOver2 && rotation < 3 * MathHelper.PiOver2)
                    {
                        rotation -= MathHelper.PiOver2; //0-Pi
                    }
                    else
                    {
                        //Range from -Pi/2 - Pi/2
                        if (rotation > MathHelper.Pi) //3Pi/2 - 2Pi
                            rotation -= MathHelper.TwoPi; //-Pi/2 - 0
                        rotation += MathHelper.PiOver2; //0 - Pi/2 + Pi/2 - Pi
                        rotation = MathHelper.Pi - rotation;
                    }
                    //rotation now from 0-Pi
                    float mult = rotation / MathHelper.Pi;
                    frameNum = (int)MathHelper.Clamp(mult * 5, 0, 5);
                    break;
                case Reload:
                    frameNum = 1;
                    break;
            }
            npc.frame.Y = frameNum * frameHeight;
            return false;
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.ai[1] == Shoot)
            {
                if(!npc.TryGetTarget(out Entity target)) return;
                if(npc.ai[0] <= 0) return;

                Color lineColor = Color.Red * 0.67f;
                if (npc.ai[0] > 90)
                    return;
                else if (npc.ai[0] > 60)
                {
                    lineColor = Color.Lerp(lineColor, Color.Orange * 0.9f, (npc.ai[0] - 60) / 30f);
                }
                else if ((int)(npc.ai[0]) % 8 > 4)
                    lineColor = Color.Transparent;

                float width = 2;
                if (npc.ai[0] < 30)
                    width = 12f - (npc.ai[0] / 30 * 10f);
                else if (npc.ai[0] > 60)
                    width = 1;

                Vector2 startPos = npc.Center + (Vector2.UnitX * npc.spriteDirection * 12);
                Vector2 direction = Vector2.UnitX.RotatedBy(npc.ai[3]);
                Vector2 targetPos = startPos;
                for (int i = 0; i < 32; i++)
                {
                    targetPos += direction * 16;
                    if (!Collision.CanHitLine(startPos, 2, 2, targetPos, 2, 2)
                        || (target.DistanceSQ(npc.Center) > 6400
                        && new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height).Contains(targetPos.ToPoint())))
                        break;
                }
                float visualTimer = (float)(Main.timeForVisualEffects + (npc.whoAmI * 10)) * 0.15f;
                float inaccuracy = 1 - (npc.ai[0] / 70f);
                if (inaccuracy > 0)
                    targetPos += new Vector2(MathF.Cos(visualTimer) * target.width, MathF.Sin(visualTimer) * target.height) * 0.5f * inaccuracy;
                Terraria.Utils.DrawLine(spriteBatch, startPos, targetPos, lineColor, lineColor * 0.75f, width);
            }
        }
        public override bool? CanFallThroughPlatforms(NPC npc) => false;

        //Hitstun
        public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (hit.DamageType.CountsAsClass(DamageClass.Melee))
            {
                if (npc.ai[1] == Shoot || npc.ai[1] == Reload)
                {
                    npc.ai[0] -= damageDone / 7f;
                }
            }
        }
        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (hit.DamageType.CountsAsClass(DamageClass.Melee))
            {
                if (npc.ai[1] == Shoot || npc.ai[1] == Reload)
                {
                    npc.ai[0] -= damageDone / 7f;
                }
            }
        }
    }
}
