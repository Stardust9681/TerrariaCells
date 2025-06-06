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
using TerrariaCells.Common.GlobalProjectiles;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Projectiles
{
    public class IceArrowFriendly : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FrostArrow;
        public override void SetDefaults()
        {
            Projectile.timeLeft = 300;
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.extraUpdates = 1;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            lightColor = Color.White;
            return base.PreDraw(ref lightColor);
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.03f;

            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
            d.scale = Main.rand.NextFloat(1.2f, 1.4f);
            d.velocity *= 0.9f;
            d.noGravity = true;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DeerclopsIceAttack, Projectile.Center);

            for (int i = 0; i < 2; i++)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<IceSpikeFriendly>(), Projectile.damage/2, 1, owner:Projectile.owner);
                proj.GetGlobalProjectile<VanillaReworksGlobalProjectile>().ForceCrit = true;
            }

            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0, 0);
                d.velocity = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-4, -2));
            }
        }
    }
}
