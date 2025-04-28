using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StructureHelper;
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
    public BasicWorldGenData basicWorldGenData;

    public override void SetStaticDefaults()
    {
        basicWorldGenData = JsonSerializer.Deserialize<BasicWorldGenData>(
            Mod.GetFileBytes(WorldGenFilePath)
        );
        if (basicWorldGenData == null)
        {
            throw new System.Exception("Could not deserialize world gen data");
        }
        basicWorldGenData.Validate();
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
        PlaceStructures(ModContent.GetContent<BasicWorldGeneration>().First().basicWorldGenData);
    }

    public static void PlaceStructures(BasicWorldGenData worldGenData)
    {
        Point16 offset = worldGenData.PlacementStartOffset;
        foreach (Level level in worldGenData.Levels)
        {
            TagCompound structure = level.Structures.First();
            Generator.Generate(structure, offset);
            offset += new Point16((level.Size + worldGenData.Margins).X, (short)0);
        }
    }
}

public class BasicWorldGenData
{
    /// <summary>
    /// How many tiles should seperate each level
    /// </summary>
    [JsonInclude]
    public Point16 Margins;

    [JsonInclude]
    public Point16 PlacementStartOffset;

    [JsonInclude]
    public List<Level> Levels;

    public Dictionary<string, TagCompound> Structures =>
        structures ??= Levels
            .Select(level => level.StructurePaths.Select(path => path))
            .SelectMany(item => item)
            .Select(item => KeyValuePair.Create(item, TagIO.FromFile(item)))
            .ToDictionary();

    private Dictionary<string, TagCompound> structures;

    public bool Validate()
    {
        int totalWidth = PlacementStartOffset.X;
        foreach (Level level in Levels)
        {
            totalWidth += level.Size.X;
        }
        if (totalWidth > Main.tile.Width)
        {
            // throw exception for now since its more visible
            throw new System.Exception(
                $"Level width is too large! Cannot safely fit all levels {totalWidth}"
            );
        }
        return true;
    }
}

public class Level
{
    [JsonInclude]
    public string Name;

    [JsonInclude]
    public List<string> StructurePaths;

    [JsonInclude]
    public Point16 SpawnOffset;

    [JsonInclude]
    public bool Surface;
    private Point16? size;
    private TagCompound[] structures;

    public Point16 Size
    {
        get
        {
            if (size == null)
            {
                int maxSizeX = 0;
                int maxSizeY = 0;
                foreach (var structure in Structures)
                {
                    Point16 size = new Point16(
                        structure.Get<int>("Width"),
                        structure.Get<int>("Height")
                    );
                    if (maxSizeX < size.X)
                    {
                        maxSizeX = size.X;
                    }
                    if (maxSizeY < size.Y)
                    {
                        maxSizeY = size.Y;
                    }
                }
                size = new Point16(maxSizeX, maxSizeY);
            }
            return size.Value;
        }
    }

    public TagCompound[] Structures => structures ??= [.. StructurePaths.Select(GetStructure)];

    static TagCompound GetStructure(string path)
    {
        System.IO.Stream stream = ModContent.GetInstance<TerrariaCells>().GetFileStream(path);
        TagCompound tagCompound = TagIO.FromStream(stream);
        stream.Dispose();
        return tagCompound;
    }
}
