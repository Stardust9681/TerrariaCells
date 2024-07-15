using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles.HeldProjectiles
{
    public class GunExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SolarWhipSwordExplosion;
        public override void SetDefaults()
        {
            Main.projFrames[Type] = 5;
            Projectile.width = Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 20;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return base.PreDraw(ref lightColor);
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, TorchID.Orange);
            Projectile.ai[0]++;
            if (Projectile.ai[0] == 20 / 5)
            {
                Projectile.ai[0] = 0;
                Projectile.frame++;
            }
            base.AI();
        }
    }
}
