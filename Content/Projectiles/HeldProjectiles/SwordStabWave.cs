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

namespace TerrariaCells.Content.Projectiles.HeldProjectiles
{
    public class SwordStabWave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.timeLeft = 1000;
            Projectile.penetrate = -2;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.CritChance = 100;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.Opacity, Projectile.rotation + MathHelper.PiOver2, t.Size() / 2, Projectile.scale, SpriteEffects.None);
            return false;
        }
        
        public override void AI()
        {
            if (Projectile.timeLeft == 1000)
            {
                Projectile.timeLeft = (int)Projectile.ai[1];
            }
            

            //Main.NewText(Projectile.timeLeft);
            Player owner = Main.player[Projectile.owner];
            Projectile proj = Main.projectile[(int)Projectile.ai[0]];

            float rot = Projectile.rotation;
            if (proj != null && proj.active && owner != null && !owner.dead && owner.active)
            {
                rot = proj.rotation;
            }
            if (owner == null || !owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Vector2 armPos = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rot);
            //armPos.Y += -20;
            float x = 1 - Projectile.timeLeft / (Projectile.ai[1]);
            //Main.NewText(x);
            float lerper = 1 - (float)Math.Pow(1 - x, 5);

            Asset<Texture2D> t = TextureAssets.Item[(int)proj.ai[0]];
            float distance = t.Size().Length() + 15;
            

            Projectile.rotation = proj.rotation;
            Projectile.Center = Vector2.Lerp(proj.Center, proj.Center - new Vector2(distance, 0).RotatedBy(Projectile.rotation), lerper);

            float lerp2 = (float)Math.Sin(Math.PI * x);
            Projectile.scale = MathHelper.Lerp(0.5f, 1, lerp2);
            Projectile.Opacity = MathHelper.Lerp(0f, 1, lerp2);

            base.AI();
        }
    }
}
