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
    public class BeeMissile : ModProjectile
    {
        
        public override void SetDefaults()
        {
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 1000;
            Projectile.width = Projectile.height = 10;
            Projectile.scale = 2;
            Projectile.tileCollide = false;
            base.SetDefaults();
        }
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Asset<Texture2D> c = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/BeeCrosshair");

            Vector2 pos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Main.EntitySpriteDraw(c.Value, pos - Main.screenPosition, null, lightColor, MathHelper.ToRadians(TCellsUtils.LerpFloat(0, 270, 1000 - Projectile.timeLeft, 60, TCellsUtils.LerpEasing.OutSine)), c.Size() / 2, Projectile.scale * 0.6f, SpriteEffects.None);
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void AI()
        {
            
            Vector2 pos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(MathHelper.Lerp(0.4f, 10, Projectile.Distance(pos) / 140f), 0.4f, 10);
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            if (Projectile.timeLeft % 5 == 0)
            {
                Vector2 vel = -Projectile.velocity;
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Honey, vel.X, vel.Y);
            }
            if (Projectile.Distance(pos) <= 15)
            {
                Projectile.Kill();
            }
                base.AI();
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item97, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, new Vector2(0, 10).RotatedByRandom(MathHelper.Pi), ModContent.ProjectileType<BeeMissileBee>(), 20, 1, -1, Projectile.Center.X, Projectile.Center.Y);
            }
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Honey, Main.rand.NextFloat(-5, 5), -10);
                Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, new Vector2(0, Main.rand.NextFloat(1, 10)).RotatedByRandom(MathHelper.Pi), Main.rand.Next([GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3]));
            }
            base.OnKill(timeLeft);
        }
    }
}
