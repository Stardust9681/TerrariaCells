using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace TerrariaCells.Common.Systems;

public class SpawnInfoDeterminer : ModSystem
{
    public Dictionary<StructureSpawnInfo, int> structurePickedIDs = [];

    // deferred call to access worldgen data after loaded
    // called @ TerrariaCells.Common.Systems.BasicWorldGen.LoadWorldData
    public new void OnWorldLoad()
    {
        structurePickedIDs.Clear();
        int seed = Main.ActiveWorldFileData.Seed;
        UnifiedRandom rand = new UnifiedRandom(seed);
        BasicWorldGenData worldGenData = StaticFileAccess.Instance.WorldGenData;

        foreach (Level level in BasicWorldGenData.LevelData)
        {
            LevelStructure structure = level.GetGeneratedStructure(worldGenData);

            if (structure.SpawnInfo == null)
            {
                Mod.Logger.Info($"Skipped setup of {structure.Name} SpawnInfo since it failed to load.");
                continue;
            }

            foreach (StructureSpawnInfo spawnInfo in structure.SpawnInfo)
            {
                spawnInfo.Init(rand);
                if (structurePickedIDs.ContainsKey(spawnInfo))
                {
                    Mod.Logger.Error($"Key already exists! {spawnInfo}");
                }

                int? id = null;
                try
                {
                    id = spawnInfo.SetID;
                }
                catch
                {
                    throw new System.Exception($"Could not find id!");
                }

                structurePickedIDs.Add(spawnInfo, spawnInfo.SetID);

            }

            Mod.Logger.Info($"Setup {structure.SpawnInfo.Count} spawns for {level.Name}.");
        }
    }
}