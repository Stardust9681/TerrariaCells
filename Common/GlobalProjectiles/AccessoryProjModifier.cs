using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ID.ProjectileID;

namespace TerrariaCells.Common.GlobalProjectiles
{
	public class AccessoryProjModifier : GlobalProjectile
	{
		public override void SetDefaults(Projectile projectile)
		{
			if (IsARocket(projectile.type))
				Sets.RocketsSkipDamageForPlayers[projectile.type] = true;
		}
		private static readonly int[] ArrowTypes = new int[] {
			WoodenArrowFriendly, FireArrow, FrostArrow, CursedArrow, IchorArrow, VenomArrow, UnholyArrow, HolyArrow,
			JestersArrow, ChlorophyteArrow, HellfireArrow, BoneArrow, DD2BetsyArrow, FlamingArrow, FrostburnArrow,
			MoonlordArrow, ShadowFlameArrow, ShimmerArrow
		};
		private static readonly int[] BulletTypes = new int[] {
			Bullet, SilverBullet, BulletHighVelocity, ChlorophyteBullet, CrystalBullet, CursedBullet, ExplosiveBullet,
			GoldenBullet, IchorBullet, MoonlordBullet, NanoBullet, PartyBullet, VenomBullet
		};
		private static readonly int[] RocketTypes = new int[] {
			//Rockets (as from Rocket Launcher)
			RocketI, RocketII, RocketIII, RocketIV, ClusterRocketI, ClusterRocketII, MiniNukeRocketI, MiniNukeRocketII,
			//Snowmen (as from Snowman Cannon)
			RocketSnowmanI, RocketSnowmanII, RocketSnowmanIII, RocketSnowmanIV, ClusterSnowmanRocketI, ClusterSnowmanRocketII, MiniNukeSnowmanRocketI, MiniNukeSnowmanRocketII,
			//Grenades (as from Grenade Launcher)
			GrenadeI, GrenadeII, GrenadeIII, GrenadeIV, ClusterGrenadeI, ClusterGrenadeII, MiniNukeGrenadeI, MiniNukeGrenadeII,
			//Mines (as from Proximity Mine Launcher)
			ProximityMineI, ProximityMineII, ProximityMineIII, ProximityMineIV, ClusterMineI, ClusterMineII, MiniNukeMineI, MiniNukeMineII,
			//Fireworks (as from Celebration)
			RocketFireworkRed, RocketFireworkGreen, RocketFireworkBlue, RocketFireworkYellow,
			//Fireworks (as from Celebration Mk 2)
			Celeb2Rocket, Celeb2RocketExplosive, Celeb2RocketLarge, Celeb2RocketExplosiveLarge,
			//Missile (as from Electrosphere Launcher)
			ElectrosphereMissile,

			DryRocket, DryGrenade, DryMine, DrySnowmanRocket, //Dry
			WetRocket, WetGrenade, WetMine, WetSnowmanRocket, //Wet
			LavaRocket, LavaGrenade, LavaMine, LavaSnowmanRocket, //Lava
			HoneyRocket, HoneyGrenade, HoneyMine, HoneySnowmanRocket //Honey
		};
		internal static bool IsARocket(int projType) => RocketTypes.Contains(projType);
		internal static bool IsABullet(int projType) => BulletTypes.Contains(projType);
		internal static bool IsAnArrow(int projType) => ArrowTypes.Contains(projType);
		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (projectile.friendly && Main.player[projectile.owner].GetModPlayer<ModPlayers.AccessoryPlayer>().fuseKitten && IsARocket(projectile.type))
			{
				target.immune[projectile.owner] = 2;
			}
		}
		public override bool PreKill(Projectile projectile, int timeLeft)
		{
			if (projectile.friendly && Main.player[projectile.owner].GetModPlayer<ModPlayers.AccessoryPlayer>().fuseKitten && IsARocket(projectile.type))
			{
				const int NEWSIZE = 256;
				projectile.Resize(NEWSIZE, NEWSIZE);
				projectile.Damage();
				int dustCount = 10 + Main.rand.Next(5);
				int[] dustTypes = new int[] { Terraria.ID.DustID.FireworkFountain_Yellow, Terraria.ID.DustID.Electric };
				for (int i = 0; i < dustCount; i++)
				{
					Dust d = Dust.NewDustDirect(projectile.Center - new Vector2(NEWSIZE * 0.25f), NEWSIZE / 2, NEWSIZE / 2, dustTypes[Main.rand.Next(3) / 2]);
					d.noGravity = false;
					d.velocity = projectile.Center.DirectionTo(d.position) * Main.rand.NextFloat(3f, 6f);
					d.scale = Main.rand.NextFloat(1.1f, 1.4f);
				}

				int goreCount = 12 + Main.rand.Next(6);
				int[] goreTypes = new int[] { Terraria.ID.GoreID.Smoke1, Terraria.ID.GoreID.Smoke2, Terraria.ID.GoreID.Smoke3 };
				for (int i = 0; i < goreCount; i++)
				{
					Vector2 pos = projectile.Center + (Vector2.UnitX.RotateRandom(MathHelper.TwoPi) * Main.rand.NextFloat(8f));
					Gore.NewGore(projectile.GetSource_Death(), pos, projectile.Center.DirectionTo(pos) * 2f, Main.rand.Next(goreTypes));
					if (i % 2 == 0)
					{
						Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, Terraria.ID.DustID.Firefly);
						dust.noGravity = true;
						dust.velocity = new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
						dust.scale = Main.rand.NextFloat(1.2f, 1.6f);
					}
				}
				return false;
			}
			return base.PreKill(projectile, timeLeft);
		}
	}
}
