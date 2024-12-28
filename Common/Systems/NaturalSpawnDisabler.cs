using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaCells.Common.Systems
{
	//Disable natural NPC spawns since we'll be handling spawns separately
	//TODO:
		//Disable invasion spawns? Shouldn't be able to occur normally...
	public class NaturalSpawnDisabler : GlobalNPC
	{
		//Set max natural spawns to 0; prevents anything from spawning
		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			maxSpawns = 0;
		}
	}
}
