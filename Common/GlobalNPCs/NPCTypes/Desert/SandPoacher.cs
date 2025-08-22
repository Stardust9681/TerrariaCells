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
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    public partial class Fighters
    {
        public bool DrawSandPoacher(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.ai[3] == 1)
            {
                float offset = 0;
                int timeDigging = 100;
                
                if (npc.ai[2] > timeDigging / 2)
                {
                    
                    offset = TCellsUtils.LerpFloat(30, 0, npc.ai[2], timeDigging / 2, TCellsUtils.LerpEasing.InSine, timeDigging / 2);
                }
                else
                {
                    offset = TCellsUtils.LerpFloat(0, 30, npc.ai[2], timeDigging / 2, TCellsUtils.LerpEasing.OutSine);
                }

                Asset<Texture2D> poacher = TextureAssets.Npc[NPCID.DesertScorpionWall];
                Main.instance.LoadNPC(NPCID.DesertScorpionWall);
                spriteBatch.Draw(poacher.Value, npc.Center - screenPos + new Vector2(0, offset), new Rectangle(0, CustomFrameY, poacher.Width(), poacher.Height() / 4), drawColor * npc.Opacity, npc.rotation, new Vector2(poacher.Width() / 2, poacher.Height() / 4 / 2), npc.scale, SpriteEffects.None, 1);
                return false;
            }
            if (npc.ai[3] == 2)
            {
                Asset<Texture2D> poacher = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/DesertScorpionStab");

                spriteBatch.Draw(poacher.Value, npc.Center - screenPos + new Vector2(-20, -10), new Rectangle(0, CustomFrameY*46, 64, 46), drawColor, npc.rotation, poacher.Size()/6/2, npc.scale, npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
                return false;
            }

            return true;
        }
        
        public void SandPoacherFrame(NPC npc)
        {
            int stabTime = 30;
            if (npc.ai[3] == 1)
            {
                CustomFrameCounter++;
                if (CustomFrameCounter >= 5)
                {
                    CustomFrameY += 80;
                    if (CustomFrameY > 80 * 3) CustomFrameY = 0;
                    CustomFrameCounter = 0;
                }
            }

            if (npc.ai[3] == 2)
            {
                CustomFrameCounter++;
                if (CustomFrameCounter > stabTime / 6)
                {
                    CustomFrameY += 1;
                    if (CustomFrameY >= 6)
                    {
                        CustomFrameY = 0;
                    }
                    CustomFrameCounter = 0;
                }
            }
        }

        public void SandPoacherAI(NPC npc, Player? target)
        {
            if (target is null)
            {
                ShouldWalk = false;
                return;
            }

            int timeWalking = 200;
            int timeDigging = 100;
            int stabTime = 30;

            npc.ai[2]++;
            //during dig
            if (npc.ai[3] == 1)
            {
                //dont walk
                ShouldWalk = false;
                npc.velocity *= 0.9f;
                
                //rotate down during dig, up during undig
                npc.rotation = npc.ai[2] < timeDigging / 2 ? MathHelper.Pi : 0;

                //change opacity over time
                if (npc.ai[2] < timeDigging / 2) {
                    npc.Opacity = TCellsUtils.LerpFloat(1, 0, npc.ai[2], timeDigging / 2, TCellsUtils.LerpEasing.Linear);
                }
                else
                {
                    npc.Opacity = TCellsUtils.LerpFloat(0, 1, npc.ai[2], timeDigging / 2, TCellsUtils.LerpEasing.Linear, 50);
                }

                //teleport try to find ground
                if (npc.ai[2] == timeDigging / 2)
                {
                    Vector2 position = target.Center;
                    float randomChange = Main.rand.NextFloat(30, 100);
                    position.X += -150 * target.direction;
                    position.Y = TCellsUtils.FindGround(new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height)).Y;
                    npc.position = position + new Vector2(0, -npc.height);

                    for (int i = 0; i < 100; i += 1)
                    {
                        Point point = npc.position.ToTileCoordinates();
                        point.Y += (npc.height / 16) - 1;
                        if (Main.tile[point].HasTile)
                        {
                            npc.position.Y -= 16;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                Dust.NewDustDirect(npc.BottomLeft, npc.width, 0, DustID.Sand, 0, -4);
            }

            bool validTargetInRange = npc.TargetInAggroRange(280);
            if (!validTargetInRange)
                this.ShouldWalk = false;

            //start dig
            if (validTargetInRange && npc.ai[2] >= timeWalking && npc.ai[3] == 0 && npc.collideY && target != null)
            {
                npc.hide = true;
                npc.ai[2] = 0;
                npc.ai[3] = 1;
            }
            //end dig
            else if (npc.ai[2] >= timeDigging && npc.ai[3] == 1)
            {
                npc.hide = false;
                npc.ai[2] = 0;
                npc.ai[3] = 0;
                CustomFrameCounter = 0;
                CustomFrameY = 0;
            }

            //start stab
            //Conditions: Has a target. Is in walking phase. Is close to target. Is facing target.
            if (npc.HasValidTarget && npc.ai[3] == 0 && npc.Distance(target.Center) < 80 && npc.IsFacingTarget(target))
            {
                npc.ai[3] = 2;
            }

            //stab
            if (npc.ai[3] == 2)
            {
                CombatNPC.ToggleContactDamage(npc, true);
                ShouldWalk = false;
                npc.velocity.X *= 0.9f;
                npc.direction = npc.oldDirection;
                ExtraAI[0]++;
                if (ExtraAI[0] == 20)
                {
                    npc.velocity.X = 5 * npc.direction;
                }
                if (ExtraAI[0] == 25)
                {
                    
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + new Vector2(0, -20), new Vector2(5 * npc.direction, 0), ModContent.ProjectileType<PoacherStab>(), 25, 1, npc.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item1, npc.Center);
                }
               

                if (ExtraAI[0] > stabTime)
                {
                    CombatNPC.ToggleContactDamage(npc, false);
                    npc.ai[3] = 3;
                    ExtraAI[0] = 0;
                    CustomFrameCounter = 0;
                    CustomFrameY = 0;
                }
            }

            //wait
            if (npc.ai[3] == 3)
            {
                CombatNPC.ToggleContactDamage(npc, false);
                ShouldWalk = false;
                npc.velocity.X *= 0.9f;
                npc.direction = npc.oldDirection;
                ExtraAI[0]++;
                if (ExtraAI[0] == 15)
                {
                    ExtraAI[0] = 0;
                    npc.ai[3] = 0;
                }
            }
        }
    }
}