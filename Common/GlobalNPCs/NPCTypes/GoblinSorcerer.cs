using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class GoblinSorcerer : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType.Equals(Terraria.ID.NPCID.GoblinSorcerer);
		}

		const int Idle = 0;
		const int Casting = 1;
		const int Teleporting = 2;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest();

			switch (npc.Phase())
			{
				case Idle:
					IdleAI(npc);
					break;
				case Casting:
					CastingAI(npc);
					break;
				case Teleporting:
					TeleportingAI(npc);
					break;
				default:
					npc.Phase(Teleporting);
					break;
			}
		}
		private void IdleAI(NPC npc)
		{
			if (
				npc.TryGetTarget(out Entity target)
				&& npc.TargetInAggroRange(target, 480, false))
			{
				npc.dontTakeDamage = false;
				npc.Phase(Casting);
				return;
			}

			npc.dontTakeDamage = true;

			if (Main.rand.NextBool(3))
				return;

			for (int i = 0; i < 2; i++)
			{
				Dust dust = Dust.NewDustDirect(npc.Center + (Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 24), 1, 1, Terraria.ID.DustID.Shadowflame);
				dust.scale = Main.rand.NextFloat(0.95f, 1.25f);
				dust.noGravity = true;
				dust.velocity = new Vector2(0, (dust.position.Y - npc.Center.Y) * 0.12f);
			}
		}
		private void CastingAI(NPC npc)
		{
			int timer = npc.Timer();
			if(timer == 0)
				CombatNPC.ToggleContactDamage(npc, true);
			if (timer % 45 == 0)
			{
				NPC ball = NPC.NewNPCDirect(npc.GetSource_FromAI(), npc.Center, Terraria.ID.NPCID.ChaosBall);
				ball.velocity = npc.DirectionTo(Main.player[npc.target].Center) * 6f;
				ball.damage = npc.damage / 2;
				ball.netUpdate = true;
			}
			if (timer > 45 * 3)
			{
				npc.Phase(Teleporting);
			}
			npc.velocity.X *= 0.8f;
			npc.Timer(npc.Timer() + 1);
		}
		private void TeleportingAI(NPC npc)
		{
			if (npc.Timer() < 5)
			{
				Player target = Main.player[npc.target];
				int direction = target.direction;

				const int PxPerTile = 16;
				const int MinDistance = 128;
				const int MaxDistance = 512;
				const int RayCount = 9;
				const int TotalAngle = 90;

				Vector2[] rays = new Vector2[RayCount];
				for (int i = 0; i < RayCount; i++)
				{
					float rayAngle = (float)((i - (RayCount / 2)) / (float)RayCount) * TotalAngle;
					rays[i] = new Vector2(-direction, 0).RotatedBy(MathHelper.ToRadians(rayAngle)) * PxPerTile;
				}

				for (int i = 0; i < RayCount; i++)
				{
					Vector2 start = target.Center * rays[i] * (MaxDistance / PxPerTile);
					for (int j = 0; j < MaxDistance / PxPerTile; j++)
					{
						Rectangle tpRect = new Rectangle((int)start.X - (npc.width / 2), (int)start.Y - (npc.height / 2), npc.width, npc.height);
						//if (!Collision.SolidTiles(tpRect.Location.ToVector2(), npc.width, npc.height)
						//	&& (Utilities.TCellsUtils.FindGround(tpRect).Y < tpRect.Bottom + (npc.height * 2)))
						//	start -= rays[i];
						if (Collision.CanHitLine(start, 8, 8, start + rays[i], 8, 8))
							start += rays[i];
						else
						{
							break;
						}
					}
					rays[i] = start;
					//Dust d = Dust.NewDustDirect(start, 1, 1, Terraria.ID.DustID.GemDiamond);
					//d.noGravity = true;
					//d.velocity = Vector2.Zero;
				}
				List<Vector2> availablePositions = new List<Vector2>();
				availablePositions.AddRange(rays.Where(x =>
				{
					float len = (x - target.Center).Length();
					return len < (MaxDistance + PxPerTile)
					&& len > (MinDistance - PxPerTile);
				}));
				if (availablePositions.Count == 0)
				{
					availablePositions.Add(npc.position);
				}

				int index = Main.rand.Next(availablePositions.Count);
				Point pos = availablePositions[index].ToPoint();
				Vector2 ground = Utilities.TCellsUtils.FindGround(new Rectangle(pos.X, pos.Y, npc.width, npc.height), 40);

				npc.ai[2] = ground.X;
				npc.ai[3] = ground.Y;
			}
			if (npc.Timer() > 210)
			{
				for (int i = 0; i < 7; i++)
				{
					Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, Terraria.ID.DustID.Shadowflame);
					d.scale = Main.rand.NextFloat(1.33f, 1.67f);
				}
				npc.position = new Vector2(npc.ai[2], npc.ai[3] - npc.height);
				CombatNPC.ToggleContactDamage(npc, true);
				npc.Phase(Casting);
				return;
			}
			else if(npc.Timer() > 5 && MathF.Pow(npc.Timer(), 2) % 60 < 5)
			{
				Dust d = Dust.NewDustDirect(new Vector2(npc.ai[2], npc.ai[3]+2), npc.width, npc.height, Terraria.ID.DustID.Shadowflame);
				d.scale = Main.rand.NextFloat(1.33f, 1.67f);
				d.velocity.Y = -MathF.Abs(d.velocity.Y) * 0.67f - (1 - MathF.Abs(d.velocity.X));
			}
			npc.velocity.X *= 0.8f;
			npc.DoTimer();
		}
	}
}