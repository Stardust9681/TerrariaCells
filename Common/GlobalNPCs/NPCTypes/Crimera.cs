using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class Crimera : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType is NPCID.Crimera or NPCID.BigCrimera or NPCID.LittleCrimera;
		}

		const int Idle = 0;
		const int Orbit = 1;
		const int Charge = 2;
		const int Stun = 3;

		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest(false);

			switch ((int)npc.ai[1])
			{
				case Idle:
					IdleAI(npc);
					break;
				case Orbit:
					OrbitAI(npc);
					break;
				case Charge:
					ChargeAI(npc);
					break;
				case Stun:
					StunAI(npc);
					break;
			}
		}

		private void IdleAI(NPC npc)
		{
			npc.rotation += MathHelper.ToRadians(3 * npc.direction);
			npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 2f, 0.33f);

			if (npc.TargetInAggroRange(360))
			{
				npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
				npc.ai[2] = Main.rand.Next(new int[] { -1, 1 });
				npc.dontTakeDamage = true;
				npc.ai[1] = Orbit;
				return;
			}
		}

		private void OrbitAI(NPC npc)
		{
			int timer = (int)npc.ai[0];
			Player target = Main.player[npc.target];
			Vector2 orbitPeak = target.Center + new Vector2(0, -240);

			float xModifier = MathF.Sin(timer * 0.0125f * npc.ai[2]);
			float yModifier = (1 - MathF.Cos(timer * 0.025f)) * 0.5f;
			Vector2 movePos = orbitPeak + (new Vector2(xModifier, yModifier) * 96);
			Dust.NewDustDirect(movePos, 1, 1, DustID.GemDiamond).velocity = Vector2.Zero;

			npc.velocity.X += MathF.Sign(movePos.X - npc.position.X) * 0.06f;
			npc.velocity.Y += MathF.Sign(movePos.Y - npc.position.Y) * 0.075f;

			if (MathF.Abs(npc.position.X - movePos.X) > 64) npc.velocity.X *= 0.97f;
			if (npc.position.Y < movePos.Y - 32) npc.velocity.Y *= 0.97f;
			if (npc.velocity.Y > 0 && npc.position.Y > movePos.Y + 64) npc.velocity.Y *= 0.95f;

			npc.rotation = npc.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2;

			if (!npc.TargetInAggroRange(420))
			{
				npc.ai[1] = Idle;
				npc.ai[0] = 0;
				return;
			}
			else if(timer > 320 + Main.rand.Next(60))
			{
				npc.ai[1] = Charge;
				npc.ai[0] = 0;
				return;
			}

			npc.ai[0]++;
		}

		private void ChargeAI(NPC npc)
		{
			npc.GetGlobalNPC<CombatNPC>().allowContactDamage = true;
			Player target = Main.player[npc.target];

			int timer = (int)npc.ai[0];

			if (timer < 20)
			{
				npc.velocity *= 0.95f;
				npc.rotation = npc.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2;
			}
			else if (timer < 40)
			{
				npc.velocity = Vector2.Lerp(npc.velocity, npc.DirectionFrom(target.Center) * 3, 0.1f);
				npc.rotation = npc.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2;
				if (npc.collideX || npc.collideY)
				{
					npc.position -= npc.oldVelocity * 2;
					npc.velocity = Vector2.Zero;
					npc.ai[0] = 40;
				}
			}
			else
			{
				npc.dontTakeDamage = false;
				if (npc.ai[2] != 0)
				{
					npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 8f, 0.3f);
				}
				if (npc.collideX || npc.collideY)
				{
					npc.ai[1] = Stun;
					npc.ai[0] = 0;
					npc.ai[2] = 0;
					return;
				}
			}

			npc.ai[0]++;
		}

		private void StunAI(NPC npc)
		{
			int timer = (int)npc.ai[0];
			npc.velocity *= 0.3f;

			if (timer == 10)
			{
				npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
			}
			if (timer > 40)
			{
				npc.dontTakeDamage = true;
				npc.ai[1] = Idle;
				npc.ai[0] = 0;
				return;
			}

			npc.ai[0]++;
		}
	}
}
