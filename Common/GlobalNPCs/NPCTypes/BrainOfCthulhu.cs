using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerrariaCells.Common.Utilities;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	internal class CameraPlayer : ModPlayer
	{
		private static Vector2 cameraTarget;
		private static float cameraLerp;
		private static int cameraTime;
		public static void SetCameraPosition(Vector2 target, int time, float lerp = 1)
		{
			cameraTarget = target;
			cameraTime = time;
			cameraLerp = lerp;
		}
		public override void ModifyScreenPosition()
		{
			if (Player.whoAmI == Main.myPlayer)
			{
				if (cameraTime > 0)
				{
					cameraTime--;
					Main.screenPosition = Vector2.Lerp(Main.screenPosition, cameraTarget - (Main.ScreenSize.ToVector2() * 0.5f), cameraLerp);
					cameraLerp = MathHelper.Clamp((cameraLerp * (cameraLerp + 1)), 0, 1); 
				}
			}
		}
	}
	public class BrainOfCthulhu : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType == NPCID.BrainofCthulhu;
		}

		//Yes I know the if/else if/else branching is SLOW, I know it's BAD, I will consider revisiting this and changing it LATER
		//Right now I JUST want this working, you are WELCOME to change it if you'd like
		public override void Behaviour(NPC npc)
		{
			int timer = npc.Timer();

			Vector2 topLeft = GetArenaCentre(npc) - (ArenaSize * 0.5f);
			Vector2 botRight = topLeft + ArenaSize;
			Dust.QuickBox(topLeft, botRight, 6, Color.Yellow, (d) => { d.velocity = Vector2.Zero; d.noGravity = true; });
			Dust.QuickDust(GetArenaCentre(npc), Color.Turquoise);

			if (timer == 0)
			{
				npc.ai[1] = NPCHelpers.Pack(npc.Center.ToTileCoordinates16());
			}
			//Originally: T < 120 // endTime:120
			if (timer < 120) EntranceCutscene(npc, 120, ref timer);
			//Originally: T < 175 // startTime:120, duration:55
			else if (timer < 175) Charge(npc, 120, 55, ref timer);
			//Originally: T < 185 // startTime:175, duration:10
			else if (timer < 185) FadeOut(npc, 175, 10, ref timer);

			//Originally: T < 200
			else if (timer < XTimeStart)
			{
				FadeIn(npc, 185, 15, ref timer);
				npc.velocity *= 0;
				//Fade in at {X1:32, Y1:14, X2:77, Y2:48}
				npc.position = GetArenaCentre(npc) - (ArenaSize * 0.5f);
			}
			if (timer == XTimeStart - 60)
			{
				Vector2 centre = GetArenaCentre(npc);
				Vector2 offset = ArenaSize * 0.5f;
				Vector2[] positions = new Vector2[] {
						centre + (offset * -1),
						centre + offset,
						centre + (offset * new Vector2(1, -1)),
						centre + (offset * new Vector2(-1, 1)),
						};
				Vector2 targetPos = Main.rand.Next(positions);
				npc.ai[2] = NPCHelpers.Pack(targetPos.ToTileCoordinates16());
			}
			if (timer == XTimeStart)
			{
				if (Main.netMode != NetmodeID.MultiplayerClient)
				{
					Vector2 centre = GetArenaCentre(npc);
					List<Vector2> positions = new List<Vector2> {
					centre - (ArenaSize*0.5f),
					centre + (ArenaSize*0.5f),
					centre + (new Vector2(ArenaSize.X, -ArenaSize.Y) *0.5f),
					centre + (new Vector2(-ArenaSize.X, ArenaSize.Y) *0.5f)
					};
					positions.RemoveAll(x => NPCHelpers.Pack(x.ToTileCoordinates16()).Equals(npc.ai[2]));
					foreach (Vector2 pos in positions)
					{
						Projectile proj = Projectile.NewProjectileDirect(
							npc.GetSource_FromAI(),
							pos,
							Vector2.Zero,
							ModContent.ProjectileType<IllusionBrainHitbox>(),
							TCellsUtils.ScaledHostileDamage(npc.damage),
							1f,
							Main.myPlayer);
					}
				}
			}
			//Originally: 200 < T < 240 // duration:40
			if (timer > XTimeStart && timer < XTimeEnd) XAttack(npc, 40, ref timer);
			//Originally: 230 < T < 240 // startTime:230, duration:10
			if (timer > 230 && timer < 240) FadeOut(npc, 230, 10, ref timer);

			//These handle the telegraphs on their own
			//Originally: T > 230 // startTime:230
			if (timer > 230) CheckTendrils(npc, 230, ref timer);

			//T == 218 (Prep for RandomMove)
			if (Main.netMode != NetmodeID.MultiplayerClient && timer == 218)
			{
				npc.ai[2] = MathHelper.ToRadians(Main.rand.NextFloat(30, 60)) + (Main.rand.Next(4) * MathHelper.PiOver2);
				npc.netUpdate = true;
			}
			//Originally: 248 < T < 258 // startTime:248, duration:10
			if (timer > 248 && timer < 258) FadeIn(npc, 248, 10, ref timer);
			//Originally: 258 < T < 2668 // startTime:258
			if (timer > 248 && timer < 2668) RandomMove(npc, 248, ref timer);

			//Originally: 2540 < T < 2668 // startTime:2540, endTime:2668
			if (timer > 2001 && timer < 2668) FallDown(npc, 2540, 2668, ref timer);

			//Originally: 2668 < T //timer=120
			if (timer > 2668) timer = 120;


			npc.Timer(timer + 1);
		}
		private void EntranceCutscene(NPC npc, int endTime, ref int timer)
		{
			Vector2 centre = GetArenaCentre(npc);
			npc.velocity *= 0;

			//From 0-120...
			CameraPlayer.SetCameraPosition(centre, endTime - timer, 0.05f);

			//At 105..
			if (timer == endTime - 15)
			{
				Main.instance.CameraModifiers.Add(new Terraria.Graphics.CameraModifiers.PunchCameraModifier(npc.Center, Vector2.UnitX, 2f, 2, 15));
				int[] goreTypes = new int[]
				{
						//Most of these literally don't have internal names to use with GoreID consts.
						//See https://terraria.wiki.gg/wiki/Gore_IDs for more details
						141, 142, 223, 225, 226, 237, 238, 352, 356, 393, 394, 395, 403, 951, 955, 1049, 1180, 1181, 1189
				};
				int goreCount = 9 + Main.rand.Next(4);
				for (int i = 0; i < goreCount; i++)
				{
					Gore.NewGore(
						npc.GetSource_FromAI(),
						centre + new Vector2(Main.rand.NextFloat(-ArenaSize.X*0.5f, ArenaSize.X*0.5f), ArenaSize.Y * -0.5f),
						Vector2.UnitY * Main.rand.NextFloat(2f) + Vector2.UnitX * Main.rand.NextFloat(-2f, 2f),
						Main.rand.Next(goreTypes));
				}
			}
			npc.Opacity = timer < (endTime - 15) ? 0 : 1;
		}
		private void Charge(NPC npc, int startTime, int duration, ref int timer)
		{
			if (timer <= startTime + 1)
			{
				npc.Center = GetArenaCentre(npc) + new Vector2(ArenaSize.X * 0.4f, 0);
				npc.velocity = Vector2.Zero;
			}
			float speed = (ArenaSize.X * 0.8f) / (float)duration;
			npc.velocity.X = -speed;
		}
		private void XAttack(NPC npc, int duration, ref int timer)
		{
			//Starts at {X=32, Y=14}
			Vector2 topLeft = GetArenaCentre(npc) - (ArenaSize * 0.5f);
			Vector2 botRight = topLeft + ArenaSize;
			float percent = TCellsUtils.GetLerpValue(timer, duration, TCellsUtils.LerpEasing.InOutBack, XTimeStart, false);
			npc.position = Vector2.Lerp(topLeft, botRight, percent);
		}
		private void CheckTendrils(NPC npc, int startTime, ref int timer)
		{
			//Offsetting the timer so that the tendril attacks still occur roughly as intended with whatever offset is provided
			int offsetTimer = timer - startTime + 230;
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				bool check = CheckLeft(npc, ref offsetTimer);
				if (!check)
					check = CheckRight(npc, ref offsetTimer);

				if (check)
					npc.netUpdate = true;
			}
		}
		private bool CheckLeft(NPC npc, ref int timer)
		{
			if (LeftTentacles.Contains(timer))
			{
				Projectile proj = Projectile.NewProjectileDirect(
					npc.GetSource_FromAI(),
					GetArenaCentre(npc) + new Vector2(ArenaSize.X * -0.5f, (ArenaSize.Y * 0.5f) - (Main.rand.Next(PlatformHeights) * 16)),
					Vector2.UnitX * 4f,
					ProjectileID.FlamesTrap,
					20,
					0,
					Main.myPlayer
				);
				proj.netUpdate = true;
				return true;
			}
			return false;
		}
		private bool CheckRight(NPC npc, ref int timer)
		{
			if (RightTentacles.Contains(timer))
			{
				Projectile proj = Projectile.NewProjectileDirect(
					npc.GetSource_FromAI(),
					GetArenaCentre(npc) + new Vector2(ArenaSize.X * 0.5f, (ArenaSize.Y * 0.5f) - (Main.rand.Next(PlatformHeights) * 16)),
					Vector2.UnitX * -4f,
					ProjectileID.FlamesTrap,
					20,
					0,
					Main.myPlayer
				);
				proj.netUpdate = true;
				return true;
			}
			return false;
		}
		private void RandomMove(NPC npc, int startTime, ref int timer)
		{
			npc.GetGlobalNPC<CombatNPC>().allowContactDamage = false;
			Vector2 centre = GetArenaCentre(npc);
			if (timer <= startTime + 1)
			{
				npc.position = centre;
				npc.velocity = Vector2.UnitX.RotatedBy(npc.ai[2]) * 4f;
			}

			Vector2 npcCentre = npc.Center;
			if (npcCentre.X < centre.X - (ArenaSize.X * 0.5f) + 10 || npcCentre.X > centre.X + (ArenaSize.X * 0.5f) - 10)
				npc.velocity.X *= -1;
			if (npcCentre.Y < centre.Y - (13*16) || npcCentre.Y > centre.Y + (13*16))
				npc.velocity.Y *= -1;
		}
		private void FallDown(NPC npc, int startTime, int endTime, ref int timer)
		{
			if (timer <= startTime + 1)
			{
				npc.dontTakeDamage = false;
				npc.netUpdate = true;
			}
			npc.velocity.Y += 0.012f;
			npc.rotation += npc.velocity.X * 0.04f;
			if (npc.position.Y > GetArenaCentre(npc).Y)
				npc.noTileCollide = false;

			if (timer >= endTime - 1)
			{
				npc.noTileCollide = true;
				npc.rotation = 0;
				npc.netUpdate = true;
			}
		}
		private void FadeOut(NPC npc, int startTime, int duration, ref int timer)
		{
			npc.velocity *= 0.8f;
			npc.Opacity = (float)(duration - (timer - startTime)) / (float)duration;
			if (timer >= startTime + duration - 1)
				npc.Opacity = 0;
		}
		private void FadeIn(NPC npc, int startTime, int duration, ref int timer)
		{
			npc.Opacity = (float)(timer - startTime) / (float)duration;
			if (timer >= startTime + duration - 1)
				npc.Opacity = 1;
		}


		public static Vector2 GetArenaCentre(NPC npc)
		{
			(ushort x, ushort y) = NPCHelpers.Unpack(npc.ai[1]);
			Terraria.DataStructures.Point16 worldCoords = new Terraria.DataStructures.Point16(x, y);
			return worldCoords.ToWorldCoordinates();
		}
		public static readonly Vector2 ArenaSize = new Vector2(105 * 16, 70 * 16);


		//Vector2 ArenaCenter => arenaAnchor + (arenaSize * 0.5f);

		//Vector2 XTopLeft => arenaAnchor + new Vector2(32, arenaSize.Y - 48);
		//Vector2 XTopRight => arenaAnchor + new Vector2(77, arenaSize.Y - 48);
		//Vector2 XBotLeft => arenaAnchor + new Vector2(32, arenaSize.Y - 14);
		//Vector2 XBotRight => arenaAnchor + new Vector2(77, arenaSize.Y - 14);

		const int TentaclesTimeOffset = 258;
		readonly int[] RightTentacles = new int[] {
			233, 285, 336, 387, 439, 490, 542, 593, 612, 645, 695, 747, 798, 851, 900, 1056,
			1107, 1159, 1210, 1261, 1313, 1365, 1467, 1518, 1570, 1621, 1673, 1724, 1776,
			1826, 1877, 1930, 1981, 2032, 2083, 2135, 2186, 2237, 2289, 2342, 2392, 2444,
			2495, 2546, 2598, 2623, 2630, 2650, 2662, 2668
		};
		const int MinX = 53;

		readonly int[] LeftTentacles = new int[] {
			260, 310, 362, 413, 465, 516, 567, 614, 671, 721, 772, 824, 874, 1082, 1133, 1184,
			1236, 1287, 1338, 1493, 1544, 1595, 1647, 1698, 1749, 1801, 1853, 1904, 1955, 2006,
			2057, 2109, 2161, 2213, 2265, 2315, 2366, 2419, 2470, 2521, 2572
		};
		const int MaxX = 54;

		readonly int[] PlatformHeights = new int[] {
			26, 31, 37, 43
		};

		public const int XTimeStart = 200;
		public const int XTimeEnd = 240;
	}

	internal class IllusionBrainHitbox : ModProjectile
	{
		public override string Texture => $"Terraria/Images/Projectile_{Terraria.ID.ProjectileID.None}";
		public override void SetStaticDefaults()
		{
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.timeLeft = 40;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			(ushort timer, ushort npcId) = NPCHelpers.Unpack(Projectile.ai[0]);
			(ushort startX, ushort startY) = NPCHelpers.Unpack(Projectile.ai[1]);
			Vector2 startPos = new Vector2(startX * 16 - 8, startY * 16 - 8);
			(ushort endX, ushort endY) = NPCHelpers.Unpack(Projectile.ai[2]);
			Vector2 endPos = new Vector2(endX * 16 - 8, endY * 16 - 8);
			float percent = TCellsUtils.GetLerpValue(timer, 40, TCellsUtils.LerpEasing.InOutBack, 0, false);
			Projectile.Center = Vector2.Lerp(startPos, endPos, percent);
			Projectile.ai[0] = NPCHelpers.Pack((ushort)(timer + 1), npcId);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			//Not sure why tML provides spritebatch for NPC PreDraw, but not projectile :|

			NPC brain = Main.npc[NPCHelpers.Unpack(Projectile.ai[0]).Item2];
			Main.EntitySpriteDraw(
				Terraria.GameContent.TextureAssets.Npc[NPCID.BrainofCthulhu].Value,
				Projectile.Center - (brain.Size * 0.5f),
				brain.frame,
				Color.White,
				0,
				Vector2.Zero,
				1f,
				Microsoft.Xna.Framework.Graphics.SpriteEffects.None,
				0);

			return false;
		}
	}
}
