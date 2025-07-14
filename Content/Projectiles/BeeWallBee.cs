using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class BeeWallBee : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GiantBee;
        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 1000;
            Projectile.width = Projectile.height = 10;
            Main.projFrames[Type] = 4;
            Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
            base.SetDefaults();
        }
        public override void AI()
        {
            base.AI();
            if (NPC.AnyNPCs(NPCID.QueenBee))
            {
                Projectile.timeLeft = 2;
            }
            Projectile.velocity.X *= 0.95f;
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
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, t.Height() / Main.projFrames[Type] * Projectile.frame, 28, t.Height() / Main.projFrames[Type]), lightColor, 0, new Vector2(t.Width(), t.Height() / 4) / 2, Projectile.scale, Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if(Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon) {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Bee);
            }
            base.OnKill(timeLeft);
        }
    }
}
