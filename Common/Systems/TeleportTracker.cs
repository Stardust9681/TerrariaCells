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
                // desert:
                Main.LocalPlayer.Teleport(new(91387.836f, 7734.0f)); 
                return;
            case 4: 
                // hive:
                Main.LocalPlayer.Teleport(new(47393.8f, 7158.0f)); 
                return;
            case 6: 
                // crimson:
                Main.LocalPlayer.Teleport(new(73025.14f, 7510.0f)); 
                return;
            case 8: 
                // frozen:
                Main.LocalPlayer.Teleport(new(56762.617f, 6790.0f)); 
                return;
            case 10: 
                // frozen city
                Main.LocalPlayer.Teleport(new(25136.639f, 6134.0f)); 
                return;
            case 12: 
                // caverns:
                Main.LocalPlayer.Teleport(new(28818.312f, 17606.0f)); 
                return;
            case 14: 
                // forest:
                Main.LocalPlayer.Teleport(new(12026.719f, 5990.0f)); 
                teleports = 0;
                return;
        }

        Main.LocalPlayer.Teleport(new Vector2(19623f, 10326f)); //inn
    }
}
