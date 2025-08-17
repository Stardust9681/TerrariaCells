using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Forest
{
    public class Wolf : Terraria.ModLoader.GlobalNPC//, Shared.PreFindFrame.IGlobal
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == Terraria.ID.NPCID.Wolf;
        //public override bool AppliesToNPC(int npcType)
        //{
        //    return npcType == Terraria.ID.NPCID.Wolf;
        //}

        const int IdleNoTarget = 0;
        const int IdleTarget = 1;
        const int Approach = 2;
        const int Lunge = 3;
        void ResetAI(NPC npc)
        {
            npc.ai[0] = 0;
            npc.ai[1] = 0;
            npc.ai[2] = 0;
            npc.ai[3] = 0;
            npc.netUpdate = true;
        }
        public override bool PreAI(NPC npc)
        {
            npc.TargetClosest(false);

            float oldAI = npc.ai[1];
            switch ((int)npc.ai[1])
            {
                case IdleNoTarget:
                    IdleNoTargetAI(npc);
                    break;
                case IdleTarget:
                    IdleTargetAI(npc);
                    break;
                case Approach:
                    ApproachAI(npc);
                    break;
                case Lunge:
                    LungeAI(npc);
                    break;
            }

            npc.spriteDirection = npc.direction;
            return false;
        }

        void IdleNoTargetAI(NPC npc)
        {
            if (npc.TryGetTarget(out Entity target) && npc.TargetInAggroRange(target, Utilities.NumberHelpers.ToTileDist(30)))
            {
                ResetAI(npc);
                npc.ai[1] = IdleTarget;
                return;
            }
            if (MathF.Abs(npc.ai[3]) == 0)
                npc.ai[3] = 1;
            npc.direction = (int)npc.ai[3];

            float newVelX = npc.velocity.X + (0.07f * npc.direction);
            if (newVelX * npc.direction < 2.4f)
                npc.velocity.X = newVelX;

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

            if (npc.FindGroundInFront().Y - npc.Bottom.Y > Utilities.NumberHelpers.ToTileDist(3)
                || npc.ai[0] > Utilities.NumberHelpers.SecToFrames(3))
            {
                npc.velocity.X *= 0.67f;
                npc.ai[3] = -MathF.Sign(npc.ai[3]);
                npc.direction = (int)npc.ai[3];
                ResetAI(npc);
                return;
            }

            npc.velocity.Y += 0.14f;

            npc.ai[0]++;
        }
        void IdleTargetAI(NPC npc)
        {
            if (!npc.TryGetTarget(out Entity target))
            {
                ResetAI(npc);
                npc.ai[1] = IdleNoTarget;
                return;
            }

            if (npc.TargetInAggroRange(target, Utilities.NumberHelpers.ToTileDist(24), true, true))
            {
                ResetAI(npc);
                npc.ai[1] = Approach;
                return;
            }

            int direction = target.position.X < npc.position.X ? -1 : 1;

            float newVelX = npc.velocity.X + (0.07f * direction);
            if (newVelX * direction < 1.5f
                && npc.FindGroundInFront().Y - npc.Bottom.Y < Utilities.NumberHelpers.ToTileDist(3))
                npc.velocity.X = newVelX;

            npc.ai[3] = MathF.Sign(npc.velocity.X);
            npc.direction = (int)npc.ai[3];
            //npc.direction = MathF.Sign(npc.velocity.X);

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

            npc.velocity.Y += 0.14f;
        }
        void ApproachAI(NPC npc)
        {
            if (!npc.TryGetTarget(out Entity target))
            {
                ResetAI(npc);
                npc.ai[1] = IdleNoTarget;
                return;
            }

            int direction = target.Center.X < npc.Center.X ? -1 : 1;

            float distX = MathF.Abs(target.Center.X - npc.Center.X);
            if (!npc.TargetInAggroRange(target, Utilities.NumberHelpers.ToTileDist(26), true, true))
            {
                ResetAI(npc);
                npc.ai[1] = IdleTarget;
                return;
            }
            else if (
                Utilities.NumberHelpers.ToTileDist(8) < distX && distX < Utilities.NumberHelpers.ToTileDist(12)
                || npc.collideX
                )
            {
                if (!npc.collideX)
                    npc.ai[0]++;
                else
                    npc.ai[0] += 0.2f;
                if (npc.ai[0] > 15)
                {
                    ResetAI(npc);
                    npc.ai[1] = Lunge;
                    return;
                }
            }

            if (MathF.Abs(npc.velocity.X) > 1)
                npc.ai[3] = MathF.Sign(npc.velocity.X);
            else
                npc.ai[3] = direction;
            npc.direction = (int)npc.ai[3];

            direction = (target.Center.X - (Utilities.NumberHelpers.ToTileDist(10) * direction)) < npc.Center.X ? -1 : 1;

            float newVelX = npc.velocity.X + (0.14f * direction);
            if (newVelX * direction < (MathF.Abs(target.velocity.X)*0.33f) + 3.5f)
                npc.velocity.X = newVelX;

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

            npc.velocity.Y += 0.14f;
        }
        void LungeAI(NPC npc)
        {
            if (!npc.TryGetTarget(out Entity target))
            {
                ResetAI(npc);
                npc.ai[1] = IdleNoTarget;
                npc.stairFall = false;
                return;
            }
            npc.stairFall = npc.Bottom.Y < target.position.Y;

            //Grounded...
            if (npc.Grounded())
            {
                CombatNPC.ToggleContactDamage(npc, false);
                if (npc.ai[0] < 20)
                {
                    npc.velocity.X *= 0.967f;
                    npc.ai[3] = target.position.X < npc.position.X ? -1 : 1;
                    npc.ai[2] = (MathF.Abs(target.position.X - npc.position.X) + (MathF.Abs(target.velocity.X) * 1.2f)) * 0.0625f * .425f;
                    if (npc.ai[2] < 5.2f)
                        npc.ai[2] = 5.2f;
                }
                else if (npc.ai[0] == 20)
                {
                    npc.velocity.X = npc.ai[3] * npc.ai[2];
                    npc.velocity.Y = -5.5f;
                    npc.position.Y += npc.velocity.Y;
                    npc.ai[0]++;
                }
                else if (npc.ai[0] < 42)
                {
                    npc.velocity.X *= 0.9f;
                }
                else
                {
                    ResetAI(npc);
                    npc.ai[1] = Approach;
                    npc.velocity *= 0.5f;
                    return;
                }

                npc.ai[0]++;
            }
            //Not grounded...
            else
            {
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
                CombatNPC.ToggleContactDamage(npc, true);
                if (npc.ai[0] > 15 && MathF.Abs(npc.velocity.X) < 1)
                {
                    npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.ai[2], 0.05f);
                    npc.velocity.Y -= 0.01f;
                }
            }
            npc.velocity.Y -= 0.0167f;
            npc.direction = (int)npc.ai[3];
        }

        public bool PreFindFrame(NPC npc, int frameHeight)
        {
            int frameRate = 0;
            switch ((int)npc.ai[1])
            {
                case IdleNoTarget:
                    frameRate = 5;
                    break;
                case IdleTarget:
                    frameRate = 5;
                    break;
                case Approach:
                    frameRate = 10;
                    break;
                case Lunge:
                    if (npc.GetGlobalNPC<CombatNPC>().allowContactDamage)
                    {
                        int frameNum = npc.frame.Y / frameHeight;
                        if (npc.velocity.Y < 0)
                            frameNum = 10;
                        else if (npc.velocity.Y < 1.5f)
                            frameNum = 11;
                        else
                            frameNum = 12;
                        npc.frame.Y = frameNum * frameHeight;
                    }
                    return false;
            }

            if (MathF.Abs(npc.velocity.X) < MathF.Abs(npc.oldVelocity.X))
            {
                if (npc.frameCounter < 0)
                    npc.frameCounter = 0;
                npc.frameCounter += (int)((MathF.Abs(npc.oldVelocity.X) - MathF.Abs(npc.velocity.X)) * 3);
                if (Math.Abs(npc.frameCounter) > frameRate)
                {
                    int newFrame = npc.frame.Y / frameHeight;
                    if (newFrame > 2)
                        newFrame = 0;
                    else if (newFrame == 0)
                        newFrame = 1;
                    else
                        newFrame = 2;

                    npc.frame.Y = newFrame * frameHeight;
                    npc.frameCounter = 0;
                }
            }
            else
            {
                if (MathF.Abs(npc.velocity.X) > 2)
                    npc.frameCounter += npc.velocity.X;
                else
                    npc.frameCounter += MathF.Sign(npc.velocity.X) * 0.5f;
                    npc.frame.Height = frameHeight;
                if (Math.Abs(npc.frameCounter) > frameRate)
                {
                    int newFrame = npc.frame.Y / frameHeight;
                    newFrame += Math.Sign(npc.frameCounter * npc.spriteDirection);
                    if (newFrame > Main.npcFrameCount[npc.type] - 4)
                    {
                        newFrame = 3;
                    }
                    else if (newFrame < 3)
                    {
                        newFrame = Main.npcFrameCount[npc.type] - 4;
                    }
                    npc.frame.Y = newFrame * frameHeight;
                    npc.frameCounter = 0;
                }
            }
            return false;
        }
    }
}