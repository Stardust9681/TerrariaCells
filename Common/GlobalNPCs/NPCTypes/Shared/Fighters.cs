using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared
{
    public partial class Fighters : GlobalNPC, OnAnyPlayerHit.IGlobal
    {
        public override bool InstancePerEntity => true;
        public int CustomFrameY = 0;
        public int CustomFrameCounter = 0;
        public bool ShouldWalk = false;
        public int[] ExtraAI = { 0, 0, 0, 0 };
        public bool JustJumped = false;

        public float WalkSpeed = 2;
        public float Acceleration = 0.1f;
        public float JumpSpeed = 8;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                Fighters.BloodCrawler_Pounce = ModContent.Request<Texture2D>("TerrariaCells/Common/Assets/BloodcrawlerPounce");
            }
        }

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return Mummies.Contains(entity.type)
                || Ghouls.Contains(entity.type)
                || entity.type == NPCID.CultistArcherBlue
                || entity.type == NPCID.BloodCrawler
                || entity.type == NPCID.BloodCrawlerWall
                || entity.type == NPCID.DesertScorpionWalk
                || entity.type == NPCID.DesertScorpionWall;
        }

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
            //entity.damage = 0; //WHY WAS THIS SET?
            if (Mummies.Contains(entity.type))
            {
                entity.scale = 1.5f;
            }
            if (Ghouls.Contains(entity.type))
            {
                JumpSpeed = 10;
                WalkSpeed = 3.5f;
            }
            if (entity.type == NPCID.CultistArcherBlue)
            {
                WalkSpeed = 1;
                JumpSpeed = 8;
            }
            if (entity.type == NPCID.BloodCrawler)
            {
                ExtraAI[1] = 1; //tell bloodcrawler to transform if on wall (only in the first ai cycle)
            }
            if (entity.type == NPCID.BloodCrawlerWall)
            {
                ExtraAI[1] = 1; //tell bloodcrawler to transform if not on wall (only in the first ai cycle)
                ExtraAI[2] = 1; //go counterclockwise when idling
            }
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type == NPCID.DesertScorpionWalk)
            {
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
            if (npc.type == NPCID.CultistArcherBlue)
            {
                return DrawCultistArcher(npc, spriteBatch, screenPos, drawColor);
            }
            if (npc.type == NPCID.BloodCrawler)
            {
                return DrawBloodCrawler(npc, spriteBatch, screenPos, drawColor);
            }
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        public override void FindFrame(NPC npc, int frameHeight)
        {
            if (npc.type == NPCID.DesertScorpionWalk)
            {
                SandPoacherFrame(npc);
            }
            if (Mummies.Contains(npc.type))
            {
                MummyFrame(npc);
            }
            if (npc.type == NPCID.CultistArcherBlue)
            {
                CultistArcherFrame(npc);
            }
            if (npc.type == NPCID.BloodCrawler)
            {
                BloodCrawlerFrame(npc);
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
            //if (Common.Systems.AIOverwriteSystem.AITypeExists(npc.type))
            //    return base.PreAI(npc);

            if (npc.aiStyle == NPCAIStyleID.Fighter || npc.type == NPCID.CultistArcherBlue)
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
                if (npc.type == NPCID.CultistArcherBlue)
                {
                    CultistArcherAI(npc, target);
                }
                if (npc.type == NPCID.BloodCrawler)
                {
                    BloodCrawlerAI(npc, target);
                }
                if (ShouldWalk)
                    Walk(npc, WalkSpeed, Acceleration);
                JustJumped = false;
                return false;
            }
            if (npc.type == NPCID.BloodCrawlerWall)
            {
                BloodCrawlerWallAI(npc);
                return false;
            }
            return base.PreAI(npc);
        }

        public void OnAnyPlayerHit(NPC npc, Player attacker, NPC.HitInfo hit, int damage)
        {
            if (npc.type == NPCID.BloodCrawler)
            {
                BloodCrawler_OnHit(npc, attacker, hit, damage);
            }
        }

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            CombatNPC.ToggleContactDamage(npc, false);
        }

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write7BitEncodedInt(CustomFrameY);
            binaryWriter.Write7BitEncodedInt(CustomFrameCounter);
            binaryWriter.Write(ShouldWalk);
            binaryWriter.Write7BitEncodedInt(ExtraAI.Length);
            for (int i = 0; i < ExtraAI.Length; i++)
                binaryWriter.Write7BitEncodedInt(ExtraAI[i]);

            base.SendExtraAI(npc, bitWriter, binaryWriter);
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            CustomFrameY = binaryReader.Read7BitEncodedInt();
            CustomFrameCounter = binaryReader.Read7BitEncodedInt();
            ShouldWalk = binaryReader.ReadBoolean();
            int length = binaryReader.Read7BitEncodedInt();
            for (int i = 0; i < length; i++)
                ExtraAI[i] = binaryReader.Read7BitEncodedInt();

            base.ReceiveExtraAI(npc, bitReader, binaryReader);
        }
    }
}