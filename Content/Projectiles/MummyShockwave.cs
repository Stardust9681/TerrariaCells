using Microsoft.Xna.Framework;
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
    public class MummyShockwave : ModProjectile
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 38;
            Projectile.height = 38;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 40;
            Projectile.tileCollide = false;
            Projectile.hide = true;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.height/2 + 3), null, lightColor, Projectile.rotation, new Vector2(t.Width()/2, t.Height()), new Vector2(Projectile.scale, Projectile.scale * (Projectile.ai[1] / 5)), Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
        public override bool CanHitPlayer(Player target)
        {
            if (Projectile.ai[1] < 5)
            {
                return false;
            }
            return base.CanHitPlayer(target);
        }
        public override void OnSpawn(IEntitySource source)
        {
            int attempts = 200;
            while (!WorldGen.SolidTile2(Main.tile[Projectile.Bottom.ToTileCoordinates()]) && attempts > 0)
            {
                Projectile.position.Y += 1;
                attempts--;
            }

            while (WorldGen.SolidTile2(Main.tile[Projectile.Bottom.ToTileCoordinates()]) && attempts > 0)
            {
                Projectile.position.Y -= 1;
                attempts--;
            } 

            for (int i = 0; i < Projectile.width; i++)
            {
                Dust.NewDustDirect(Projectile.BottomLeft + new Vector2(i, 0), 0, 0, DustID.Sand, 0, -5);
            }
            base.OnSpawn(source);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return base.Colliding(projHitbox, targetHitbox);
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 0)
            {
                Projectile.spriteDirection = Main.rand.NextBool() ? 1 : -1;
            }
            if (Projectile.ai[1] < 5)
            {
                Projectile.ai[1]++;
            }
            if (Projectile.ai[1] == 4 && Projectile.scale < 2)
            {
                Projectile proj =  Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(25 * Projectile.scale * Projectile.ai[0], 0), Vector2.Zero, ModContent.ProjectileType<MummyShockwave>(), Projectile.damage, 1, -1, Projectile.ai[0]);
                proj.scale = Projectile.scale + 0.2f;
            }
            if (Projectile.timeLeft == 1)
            {
                for (int i = 0; i < 100; i++)
                {
                    Dust.NewDustDirect(Projectile.TopLeft, Projectile.width, Projectile.height, DustID.Sand, 0, 0);
                }
            }
            base.AI();
        }
    }
}
