using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerrariaCells.Common.Utilities;

using static TerrariaCells.Common.Utilities.NPCHelpers;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Reflection;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class BrainOfCthulhu : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType == NPCID.BrainofCthulhu;
		}

		public override void Behaviour(NPC npc)
		{
			int timer = npc.Timer();

			if (timer == 0)
			{
				//Basically combining the X/Y position in such a way that it can be extracted later:
				//X = (int)(npc.ai[1] / (Main.maxTilesY * 16));
				//Y = (int)(npc.ai[1] % X)
				//There are a couple edge cases, where the NPC is spawned at the world borders

				//npc.ai[1] = ((uint)npc.Center.X * worldHeight) + (uint)npc.Center.Y;
				npc.ai[1] = PositionToFloat(npc.Center);
				npc.DoTimer();
				npc.EncourageDespawn(0);
				npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
				npc.Opacity = 0;
				return;
			}

			//Vector2 centre = Vector2.Zero;
			//centre.X = (int)(npc.ai[1] / worldHeight);
			//centre.Y = (int)(npc.ai[1] - (centre.X * worldHeight));
			Vector2 centre = FloatToPosition(npc.ai[1]);

			CombatNPC globalNPC = npc.GetGlobalNPC<CombatNPC>();

			Systems.CameraManipulation.SetZoom(45, new Vector2(95, 55) * 16, null);
			Systems.CameraManipulation.SetCamera(45, centre - (Main.ScreenSize.ToVector2() * 0.5f));

			void Entrance()
			{
				const int Start = 0;
				const int End = 120;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				npc.velocity *= 0;

				globalNPC.allowContactDamage = false;

				if(timer < End - 15)
					npc.Opacity = 0;
				else if (timer == End - 15)
				{
					//Should add a check for whether gores are enabled or not
					//Would be weird for BoC to emerge, and there's just. Smoke? From the top???
					int[] goreTypes = new int[] {
						//Most of these literally don't have internal names to use with GoreID consts.
						//See https://terraria.wiki.gg/wiki/Gore_IDs for more details
						141, 142, 223, 225, 226, 237, 238, 352, 356, 393, 394, 395, 403, 951, 955, 1049, 1180, 1181, 1189
					};
					int goreCount = 9 + Main.rand.Next(4);
					for (int i = 0; i < goreCount; i++)
					{
						Gore.NewGore(
							npc.GetSource_FromAI(),
							centre + new Vector2(Main.rand.NextFloat(-45 * 16, 45 * 16), -32 * 16),
							Vector2.UnitY * Main.rand.NextFloat(2f) + Vector2.UnitX * Main.rand.NextFloat(-2f, 2f),
							Main.rand.Next(goreTypes));
					}

					npc.Opacity = 1;
				}
			}
			void WarnCharge()
			{
				const int Start = 105;
				const int End = 120;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;
			}
			void Charge()
			{
				const int Start = 120;
				const int End = 175;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				float offset = 320;
				if (timer == Start) npc.Center = centre + new Vector2(-offset, -48);

				npc.velocity.Y = 0;
				npc.velocity.X = 2 * offset / Duration;

				int opacityTime = 8;
				npc.Opacity = MathF.Min(1, Duration / (2f * opacityTime) - MathF.Abs(((2 * (timer - Start)) - Duration) / (2 * opacityTime)));
			}
			void WarnCross()
			{
				const int Start = 145;
				const int End = 175;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				if(timer == Start)
				{
					//Spawn Projectiles
					//Handle Telegraphs there
					//Projectiles will move on their own accordingly (pass in duration, and which one will have BoC)

					const int Dist = 320;
					Vector2[] positions = new Vector2[]
					{
						new Vector2(centre.X + -Dist, centre.Y + -Dist),
						new Vector2(centre.X + Dist, centre.Y + -Dist),
						new Vector2(centre.X + -Dist, centre.Y + Dist),
						new Vector2(centre.X + Dist, centre.Y + Dist),
					};
					int index = Main.rand.Next(4);
					npc.ai[2] = PositionToFloat(positions[index]);

					for (int i = 0; i < 4; i++)
					{
						Projectile proj = Projectile.NewProjectileDirect(
							npc.GetSource_FromAI(),
							positions[i],
							Vector2.Zero,
							ModContent.ProjectileType<TelegraphWarning>(),
							0,
							0,
							Main.myPlayer,
							centre.X,
							centre.Y,
							i == index ? TelegraphWarning.Yellow : TelegraphWarning.Red
						);
						proj.localAI[0] = 0.333f;
						proj.timeLeft = Duration;
						proj.netUpdate = true;
					}
				}
			}
			void Cross()
			{
				const int Start = 175;
				const int End = 215;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				Vector2 newPos = FloatToPosition(npc.ai[2]);

				if (timer == Start)
				{
					const int Dist = 320;
					Vector2[] positions = new Vector2[]
					{
						new Vector2(centre.X + -Dist, centre.Y + -Dist),
						new Vector2(centre.X + Dist, centre.Y + -Dist),
						new Vector2(centre.X + -Dist, centre.Y + Dist),
						new Vector2(centre.X + Dist, centre.Y + Dist),
					};
					foreach (Vector2 pos in positions)
					{
						if (PositionToFloat(pos) == npc.ai[2]) continue;
						Projectile proj = Projectile.NewProjectileDirect(
							npc.GetSource_FromAI(),
							pos,
							Vector2.Zero,
							ModContent.ProjectileType<IllusionBrainHitbox>(),
							TCellsUtils.ScaledHostileDamage(npc.damage),
							1f,
							Main.myPlayer,
							PositionToFloat(pos),
							PositionToFloat(centre),
							Duration);
						proj.timeLeft = Duration;
						proj.netUpdate = true;
					}
					npc.Center = newPos;
				}

				npc.Center = TCellsUtils.LerpVector2(newPos, (2*centre)-newPos, timer - Start, Duration, TCellsUtils.LerpEasing.InOutSine);

				int opacityTime = 6;
				npc.Opacity = MathHelper.Clamp(Duration / (2f * opacityTime) - MathF.Abs(((2 * (timer-Start)) - Duration) / (2 * opacityTime)), 0, 1);
				if (timer == End) npc.Opacity = 0;
			}
			void Tendrils()
			{
				const int Start = 230;
				const int End = 2668;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				if (Main.netMode == NetmodeID.MultiplayerClient) return;

				Vector2 vel = Vector2.Zero;
				if (LeftTentacles.Contains(timer - Start)) vel.X = 4;
				if (RightTentacles.Contains(timer - Start)) vel.X = -4;
				if (vel.X != 0)
				{
					Vector2 position = centre + new Vector2(44 * 16 * -MathF.Sign(vel.X), (36.5f * 16) - (Main.rand.Next(PlatformHeights) * 16));
					Projectile proj = Projectile.NewProjectileDirect(
						npc.GetSource_FromAI(),
						position,
						Vector2.Zero,
						ModContent.ProjectileType<TelegraphWarning>(),
						0,
						0,
						Main.myPlayer,
						(position + (vel * 16 * 6)).X,
						(position + (vel * 16 * 6)).Y,
						TelegraphWarning.Yellow
					);
					proj.localAI[0] = 0.25f;
					proj.timeLeft = 25;
					proj.netUpdate = true;

					proj = Projectile.NewProjectileDirect(
						npc.GetSource_FromAI(),
						position,
						Vector2.Zero,
						ModContent.ProjectileType<TendrilAttack>(),
						TCellsUtils.ScaledHostileDamage(20),
						1f,
						Main.myPlayer,
						position.X,
						centre.X,
						40
						);
					proj.timeLeft = 65;
					proj.netUpdate = true;
				}
			}
			void WarnMoveRandom()
			{
				const int Start = 215;
				const int End = 248;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				if (timer == Start)
				{
					npc.ai[2] = Main.rand.NextFloat(MathHelper.TwoPi);
					Vector2 targetDirection = Vector2.UnitX.RotatedBy(npc.ai[2]);
					Projectile proj = Projectile.NewProjectileDirect(
							npc.GetSource_FromAI(),
							centre,
							Vector2.Zero,
							ModContent.ProjectileType<TelegraphWarning>(),
							0,
							0,
							Main.myPlayer,
							(centre + (targetDirection * 240f)).X,
							(centre + (targetDirection * 240f)).Y,
							TelegraphWarning.Yellow
						);
					proj.localAI[0] = 0.333f;
					proj.timeLeft = Duration;
				}
			}
			void MoveRandom()
			{
				const int Start = 248;
				const int End = 2000;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				if (timer == Start)
				{
					npc.Center = centre;
					Vector2 targetDirection = Vector2.UnitX.RotatedBy(npc.ai[2]);
					npc.velocity = targetDirection * 4;
				}
				if (timer == End)
				{
					npc.dontTakeDamage = false;
				}

				int opacityTime = 15;
				if (timer < Start + opacityTime) npc.Opacity = MathF.Min(1, Duration / (2f * opacityTime) - MathF.Abs(((2f * (timer - Start)) - Duration) / (2f * opacityTime)));
				else npc.Opacity = 1;

				globalNPC.allowContactDamage = false;

				if (npc.Center.Y < centre.Y - (20 * 16) || npc.Center.Y > centre.Y + (8 * 16))
					npc.velocity.Y = -npc.velocity.Y;
				if (MathF.Abs(npc.Center.X - centre.X) > 40 * 16)
					npc.velocity.X = -npc.velocity.X;

				//Previous behaviour, I used NPC collision to bounce around, but..
				//Slopes.... Fuck slopes dude. I ain't doing that. You'd have to offer some crazy shit for me to take that with this.
				/*if (npc.collideY)
				{
					Vector2 collideVector = Collision.TileCollision(npc.oldPosition, npc.oldVelocity, npc.width, npc.height, true, true);
					if (npc.velocity.Y > 0 && collideVector.Y > npc.velocity.Y * 1.1f)
					{
						npc.position = npc.oldPosition + npc.oldVelocity;
						npc.velocity = npc.oldVelocity;
					}
					else
					{
						npc.position -= 2 * npc.oldVelocity;
						npc.velocity.Y = -npc.oldVelocity.Y;
					}
				}
				if (npc.Center.Y < centre.Y - 256 || npc.Center.Y > centre.Y + 256)
				{
					npc.position -= 2 * npc.oldVelocity;
					npc.velocity.Y = -npc.oldVelocity.Y;
				}
				if (npc.collideX)
				{
					npc.position -= 2 * npc.velocity;
					npc.velocity.X = -npc.oldVelocity.X;
				}*/
			}
			void Fall()
			{
				const int Start = 2000;
				const int End = 2668;
				const int Duration = End - Start;

				if (timer < Start) return;
				if (timer > End) return;

				if (timer == Start)
				{
					npc.noTileCollide = false;
				}
				if (timer == End)
				{
					npc.rotation = 0;
					npc.noTileCollide = true;
					npc.Timer(105);
				}

				npc.velocity.X *= 0.995f;
				npc.velocity.Y += 0.1f;
				if (npc.collideX)
				{
					npc.position -= 2 * npc.velocity;
					npc.velocity.X *= -1.3f;
				}

				npc.rotation += npc.velocity.X * 0.025f;
			}

			Entrance();
			WarnCharge();
			Charge();
			WarnCross();
			Cross();
			Tendrils();
			WarnMoveRandom();
			MoveRandom();
			Fall();

			npc.DoTimer();
		}

		readonly int[] RightTentacles = new int[] {
			233, 285, 336, 387, 439, 490, 542, 593, 612, 645, 695, 747, 798, 851, 900, 1056,
			1107, 1159, 1210, 1261, 1313, 1365, 1467, 1518, 1570, 1621, 1673, 1724, 1776,
			1826, 1877, 1930, 1981, 2032, 2083, 2135, 2186, 2237, 2289, 2342, 2392, 2444,
			2495, 2546, 2598, 2623, 2630, 2650, 2662, 2668
		};

		readonly int[] LeftTentacles = new int[] {
			260, 310, 362, 413, 465, 516, 567, 614, 671, 721, 772, 824, 874, 1082, 1133, 1184,
			1236, 1287, 1338, 1493, 1544, 1595, 1647, 1698, 1749, 1801, 1853, 1904, 1955, 2006,
			2057, 2109, 2161, 2213, 2265, 2315, 2366, 2419, 2470, 2521, 2572
		};

		readonly int[] PlatformHeights = new int[] {
			26, 31, 37, 43
		};

		public override bool FindFrame(NPC npc)
		{
			npc.frameCounter++;
			if (npc.dontTakeDamage)
			{
				if (npc.frameCounter > 8)
				{
					npc.frame.Y += npc.frame.Height;
					if (npc.frame.Y > npc.frame.Height * 3)
						npc.frame.Y = 0;
					npc.frameCounter = 0;
				}
			}
			else
			{
				if (npc.frameCounter > 5)
				{
					npc.frame.Y += npc.frame.Height;
					if (npc.frame.Y > npc.frame.Height * 7)
						npc.frame.Y = npc.frame.Height * 4;
					npc.frameCounter = 0;
				}
			}
			return false;
		}
	}

	/// <summary>
	/// <para><c><see cref="Projectile.ai"/>[0]</c> --> Target position X</para>
	/// <para><c><see cref="Projectile.ai"/>[1]</c> --> Target position Y</para>
	/// <para><c><see cref="Projectile.ai"/>[2]</c> --> Index of colour to use in <c><see cref="Terraria.Utils.DrawLine(SpriteBatch, Vector2, Vector2, Color, Color, float)"/></c></para>
	/// <para><c><see cref="Projectile.localAI"/>[0]</c> --> Width multiplier (multiplied into <c><see cref="Projectile.timeLeft"/></c>)</para>
	/// </summary>
	internal class TelegraphWarning : ModProjectile
	{
		internal const int Transparent = 0;
		internal const int Violet = 1;
		internal const int Indigo = 2;
		internal const int Blue = 3;
		internal const int Turquoise = 4;
		internal const int Green = 5;
		internal const int YellowGreen = 6;
		internal const int Yellow = 7;
		internal const int Orange = 8;
		internal const int RedOrange = 9;
		internal const int Red = 10;
		internal const int VioletRed = 11;
		private static readonly Color[] Colors = new Color[]
		{
			Color.Transparent,
			Color.Violet,
			Color.Indigo,
			Color.Blue,
			Color.Turquoise,
			Color.Green,
			Color.GreenYellow,
			Color.Yellow,
			Color.Orange,
			Color.OrangeRed,
			Color.Red,
			Color.MediumVioletRed,
		};

		public override string Texture => $"Terraria/Images/Projectile_{Terraria.ID.ProjectileID.None}";

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.penetrate = -1;
			Projectile.aiStyle = -1;
		}

		public override bool PreAI()
		{
			Projectile.position -= Projectile.velocity;
			return base.PreAI();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float width = Projectile.localAI[0] * Projectile.timeLeft;
			if (width == 0)
				return false;
			//Utils.DrawLine(Main.spriteBatch, Projectile.Center, Projectile.Center + Projectile.velocity, Colors[(int)Projectile.ai[2]], Color.Transparent, width);
			//Utils.DrawLine(Main.spriteBatch, Projectile.Center, FloatToPosition(Projectile.ai[0]), Colors[(int)Projectile.ai[2]], Color.Transparent, width);
			Utils.DrawLine(Main.spriteBatch, Projectile.Center, new Vector2(Projectile.ai[0], Projectile.ai[1]), Colors[(int)Projectile.ai[2]], Color.Transparent, width);
			return false;
		}
	}

	/// <summary>
	/// <para><c><see cref="Projectile.ai"/>[0]</c> --> Start position (use <c><see cref="NPCHelpers.PositionToFloat(Vector2)"/></c>)</para>
	/// <para><c><see cref="Projectile.ai"/>[1]</c> --> Midway point (use <c><see cref="NPCHelpers.PositionToFloat(Vector2)"/></c>)</para>
	/// <para><c><see cref="Projectile.ai"/>[2]</c> --> Duration of attack (in ticks)</para>
	/// </summary>
	internal class IllusionBrainHitbox : ModProjectile
	{
		private static Rectangle? BoCFrame = null;

		public override string Texture => $"Terraria/Images/NPC_{Terraria.ID.NPCID.BrainofCthulhu}";
		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.width = 160;
			Projectile.height = 110;
		}

		public override void AI()
		{
			if (BoCFrame == null || !BoCFrame.HasValue)
			{
				NPC brain = Main.npc.First(x => x.type.Equals(NPCID.BrainofCthulhu));
				BoCFrame = brain.frame;
			}
			Vector2 start = FloatToPosition(Projectile.ai[0]);
			Vector2 centre = FloatToPosition(Projectile.ai[1]);

			Projectile.Center = TCellsUtils.LerpVector2(start, (2 * centre) - start, Projectile.ai[2] - Projectile.timeLeft, Projectile.ai[2], TCellsUtils.LerpEasing.InOutSine);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			//Not sure why tML provides spritebatch for NPC PreDraw, but not projectile :|

			Main.spriteBatch.Draw(
				Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu].Value,
				Projectile.position - Main.screenPosition,
				BoCFrame ?? new Rectangle(0, 0, Projectile.width, Projectile.height),
				lightColor,
				0,
				Vector2.Zero,
				1f,
				SpriteEffects.None,
				0);

			return false;
		}
	}

	/// <summary>
	/// [0] -> Anchor
	/// [1] -> Target
	/// [2] -> Trigger Point (timeLeft in ticks when projectile should shoot out)
	/// </summary>
	internal class TendrilAttack : ModProjectile
	{
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.None}";

		public ref float Anchor => ref Projectile.ai[0];
		public ref float Target => ref Projectile.ai[1];
		public int Direction => Target.CompareTo(Anchor);

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 8;
			Projectile.hostile = true;
			Projectile.damage = 25;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			float lerpValue;
			if (Projectile.timeLeft < Projectile.ai[2] * 0.5f)
				lerpValue = (6f / MathF.Pow(Projectile.ai[2], 2f)) * MathF.Pow(Projectile.timeLeft, 2f);
			else if (Projectile.timeLeft < Projectile.ai[2])
				lerpValue = (6f / MathF.Pow(Projectile.ai[2], 2f)) * MathF.Pow(Projectile.timeLeft - Projectile.ai[2], 2);
			else goto IgnorePosition;

			lerpValue = MathF.Min(lerpValue, 1);
			if (Direction < 0)
			{
				Projectile.position.X = MathHelper.Lerp(Anchor, Target, lerpValue);
			}
			Projectile.width = (int)MathHelper.Lerp(0, MathF.Abs(Target - Anchor), lerpValue);

		IgnorePosition:
			return;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft < Projectile.ai[2])
			{
				int segmentsCount = Projectile.width / 28;
				int remainder = Projectile.width - (segmentsCount * 28);
				Vector2 anchor = new Vector2(Anchor, Projectile.position.Y) - Main.screenPosition;
				Vector2 start = anchor;
				Color drawColour = Color.Lerp(Color.OrangeRed, Color.DarkRed, 0.4f);
				for (int i = 0; i < segmentsCount; i++)
				{
					Main.EntitySpriteDraw(
						Terraria.GameContent.TextureAssets.Chains[0].Value,
						anchor,
						null,
						drawColour,
						MathHelper.PiOver2,
						Vector2.Zero,
						1f,
						Direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
					anchor.X += 28 * Direction;
				}
				Main.EntitySpriteDraw(
						Terraria.GameContent.TextureAssets.Chains[0].Value,
						anchor,
						new Rectangle(0, 0, 10, remainder),
						drawColour,
						MathHelper.PiOver2,
						Vector2.Zero,
						1f,
						Direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
				//Utils.DrawLine(Main.spriteBatch, start + Main.screenPosition, anchor + Main.screenPosition, Color.Green);
				//Utils.DrawLine(Main.spriteBatch, Projectile.position, Projectile.position + new Vector2(Projectile.width, 0), Color.Orange);
			}
			return false;
		}
	}
}