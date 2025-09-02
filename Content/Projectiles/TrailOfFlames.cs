using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class TrailOfFlames : ModProjectile //TODO: Make projectile stick to floor but not phase through platforms
    {
        private int originalTimeLeft;

        public float scaleFactor = 0.5f;

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Flamelash;

        public override void SetStaticDefaults()
        {
            TextureAssets.Projectile[Type] = ModContent.Request<Texture2D>(Texture);
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.timeLeft = 120;
            Projectile.damage = 10;
            Projectile.penetrate = -1;
            Projectile.friendly = true;
            Projectile.frame = Main.rand.Next(6);
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 12;
            //Projectile.Hitbox = Projectile.Hitbox with { Height = 4 * Projectile.Hitbox.Height };

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
            Projectile.scale = scaleFactor * Projectile.timeLeft / originalTimeLeft;
            Projectile.frame = (Projectile.frame + 5) % 6;
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(new DrawData(
                t.Value,
                Projectile.Center - Main.screenPosition + new Vector2(0f, 4f),
                t.Frame(verticalFrames: 6, frameY: Projectile.frame),
                Color.White * Projectile.Opacity,
                0,
                t.Size() * new Vector2(0.5f, 0.109375f),
                Projectile.scale,
                SpriteEffects.None
            ));
            return false;
        }

        public override void AI()
        {
            for (int i = 0; i < 24; i++)
            {
                if (Collision.IsWorldPointSolid(Projectile.position + new Vector2(0, 1)))
                {
                    break;
                }
                Projectile.position.Y++;

                if (i == 23)
                {
                    Projectile.active = false;
                }
            }
            Lighting.AddLight(Projectile.Center, TorchID.Red);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
    }
}
