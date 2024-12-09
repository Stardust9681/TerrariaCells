using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    //splits off from ice ball
    public class IceShard : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_" + ExtrasID.CultistIceshard;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 20;
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.extraUpdates = 0;
            Projectile.scale = 0.5f;
            Projectile.tileCollide = false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 74 * Projectile.frame, 24, 74), lightColor, Projectile.rotation, new Vector2(t.Width() / 2, t.Height() / 6), Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Projectile.ai[0] = 1;
                Projectile.frame = Main.rand.Next(0, 3);
            }
            if (Projectile.timeLeft < 5)
            {
                Projectile.Opacity -= 0.2f;
            }
            if (Projectile.timeLeft == 1)
            {
                for (int i = 0; i < 20; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                    d.noGravity = true;
                    d.velocity *= 2;
                }
            }
            Projectile.rotation = Projectile.ai[1] - MathHelper.PiOver2;
            
            base.AI();
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }
    }
}
