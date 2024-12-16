using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Content.Buffs
{
    public class FrozenEnemyDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<FrozenEnemyDebuffNPC>().frozen = true;
        }
    }

    public class FrozenEnemyDebuffNPC : GlobalNPC
    {
        public int? originalAIStyle;
        public bool frozen = false;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            npc.GetGlobalNPC<FrozenEnemyDebuffNPC>().frozen = false;
        }

        public override void AI(NPC npc)
        {
            if (frozen)
            {
                if (originalAIStyle == null)
                {
                    originalAIStyle = npc.aiStyle;
                }
                npc.aiStyle = -1;

                for (int i = 0; i < 2; i++)
                {
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Ice);
                    dust.scale = 1f;
                    dust.velocity *= 0.4f;
                    dust.noGravity = false;
                }

                npc.velocity.X *= npc.collideY ? 0.8f : 0.98f;
                npc.velocity.Y += 0.3f;
            }
            else if (originalAIStyle != null)
            {
                npc.aiStyle = originalAIStyle.Value;
                originalAIStyle = null;
            }
        }
    }
}
