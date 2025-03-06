using Microsoft.Build.Execution;
using System;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using TerrariaCells.Content.Buffs;
using TerrariaCells.Content.Projectiles.HeldProjectiles;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class VanillaReworksGlobalProjectile : GlobalProjectile
    {
        // Used for stake launcher bonus damage
        private static int[] undeadNPCs = { NPCID.Zombie, NPCID.Skeleton };
        private static int[] bossNPCs = { NPCID.EyeofCthulhu };
        private static float stakeToUndeadDamageModifier = 1.5f;
        public bool ForceCrit = false;
        public override bool InstancePerEntity => true;
        public override void SetDefaults(Projectile projectile)
		{
			switch (projectile.type)
			{
				case ProjectileID.PurpleLaser:
				case ProjectileID.BulletHighVelocity:
					projectile.penetrate = 1;
					break;
				case ProjectileID.PulseBolt:
					projectile.penetrate = 3;
					break;
			}
		}

		public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (projectile.type == ProjectileID.PulseBolt)
            {
                projectile.penetrate--;
                if (projectile.penetrate <= 0)
                {
                    projectile.Kill();
                }
                else
                {
                    Collision.HitTiles(projectile.position, projectile.velocity, projectile.width, projectile.height);

                    int targetID = projectile.FindTargetWithLineOfSight();
                    if (targetID >= 0)
                    {
                        Vector2 newVel = projectile.DirectionTo(Main.npc[targetID].Center);
                        newVel.Normalize();
                        newVel *= projectile.oldVelocity.Length();

                        projectile.velocity = newVel;
                    }
                    else
                    {
                        if (Math.Abs(projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                        {
                            projectile.velocity.X = -oldVelocity.X;
                        }

                        if (Math.Abs(projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                        {
                            projectile.velocity.Y = -oldVelocity.Y;
                        }
                    }
                }

                return false;
            }
            else if (projectile.type == ProjectileID.GrenadeI)
            {
                if (Math.Abs(projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                {
                    projectile.velocity.X = -oldVelocity.X;
                }

                if (Math.Abs(projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                {
                    projectile.velocity.Y = -oldVelocity.Y;
                }
                projectile.velocity.Y -= 0.8f;
            }
            else if (projectile.type == ProjectileID.Starfury)
            {
                projectile.velocity = projectile.oldVelocity;
                return false;
            }

            return true;
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            switch (projectile.type)
            {
                case ProjectileID.Stake:
                    if (undeadNPCs.Contains(target.type))
                        modifiers.FinalDamage += stakeToUndeadDamageModifier;
                    break;
                case ProjectileID.Volcano:
                    modifiers.SetCrit();
                    break;
                case ProjectileID.Starfury:
                    modifiers.SetCrit();
                    break;
                case ProjectileID.GladiusStab:
                    if  (target.HasBuff(BuffID.Poisoned) || target.HasBuff(BuffID.BloodButcherer))
                        modifiers.SetCrit();
                    break;
            }

			if (projectile.TryGetGlobalProjectile(out SourceGlobalProjectile gProj) && gProj.itemSource != null)
			{
				Item source = gProj.itemSource;
				if (source.type == ItemID.SniperRifle && target.boss)
					ForceCrit = true;
				else if (source.type == ItemID.PhoenixBlaster && Content.WeaponAnimations.Gun.TryGetGlobalItem(source, out Content.WeaponAnimations.Gun gun))
				{
                    if (gun.Ammo < 5)
                        ForceCrit = true;
                }
			}

            if (ForceCrit)
            {
                modifiers.SetCrit();
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            switch (projectile.type)
            {
                case ProjectileID.FrostArrow:
                    if (hit.Crit)
                    {
                        target.AddBuff(ModContent.BuffType<FrozenEnemyDebuff>(), 60 * 3);
                    }
                    break;
                case ProjectileID.ToxicBubble:
                    target.AddBuff(BuffID.Oiled, 60 * 6);
                    for (int i = 0; i < target.buffType.Length; i++)
                    {
                        if (target.buffType[i] == BuffID.Poisoned)
                        {
                            target.DelBuff(i);
                            break;
                        }
                    }
                    break;
                case ProjectileID.Ale:
                    target.AddBuff(BuffID.Oiled, 60 * 8);
                    break;
                case ProjectileID.SawtoothShark:
					GlobalNPCs.BuffNPC.AddBuff(target, BuffID.Bleeding, 60 * 2, damageDone);
                    //target.AddBuff(BuffID.Bleeding, 60 * 5);
                    break;
            }
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (projectile.type == ProjectileID.Volcano && projectile.ai[1] != 1)
            {
                projectile.Kill();
            }
            else if (projectile.type == ProjectileID.Starfury && projectile.ai[1] != 1)
            {
                projectile.Kill();
            }

			//Disable gravestones (starting to get unsightly)
			if (ProjectileID.Sets.IsAGravestone[projectile.type])
			{
				projectile.Kill();
			}

            if (projectile.type == ProjectileID.BouncyDynamite)
            {
                projectile.timeLeft = 90;
            }
        }

        /*public override void AI(Projectile projectile)
        {
			//Literally wasn't doing anything ?
			/*if (projectile.type == ProjectileID.Starfury)
            {
                int targetID = projectile.FindTargetWithLineOfSight();
                if (targetID >= 0)
                {
                    Vector2 directionToTarget = projectile.DirectionTo(Main.npc[targetID].position);

                }
            }//
		}*/

		public override bool PreAI(Projectile projectile)
		{
			if (projectile.type == ProjectileID.DesertDjinnCurse)
			{
				projectile.velocity = Vector2.Zero;
				Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.5f);
				return false;
			}
			return base.PreAI(projectile);
		}
	}
}
