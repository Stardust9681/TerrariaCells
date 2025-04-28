using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StructureHelper;
using StructureHelper.API;
using StructureHelper.Models;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.Systems;

public class BasicWorldGeneration : ModSystem
{
    private const string WorldGenFilePath = "worldgen.json";
    public static BasicWorldGenData basicWorldGenData;

    public override void SetStaticDefaults()
    {
        basicWorldGenData = JsonSerializer.Deserialize<BasicWorldGenData>(
            Mod.GetFileBytes(WorldGenFilePath)
        );
        if (basicWorldGenData == null)
        {
            throw new System.Exception("Could not deserialize world gen data");
        }
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        if (!DevConfig.Instance.EnableCustomWorldGen)
        {
            return;
        }

        // tasks.Clear();

        // Disable vanilla world gen tasks.
        foreach (var task in tasks)
        {
            // TODO: I'm not sure if anything non-obvious breaks by skipping the Reset task.
            if (task.Name != "Reset")
            {
                task.Disable();
            }
        }

        tasks.Add(new CustomWorldGenPass("TerraCells World Gen", 1.0));
    }
}

public class CustomWorldGenPass(string name, double loadWeight) : GenPass(name, loadWeight)
{
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        PlaceStructures(BasicWorldGeneration.basicWorldGenData);
    }

    public static void PlaceStructures(BasicWorldGenData worldGenData)
    {
        Mod mod = ModLoader.GetMod("TerrariaCells");
        Point16 offset = new Point16(
            worldGenData.PlacementStartOffsetX,
            worldGenData.PlacementStartOffsetY
        );
        foreach (Level level in worldGenData.Levels)
        {
            Structure structure = level.Structures.First();
            string path = structure.Path;
            Point16 pos = offset + new Point16(structure.SpawnOffsetX, structure.SpawnOffsetY);
            StructureHelper.API.Generator.GenerateStructure(path, pos, mod);
            short width = (short)StructureHelper.API.Generator.GetStructureData(path, mod).width;
            offset += new Point16(width + worldGenData.MarginsX, 0);
        }
    }
}

public class BasicWorldGenData
{
    /// <summary>
    /// How many tiles should seperate each level
    /// </summary>
    [JsonInclude]
    public short MarginsX;
    public short MarginsY;

    [JsonInclude]
    public short PlacementStartOffsetX;

    [JsonInclude]
    public short PlacementStartOffsetY;

    [JsonInclude]
    public List<Level> Levels;

    // public Dictionary<string, StructureData> Structures =>
    //     structures ??= Levels
    //         .Select(level => level.StructurePaths.Select(path => path))
    //         .SelectMany(flatten => flatten)
    // .Select(path => KeyValuePair.Create(path, TagIO.FromFile(path)))
    // .ToDictionary();

    private Dictionary<string, StructureData> structures;

}

public class Level
{
    [JsonInclude]
    public string Name;

    [JsonInclude]
    public List<Structure> Structures;

    [JsonInclude]
    public bool Surface;
}

public class Structure
{
    [JsonInclude]
    public string Path;

    [JsonInclude]
    public short SpawnOffsetX;

    [JsonInclude]
    public short SpawnOffsetY;
}
