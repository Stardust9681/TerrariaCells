using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace TerrariaCells.Common.Systems
{
	public class WorldTimeHandler : ModSystem
	{
		public override void Load()
		{
			On_Main.ShouldNormalEventsBeAbleToStart += DisableNormalEvents;
			On_Main.UpdateTime_SpawnTownNPCs += DisableTownNPCSpawns;
		}

		public override void Unload()
		{
			On_Main.ShouldNormalEventsBeAbleToStart -= DisableNormalEvents;
			On_Main.UpdateTime_SpawnTownNPCs -= DisableTownNPCSpawns;
		}

		//Return, do not call orig, do not spawn Town NPCs
		private void DisableTownNPCSpawns(On_Main.orig_UpdateTime_SpawnTownNPCs orig) { }

		private bool DisableNormalEvents(On_Main.orig_ShouldNormalEventsBeAbleToStart orig)
		{
			//orig.Invoke(); //I don't think there's anything we need to invoke orig for? Let me know.
			return true;
			//For some reason, "ShouldNormalEventsBeAbleToStart" returns true if normal events should NOT be able to start
			//Fuckin' Relogic, man. Fuckin' Relogic...
		}

		//Prevent time updates from happening. Period.
		//No day/night cycle
		//No tile updates
		//No events
		public override void ModifyTimeRate(ref double timeRate, ref double tileUpdateRate, ref double eventUpdateRate)
		{
			timeRate = 0;
			tileUpdateRate = 0;
			eventUpdateRate = 0;
		}
	}
}
