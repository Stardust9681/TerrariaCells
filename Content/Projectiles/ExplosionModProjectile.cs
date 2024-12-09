using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    /// <summary>
    /// This projectile is just a 3 frame hitbox with an explosion effect when it ends
    /// </summary>
    public class ExplosionModProjectile : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.alpha = 255;
            Projectile.width = 108;
            Projectile.height = 108;
            Projectile.friendly = true;
            Projectile.damage = 20;
            Projectile.knockBack = 4;
            Projectile.penetrate = -1; // Infinite penetration so that the blast can hit all enemies within its radius.
            Projectile.DamageType = DamageClass.Ranged;

            // usesLocalNPCImmunity and localNPCHitCooldown of -1 mean the projectile can only hit the same target once.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;

            Projectile.timeLeft = 3;
        }

        public override void OnKill(int timeLeft)
        {
            Projectile.Resize(5, 5);
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

            // Smoke Dust spawn
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0f, 0f, 100, default, 1.4f);
                dust.velocity *= 1.1f;
            }

            // Fire Dust spawn
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2.5f);
                dust.noGravity = true;
                dust.velocity *= 4f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.25f);
                dust.velocity *= 2f;
            }

            // Large Smoke Gore spawn
            for (int g = 0; g < 1; g++)
            {
                var goreSpawnPosition = new Vector2(Projectile.position.X + Projectile.width / 2 - 24f, Projectile.position.Y + Projectile.height / 2 - 24f);
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1f;
                gore.velocity.X += 0.4f;
                gore.velocity.Y += 0.4f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1f;
                gore.velocity.X -= 0.4f;
                gore.velocity.Y += 0.4f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1f;
                gore.velocity.X += 0.4f;
                gore.velocity.Y -= 0.4f;
                gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), goreSpawnPosition, default, Main.rand.Next(61, 64), 1f);
                gore.scale = 1f;
                gore.velocity.X -= 0.4f;
                gore.velocity.Y -= 0.4f;
            }
        }

    }
}
