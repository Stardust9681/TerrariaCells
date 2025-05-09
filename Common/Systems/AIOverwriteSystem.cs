using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

using TerrariaCells.Common.GlobalNPCs.NPCTypes;
using TerrariaCells.Common.GlobalNPCs.NPCTypes.Shared;

namespace TerrariaCells.Common.Systems
{
	public class AIOverwriteSystem : ModSystem
	{
		private static int[] _AIPointers;
		private static AIType[] _AITypesByIndex;
		public override void PostSetupContent()
		{
			_AIPointers = new int[NPCLoader.NPCCount];
			_AITypesByIndex = ModContent.GetContent<AIType>().ToArray();

			for (int i = 0; i < NPCLoader.NPCCount; i++)
			{
				_AIPointers[i] = Array.FindIndex(_AITypesByIndex, x => x.AppliesToNPC(i));
			}
		}

		public override void Unload()
		{
			_AIPointers = null;
			_AITypesByIndex = null;
		}

		public static bool AITypeExists(int forNPC)
		{
			return _AIPointers[forNPC] != -1;
		}
		public static bool TryGetAIType(int forNPC, out AIType ai)
		{
			int index = _AIPointers[forNPC];
			if (index == -1)
			{
				ai = null;
				return false;
			}
			ai = _AITypesByIndex[index];
			return true;
		}
	}
}
