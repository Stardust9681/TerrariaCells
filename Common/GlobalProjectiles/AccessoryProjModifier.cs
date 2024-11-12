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
		public override void OnKill(Projectile projectile, int timeLeft)
		{
			if (projectile.friendly && Main.player[projectile.owner].GetModPlayer<ModPlayers.AccessoryPlayer>().fuseKitten && IsARocket(projectile.type))
			{
				const int NEWSIZE = 256;
				projectile.Resize(NEWSIZE, NEWSIZE);
				projectile.Damage();
				int dustCount = 6 + Main.rand.Next(4);
				int[] dustTypes = new int[] { Terraria.ID.DustID.MartianSaucerSpark, Terraria.ID.DustID.Electric };
				for (int i = 0; i < dustCount; i++)
				{
					Dust d = Dust.NewDustDirect(projectile.Center - new Vector2(NEWSIZE*0.25f), NEWSIZE/2, NEWSIZE/2, dustTypes[Main.rand.Next(3)/2]);
					d.noGravity = false;
					d.velocity = projectile.DirectionTo(d.position) * Main.rand.NextFloat(2.5f, 5f);
					d.scale = Main.rand.NextFloat(1.1f, 1.4f);
				}
			}
		}
	}
}
