using System;
using Terraria;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using static TerrariaCells.Common.Utilities.NPCHelpers;
using TerrariaCells.Common.Utilities;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Forest
{
	public class GoblinArcher : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType.Equals(Terraria.ID.NPCID.GoblinArcher);
		}

		const int Idle = 0;
		const int ApproachTarget = 1;
		const int Jump = 2;
		const int FireArrows = 3;

		const float MaxSpeed = 2f;
		const float Accel = 0.075f;
		const float JumpStrength = 5.5f;

        private static void ResetAI(NPC npc)
        {
            CombatNPC.ToggleContactDamage(npc, false);
            npc.ai[0] = 0;
            npc.ai[1] = Idle;
            npc.ai[2] = 0;
            npc.ai[3] = 0;
        }

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest(false);

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
				case FireArrows:
					FireArrowsAI(npc);
					break;
				default:
                    ResetAI(npc);
					break;
			}
            npc.spriteDirection = npc.direction;
		}

		void IdleAI(NPC npc)
		{
			if (npc.TargetInAggroRange(NumberHelpers.ToTileDist(22)))
			{
                ResetAI(npc);
				npc.ai[1] = ApproachTarget;
				return;
			}

			npc.direction = MathF.Sign(npc.ai[2]);

			float newVel = npc.velocity.X + npc.direction * Accel;
			if (npc.direction == 0)
			{
				newVel = npc.velocity.X + npc.spriteDirection * Accel;
				npc.direction = MathF.Sign(newVel);
			}
			const float IdleMaxSpeed = MaxSpeed * 0.67f;
			if (MathF.Abs(newVel) < IdleMaxSpeed)
				npc.velocity.X = newVel;
			else
				npc.velocity.X = MathF.Sign(npc.velocity.X) * IdleMaxSpeed;

			if (npc.collideX)
			{
				Vector2 oldPos = npc.position;
				Vector2 oldVel = npc.velocity;
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (npc.position.Equals(oldPos))
				{
                    ResetAI(npc);
                    npc.position -= npc.oldVelocity * 2;
					npc.ai[2] = -npc.direction;
					npc.ai[1] = Idle;
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
                ResetAI(npc);
                npc.ai[2] = -npc.direction;
				npc.ai[1] = Idle;
				return;
			}

			if (npc.ai[0] > 150)
			{
                ResetAI(npc);
				npc.ai[2] = -npc.direction;
				npc.ai[1] = Idle;
				return;
			}

			npc.velocity.Y += 0.036f; //Apply gravity
            npc.ai[0]++;
		} //Hitbox doesn't matter, target not near enough to take contact damage
		void ApproachTargetAI(NPC npc) //No hitbox when walking
		{
			Player target = Main.player[npc.target];
			int directionToMove = target.position.X < npc.position.X ? -1 : 1;
			Vector2 distance = new Vector2(MathF.Abs(target.position.X - npc.position.X), MathF.Abs(target.position.Y - npc.position.Y));
			float additiveDist = distance.X + distance.Y;
			//Within ~5 tiles or between 15-20 tiles away, try to fire arrows instead
			if(additiveDist < NumberHelpers.ToTileDist(10) || (NumberHelpers.ToTileDist(15) < additiveDist && additiveDist < NumberHelpers.ToTileDist(25)))
			{
				if (npc.LineOfSight(target.position))
				{
                    ResetAI(npc);
					npc.ai[1] = FireArrows;
					return;
				}
			}

            if (npc.FindGroundInFront().Y > npc.Bottom.Y + npc.height)
            {
                if (!npc.TargetInAggroRange(target, NumberHelpers.ToTileDist(22), false))
                {
                    ResetAI(npc);
                    npc.ai[2] = npc.direction;
                    npc.ai[1] = Idle;
                }
                else if (npc.LineOfSight(target.position))
                {
                    ResetAI(npc);
                    npc.velocity.X = 0;
                    npc.ai[1] = FireArrows;
                }
                else
                {
                    ResetAI(npc);
                    npc.ai[2] = npc.direction;
                    npc.ai[1] = Idle;
                }
                return;
            }
            else
            {
                float newVel = npc.velocity.X + directionToMove * Accel;
                if (MathF.Abs(newVel) < MaxSpeed)
                    npc.velocity.X = newVel;
                else
                    npc.velocity.X = npc.direction * MaxSpeed;
            }

            if (npc.collideX)
            {
                //Vector2 oldPos = npc.position;
                //Vector2 oldVel = npc.velocity;
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
                if (npc.Grounded())
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
			if (npc.Grounded())
			{
				if (npc.ai[0] == 0)
				{
					npc.velocity.Y -= JumpStrength;
				}
				else
				{
                    ResetAI(npc);
					npc.ai[1]=Idle;
					return;
				}
			}
			if (npc.collideX)
			{
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
			}
			if (MathF.Abs(npc.velocity.X) < 1.4f)
				npc.velocity.X += npc.direction * 0.014f;
			npc.velocity.Y += 0.036f;
            npc.ai[0]++;
		}
		void FireArrowsAI(NPC npc) //No hitbox when firing arrows
		{
			Player target = Main.player[npc.target];

            int time = (int)npc.ai[0];
			if (5 < time && time < 55)
			{
                npc.direction = MathF.Sign(target.position.X - npc.position.X);

                //Lossy compression into only ai[3] because for some reason ai[2] turns the archer INVISIBLE when used ???
                //Didn't want projectile fired DIRECTLY at player, so there's some opportunity to respond
                //npc.ai[3] = Pack(target.Center.ToTileCoordinates16());
                Vector2 aimDirection = (target.Center - npc.Center);
                aimDirection.Y -= MathF.Abs(aimDirection.X) * 0.02f;
                float rotation = aimDirection.ToRotation();
                npc.ai[3] = rotation;

                if (time > 50)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Dust d = Dust.NewDustDirect(npc.Center + new Vector2(0, -4), 4, 4, Terraria.ID.DustID.Torch);
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1.6f);
                        d.velocity = npc.DirectionTo(target.Center) * (3.5f - d.scale) * i;
                        d.velocity = d.velocity.RotatedByRandom(MathHelper.ToRadians(10));
                    }
                }
			}
			else if (time > 75)
			{
				//Just add new projectile types in if you want to adjust this I guess I dunno
				int[] arrowsToFire = new int[] { Terraria.ID.ProjectileID.WoodenArrowHostile, Terraria.ID.ProjectileID.FireArrow };

                float rotation = npc.ai[3];
                Vector2 vel = Vector2.UnitX.RotatedBy(rotation) * 9f;
				Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, vel, Main.rand.Next(arrowsToFire), TCellsUtils.ScaledHostileDamage(npc.damage, 1.5f, 2f), 1f, Main.myPlayer);
				proj.hostile = true;
				proj.friendly = false;

                if (npc.TargetInAggroRange(NumberHelpers.ToTileDist(22), false))
                {
                    ResetAI(npc);
                    npc.ai[2] = npc.direction;
                    npc.ai[1] = ApproachTarget;
                    return;
                }
				else
				{
                    ResetAI(npc);
                    npc.ai[2] = npc.direction;
					npc.ai[1] = Idle;
                    return;
				}
			}
			npc.velocity.X *= 0.9f;
			npc.ai[0]++;
		}

        public override bool FindFrame(NPC npc, int frameHeight)
        {
            //Frames 0-4 = Aim bow down-up
            //Frames 5,6 = ???
            //Frames 7+ = Walk Cycle

            if (npc.ai[1] == FireArrows && npc.ai[0] > 5)
            {
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
                int frameNum = (int)((mult + 0.025f) * 4);
                frameNum = (int)MathHelper.Clamp(frameNum, 0, 4);
                npc.frame.Y = frameNum * frameHeight;
            }
            else
            {
                npc.frameCounter += (int)(npc.velocity.X);
                if (Math.Abs(npc.frameCounter) > 4)
                {
                    const int Offset = 7;
                    int frameCount = Main.npcFrameCount[npc.type];
                    int cFrame = npc.frame.Y / frameHeight;
                    int newFrame = cFrame + Math.Sign(npc.frameCounter * npc.spriteDirection);
                    newFrame = Offset + (((newFrame - Offset) + (frameCount - Offset)) % (frameCount - Offset));
                    npc.frame.Y = newFrame * frameHeight;
                    npc.frameCounter = 0;
                }
            }
            return false;
        }
    }
}