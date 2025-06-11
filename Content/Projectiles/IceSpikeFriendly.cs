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

namespace TerrariaCells.Content.Projectiles
{
    public class IceSpikeFriendly : ModProjectile
    {
        public override string Texture => "Terraria/Images/Extra_" + ExtrasID.CultistIceshard;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.timeLeft = 120;
            Projectile.width = Projectile.height = 20;
            //Projectile.height = 40;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.hide = true;
            Projectile.tileCollide = false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> t = TextureAssets.Projectile[Type];
            int originY = 40;
            if (Projectile.frame == 1) originY = 38;
            if (Projectile.frame == 2) originY = 50;
            Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 74 * Projectile.frame, 24, 74), Color.White, Projectile.rotation, new Vector2(t.Width() * 0.5f, originY), new Vector2(Projectile.scale, TCellsUtils.LerpFloat(0, 1, Projectile.ai[1], 1, TCellsUtils.LerpEasing.OutCubic)), SpriteEffects.None);
            return false;
        }
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindNPCsAndTiles.Add(index);
        }
        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                Vector2 tilePos = Projectile.Center;
                for (int i = -16; i <= 16; i += 16)
                {
                    for (int j = -16; j <= 16; j += 16)
                    {
                        Vector2 pos = Projectile.Center + new Vector2(i, j);
                        
                        if (WorldGen.SolidTile(pos.ToTileCoordinates()) && (pos.DistanceSQ(Projectile.Center) < tilePos.DistanceSQ(Projectile.Center) || Projectile.Center == tilePos))
                        {
                            tilePos = pos;
                        }
                    }
                }
                Projectile.rotation = Projectile.AngleTo(tilePos) + MathHelper.ToRadians(Main.rand.NextFloat(-45, 45)) + MathHelper.PiOver2;
                int offX = 0;
                int offY = 0;
                if (tilePos.X < Projectile.Center.X)
                {
                    offX = 16;
                }
                if (tilePos.Y < Projectile.Center.Y)
                {
                    offY = 16;
                }
                Vector2 tileSnap = tilePos.ToTileCoordinates().ToWorldCoordinates(offX, offY);
                if (Math.Abs(tilePos.X - Projectile.Center.X) > Math.Abs(tilePos.Y - Projectile.Center.Y))
                {
                    Projectile.Center = new Vector2(tileSnap.X, Projectile.Center.Y);
                }
                else
                {

                    Projectile.Center = new Vector2(Projectile.Center.X, tileSnap.Y);
                }
                Projectile.Center -= new Vector2(8, 0).RotatedBy(Projectile.AngleTo(tilePos));
                Projectile.ai[0] = 1;
                Projectile.frame = Main.rand.Next(0, 3);
            }

            if (Projectile.ai[1] < 1 && Projectile.timeLeft > 50)
            {
                Projectile.ai[1] += 0.075f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Frost, 0, 0);
                d.noGravity = true;
            }
        }
    }
}
