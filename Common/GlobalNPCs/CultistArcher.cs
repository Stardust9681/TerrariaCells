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
using TerrariaCells.Content.Projectiles;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fighters : GlobalNPC
    {
        public bool DrawCultistArcher(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            if (npc.ai[3] == 1)
            {
                spriteBatch.Draw(t.Value, npc.Center - screenPos + new Vector2(0, -5), new Rectangle(npc.frame.X, CustomFrameY, npc.frame.Width, npc.frame.Height), drawColor, npc.rotation, new Vector2(t.Width() / 2, t.Height() / Main.npcFrameCount[npc.type]/2), new Vector2(npc.scale, npc.scale), npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                return false;
            }
            return true;
        }

        public void CultistArcherFrame(NPC npc)
        {
            if (npc.ai[3] == 1)
            {
                Player target = Main.player[npc.target];
                if (target != null)
                {
                    //fucked up and evil checks to make it point the bow at the player
                    float angle = MathHelper.ToDegrees(npc.AngleTo(target.Center));
                    if ((angle < -30 && angle > -60) || (angle < -120 && angle > -150)) 
                    {
                        CustomFrameY = 58 * 5;
                    }
                    if (angle <= -60 && angle >= -120)
                    {
                        CustomFrameY = 58 * 6;
                    }
                    if (angle >= 60 && angle <= 120)
                    {
                        CustomFrameY = 58 * 2;
                    }
                    if ((angle  < 60 && angle > 30) || (angle > 120 && angle < 150))
                    {
                        CustomFrameY = 58 * 3;
                    }
                    if ((angle > -30 && angle < 30) || (angle > 150 || angle < -150))
                    {
                        CustomFrameY = 58 * 4;
                    }
                }
            }
        }

        public bool CultistArcherAI(NPC npc, Player target)
        {
            if (npc.HasValidTarget && npc.Distance(target.Center) < 300 && Collision.CanHitLine(npc.Center, 1, 1, target.Center, 1, 1) && npc.collideY)
            {
                ShouldWalk = false;
                npc.velocity.X *= 0.9f;
                npc.ai[3] = 1;
                npc.ai[2]++;
                if (npc.ai[2] >= 60)
                {
                    Vector2 pos = npc.Center + new Vector2(10 * npc.direction, 0);
                    Projectile.NewProjectileDirect(npc.GetSource_FromAI(), pos, (target.Center - pos).SafeNormalize(Vector2.Zero)*5, ModContent.ProjectileType<IceArrow>(), TCellsUtils.ScaledHostileDamage(25), 1);
                    SoundEngine.PlaySound(SoundID.Item5, npc.Center);
                    npc.ai[2] = 0;
                }
            }
            else
            {
                npc.ai[3] = 0;
                npc.ai[2] = 0;
            }
            return false;
        }
    }
}
