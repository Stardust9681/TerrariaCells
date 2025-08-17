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

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
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
        public void GhoulAI(NPC npc, Player? target)
        {
            const int TimeWalking = 30;
            const int TimeSlashing = 60;
            const int SlashDelay = TimeSlashing / 3;

            if (!npc.HasValidTarget)
            {
                return;
            }

			if (!npc.TargetInAggroRange(target, 384))
			{
				ShouldWalk = false;
				npc.velocity.X *= 0.9f;
				if ((npc.collideY && npc.velocity.Y == 0) && MathF.Abs(npc.velocity.X) < 1)
				{
					if (Main.rand.NextBool(10))
					{
						int direction = Main.rand.NextBool() ? -1 : 1;
						npc.spriteDirection = npc.direction = direction;
						npc.velocity.X = npc.direction * 10f;
						SoundEngine.PlaySound(SoundID.Item1, npc.Center);
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile slash = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<Slash>(), TCellsUtils.ScaledHostileDamage(npc.damage), 1, -1, npc.whoAmI, 0, npc.direction);
                            if (npc.direction < 0)
                                slash.rotation = MathHelper.Pi;
                        }
					}
					else
					{
						npc.spriteDirection = npc.direction = target.position.X < npc.position.X ? -1 : 1;
						npc.velocity.Y = -5f;
					}
				}
				npc.velocity.Y += 0.01f;
				return;
			}
            
            if (npc.ai[2] >= TimeWalking && target.Distance(npc.Center) < 200 && npc.ai[3] == 0 && (float)Math.Abs(npc.Center.Y - target.Center.Y) < 30)
            {
                npc.ai[2] = 0;
                npc.ai[3] = 1;
            }

            if (npc.ai[0] == 1 && npc.ai[1] == 2)
            {
                SoundEngine.PlaySound(SoundID.Item1, npc.Center);
                Projectile slash = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<Slash>(), TCellsUtils.ScaledHostileDamage(25), 1, -1, npc.whoAmI, 0, npc.direction);
                //slash.scale = 1; //Scale defaults to 1f anyway
                slash.rotation = npc.AngleTo(target.Center);
            }

            if (npc.ai[3] == 1)
            {
                npc.velocity.X *= 0.9f;
                npc.direction = npc.oldDirection;
                ShouldWalk = false;

                if (npc.ai[2] % SlashDelay == 0 && npc.ai[2] != TimeSlashing)
                {
                    if (npc.ai[2] > 0 && ExtraAI[0] == 0 && !npc.IsFacingTarget(target))
                    {
                        npc.direction = -npc.direction;
                        npc.oldDirection = npc.direction;
                        npc.ai[2] = SlashDelay;
                        ExtraAI[0] = 1; //Consider using 'npc.localAI[0]' instead
                    }
                    SoundEngine.PlaySound(SoundID.Item1, npc.Center);
                    npc.velocity.X = 10 * npc.direction;
                    Projectile slash =  Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<Slash>(), TCellsUtils.ScaledHostileDamage(25), 1, -1, npc.whoAmI, npc.ai[2] == SlashDelay ? 1 : 0, npc.direction);
                    slash.scale = 1;
                    slash.rotation = npc.direction == 1 ? 0 : MathHelper.Pi;
                }

                if (npc.ai[2] >= TimeSlashing)
                {
                    npc.ai[3] = 0;
                    npc.ai[2] = 0;
                    ExtraAI[0] = 0; //Consider using 'npc.localAI[0]' instead
                }
            }

            npc.ai[2]++; //Timer variable
        }
    }
}
