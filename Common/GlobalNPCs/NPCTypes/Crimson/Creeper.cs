using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes.Crimson
{
	public class Creeper : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType == Terraria.ID.NPCID.Creeper;
		}

		public override void Behaviour(NPC npc)
		{
			bool horizontalMovement = npc.ai[1] != 0;

			if(npc.ai[0] == 0 && npc.TryGetGlobalNPC(out CombatNPC cNPC))
				cNPC.allowContactDamage = true;

			if (npc.ai[0] > 100 + Main.rand.Next(60))
			{
				if (horizontalMovement)
				{
					npc.velocity.Y *= -1;
				}
				else
				{
					npc.velocity.X *= -1;
				}
				npc.ai[0] = 0;
				npc.ai[2]++;
			}
			if(horizontalMovement)
				npc.ai[0] += MathF.Abs(npc.velocity.X);
			else
				npc.ai[0] += MathF.Abs(npc.velocity.Y);

			if (npc.ai[2] > 10)
			{
				npc.active = false;
			}
		}
	}
}
