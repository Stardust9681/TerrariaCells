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

using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class VanillaReworksGlobalProjectile : GlobalProjectile
    {
        public override void Load()
        {
            IL_Projectile.StatusNPC += IL_Projectile_StatusNPC;
        }
        public override void Unload()
        {
            IL_Projectile.StatusNPC -= IL_Projectile_StatusNPC;
        }

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
				case ProjectileID.RubyBolt:
				case ProjectileID.EmeraldBolt:
					projectile.penetrate = 1;
					projectile.extraUpdates = 14;
					break;
                case ProjectileID.BabySpider:
                    projectile.usesIDStaticNPCImmunity = true;
                    projectile.idStaticNPCHitCooldown = 5;
                    break;
			}
		}

        //This had to be either detour-no-orig or IL
        //No orig could have other effects, I earnestly believe IL is going to be more friendly to existing functionality
        private void IL_Projectile_StatusNPC(ILContext context)
        {
            try
            {
                ReplaceProjectileDebuff(context, ProjectileID.InfernoFriendlyBolt, BuffID.OnFire3, BuffID.OnFire);
                ReplaceProjectileDebuff(context, ProjectileID.InfernoFriendlyBlast, BuffID.OnFire3, BuffID.OnFire);
            }
            catch (Exception x)
            {
                ModContent.GetInstance<TerrariaCells>().Logger.Error(x);
            }
        }
        private static bool ReplaceProjectileDebuff(ILContext context, int projID, int oldBuffID, int newBuffID)
        {
            ILCursor cursor = new ILCursor(context);

            if (!cursor.TryGotoNext(
                i => i.Match(OpCodes.Ldarg_0),
                i => i.Match(OpCodes.Ldfld),
                i => i.Match(OpCodes.Ldc_I4, projID)))
            {
                return false;
            }
            if (!cursor.TryGotoNext(
                    i => i.Match(OpCodes.Ldc_I4, oldBuffID)))
            {
                return false;
            }

            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, newBuffID);

            return true;
        }
        private static bool RemoveProjectileDebuff(ILContext context, int projID, int buffID)
        {
            ILCursor cursor = new ILCursor(context);

            if (!cursor.TryGotoNext(
                i => i.Match(OpCodes.Ldarg_0),
                i => i.Match(OpCodes.Ldfld),
                i => i.Match(OpCodes.Ldc_I4, projID)))
            {
                return false;
            }
            ILLabel beforeIf = cursor.MarkLabel();
            if (!cursor.TryGotoNext(
                    i => i.Match(OpCodes.Ldc_I4, buffID)))
            {
                return false;
            }
            ILLabel exitIf = default;
            if (!cursor.TryGotoPrev(
                MoveType.Before,
                i => i.MatchBneUn(out exitIf)))
            {
                return false;
            }

            cursor.GotoLabel(beforeIf);
            cursor.EmitBr(exitIf);

            return true;
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
            if (!projectile.DamageType.CountsAsClass(DamageClass.Melee))
            {
                modifiers.Knockback *= 0;
            }

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
                    if (target.HasBuff(BuffID.Poisoned) || target.HasBuff(BuffID.Bleeding))
                    {
                        modifiers.SetCrit();
                        modifiers.CritDamage += 0.25f;
                    }
                    break;
            }

			if (projectile.TryGetGlobalProjectile(out SourceGlobalProjectile gProj) && gProj.itemSource != null)
			{
				Item source = gProj.itemSource;
                bool honouraryBoss = target.type >= NPCID.EaterofWorldsHead && target.type <= NPCID.EaterofWorldsTail;
				if (source.type == ItemID.SniperRifle && (target.boss || honouraryBoss))
					ForceCrit = true;
				else if (source.type == ItemID.PhoenixBlaster && Content.WeaponAnimations.Gun.TryGetGlobalItem(source, out Content.WeaponAnimations.Gun gun))
				{
					if (gun.Ammo < 5)
						ForceCrit = true;
				}
				else if (source.type == ItemID.Minishark && target.DistanceSQ(Main.player[projectile.owner].Center) < MathF.Pow(5.5f*16, 2))
				{
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
					GlobalNPCs.BuffNPC.AddBuff(target, BuffID.Bleeding, 60 * 3, damageDone);
                    //target.AddBuff(BuffID.Bleeding, 60 * 5);
                    break;
				case ProjectileID.RubyBolt:
					GlobalNPCs.BuffNPC.AddBuff(target, BuffID.OnFire, 60 * 5, damageDone);
					break;
				case ProjectileID.EmeraldBolt:
					GlobalNPCs.BuffNPC.AddBuff(target, BuffID.Poisoned, 60 * 5, damageDone);
					break;

				case ProjectileID.ToxicCloud:
				case ProjectileID.ToxicCloud2:
				case ProjectileID.ToxicCloud3:
					GlobalNPCs.BuffNPC.AddBuff(target, BuffID.Poisoned, 60 * 20, damageDone);
					break;
                case ProjectileID.GladiusStab:
                    target.immune[projectile.owner] = 8;
                    break;
			}
        }

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!projectile.DamageType.CountsAsClass(DamageClass.Melee))
                projectile.knockBack = 0f;

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
			switch (projectile.type)
			{
				case ProjectileID.DesertDjinnCurse:
					projectile.velocity = Vector2.Zero;
					Lighting.AddLight(projectile.Center, Color.White.ToVector3() * 0.5f);
					return false;
				case ProjectileID.RockGolemRock:
					projectile.velocity.Y += 0.07f;
					break;
			}
			
			return base.PreAI(projectile);
		}

		public override void PostAI(Projectile projectile)
		{
			switch (projectile.type)
			{
				case ProjectileID.DD2ExplosiveTrapT1:
					if (projectile.localAI[0] > Projectile.GetExplosiveTrapCooldown(Main.player[projectile.owner]) - 5
						&& Main.projectile.Any(x => x.active && x.type == ProjectileID.DD2ExplosiveTrapT1Explosion))
					{
						projectile.timeLeft = 2;
						projectile.Kill();
						projectile.active = false;
					}
					break;
			}
		}
	}
}
