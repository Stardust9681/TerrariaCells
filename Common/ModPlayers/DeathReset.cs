using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaCells.Common.Systems;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Common.ModPlayers;

public class DeathReset : ModPlayer
{
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        Mod.GetContent<TeleportTracker>().First().Reset();
        Mod.GetContent<ChestLootSpawner>().First().Reset();

    }
}
