using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class BeeMissileBee : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GiantBee;
        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
            Projectile.width = Projectile.height = 10;
            Main.projFrames[Type] = 4;
            Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
            Projectile.tileCollide = false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, t.Height() / Main.projFrames[Type] * Projectile.frame, 28, t.Height() / Main.projFrames[Type]), lightColor, 0, new Vector2(t.Width(), t.Height() / 4) / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            return false;
        }
        public override void AI()
        {
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] >= 2)
            {
                Projectile.localAI[0] = 0;
                Projectile.frame++;
                if (Projectile.frame >= 4)
                {
                    Projectile.frame = 0;
                }
            }

            Vector2 center = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Vector2 targetPos = center + new Vector2(0, 25).RotatedBy(Projectile.ai[2]);

            if (Projectile.ai[2] == 0 || Projectile.Distance(targetPos) < 4)
            {
                Projectile.ai[2] = Main.rand.NextFloat(0, MathHelper.TwoPi);
            }
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.AngleTo(targetPos).ToRotationVector2() * 10, 0.08f);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Bee);
            }
        }
    }
}
