using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static TerrariaCells.Common.Utilities.NPCHelpers;
using Microsoft.Xna.Framework.Graphics;

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
			if (!npc.HasValidTarget)
			{
				IdleAI(npc);
				return;
			}

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
			CombatNPC.ToggleContactDamage(npc, false);
			if (npc.TargetInAggroRange(420))
			{
				npc.ai[2] = Main.rand.Next(new int[] { -1, 1 });
				npc.ai[1] = Orbit;
				return;
			}

			npc.rotation += MathHelper.ToRadians(3 * npc.direction);
			npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 2f, 0.33f);

			if (!npc.dontTakeDamage)
			{
				npc.dontTakeDamage = true;
				npc.netUpdate = true;
			}
		}

		private void OrbitAI(NPC npc)
		{
			int timer = (int)npc.ai[0];
			Player target = Main.player[npc.target];
			Vector2 orbitPeak = target.Center + new Vector2(0, -(16 * 16));
			orbitPeak.X += 24 * MathF.Sin(npc.whoAmI);
			orbitPeak.Y += 32 * MathF.Cos(npc.whoAmI);

			float xModifier = MathF.Sin(timer * 0.0125f * npc.ai[2]);
			float yModifier = (1 - MathF.Cos(timer * 0.025f)) * 0.5f;
			Vector2 movePos = orbitPeak + new Vector2(xModifier * (4 * 16), yModifier * (5 * 16));

			//Dust.NewDustDirect(movePos, 1, 1, DustID.GemDiamond).velocity = Vector2.Zero;

			int moveDirX = MathF.Sign(movePos.X - npc.position.X);
			int moveDirY = MathF.Sign(movePos.Y - npc.position.Y);
			if(npc.velocity.X * moveDirX < 3.6f)
				npc.velocity.X += moveDirX * 0.1f;
			if(npc.velocity.Y * moveDirY < 3.6f)
				npc.velocity.Y += moveDirY * 0.075f;

			//if (MathF.Abs(npc.position.X - movePos.X) > 160) npc.noTileCollide = true;
			//else if (MathF.Abs(npc.position.X - movePos.X) > 80) npc.velocity.X *= 0.97f;
			//else npc.noTileCollide = false;

			if (npc.velocity.Y < 0 && npc.position.Y < movePos.Y - (4 * 16)) npc.velocity.Y += 0.125f;
			if (npc.velocity.Y > 0 && npc.position.Y > movePos.Y + (1 * 16)) npc.velocity.Y -= 0.175f;

			npc.rotation = npc.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2;
			
			//Insane amount of randomness, because this is run every tick, and will be weighted towards lower numbers
			if(timer > 150 + Main.rand.Next(150))
			{
				npc.ai[1] = Charge;
				npc.ai[0] = 0;
				//npc.noTileCollide = false;
				return;
			}

			npc.ai[0]++;
		}

		private void ChargeAI(NPC npc)
		{
			if (npc.dontTakeDamage)
			{
				npc.dontTakeDamage = false;
				npc.netUpdate = true;
			}

			CombatNPC.ToggleContactDamage(npc, true);
			Player target = Main.player[npc.target];

			int timer = (int)npc.ai[0];

			if (timer < 15)
			{
				npc.velocity *= 0.95f;
				npc.rotation = npc.DirectionTo(target.Center).ToRotation() - MathHelper.PiOver2;
				//npc.noTileCollide = false;
			}
			else if (timer < 30)
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
				if (npc.ai[2] != 0)
				{
					npc.velocity = Vector2.Lerp(npc.velocity, Vector2.UnitX.RotatedBy(npc.rotation + MathHelper.PiOver2) * 10f, 0.3f);
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
				CombatNPC.ToggleContactDamage(npc, false);
			}
			if (timer > 45)
			{
				npc.ai[1] = Idle;
				npc.ai[0] = 0;
				return;
			}

			npc.ai[0]++;
		}

		public override bool PreDraw(NPC npc, SpriteBatch spritebatch, Vector2 screenPos, Color lightColor)
		{
			if (!npc.dontTakeDamage)
			{
				lightColor = Color.Lerp(lightColor, Color.DarkRed, 0.33f);
			}
			return base.PreDraw(npc, spritebatch, screenPos, lightColor);
		}
	}
}
