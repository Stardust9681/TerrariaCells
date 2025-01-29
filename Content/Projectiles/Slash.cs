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
    public class Slash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Arkhalis;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 28;
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            Projectile.hostile = true;
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.friendly = false;
            Projectile.timeLeft = 15;
            base.SetDefaults();
        }
        public override bool CanHitPlayer(Player target)
        {
            return base.CanHitPlayer(target);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int totalFrames = Projectile.ai[1] == 0 ? 8 : 7;
            float time = 15;
            Projectile.frame = (int)MathHelper.Lerp(0, totalFrames - 1, 1 - Projectile.timeLeft / time);

            int frameOffset = Projectile.ai[1] == 0 ? 0 : 1344;
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            SpriteEffects effects = Projectile.ai[2] == 1 ? SpriteEffects.None : SpriteEffects.FlipVertically;
            //sprite is transparent for some reason so i draw twice to make it less so
            for (int i = 0; i < 2; i++)
            {
                Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, Projectile.frame * 64 + frameOffset, 68, 64), lightColor, Projectile.rotation, new Vector2(t.Width(), t.Height() / 28) / 2, Projectile.scale, effects);
            }


            return false;
        }
        //ai0: owner's whoami
        //ai1: 0 for down slash, 1 for up
        //ai2: direction
        public override void AI()
        {
            NPC owner = Main.npc[(int)Projectile.ai[0]];
            if (owner == null || !owner.active || owner.friendly)
            {
                Projectile.Kill();
                return;
            }
           
            
            Projectile.Center = owner.Center + new Vector2(owner.width, 0).RotatedBy(Projectile.rotation);


            base.AI();
        }
    }
}
