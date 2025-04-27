using System;
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
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.Systems;

public class BasicWorldGeneration : ModSystem
{
    private const string WorldGenFilePath = "worldgen.json";
    private static readonly string[] whitelist =
    [
        "Reset",
        // "Terrain",
        // "Spawn Point"
    ];

    /// <summary>
    /// Returns the TerraCells related data this world was generated with, if any.
    ///
    /// Returns null if the world was not generated with any.
    /// </summary>
    public static BasicWorldGenData BasicWorldGenData { get; private set; }

    public override void SetStaticDefaults()
    {
        BasicWorldGenData = JsonSerializer.Deserialize<BasicWorldGenData>(
            Mod.GetFileBytes(WorldGenFilePath)
        );
        if (BasicWorldGenData == null)
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

        // Disable vanilla world gen tasks.
        foreach (var task in tasks)
        {
            // TODO: I'm not sure if anything non-obvious breaks by skipping the Reset task.
            if (!whitelist.Contains(task.Name))
            {
                task.Disable();
            }
        }

        tasks.Add(new CustomWorldGenPass("TerraCells World Gen", 1.0));
    }

    public override void OnWorldUnload()
    {
        BasicWorldGenData = null;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["TerraCellsWorldGenData"] = BasicWorldGenData.SerializeData();
    }

    public override void LoadWorldData(TagCompound tag)
    {
        try
        {
            BasicWorldGenData = BasicWorldGenData.Deserialize(
                (TagCompound)tag["TerraCellsWorldGenData"]
            );
            if (BasicWorldGenData.GeneratedWithModVersion != Mod.Version.ToString())
            {
                throw new Exception();
            }
        }
        catch
        {
            BasicWorldGenData = null;
        }
    }
}

public class CustomWorldGenPass(string name, double loadWeight) : GenPass(name, loadWeight)
{
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        PlaceStructures(BasicWorldGeneration.BasicWorldGenData);

        Main.spawnTileX = Main.tile.Width / 2;
        Main.spawnTileY = Main.tile.Height / 2;
        Main.spawnTileX = BasicWorldGeneration.BasicWorldGenData.WorldSpawnX;
        Main.spawnTileY = BasicWorldGeneration.BasicWorldGenData.WorldSpawnY;
    }

    public static void PlaceStructures(BasicWorldGenData basicWorldGenData)
    {
        Mod mod = ModLoader.GetMod("TerrariaCells");

        basicWorldGenData.GeneratedWithModVersion = mod.Version.ToString();

        Point16 offset = new Point16(
            basicWorldGenData.PlacementStartOffsetX,
            basicWorldGenData.PlacementStartOffsetY
        );
        foreach (Level level in basicWorldGenData.LevelData)
        {
            int index = WorldGen.genRand.Next(level.Structures.Count);
            basicWorldGenData.PickedLevels.Add(level.Name, index);
            LevelStructure structure = level.Structures[index];

            string path = structure.Path;
            Point16 pos = offset + new Point16(structure.OffsetX, structure.OffsetY);
            StructureHelper.API.Generator.GenerateStructure(path, pos, mod);

            WorldGen.PlaceTile(pos.X, pos.Y, TileID.LunarOre);

            short width = (short)StructureHelper.API.Generator.GetStructureData(path, mod).width;
            offset += new Point16(width + basicWorldGenData.MarginsX, 0);
        }
    }
}

/// <summary>
/// The worldgen.json information that a world was generated with
/// </summary>
public class BasicWorldGenData : TagSerializable
{
    [JsonIgnore]
    public string GeneratedWithModVersion;

    [JsonInclude]
    public short WorldSpawnX;

    [JsonInclude]
    public short WorldSpawnY;

    /// <summary>
    /// How many tiles should seperate each level
    /// </summary>
    [JsonInclude]
    public short MarginsX;

    [JsonInclude]
    public short MarginsY;

    [JsonInclude]
    public short PlacementStartOffsetX;

    [JsonInclude]
    public short PlacementStartOffsetY;

    /// <summary>
    /// The level information available at time of generation.
    ///
    /// It is unuseful to store with the world data, so instead the level data is pulled straight from the mod.
    /// This only works if the world was generated with the current version of the mod,
    /// and breaks if you try to load a world with an invalid version of the mod.
    /// <seealso cref="GeneratedWithModVersion">
    /// </summary>
    [JsonInclude]
    public List<Level> LevelData;

    [JsonIgnore]
    /// <summary>
    /// After generating the world, these are the level variations picked per level.
    ///
    /// The dictionary maps each generated level's name to the picked variations index in the list of LevelStructure data.
    /// <summary>
    public Dictionary<string, int> PickedLevels = [];

    public TagCompound SerializeData()
    {
        var levels = PickedLevels.Keys.ToList();
        var indices = PickedLevels.Values.ToList();

        return new TagCompound
        {
            ["GeneratedWithModVersion"] = GeneratedWithModVersion,
            ["WorldSpawnX"] = WorldSpawnX,
            ["WorldSpawnY"] = WorldSpawnY,
            ["MarginsX"] = MarginsX,
            ["MarginsY"] = MarginsY,
            ["PlacementStartOffsetX"] = PlacementStartOffsetX,
            ["PlacementStartOffsetY"] = PlacementStartOffsetY,
            ["PickedLevelsKeys"] = levels,
            ["PickedLevelsIndices"] = indices,
        };
    }

    public static BasicWorldGenData Deserialize(TagCompound compound)
    {
        BasicWorldGenData data = new()
        {
            GeneratedWithModVersion = (string)compound["GeneratedWithModVersion"],
            WorldSpawnX = (short)compound["WorldSpawnX"],
            WorldSpawnY = (short)compound["WorldSpawnY"],
            MarginsX = (short)compound["MarginsX"],
            MarginsY = (short)compound["MarginsY"],
            PlacementStartOffsetX = (short)compound["PlacementStartOffsetX"],
            PlacementStartOffsetY = (short)compound["PlacementStartOffsetY"],
            PickedLevels = ((List<string>)compound["PickedLevelsKeys"])
                .Zip((List<int>)compound["PickedLevelsIndices"])
                .ToDictionary(),
        };

        return data;
    }
}

public class Level : TagSerializable
{
    [JsonInclude]
    public string Name;

    [JsonInclude]
    public List<LevelStructure> Structures;

    [JsonInclude]
    public bool Surface;

    public TagCompound SerializeData()
    {
        throw new System.NotImplementedException();
    }
}

public class LevelStructure
{
    [JsonInclude]
    public string Path;

    [JsonInclude]
    public short OffsetX;

    [JsonInclude]
    public short OffsetY;

    [JsonInclude]
    public short SpawnX;

    [JsonInclude]
    public short SpawnY;
}
