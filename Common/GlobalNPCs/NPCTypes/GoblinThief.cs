using System;
using Terraria;

using static TerrariaCells.Common.Utilities.NPCHelpers;

namespace TerrariaCells.Common.GlobalNPCs.NPCTypes
{
	public class GoblinThief : AIType
	{
		public override bool AppliesToNPC(int npcType)
		{
			return npcType.Equals(Terraria.ID.NPCID.GoblinThief);
		}
		public override void Behaviour(NPC npc)
		{
			if (!npc.HasValidTarget)
				npc.TargetClosest();
			switch (npc.Phase())
			{
				default:
					npc.Phase(0);
					break;
			}
		}


	}
}
