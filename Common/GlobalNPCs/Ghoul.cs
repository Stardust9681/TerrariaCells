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
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fighters
    {
        int[] Ghouls = { NPCID.DesertGhoul, NPCID.DesertGhoulCorruption, NPCID.DesertGhoulCrimson, NPCID.DesertGhoulHallow };
        public bool DrawGhoul(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            
            if (npc.ai[3] == 1)
            {
                spriteBatch.Draw(t.Value, npc.Center - screenPos, new Rectangle(0, 52 * 3, 36, 52), drawColor, npc.rotation, new Vector2(t.Width(), t.Height() / 8) / 2, npc.scale, npc.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
                return false;
            }

            return true;
        }
        public void GhoulFrame(NPC npc)
        {
            if (npc.ai[3] == 1)
            {
                CustomFrameY = 3;
            }
            else
            {
                CustomFrameY = 0;
            }
        }
        public void GhoulAI(NPC npc, Player target)
        {
            int timeWalking = 30;
            int timeSlashing = 60;
            int slashDelay = timeSlashing / 3;

            if (!npc.HasValidTarget)
            {
                return;
            }

            
            
            if (npc.ai[2] >= timeWalking && target.Distance(npc.Center) < 200 && npc.ai[3] == 0 && (float)Math.Abs(npc.Center.Y - target.Center.Y) < 30)
            {
                npc.ai[2] = 0;
                npc.ai[3] = 1;
            }

            if (npc.ai[0] == 1 && npc.ai[1] == 2)
            {
                SoundEngine.PlaySound(SoundID.Item1, npc.Center);
                Projectile slash = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<Slash>(), TCellsUtils.ScaledHostileDamage(25), 1, -1, npc.whoAmI, 0, npc.direction);
                slash.scale = 1;
                slash.rotation = npc.AngleTo(target.Center);
            }

            if (npc.ai[3] == 1)
            {
                npc.velocity.X *= 0.9f;
                npc.direction = npc.oldDirection;
                ShouldWalk = false;
                if (npc.ai[2] % slashDelay == 0 && npc.ai[2] != timeSlashing)
                {
                    
                    if (npc.ai[2] > 0 && ExtraAI[0] == 0 && !FacingPlayer(npc, target))
                    {
                        npc.direction = -npc.direction;
                        npc.oldDirection = npc.direction;
                        npc.ai[2] = slashDelay;
                        ExtraAI[0] = 1;
                    }
                    SoundEngine.PlaySound(SoundID.Item1, npc.Center);
                    npc.velocity.X = 10 * npc.direction;
                    Projectile slash =  Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<Slash>(), TCellsUtils.ScaledHostileDamage(25), 1, -1, npc.whoAmI, npc.ai[2] == slashDelay ? 1 : 0, npc.direction);
                    slash.scale = 1;
                    slash.rotation = npc.direction == 1 ? 0 : MathHelper.Pi;
                }
                if (npc.ai[2] >= timeSlashing)
                {
                    npc.ai[3] = 0;
                    npc.ai[2] = 0;
                    ExtraAI[0] = 0;
                }
            }

            npc.ai[2]++;
        }
    }
}
