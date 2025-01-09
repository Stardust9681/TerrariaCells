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
        switch (teleports)
        {
            case 2:
                Main.LocalPlayer.Teleport(new Vector2(32461f, 7814f));
                return; //desert
            case 4:
                Main.LocalPlayer.Teleport(new Vector2(47403f, 7158f));
                return; //hive
            case 6:
                Main.LocalPlayer.Teleport(new Vector2(56771f, 6790f));
                return; //ice
            case 8:
                Main.LocalPlayer.Teleport(new Vector2(8771f, 6102f));
                teleports = 0;
                return; //forest
        }
        Main.LocalPlayer.Teleport(new Vector2(19623f, 10326f)); //inn
    }
}
