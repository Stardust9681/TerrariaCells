﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fliers : GlobalNPC
    {
        public bool ShouldFly = true;
        public override bool InstancePerEntity => true;
        public static int[] FlyingEnemies = { NPCID.Vulture };
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return FlyingEnemies.Contains(entity.type);
        }
        
		//Not doing anything with these, no point in overriding
		/*
		 * public override void SetDefaults(NPC entity)
        {
            base.SetDefaults(entity);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        public override void FindFrame(NPC npc, int frameHeight)
        {
            base.FindFrame(npc, frameHeight);
        }
		*/
        
        public override bool PreAI(NPC npc)
        {
            Update(npc);
            if (npc.type == NPCID.Vulture)
            {
                VultureAI(npc);
                return false;
            }
            return true;
        }
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            if (npc.type == NPCID.Vulture)
            {
                return true;
            }
            return base.CanFallThroughPlatforms(npc);
        }
        public void Update(NPC npc)
        {
            npc.TargetClosest();
            ShouldFly = true;
        }
    }
}
