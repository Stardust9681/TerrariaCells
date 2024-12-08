using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class ProjectileChanges : GlobalProjectile
    {
        public override void SetDefaults(Projectile entity)
        {

            base.SetDefaults(entity);
            if (entity.type == ProjectileID.PulseBolt)
            {
                entity.penetrate = 2;
            }
        }
        
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ProjectileID.ToxicBubble)
            {
                target.AddBuff(BuffID.Oiled, 300);
            }
            if (projectile.type == ProjectileID.RocketI || projectile.type == ProjectileID.GrenadeI)
            {
                projectile.hostile = true;

            }
            if (projectile.type == ProjectileID.FrostArrow)
            {
                target.AddBuff(BuffID.Frozen, 200);
            }
            base.OnHitNPC(projectile, target, hit, damageDone);
        }
        public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            bool returnValue = base.OnTileCollide(projectile, oldVelocity);
            if (projectile.type == ProjectileID.PulseBolt)
            {
                projectile.ai[1] = 1;
            }

            return returnValue;
        }
        public override void AI(Projectile projectile)
        {
            if (projectile.type == ProjectileID.PulseBolt && projectile.ai[1] == 1)
            {
                //projectile.velocity = new Vector2(0, -20);
                projectile.ai[1] = 2;
                int targetIN = projectile.FindTargetWithLineOfSight();
                if (targetIN >= 0)
                {
                    NPC target = Main.npc[targetIN];
                    if (target != null && target.active)
                    {
                        projectile.velocity = (target.Center - projectile.Center).SafeNormalize(Vector2.Zero) * projectile.velocity.Length();
                    }
                }
            }
            base.AI(projectile);
        }
    }
}
