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

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class DesertSpiritFlame : GlobalProjectile
    {
        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            if (projectile.type == ProjectileID.DesertDjinnCurse) {
                Asset<Texture2D> t = TextureAssets.Projectile[projectile.type];
                Main.EntitySpriteDraw(t.Value, projectile.Center - Main.screenPosition, new Rectangle(0, 30 * projectile.frame, 20, 30), lightColor * projectile.Opacity, projectile.rotation, new Vector2(10, 15), projectile.scale, SpriteEffects.None);
                return false;
            }
            return base.PreDraw(projectile, ref lightColor);
        }
        public override bool PreAI(Projectile projectile)
        {
            

            if (projectile.type == ProjectileID.DesertDjinnCurse)
            {
                if (projectile.timeLeft == 180)
                {
                    projectile.Opacity = 0;
                    SoundEngine.PlaySound(SoundID.Item8, projectile.Center);
                    for (int i = 0; i < 10; i++)
                    {
                        Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.Shadowflame);
                    }
                }
                if (projectile.Opacity < 1)
                {
                    projectile.Opacity += 0.1f;
                }
                projectile.frameCounter++;
                if (projectile.frameCounter >= 5)
                {
                    projectile.frameCounter = 0;
                    projectile.frame++;
                    if (projectile.frame >= 4)
                    {
                        projectile.frame = 0;
                    }
                }
                
                if (projectile.ai[2] == 0)
                {
                    NPC owner = Main.npc[(int)projectile.ai[0]];
                    Player target = Main.player[(int)projectile.ai[1]];
                    if (owner != null)
                    {
                        if (owner.ai[3] == 240)
                        {
                            projectile.ai[0] = target.Center.X + Main.rand.Next(-50, 50) + target.velocity.X;
                            projectile.ai[1] = target.Center.Y + Main.rand.Next(-50, 50) + target.velocity.Y;
                            projectile.ai[2] = 1;
                            projectile.timeLeft = 40;
                        }
                    }
                }
                else
                {
                    Vector2 toPos = (new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center) / 12;
                    projectile.velocity = Vector2.Lerp(projectile.velocity, toPos, 0.03f);
                }
                if (projectile.timeLeft < 3)
                {
                    projectile.damage = TCellsUtils.ScaledHostileDamage(25);
                }
                return false;
            }
            return base.PreAI(projectile);
        }
    }
}
