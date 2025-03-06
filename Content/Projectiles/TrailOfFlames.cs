using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class TrailOfFlames : ModProjectile
    {
        private int originalTimeLeft;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MolotovFire;

        public override void SetDefaults()
        {
            Projectile.timeLeft = 120;
            Projectile.damage = 10;
            Projectile.penetrate = -1;
            Projectile.friendly = true;

            originalTimeLeft = Projectile.timeLeft;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.life += damageDone;
            target.AddBuff(BuffID.OnFire, 600);
            base.OnHitNPC(target, hit, damageDone);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.scale = (float)Projectile.timeLeft / originalTimeLeft;
            Projectile.gfxOffY = 8f - 8f * Projectile.scale;
            return base.PreDraw(ref lightColor);
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, TorchID.Red);
        }
    }
}
