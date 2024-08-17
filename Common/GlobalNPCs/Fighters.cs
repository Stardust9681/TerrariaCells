using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    public partial class Fighters : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int CustomFrameY = 0;
        public int CustomFrameCounter = 0;
        public bool ShouldWalk = false;

        public float WalkSpeed = 2;
        public float Acceleration = 0.1f;
        public float JumpSpeed = 8;
        public override void SetStaticDefaults()
        {
            for (int i = 0; i < Ghouls.Length; i++)
            {
                NPCID.Sets.TrailCacheLength[Ghouls[i]] = 20;
                NPCID.Sets.TrailingMode[Ghouls[i]] = 3;
            }
        }
        public override void SetDefaults(NPC entity)
        {
            if (Mummies.Contains(entity.type))
            {
                entity.scale = 1.5f;
            }
            base.SetDefaults(entity);
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type == NPCID.DesertScorpionWalk) {
                return DrawSandPoacher(npc, spriteBatch, screenPos, drawColor);
            }
            if (Ghouls.Contains(npc.type))
            {
                return DrawGhoul(npc, spriteBatch, screenPos, drawColor);
            }
            if (Mummies.Contains(npc.type))
            {
                return DrawMummy(npc, spriteBatch, screenPos, drawColor);
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
        
        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.type == NPCID.DesertScorpionWalk)
            {
                SandPoacherFrame(npc);
            }
            base.FindFrame(npc, frameHeight);
        }
        public override void DrawBehind(NPC npc, int index)
        {
            if (npc.type == NPCID.DesertScorpionWalk)
            {
                Main.instance.DrawCacheNPCsBehindNonSolidTiles.Add(index);
            }
        }
        public override bool PreAI(NPC npc)
        {
            if (npc.aiStyle == NPCAIStyleID.Fighter)
            {
                Update(npc);
                Player target = null;
                if (npc.HasValidTarget) target = Main.player[npc.target];

                if (npc.type == NPCID.DesertScorpionWalk)
                {
                    SandPoacherAI(npc, target);
                }
                if (Ghouls.Contains(npc.type))
                {
                    GhoulAI(npc, target);
                }
                if (Mummies.Contains(npc.type))
                {
                    MummyAI(npc, target);
                }
                if (ShouldWalk)
                    Walk(npc, WalkSpeed, Acceleration);

                return false;
            }
            return base.PreAI(npc);
        }
    }
}
