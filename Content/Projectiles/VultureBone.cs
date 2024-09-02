using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Projectiles
{
    public class VultureBone : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Bone;
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            t = (int)Projectile.ai[0] switch
            {
                1 => TextureAssets.Gore[42],
                2 => TextureAssets.Gore[57],
                3 => TextureAssets.Gore[68],
                4 => TextureAssets.Gore[972],
                5 => TextureAssets.Projectile[ProjectileID.BoneGloveProj],
                _ => TextureAssets.Projectile[Type]
            };
            Main.instance.LoadGore(42);
            Main.instance.LoadGore(57);
            Main.instance.LoadGore(68);
            Main.instance.LoadGore(972);
            Main.instance.LoadProjectile(ProjectileID.BoneGloveProj);

            float scale = 25 / t.Size().Length();
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, new Vector2(t.Width()/2, t.Height()/2), Projectile.scale * scale ,SpriteEffects.None);
            return false;
        }
        public override bool CanHitPlayer(Player target)
        {
            return base.CanHitPlayer(target);
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[0] = Main.rand.Next(0, 6);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Bone, 0, -3);
            }
            return base.OnTileCollide(oldVelocity);
        }

        public override void AI()
        {
            Projectile.rotation += MathHelper.ToRadians(20);
            if (Projectile.timeLeft < 280)
            {
                if (Projectile.velocity.Y < 10)
                {
                    Projectile.velocity.Y += 0.2f;
                }
            }
            base.AI();
        }
    }
}
