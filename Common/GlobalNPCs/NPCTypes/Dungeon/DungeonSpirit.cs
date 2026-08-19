using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

using TerrariaCells.Common.Utilities;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;
using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Dungeon
{
    public class DungeonSpirit: GlobalNPC
    {
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.type == NPCID.DungeonSpirit;
        }

        public override void SetStaticDefaults()
        {
            NPCID.Sets.NeverDropsResourcePickups[NPCID.DungeonSpirit] = true;
        }

        public override void SetDefaults(NPC entity)
        {
            entity.value = 0f;
            entity.velocity.Y = 2.0f;
            entity.HitSound = SoundID.Item20;
            entity.DeathSound = SoundID.Item20;
        }

        public override void OnSpawn(NPC npc, IEntitySource source) => CombatNPC.ToggleContactDamage(npc, false);

        public override bool PreAI(NPC npc)
        {
            npc.ai[0] ++;
            const float MaxVelocity = -5.0f;
            const int TimeToAttack = 40;

            if (npc.ai[0] < TimeToAttack)
            {
                npc.rotation = MathHelper.ToRadians(180);
                float VelocityAmount =  Math.Abs(MaxVelocity) / (float)TimeToAttack;
                npc.velocity.Y = MathHelper.Lerp(npc.velocity.Y, MaxVelocity, VelocityAmount);
                return false;
            }

            if (npc.ai[0] == TimeToAttack)
            {
                CombatNPC.ToggleContactDamage(npc, true);
                npc.DoAttackWarning();
                return true; //Vanilla AI
            }

        return true; //Vanilla AI
        }
    }
}
