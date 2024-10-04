using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaCells.Common.GlobalNPCs
{
	public class CombatNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public bool allowContactDamage;

		public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
		{
			if (!allowContactDamage) return false;
			return base.CanHitPlayer(npc, target, ref cooldownSlot);
		}

		public static void ToggleContactDamage(NPC npc, bool value) => npc.GetGlobalNPC<CombatNPC>().allowContactDamage = value;
	}
}
