using System;
using Terraria;
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

        base.OnWorldLoad();
    }

    public void Teleport()
    {
        teleports += 1;
		Vector2 position;
		// forest --> crimson --> desert --> hive --> frozen city
		switch (teleports)
        {
			case 10: //Forest
				position = new Vector2(12026.719f, 5990);
				teleports = 0;
				break;
			case 2: //Crimson
				position = new Vector2(4433, 453) * 16;
				break;
			case 4: //Desert
				position = new Vector2(91387.836f, 7734);
				break;
			case 6: //Hive
				position = new Vector2(47393.8f, 7158);
				break;
			case 8: //Frozen City
				position = new Vector2(25136.639f, 6134);
				break;
			default: //Inn-Between
				position = new Vector2(19623f, 10326f);
				break;
			//case 10: //Caverns
			//  position = new Vector2(28818.312f, 17606);
			//  return;
		}
		Main.LocalPlayer.Teleport(position);
    }
}
