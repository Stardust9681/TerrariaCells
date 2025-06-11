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
using Terraria.ObjectData;
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
    private BasicWorldGenData basicWorldGenData;

    /// <summary>
    /// Returns the TerraCells related data this world was generated with, if any.
    ///
    /// Returns null if the world was not generated with any.
    /// </summary>
    public BasicWorldGenData BasicWorldGenData
    {
        get
        {
            if (basicWorldGenData == null)
            {
                Main.NewText(
                    "TerraCells world generation data missing! Some TerraCells features may not work."
                );
                Mod.Logger.Error("Missing BasicWorldGenData!");
            }
            return basicWorldGenData;
        }
        private set => basicWorldGenData = value;
    }
    public static List<Level> StaticLevelData { get; private set; }

    public override void SetStaticDefaults()
    {
        BasicWorldGenData = JsonSerializer.Deserialize<BasicWorldGenData>(
            Mod.GetFileBytes(WorldGenFilePath)
        );
        if (BasicWorldGenData == null)
        {
            throw new Exception(
                "Could not deserialize worldgen.json! (If building, check if worldgen.json is present in the source.)"
            );
        }
        StaticLevelData = BasicWorldGenData.LevelData;
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        SetStaticDefaults();

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

    public override void SaveWorldData(TagCompound tag)
    {
        var data = BasicWorldGenData;
        if (data is null) {
            // TerraCells didn't generate this world, skip saving its data
            return;
        }
        tag["TerraCellsWorldGenData"] = BasicWorldGenData.SerializeData();
    }

    public override void OnWorldUnload()
    {
        BasicWorldGenData = null;
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
                throw new Exception("Invalid mod version!");
            }
            Mod.Logger.Info("Deserialized worldgen data successfully");
        
            NPCRoomSpawner.ResetSpawns();
        }
        catch (Exception e)
        {
            BasicWorldGenData = null;

            Mod.Logger.Error(e);
        }
    }
}

public class CustomWorldGenPass(string name, double loadWeight) : GenPass(name, loadWeight)
{
    private const string STARTING_LEVEL = "Forest";

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        BasicWorldGenData data = ModContent
            .GetContent<BasicWorldGeneration>()
            .First()
            .BasicWorldGenData;

        PlaceStructures(data);

        Level structure = data.LevelData.Find(x => x.Name == STARTING_LEVEL);
        int variation = data.LevelVariations[STARTING_LEVEL];
        Point16 offset = data.LevelPositions[STARTING_LEVEL];
        Main.spawnTileX = structure.Structures[variation].SpawnX + offset.X;
        Main.spawnTileY = structure.Structures[variation].SpawnY + offset.Y;
        //Maybe something we want to consider:
        //Use the loaded character's difficulty to determine what difficulty the world should be
        //That, or change the thing we're detouring, so player can specify world difficulty
        //Though it'd be odd to have the option to have world size or crimson/corruption when those won't apply
        Main.GameMode = GameModeID.Normal;
    }

    public static void PlaceStructures(BasicWorldGenData basicWorldGenData)
    {
        Mod mod = ModLoader.GetMod("TerrariaCells");

        basicWorldGenData.GeneratedWithModVersion = mod.Version.ToString();

        Point16 offset = new Point16(
            basicWorldGenData.PlacementStartOffsetX,
            basicWorldGenData.PlacementStartOffsetY
        );
        foreach (Level level in BasicWorldGeneration.StaticLevelData)
        {
            int index = WorldGen.genRand.Next(level.Structures.Count);
            basicWorldGenData.LevelVariations.Add(level.Name, index);
            LevelStructure structure = level.Structures[index];

            string path = structure.Path;
            Point16 pos = offset + new Point16(structure.OffsetX, structure.OffsetY);
            StructureHelper.API.Generator.GenerateStructure(path, pos, mod);
            basicWorldGenData.LevelPositions.Add(level.Name, pos);

            WorldGen.PlaceTile(pos.X, pos.Y, TileID.LunarOre);

            short width = (short)StructureHelper.API.Generator.GetStructureData(path, mod).width;
            short height = (short)StructureHelper.API.Generator.GetStructureData(path, mod).height;

            offset += new Point16(width + basicWorldGenData.MarginsX, 0);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Tile tile = Main.tile[i + pos.X, j + pos.Y];
                    if (TileID.Sets.BasicChest[tile.TileType])
                    {
                        if (tile.TileFrameX % 36 == 0 && tile.TileFrameY % 36 == 0)
                        {
                            Chest.CreateChest(i + pos.X, j + pos.Y);
                        }
                    }
                }
            }
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
    public Dictionary<string, int> LevelVariations = [];

    /// <summary>
    /// After generating the world, these are the tile positions of each level's placement.
    ///
    /// The dictionary maps each generated level's name to the picked variations index in the list of LevelStructure data.
    /// <summary>
    public Dictionary<string, Point16> LevelPositions = [];

    public TagCompound SerializeData()
    {
        var levelOrdering = LevelVariations.Keys.ToList();
        var indices = LevelVariations.Values.ToList();
        var positions = LevelPositions.Values.ToList();

        return new TagCompound
        {
            ["GeneratedWithModVersion"] = GeneratedWithModVersion,
            ["WorldSpawnX"] = WorldSpawnX,
            ["WorldSpawnY"] = WorldSpawnY,
            ["MarginsX"] = MarginsX,
            ["MarginsY"] = MarginsY,
            ["PlacementStartOffsetX"] = PlacementStartOffsetX,
            ["PlacementStartOffsetY"] = PlacementStartOffsetY,
            ["Levels"] = levelOrdering,
            ["LevelVariations"] = indices,
            ["LevelPositions"] = positions,
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
            LevelVariations = ((List<string>)compound.GetList<string>("Levels"))
                .Zip((List<int>)compound.GetList<int>("LevelVariations"))
                .ToDictionary(),
            LevelPositions = ((List<string>)compound.GetList<string>("Levels"))
                .Zip((List<Point16>)compound.GetList<Point16>("LevelPositions"))
                .ToDictionary(),
        };

        return data;
    }
}

/// <summary>
/// Level data pulled from worldgen.json
/// </summary>
public class Level
{
    [JsonInclude]
    public string Name;

    [JsonInclude]
    public List<LevelStructure> Structures;

    [JsonInclude]
    public bool Surface;
}

public class LevelStructure
{
    [JsonInclude]
    public string Name;

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

