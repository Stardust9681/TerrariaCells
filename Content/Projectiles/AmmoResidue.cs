using Microsoft.Build.Construction;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
    public class AmmoResidue : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bullet;
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Glowstick);
            Projectile.scale = 0.8f;
            Projectile.timeLeft = 300;
            Projectile.light = 0;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = null;
            if (Projectile.ai[1] == 0)
            {
                t = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/ShotgunShell");
                
            }else if (Projectile.ai[1] == 1)
            {
                t = ModContent.Request<Texture2D>("TerrariaCells/Content/Projectiles/Mag");
            }

            if (t != null) {
                Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            }
            return false;
        }
    }

}
