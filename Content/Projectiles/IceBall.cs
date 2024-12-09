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
using TerrariaCells.Common.Utilities;
using Terraria.Audio;

namespace TerrariaCells.Content.Projectiles
{
    //thrown by cultist devotee
    public class IceBall : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CultistBossIceMist;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 300;
            Projectile.width = Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.scale = 0.1f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor * Projectile.Opacity, Projectile.rotation, new Vector2(t.Width() / 2, t.Height() / 2), Projectile.scale, SpriteEffects.None);
            return false;
        }
        public override void AI()
        {

            int maxTimeLeft = 300;
            int timeCharging = 60;
            int numShards = 5;
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Frost);
            d.noGravity = true;
            Projectile.ai[0]++;
            if (Projectile.timeLeft > maxTimeLeft - timeCharging)
            {
                if (Projectile.scale < 0.5f)
                {
                    Projectile.scale = TCellsUtils.LerpFloat(0.1f, 0.5f, Projectile.ai[0], timeCharging, TCellsUtils.LerpEasing.InOutSine, 0);
                    Projectile.Resize((int)(60 * Projectile.scale), (int)(60 * Projectile.scale));
                }
                Projectile.rotation += MathHelper.ToRadians(10);
                
            }
            else
            {
                Projectile.rotation -= MathHelper.ToRadians(-7 * Projectile.direction);
            }

            if (Projectile.timeLeft == maxTimeLeft - timeCharging)
            {
                SoundEngine.PlaySound(SoundID.Item120, Projectile.Center);
                Projectile.ai[0] = 0;
                Player target = Main.player[(int)Projectile.ai[1]];
                if (target != null && target.active && !target.dead)
                {
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 5;
                }
            }

            if (Projectile.ai[0] == 60 && Projectile.timeLeft > 100 && Projectile.timeLeft < 240)
            {
                Projectile.ai[0] = 0;
                for (int i = 0; i < numShards; i++)
                {
                    Vector2 vel = new Vector2(0, -10).RotatedBy(MathHelper.ToRadians((float)i / numShards * 360));
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center,  vel, ModContent.ProjectileType<IceShard>(), TCellsUtils.ScaledHostileDamage(25), 1, ai1: vel.ToRotation());
                }
                SoundEngine.PlaySound(SoundID.Item28, Projectile.Center);
            }
            if (Projectile.timeLeft < 10)
            {
                Projectile.Opacity -= 0.1f;
            }
            base.AI();
        }
        public override void OnKill(int timeLeft)
        {
            base.OnKill(timeLeft);
        }
    }
}
