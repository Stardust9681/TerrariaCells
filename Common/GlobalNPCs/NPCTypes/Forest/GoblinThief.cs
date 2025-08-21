using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Forest
{
	public class GoblinThief : Terraria.ModLoader.GlobalNPC, Shared.PreFindFrame.IGlobal
	{
        private static ReLogic.Content.Asset<Texture2D> goblin_StabSprite;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                goblin_StabSprite = Terraria.ModLoader.ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/GoblinStab");
            }
            base.Load();
        }
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == Terraria.ID.NPCID.GoblinThief;
        //public override bool AppliesToNPC(int npcType)
        //{
        //  return npcType is Terraria.ID.NPCID.GoblinThief;
        //}

        const int Idle = 0;
		const int ApproachTarget = 1;
		const int Jump = 2;
        const int Stab = 3;
        const int Stun = 4;

		const float MaxSpeed = 4f;
		const float Accel = 0.1f;

		public override bool PreAI(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest(false);

            float oldAI = npc.ai[1];
            switch (npc.ai[1])
			{
				case Idle:
					IdleAI(npc);
					break;
				case ApproachTarget:
					ApproachTargetAI(npc);
					break;
				case Jump:
					JumpAI(npc);
					break;
                case Stab:
                    StabAI(npc);
                    break;
                case Stun:
                    StunAI(npc);
                    break;
				default:
					npc.ai[1] = Idle;
					break;
			}
            if (npc.ai[1] != oldAI)
                npc.netUpdate = true;
            npc.spriteDirection = npc.direction;
            return false;
		}

        private void ResetAI(NPC npc)
        {
            npc.ai[0] = 0;
            npc.ai[1] = 0;
            npc.ai[2] = 0;
            npc.ai[3] = 0;
            npc.netUpdate = true;
        }

		void IdleAI(NPC npc)
		{
            if (npc.TargetInAggroRange(lineOfSight: true))
            {
                npc.ai[1] = ApproachTarget;
				return;
			}

			npc.direction = MathF.Sign(npc.ai[3]);
			float newVel = npc.velocity.X + npc.direction * Accel;
			if (npc.direction == 0)
			{
				newVel = npc.velocity.X + npc.spriteDirection * Accel;
				npc.direction = npc.spriteDirection;
			}
			const float IdleMaxSpeed = MaxSpeed * 0.5f;
			if (MathF.Abs(newVel) < IdleMaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * IdleMaxSpeed;

			if (npc.collideX)
			{
				Vector2 oldPos = npc.position;
				Vector2 oldVel = npc.velocity;
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (npc.position.Equals(oldPos))
				{
                    npc.position -= npc.oldVelocity * 2;
                    npc.ai[3] = -npc.direction;
                    npc.ai[1] = Idle;
                    return;
				}
				else
				{
					if (MathF.Abs(npc.velocity.X) < MathF.Abs(oldVel.X))
					{
						npc.velocity.X = (npc.velocity.X + oldVel.X) * 0.5f;
					}
					npc.ai[0] -= 2;
				}
			}

			Vector2 nextGround = npc.FindGroundInFront();
			if (npc.Grounded() && nextGround.Y > npc.Bottom.Y + npc.height)
			{
				npc.velocity.X *= 0.5f;
				npc.ai[0] = 0;
				npc.ai[3] = -npc.direction;
				npc.ai[1] = Idle;
				return;
			}

			if (npc.ai[0] > 90)
			{
				npc.ai[0] = 0;
				npc.ai[3] = -npc.direction;
				npc.ai[1] = Idle;
				return;
			}

			npc.velocity.Y += 0.036f; //Apply gravity
			npc.ai[0]++;
		} //Hitbox doesn't matter, target too far to take contact damage
		void ApproachTargetAI(NPC npc) //No hitbox when walking
		{
			if (
				!npc.TryGetTarget(out Entity target)
				|| (npc.ai[0] == 0 && !npc.TargetInAggroRange(target, lineOfSight: false))
				|| !npc.TargetInAggroRange(target, 480, true))
			{
                ResetAI(npc);
                npc.ai[1] = Idle;
                npc.ai[3] = npc.direction;
                return;
			}

			npc.direction = npc.velocity.X < 0 ? -1 : 1;
			Vector2 distance = new Vector2(MathF.Abs(target.position.X - npc.position.X), MathF.Abs(target.position.Y - npc.position.Y));
			if (
				npc.IsFacingTarget(target)
				&& distance.X < Utilities.NumberHelpers.ToTileDist(12))
			{
                ResetAI(npc);
                if (distance.Y < 32)
                {
                    npc.ai[1] = Stab;
                }
                else
                {
                    if (target.position.Y < npc.position.Y)
                        npc.ai[1] = Jump;
                    else
                        npc.ai[1] = Idle;
                }
                npc.ai[3] = (target.position.X < npc.position.X) ? -1 : 1;
                return;
			}

			int directionToMove = target.position.X < npc.position.X ? -1 : 1;
			if (npc.ai[0] < 60 && !npc.IsFacingTarget(target) && distance.X < 80)
				directionToMove *= -1;

			float newVel = npc.velocity.X + directionToMove * Accel;
			if (MathF.Abs(newVel) < MaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = npc.direction * MaxSpeed;

			if (npc.FindGroundInFront().Y > npc.Bottom.Y + npc.height * 2)
			{
                ResetAI(npc);
                npc.ai[1] = Jump;
				return;
			}

			if (npc.collideX)
			{
				Vector2 oldPos = npc.position;
				Vector2 oldVel = npc.velocity;
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (oldPos == npc.position && oldVel == npc.velocity && npc.Grounded())
				{
                    ResetAI(npc);
                    npc.ai[1] = Jump;
					return;
				}
			}
			npc.velocity.Y += 0.036f; //Apply gravity
			npc.ai[0]++;
		}
		void JumpAI(NPC npc) //Hitbox when jumping
		{
			if (npc.ai[0] == 0)
				CombatNPC.ToggleContactDamage(npc, true);
			if (npc.Grounded())
			{
				if (npc.ai[0] == 0)
				{
					npc.velocity.Y -= 7.2f;
				}
				else
				{
					npc.ai[1] = ApproachTarget;
					return;
				}
			}
			if (MathF.Abs(npc.velocity.X) < 2.4f)
				npc.velocity.X += npc.direction * 0.024f;
			if (!npc.IsFacingTarget() && npc.velocity.Y > 0)
			{
				npc.velocity.Y *= 1.014f;
				npc.velocity.X *= 0.996f;
			}
			npc.velocity.Y += 0.024f;
			npc.ai[0]++;
		}
        const int Stab_Windup = 25;
        const int Stab_DashLen = 45;
        void StabAI(NPC npc)
        {
            if (!npc.TryGetTarget(out Entity target))
            {
                ResetAI(npc);
                return;
            }
            npc.ai[0]++;
            if (npc.ai[0] < Stab_Windup)
            {
                npc.velocity.X *= 0.9f;
            }
            else if (npc.ai[0] == Stab_Windup)
            {
                CombatNPC.ToggleContactDamage(npc, true);
                npc.velocity.X = 9f * npc.ai[3];
            }
            else
            {
                if (MathF.Abs(npc.velocity.X) > 1)
                    npc.velocity.X -= MathF.Sign(npc.velocity.X) * 0.15f;
                else
                    npc.velocity.X *= 0.975f;
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
                //Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
                if (npc.ai[0] > Stab_DashLen + (Stab_Windup/2))
                {
                    CombatNPC.ToggleContactDamage(npc, false);
                }
                if (npc.ai[0] > Stab_DashLen + Stab_Windup || (npc.collideX && (MathF.Abs(npc.oldVelocity.X) - MathF.Abs(npc.velocity.X) > 2.5f)))
                {
                    ResetAI(npc);
                    CombatNPC.ToggleContactDamage(npc, false);
                    npc.ai[1] = Stun;
                    npc.netUpdate = true;
                }
            }
        }
        const int Stun_Delay = 25;
        void StunAI(NPC npc)
        {
            npc.ai[0]++;
            npc.velocity.X *= 0.8f;

            if (npc.ai[0] > Stun_Delay)
            {
                ResetAI(npc);
                npc.ai[3] = npc.direction;
            }
        }

		public bool PreFindFrame(NPC npc, int frameHeight)
		{
            if (npc.ai[1] < Stab)
            {
                npc.frameCounter += (int)(npc.velocity.X);
                npc.frame.Height = frameHeight;
                if (Math.Abs(npc.frameCounter) > 5)
                {
                    int newFrame = (npc.frame.Y / frameHeight) + Math.Sign(npc.frameCounter * npc.spriteDirection);
                    if (newFrame > Main.npcFrameCount[npc.type] - 2)
                    {
                        newFrame = 2;
                    }
                    else if (newFrame < 2)
                    {
                        newFrame = Main.npcFrameCount[npc.type] - 2;
                    }
                    npc.frame.Y = newFrame * frameHeight;
                    npc.frameCounter = 0;
                }
            }
            else
            {
                int sheetHeight = goblin_StabSprite?.Height() ?? 1;
                if (npc.ai[1] == Stab)
                {
                    npc.frame.Y = (4 * (int)npc.ai[0] / (Stab_Windup + Stab_DashLen)) * (sheetHeight / 6);
                }
                else
                {
                    npc.frame.Y = Math.Min(((2 * (int)npc.ai[0] / Stun_Delay) + 4) * (sheetHeight / 6), 5 * sheetHeight / 6);
                }
            }
            return false;
		}
        public override bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor)
        {
            if (npc.ai[1] >= Stab)
            {
                Vector2 size = goblin_StabSprite.Size();
                spritebatch.Draw(goblin_StabSprite.Value, npc.Top - screenPos, npc.frame with { Width = (int)size.X, Height = (int)(size.Y / 6) } , lightColor, 0, new Vector2(size.X*0.5f, 8), npc.scale, npc.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                return false;
            }
            return base.PreDraw(npc, spritebatch, screenPos, lightColor);
        }

        public override bool? CanFallThroughPlatforms(NPC npc) => npc.stairFall;
    }
}