using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;
using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Forest
{
    public class Wolf : AIType
    {
        public override bool AppliesToNPC(int npcType)
        {
            return npcType == Terraria.ID.NPCID.Wolf;
        }

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
        }
        public override void Behaviour(NPC npc)
        {
            npc.TargetClosest(false);
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
        }

        void IdleNoTargetAI(NPC npc)
        {
            if (npc.TryGetTarget(out _))
            {
                ResetAI(npc);
                npc.ai[1] = IdleTarget;
                return;
            }
            if (npc.direction == 0)
                npc.direction = 1;

            float newVelX = npc.velocity.X + (0.07f * npc.direction);
            if (newVelX * npc.direction < 2.6f)
                npc.velocity.X = newVelX;

            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

            if (npc.FindGroundInFront().Y - npc.Bottom.Y > Utilities.NumberHelpers.ToTileDist(1) + 4
                || npc.ai[0] > Utilities.NumberHelpers.SecToFrames(3))
            {
                npc.velocity.X *= 0.67f;
                npc.direction *= -1;
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

            if (npc.TargetInAggroRange(target, Utilities.NumberHelpers.ToTileDist(30), true, true))
            {
                ResetAI(npc);
                npc.ai[1] = Approach;
                return;
            }

            int direction = target.position.X < npc.position.X ? -1 : 1;

            float newVelX = npc.velocity.X + (0.07f * direction);
            if (newVelX * direction < 1
                && npc.FindGroundInFront().Y - npc.Bottom.Y < Utilities.NumberHelpers.ToTileDist(1))
                npc.velocity.X = newVelX;

            npc.direction = MathF.Sign(npc.velocity.X);

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

            float distX = MathF.Abs(target.position.X - npc.position.X);
            if (!npc.TargetInAggroRange(target, Utilities.NumberHelpers.ToTileDist(32), true, true))
            {
                ResetAI(npc);
                npc.ai[1] = IdleTarget;
                return;
            }
            else if (npc.velocity.X * direction > 2 && Utilities.NumberHelpers.ToTileDist(6) < distX && distX < Utilities.NumberHelpers.ToTileDist(16))
            {
                ResetAI(npc);
                npc.ai[1] = Lunge;
                return;
            }

            direction = (target.Center.X - (Utilities.NumberHelpers.ToTileDist(10) * direction)) < npc.Center.X ? -1 : 1;

            float newVelX = npc.velocity.X + (0.14f * direction);
            if (newVelX * direction < 3.6f)
                npc.velocity.X = newVelX;
            npc.direction = MathF.Sign(npc.velocity.X);

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
                return;
            }

            
            //Grounded...
            if (npc.Grounded())
            {
                CombatNPC.ToggleContactDamage(npc, false);
                if (npc.ai[0] < 15)
                {
                    npc.velocity.X *= 0.95f;
                    npc.direction = target.position.X < npc.position.X ? -1 : 1;
                    npc.ai[2] = MathF.Abs(target.position.X - npc.position.X) * 0.0625f * .57f; //N * 1/16 * 0.57
                }
                else if (npc.ai[0] == 15)
                {
                    npc.velocity.X = npc.direction * npc.ai[2];
                    npc.velocity.Y = -6f;
                    npc.ai[0]++;
                }
                else if (npc.ai[0] < 30)
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
                CombatNPC.ToggleContactDamage(npc, true);
                if (npc.ai[0] > 15 && MathF.Abs(npc.velocity.X) < 1)
                {
                    npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.ai[2], 0.05f);
                    npc.velocity.Y -= 0.01f;
                }
            }
            npc.velocity.Y += 0.024f;
        }

        public override bool FindFrame(NPC npc, int frameHeight)
        {
            int frameRate = 0;
            switch ((int)npc.ai[1])
            {
                case IdleNoTarget:
                    frameRate = 4;
                    break;
                case IdleTarget:
                    frameRate = 4;
                    break;
                case Approach:
                    frameRate = 8;
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
                npc.frameCounter += (int)(npc.velocity.X);
                npc.frame.Height = frameHeight;
                if (Math.Abs(npc.frameCounter) > 4)
                {
                    int newFrame = npc.frame.Y / frameHeight;
                    newFrame += Math.Sign(npc.frameCounter * npc.spriteDirection);
                    if (newFrame > Main.npcFrameCount[npc.type] - 3)
                    {
                        newFrame = 3;
                    }
                    else if (newFrame < 3)
                    {
                        newFrame = Main.npcFrameCount[npc.type] - 3;
                    }
                    npc.frame.Y = newFrame * frameHeight;
                    npc.frameCounter = 0;
                }
            }
            return false;
        }
    }
}