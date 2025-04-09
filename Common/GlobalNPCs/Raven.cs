using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.Projectiles;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fliers
    {
        bool ravenSettled = false;
        public void RavenSpawn(NPC npc, IEntitySource source)
        {
            for (int i = 0; i < 16000; i++)
            {
                if (Collision.IsWorldPointSolid(npc.Center + new Vector2(0, npc.height / 2 + 1)))
                {
                    break;
                }
                npc.position.Y++;
            }
            npc.ai[0] = 0f;
        }

        public void RavenAI(NPC npc)
        {
            const float ravenSpeedFactor = 1.67f;
			const float invRavenSpeedFactor = 1f / ravenSpeedFactor;

            //make sure npc is real
            if (npc == null || !npc.active) return;

            npc.oldVelocity *= invRavenSpeedFactor;
            npc.velocity *= invRavenSpeedFactor;

            VanillaRavenAI(npc);

            npc.oldVelocity *= ravenSpeedFactor;
            npc.velocity *= ravenSpeedFactor;
        }

        public void RavenPostAI(NPC npc)
        {
            if (ravenSettled)
            {
                npc.noTileCollide = npc.ai[0] == 1;
            }
            else
            {
                npc.ai[0] = 0;
                if (npc.collideY)
                {
                    ravenSettled = true;
                }
            }
        }

        //copied and adjusted from tmodloader source code
        void VanillaRavenAI(NPC npc)
        {
            npc.noGravity = true;
            if (npc.ai[0] == 0f)
            {
                npc.noGravity = false;
                npc.TargetClosest();
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    if (npc.velocity.X != 0f || npc.velocity.Y < 0f || npc.velocity.Y > 0.3) //raven starts flying when moved
                    {
                        npc.ai[0] = 1f;
                        npc.netUpdate = true;
                    }
                    else
                    {
                        Rectangle rectangle = new Rectangle((int)Main.player[npc.target].position.X, (int)Main.player[npc.target].position.Y, Main.player[npc.target].width, Main.player[npc.target].height);
                        if (new Rectangle((int)npc.position.X - 100, (int)npc.position.Y - 100, npc.width + 200, npc.height + 200).Intersects(rectangle) || npc.life < npc.lifeMax) //raven starts flying when damaged or when player is close
                        {
                            npc.ai[0] = 1f;
                            npc.velocity.Y -= 6f;
                            npc.netUpdate = true;
                        }
                    }
                }
            }
            else if (!Main.player[npc.target].dead)
            {
                if (npc.collideX && !npc.noTileCollide)
                {
                    npc.velocity.X = npc.oldVelocity.X * -0.5f;
                    if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                    {
                        npc.velocity.X = 2f;
                    }
                    if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                    {
                        npc.velocity.X = -2f;
                    }
                }
                if (npc.collideY && !npc.noTileCollide)
                {
                    npc.velocity.Y = npc.oldVelocity.Y * -0.5f;
                    if (npc.velocity.Y > 0f && npc.velocity.Y < 1f)
                    {
                        npc.velocity.Y = 1f;
                    }
                    if (npc.velocity.Y < 0f && npc.velocity.Y > -1f)
                    {
                        npc.velocity.Y = -1f;
                    }
                }
                npc.TargetClosest();
                if (npc.direction == -1 && npc.velocity.X > -3f)
                {
                    npc.velocity.X -= 0.1f;
                    if (npc.velocity.X > 3f)
                    {
                        npc.velocity.X -= 0.1f;
                    }
                    else if (npc.velocity.X > 0f)
                    {
                        npc.velocity.X -= 0.05f;
                    }
                    if (npc.velocity.X < -3f)
                    {
                        npc.velocity.X = -3f;
                    }
                }
                else if (npc.direction == 1 && npc.velocity.X < 3f)
                {
                    npc.velocity.X += 0.1f;
                    if (npc.velocity.X < -3f)
                    {
                        npc.velocity.X += 0.1f;
                    }
                    else if (npc.velocity.X < 0f)
                    {
                        npc.velocity.X += 0.05f;
                    }
                    if (npc.velocity.X > 3f)
                    {
                        npc.velocity.X = 3f;
                    }
                }
                float num269 = Math.Abs(npc.position.X + (npc.width / 2) - (Main.player[npc.target].position.X + (Main.player[npc.target].width / 2)));
                float targetHeight = Main.player[npc.target].position.Y - (npc.height / 2);
                if (num269 > 50f)
                {
                    targetHeight -= 100f;
                    //Main.NewText(7);
                }
                if (npc.position.Y < targetHeight)
                {
                    npc.velocity.Y += 0.05f;
                    if (npc.velocity.Y < 0f)
                    {
                        npc.velocity.Y += 0.01f;
                    }
                }
                else
                {
                    npc.velocity.Y -= 0.05f;
                    if (npc.velocity.Y > 0f)
                    {
                        npc.velocity.Y -= 0.01f;
                    }
                }
                npc.velocity.Y = Math.Clamp(npc.velocity.Y, -3f, 3f);
            }
            if (npc.wet)
            {
                if (npc.velocity.Y > 0f)
                {
                    npc.velocity.Y *= 0.95f;
                }
                npc.velocity.Y -= 0.5f;
                if (npc.velocity.Y < -4f)
                {
                    npc.velocity.Y = -4f;
                }
                npc.TargetClosest();
            }
        }
    }
}
