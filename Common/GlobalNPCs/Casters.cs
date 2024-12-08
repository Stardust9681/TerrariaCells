using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Casters : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int CustomFrameCounter = 0;
        public int CustomFrameY = 0;
        public override void SetDefaults(NPC entity)
        {
            base.SetDefaults(entity);
            if (entity.type == NPCID.DesertDjinn)
            {
                entity.noTileCollide = true;
                NPCID.Sets.TrailCacheLength[entity.type] = 5;
                NPCID.Sets.TrailingMode[entity.type] = 1;
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type == NPCID.DesertDjinn)
            {
                return DesertSpiritDraw(npc, spriteBatch, screenPos, drawColor);
            }
            if (npc.type == NPCID.CultistDevote)
            {
                return DrawCultistDevotee(npc, spriteBatch, screenPos, drawColor);
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.type == NPCID.DesertDjinn)
            {
                DesertSpiritFrame(npc);
            }
            if (npc.type == NPCID.CultistDevote)
            {
                CultistDevoteeFrame(npc);
            }
            base.FindFrame(npc, frameHeight);
        }
        
        public override bool PreAI(NPC npc)
        {
            npc.TargetClosest();
            Player target = null;
            if (npc.HasValidTarget)
            {
                target = Main.player[npc.target];
            }
            if (npc.type == NPCID.DesertDjinn)
            {
                return DesertSpiritAI(npc, target);
            }
            if (npc.type == NPCID.CultistDevote)
            {
                return CultistDevoteeAI(npc, target);
            }
            return base.PreAI(npc);
        }

        //teleport to a random position. teleports near the given position.
        //returns false if it fails.
        public bool Teleport(NPC npc, Vector2 centerPos, float radius, bool preferLineOfSight = true, int tries = 10)
        {
            Vector2[] spots = {  };
            int[] los = { };
            
            for (int i = 0; i < tries; i++)
            {
                
                Vector2 spot = centerPos + new Vector2(Main.rand.NextFloat(2, radius), 0).RotatedByRandom(MathHelper.TwoPi);
                spot = TCellsUtils.FindGround(new Rectangle((int)spot.X - npc.width / 2, (int)spot.Y - npc.height / 2, npc.width, npc.height));

                bool available = true;
                if (Collision.SolidCollision(spot - npc.Size/2, npc.width, npc.height))
                {
                    available = false;
                }
                if (available)
                {

                    
                    spots = spots.Append(spot).ToArray();
                    if (Collision.CanHitLine(npc.Center, 1, 1, centerPos, 1, 1))
                    {
                        los = los.Append(spots.Length - 1).ToArray();
                    }
                    
                }
            }
           
            if (spots.Length > 0)
            {
                
                Vector2 selected = spots[Main.rand.Next(0, spots.Length)];
                if (preferLineOfSight && los.Length > 0)
                {
                    selected = spots[los[Main.rand.Next(0, los.Length)]];
                }
                
                npc.Center = selected;
                return true;
            }
            return false;
        }
    }
}
