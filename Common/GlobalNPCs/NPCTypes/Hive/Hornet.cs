using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Hive
{
    public class Hornet : GlobalNPC, OnAnyPlayerHit.IGlobal
    {
        private static readonly int[] HORNETS = new int[]
        {
            NPCID.Hornet, NPCID.HornetFatty, NPCID.HornetHoney, NPCID.HornetLeafy,
            NPCID.HornetSpikey, NPCID.HornetStingy, NPCID.MossHornet
        };
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return HORNETS.Contains(entity.type);
        }

        public override bool PreAI(NPC npc)
        {
            switch ((int)npc.ai[1])
            {
                case 0:
                    IdleAI(npc);
                    break;
                case 1:
                    MoveAI(npc);
                    break;
                case 2:
                    ShootAI(npc);
                    break;
                case 3:
                    JabAI(npc);
                    break;
            }
            return false;
        }

        private void IdleAI(NPC npc)
        {
            npc.TargetClosest(false);
            if (npc.TryGetTarget(out Entity target) && npc.TargetInAggroRange(target, 512))
            {
                npc.ai[0] = 0;
                npc.ai[1] = 1;
                npc.ai[2] = -MathF.Sign(npc.position.X - target.position.X);
                npc.ai[3] = 0;
                return;
            }

            CombatNPC.ToggleContactDamage(npc, false);

            Vector2 movePos = new Vector2(npc.ai[2], npc.ai[3]);

            if (movePos.X == 0 || movePos.Y == 0)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return;

                for (int i = 0; i < 15; i++)
                {
                    movePos = npc.position;
                    Vector2 direction = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);

                    int iterations = 0;
                    while (Collision.CanHitLine(npc.position, npc.width, npc.height, movePos, npc.width, npc.height) && iterations < 6)
                    {
                        movePos += direction * 24;
                        iterations++;
                    }
                    movePos -= direction * 24;
                    if (iterations > 2)
                        break;
                }
                npc.ai[2] = movePos.X;
                npc.ai[3] = movePos.Y;
                npc.netUpdate = true;
                return;
            }

            if (movePos.X - npc.position.X != 0)
                npc.velocity.X += MathF.Sign(movePos.X - npc.position.X) * MathF.Sqrt(MathF.Abs(movePos.X - npc.position.X)) * 0.005f;

            npc.velocity.Y += (movePos.Y < npc.position.Y ? -1 : 1) * 0.024f;

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

            npc.rotation = MathHelper.ToRadians(npc.velocity.X) * 4.5f;
            npc.direction = MathF.Sign(npc.velocity.X);
            npc.spriteDirection = npc.direction;

            //Dust.NewDustDirect(movePos, 1, 1, DustID.GemDiamond).velocity = Vector2.Zero;

            if (npc.DistanceSQ(movePos) < 40 * 40)
            {
                npc.ai[0]++;
                npc.velocity *= 0.98f;
            }
            else if (npc.ai[0] > 0)
            {
                npc.ai[0] -= 0.33f;
            }
            if (npc.ai[0] > 80 || !Collision.CanHitLine(npc.position, npc.width, npc.height, movePos, npc.width, npc.height))
            {
                npc.ai[0] = 0;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }
        }

        private void MoveAI(NPC npc)
        {
            npc.TargetClosest(false);
            if (!npc.TryGetTarget(out Entity target) || !npc.TargetInAggroRange(target, 550, false))
            {
                npc.ai[0] = 0;
                npc.ai[1] = 0;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }

            Vector2 movePos = target.Center;
            movePos.X += npc.ai[2] * 56;
            movePos.Y -= 112;
            //Dust.NewDustDirect(movePos, 1, 1, DustID.GemDiamond).velocity = Vector2.Zero;

            if (movePos.X - npc.position.X != 0 && MathF.Abs(npc.velocity.X) < 4f)
                npc.velocity.X += MathF.Sign(movePos.X - npc.position.X) * MathF.Sqrt(MathF.Abs(movePos.X - npc.position.X)) * 0.045f;

            npc.velocity.Y += (movePos.Y < npc.position.Y ? -1 : 1) * 0.06f;

            if (MathF.Sign(movePos.X - npc.position.X) != MathF.Sign(npc.velocity.X))
            {
                npc.velocity.X *= 0.95f;
            }
            if (MathF.Sign(movePos.Y - npc.position.Y) != MathF.Sign(npc.velocity.Y))
            {
                npc.velocity.Y *= 0.95f;
            }

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

            npc.rotation = MathHelper.ToRadians(npc.velocity.X) * 4.5f;
            npc.direction = -(int)npc.ai[2];
            npc.spriteDirection = npc.direction;



            bool isGoodX = MathF.Abs(npc.position.X - movePos.X) < 48 || npc.collideX;
            bool isGoodY = MathF.Abs(npc.position.Y - movePos.Y) < 48 || npc.collideY;
            if (isGoodX && isGoodY)
            {
                npc.ai[0]++;
                npc.velocity *= 0.975f;
                if (npc.ai[0] > 45)
                {
                    npc.ai[0] = 0;
                    npc.ai[1] = 2;
                    npc.ai[2] = 0;
                    npc.ai[3] = 0;
                }
            }
        }

        private void ShootAI(NPC npc)
        {
            if (!npc.TryGetTarget(out Entity target) || !npc.TargetInAggroRange(target, 520, false))
            {
                npc.ai[0] = 0;
                npc.ai[1] = 0;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                return;
            }

            if (npc.velocity.Y > -1.6f)
                npc.velocity.Y -= 0.03f;
            else
                npc.velocity.Y *= 0.9f;
            npc.velocity.X *= 0.9f;

            npc.ai[0]++;
            if (npc.ai[0] < (24 * 3) && (int)(npc.ai[0]) % 24 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = npc.DirectionTo(target.Center) * 6.75f;
                Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Bottom, vel, ProjectileID.Stinger, TCellsUtils.ScaledHostileDamage(npc.damage), 1f, Main.myPlayer);
            }

            npc.direction = MathF.Sign(target.position.X - npc.position.X);
            npc.spriteDirection = npc.direction;
            npc.rotation = 0;

            if (npc.ai[0] > (24 * 3.5f))
            {
                npc.ai[0] = 0;
                npc.ai[1] = 3;
                npc.ai[2] = MathF.Sign(target.position.X - npc.position.X) * MathF.Sqrt(MathF.Abs(target.position.X - npc.position.X)) * 0.0425f;
                npc.ai[3] = 0;
                return;
            }
        }

        private void JabAI(NPC npc)
        {
            if (npc.TryGetTarget(out Entity target))
            {
                npc.ai[2] = MathF.Sign(target.position.X - npc.position.X) * MathF.Sqrt(MathF.Abs(target.position.X - npc.position.X)) * 0.0425f;
            }

            if (npc.collideY && npc.oldVelocity.Y > 0 || npc.ai[3] > 0)
            {
                npc.velocity *= 0;
                npc.ai[3]++;
            }
            if (npc.ai[3] > 24)
            {
                npc.velocity *= 0.95f;
            }
            else if (npc.ai[3] > 6)
            {
                npc.velocity.Y = -6;
                npc.velocity.X = -npc.ai[2] * 4f;
            }
            else if (npc.ai[0] > 25)
            {
                npc.velocity.Y = 6;
                npc.velocity.X += npc.ai[2];
            }
            else
            {
                npc.velocity *= 0.95f;
                npc.velocity.Y *= 0.95f;
            }

            CombatNPC.ToggleContactDamage(npc, npc.ai[0] > 25 && npc.ai[3] < 1);

            npc.rotation = -MathHelper.ToRadians(npc.velocity.X) * 2.8f;
            npc.direction = MathF.Sign(npc.ai[2]);
            npc.spriteDirection = npc.direction;

            npc.ai[0]++;

            if (npc.ai[3] > 36)
            {
                npc.ai[0] = 0;
                npc.ai[1] = 1;
                npc.ai[2] = -MathF.Sign(npc.ai[2]);
                npc.ai[3] = 0;
                return;
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            CombatNPC.ToggleContactDamage(npc, false);
        }

        public void OnAnyPlayerHit(NPC npc, Player attacker, NPC.HitInfo info, int damage)
        {
            if (info.DamageType.CountsAsClass(DamageClass.Melee))
            {
                switch ((int)npc.ai[1])
                {
                    case 2:
                        npc.ai[0] = 0;
                        npc.ai[1] = 3;
                        npc.ai[2] = MathF.Sign(attacker.position.X - npc.position.X) * MathF.Sqrt(MathF.Abs(attacker.position.X - npc.position.X)) * 0.0425f;
                        npc.ai[3] = 0;
                        break;
                }
            }
        }
    }
}