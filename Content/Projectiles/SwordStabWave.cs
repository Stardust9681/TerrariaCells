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
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Projectiles
{
    public class SwordStabWave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowCrystalExplosion;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
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
            Main.instance.LoadProjectile(Type);
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

            float rot = owner.itemRotation + MathHelper.ToRadians(owner.direction == 1 ? 136 : 45);

            Vector2 armPos = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, rot);


            Asset<Texture2D> t = TextureAssets.Item[owner.HeldItem.type];
            float distance = t.Size().Length() + (owner.direction == -1 ? 15 : 10);


            Projectile.rotation = rot;

            //Main.NewText(proj.spriteDirection);
            int xOff = owner.direction == -1 ? 8 : 12;
            Projectile.Center = armPos - TCellsUtils.LerpVector2(new Vector2(distance / 2, xOff).RotatedBy(rot), new Vector2(distance, xOff).RotatedBy(rot), Projectile.ai[1] - Projectile.timeLeft, Projectile.ai[1], TCellsUtils.LerpEasing.OutQuint);


            Projectile.scale = TCellsUtils.LerpFloat(0.5f, 1, Projectile.ai[1] - Projectile.timeLeft, Projectile.ai[1], TCellsUtils.LerpEasing.DownParabola);
            Projectile.Opacity = TCellsUtils.LerpFloat(0f, 1, Projectile.ai[1] - Projectile.timeLeft, Projectile.ai[1], TCellsUtils.LerpEasing.DownParabola);

            base.AI();
        }
    }
}
