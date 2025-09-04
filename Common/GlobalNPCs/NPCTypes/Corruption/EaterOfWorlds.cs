using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

using TerrariaCells.Common.GlobalNPCs.NPCTypes.Crimson;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using System.IO;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Corruption
{
    public class EaterOfWorlds : GlobalNPC
    {
        public override bool AppliesToEntity(NPC npc, bool lateInstantiation) => npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail;

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            SpawnPos = npc.Center + new Vector2(0, -64);
        }
        public override void SetDefaults(NPC entity)
        {
            entity.lifeMax = 50;
            entity.defense = 0;
        }

        public static Vector2? SpawnPos { get; internal set; } = null;
        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
            {
                bitWriter.WriteBit(SpawnPos.HasValue);
                if (SpawnPos != null)
                {
                    binaryWriter.Write(SpawnPos.Value.X);
                    binaryWriter.Write(SpawnPos.Value.Y);
                }
            }
        }
        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
            {
                if (bitReader.ReadBit())
                {
                    SpawnPos = new Vector2(binaryReader.ReadSingle(), binaryReader.ReadSingle());
                }
            }
        }
    }

    public class EaterOfWorldsCamera : ModPlayer
    {
        private bool nearEoW = false;
        public override void PreUpdate()
        {
            if (Main.netMode == 2)
                return;
            nearEoW = false;
            foreach (NPC npc in Main.ActiveNPCs)
                if (npc.type >= NPCID.EaterofWorldsHead && npc.type <= NPCID.EaterofWorldsTail)
                {
                    nearEoW = true;
                    break;
                }
            if (nearEoW && EaterOfWorlds.SpawnPos.HasValue)
            {
                Systems.CameraManipulation.SetZoom(45, new Vector2(80, 45) * 16, null);
                Systems.CameraManipulation.SetCamera(45, EaterOfWorlds.SpawnPos.Value - Main.ScreenSize.ToVector2() * 0.5f);
            }
        }
    }
}
