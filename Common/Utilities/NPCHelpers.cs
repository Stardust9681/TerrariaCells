using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace TerrariaCells.Common.Utilities
{
	public static class NPCHelpers
	{
		public static int Timer(this NPC npc, int? value = null)
		{
			return (int)(npc.ai[0] = (value ?? (int)npc.ai[0]));
		}
		public static int Phase(this NPC npc, int? value = null)
		{
			if (value is null)
			{
				return (int)npc.ai[1];
			}
			npc.ai[1] = value.Value;
			npc.Timer(-1); //Switching to new phase, reset phase timer
			return (int)npc.ai[1];

		}
	}
}
