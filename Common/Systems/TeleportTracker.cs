using System;
using System.Security.Cryptography.Pkcs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Systems;

public class TeleportTracker : ModSystem
{
    public int teleports = 0;

    public override void OnModLoad()
    {
        base.OnModLoad();
    }

    public void Reset()
    {
        teleports = 0;
    }

    public override void OnWorldLoad()
    {
        teleports = 0;
		Main.dayTime = true;
		Main.time = 4f * 3600f;
		Main.StopRain();
    }

    public void Teleport()
    {
        teleports += 1;
		Vector2 position;
		// forest --> crimson --> desert --> hive --> frozen city
		float hour = 7.5f;
		bool day = true;
		float rain = 0f;
		switch (teleports)
        {
			case 10: //Forest
				position = new Vector2(12026.719f, 5990);
				teleports = 0;
				hour = 4f;
				break;
			case 2: //Crimson
				position = new Vector2(4433, 453) * 16;
				hour = 2.5f;
				break;
			case 4: //Desert
				position = new Vector2(91387.836f, 7734);
				break;
			case 6: //Hive
				position = new Vector2(47393.8f, 7158);
				hour = 4;
				day = false;
				break;
			case 8: //Frozen City
				position = new Vector2(25136.639f, 6134);
				hour = 4.5f;
				rain = 1f;
				var bottle = new Item(ItemID.CloudinaBottle);
				Utils.Swap(ref Main.LocalPlayer.armor[5], ref bottle);
				break;
			default: //Inn-Between
				position = new Vector2(19623f, 10326f);
				break;
			//case 10: //Caverns
			//  position = new Vector2(28818.312f, 17606);
			//  return;
		}
		Main.StartRain();
		Main.raining = rain != 0;
		Main.rainTime = rain != 0 ? 100000f : 0f;
		Main.dayTime = day;
		Main.time = hour * 3600;
		Main.LocalPlayer.Teleport(position);
    }
}
