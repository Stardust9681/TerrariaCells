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
using Terraria.Audio;

using TerrariaCells.Common.Utilities;
using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
	public partial class Casters : GlobalNPC
	{
        public bool DrawRaggedCaster(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Asset<Texture2D> t = TextureAssets.Npc[npc.type];
            spriteBatch.Draw(t.Value, npc.Center - screenPos + new Vector2(0, -3), new Rectangle(npc.frame.X, CustomFrameY, npc.frame.Width, npc.frame.Height), drawColor, npc.rotation, new Vector2(t.Width() / 2, t.Height() / Main.npcFrameCount[npc.type] / 2), new Vector2(npc.scale, npc.scale), npc.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
            return false;
        }

        public void RaggedCasterFrame(NPC npc)
        {
            if (npc.ai[0] <= 40)
            {
                CustomFrameY = (int)TCellsUtils.LerpFloat(0, 1, npc.ai[0], 20, TCellsUtils.LerpEasing.Linear) * 54;
            }
            if (npc.ai[0] >= 280)
            {
                CustomFrameY = (int)TCellsUtils.LerpFloat(1, 0, npc.ai[0], 10, TCellsUtils.LerpEasing.Linear, 75) * 54;
            }
        }

		public bool RaggedCasterAI(NPC npc, Player? target)
		{
			bool validTarget;
			if (target != null)
				validTarget = npc.TargetInAggroRange(target, 400, false);
			else
				validTarget = npc.TargetInAggroRange(400, false);

            if (target != null)
            {
                npc.direction = npc.Center.X > target.Center.X ? -1 : 1;
                npc.spriteDirection = npc.direction;
            }

            int Timer = (int)npc.ai[0];
            if (Timer > 0 || validTarget)
            {
                npc.ai[0]++;
                
                //Attack Warning
                if (Timer == 40 && npc.HasValidTarget)
                {
                    npc.DoAttackWarning();
                }


                //Attack
                if (Timer >= 60 && Timer < 280 && npc.HasValidTarget)
                {
                    const int SpiritDelay = 11; //Delay between spirits being summoned
                    const int PxPerTile = 16;
                    const int MinDistance = 1 * PxPerTile;
                    const int MaxDistance = 4 * PxPerTile;

                    //Summon spirits
                    if (Timer % SpiritDelay == 0)
                    {
                        int dist = Main.rand.Next(MinDistance, MaxDistance);
                        int angle = Main.rand.Next(1, 360);

                        Vector2 pos = npc.Center + (Vector2.UnitX * dist).RotatedBy(MathHelper.ToRadians(angle));
                        NPC spirit = NPC.NewNPCDirect(npc.GetSource_FromAI(), (int)pos.X, (int)pos.Y, NPCID.DungeonSpirit);
                        SoundEngine.PlaySound(SoundID.Item8.WithVolumeScale(0.5f).WithPitchOffset(0.5f), spirit.position);
                    }

                    //Create Dust
                    if (Timer % 3 == 0)
                    {
                        int dist = Main.rand.Next(MinDistance, MaxDistance);
                        int angle = Main.rand.Next(1, 360);

                        Vector2 pos = npc.Center + (Vector2.UnitX * dist).RotatedBy(MathHelper.ToRadians(angle));
                        Dust d = Dust.NewDustDirect(new Vector2(pos.X, pos.Y), 1, 1, Terraria.ID.DustID.DungeonSpirit);
                        d.noGravity = true;
                        d.scale = Main.rand.NextFloat(1.33f, 2.0f);
                        d.velocity.Y = -MathF.Abs(d.velocity.Y) * 0.67f - (1 - MathF.Abs(d.velocity.X));
                    }
                }


                //Determine teleportation location
                if (Timer == 280)
                {
                    int direction = target.direction;

                    const int PxPerTile = 16;
                    const int MinDistance = 12 * PxPerTile;
                    const int MaxDistance = 18 * PxPerTile;
                    const int RayCount = 9;
                    const int TotalAngle = 90;

                    Vector2[] rays = new Vector2[RayCount];
                    for (int i = 0; i < RayCount; i++)
                    {
                        float rayAngle = (float)((i - RayCount / 2) / (float)RayCount) * TotalAngle;
                        rays[i] = (Vector2.UnitX * -direction).RotatedBy(MathHelper.ToRadians(rayAngle)) * PxPerTile;
                    }

                    for (int i = 0; i < RayCount; i++)
                    {
                        Vector2 start = target.Center + rays[i] * MinDistance / PxPerTile;
                        for (int j = MinDistance / PxPerTile; j < MaxDistance / PxPerTile; j++)
                        {
                            Vector2 testLocation = start + rays[i];
                            if (Collision.SolidCollision(testLocation, npc.width, npc.height))
                                break;
                            if (Collision.AnyCollision(testLocation, Vector2.UnitY, npc.width, npc.height, false).Y != 0)
                                break;
                            if (!Collision.CanHitLine(testLocation, npc.width, npc.height, target.Center, 32, 64))
                                break;
                            start += rays[i];
                        }
                        rays[i] = start;
                    }
                    List<Vector2> availablePositions = new List<Vector2>();
                    availablePositions.AddRange(rays.Where(x =>
                    {
                        float len = (x - target.Center).Length();
                        return len < MaxDistance + PxPerTile
                        && len > MinDistance - PxPerTile;
                    }));
                    if (availablePositions.Count == 0)
                    {
                        availablePositions.Add(npc.position);
                    }

                    int index = Main.rand.Next(availablePositions.Count);
                    Point pos = availablePositions[index].ToPoint();
                    pos.Y -= 24;
                    Vector2 ground = Utilities.TCellsUtils.FindGround(new Rectangle(pos.X, pos.Y, npc.width, npc.height), 40);

                    npc.ai[2] = ground.X;
                    npc.ai[3] = ground.Y;
                    npc.netUpdate = true;
                }


                //Create Dust
                else if (320 < Timer && Timer < 420 && MathF.Pow(Timer, 1.8f) % 60 < 18)
                {
                    Dust d = Dust.NewDustDirect(new Vector2(npc.ai[2], npc.ai[3]-2), npc.width, npc.height, Terraria.ID.DustID.DungeonSpirit);
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(1.33f, 1.67f);
                    d.velocity.Y = -MathF.Abs(d.velocity.Y) * 0.67f - (1 - MathF.Abs(d.velocity.X));
                }


                //Teleport
                if (Timer == 420)
                {
                    if (npc.ai[2] != 0)
                    {
                        npc.position = new Vector2(npc.ai[2], npc.ai[3] - npc.height);
                        npc.ai[2] = 0;
                        npc.ai[3] = 0;
                        npc.netUpdate = true;
                        npc.TargetClosest(false);
                    }
                    npc.velocity.Y += 0.14f;
                }


                //Reset and do attack warning
                else if (Timer >= 550)
                {
                    npc.ai[0] = 0;
                }
            }
            
            return false;
		}
    }
}