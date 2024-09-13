using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Projectiles
{
    //shot by cultist archer
    public class IceArrow : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FrostArrow;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 300;
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.extraUpdates = 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(t.Width() / 2, t.Height() / 2), Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.005f;
            Dust d = Dust.NewDustDirect(Projectile.position, 10, 10, DustID.IceTorch);
            d.velocity *= 0.9f;
            d.noGravity = true;
            base.AI();
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IceSpike>(), TCellsUtils.ScaledHostileDamage(25), 1);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Hitbox.Left(), 10, 0, DustID.IceTorch, 0, 0);
                d.velocity = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-4, -2));
            }
            base.OnKill(timeLeft);
        }
    }
}
