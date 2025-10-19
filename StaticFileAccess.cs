using System.Collections.Generic;
using System.Linq;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;

namespace TerrariaCells;

public class StaticFileAccess
{
    public BasicWorldGenData WorldGenData => worldGenSystem.BasicWorldGenData;
    private BasicWorldGeneration worldGenSystem;

    private ChestLootSpawner lootSpawnerSystem;

    public static StaticFileAccess Instance { get; private set; }

    internal static void Init(
        TerrariaCells mod
    )
    {
        Instance = new StaticFileAccess
        {
            worldGenSystem = mod.GetContent<BasicWorldGeneration>().First(),
            lootSpawnerSystem = mod.GetContent<ChestLootSpawner>().First(),
        };
    }
}
