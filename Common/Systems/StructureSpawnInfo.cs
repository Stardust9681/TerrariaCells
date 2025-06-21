using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace TerrariaCells.Common.Systems;

public class StructureSpawnInfo
{
    public StructureSpawnInfo() {}
    private StructureSpawnInfo(int x, int y)
    {
        X = x;
        Y = y;
    }

    public StructureSpawnInfo(int id, int x, int y) : this(x, y)
    {
        Id = id;
        SetID = id;
    }
    public StructureSpawnInfo(string name, int x, int y) : this(x, y)
    {
        Name = name;
        if (Name != null && NPCID.Search.TryGetId(Name, out int result2))
        {
            SetID = result2;
        }


    }
    public StructureSpawnInfo(int[] idPool, UnifiedRandom rand, int x, int y) : this(x, y)
    {
        IdPool = idPool;
        if (IdPool != null)
        {
            SetID = rand.Next(IdPool);
        }
    }

    public StructureSpawnInfo(WeightedID[] widPool, UnifiedRandom rand, int x, int y) : this(x, y)
    {
        WIdPool = widPool;

        if (WIdPool != null)
        {
            WeightedRandom<int> rand2 = new(rand);

            foreach (var wId in WIdPool)
            {
                rand2.Add(wId.GetID(), wId.Weight);
            }

            SetID = rand2.Get();
        }
    }

    [JsonIgnore]
    public int SetID { get => setID.Value; private set => setID = value; }

    [JsonIgnore]
    public Point Position { get => new(X, Y); private set => (X, Y) = (value.X, value.Y); }

    /// <summary>
    /// The NPC that this SpawnInfo was used to spawn.
    /// If this SpawnInfo hasn't been used to spawn an NPC, this will be null.
    /// 
    /// This DOES NOT check against inactive/"despawned" NPCs. 
    /// </summary>
    [JsonIgnore]
    public NPC SpawnedNPC;

    [JsonInclude]
    [JsonRequired]
    public int X;

    [JsonInclude]
    [JsonRequired]
    public int Y;

    [JsonInclude]
    private int? Id;

    [JsonInclude]
    private string Name;

    [JsonInclude]
    private int[] IdPool;

    [JsonInclude]
    private WeightedID[] WIdPool;

    private int? setID = null;

    internal int Init(UnifiedRandom rand)
    {
        if (Id.HasValue)
        {
            return SetID = Id.Value;
        }
        if (Name != null && NPCID.Search.TryGetId(Name, out int result2))
        {
            return SetID = result2;
        }
        if (IdPool != null)
        {
            return SetID = rand.Next(IdPool);
        }
        if (WIdPool != null)
        {
            WeightedRandom<int> rand2 = new(rand);

            foreach (var wId in WIdPool)
            {
                rand2.Add(wId.GetID(), wId.Weight);
            }

            return SetID = rand2.Get();
        }
        return SetID = NPCID.FairyCritterBlue;
    }
}

public struct WeightedID
{
    public WeightedID() {}
    public WeightedID(int id, float weight)
    {
        Id = id;
        Weight = weight;
    }
    public WeightedID(string name, float weight)
    {
        Name = name;
        Weight = weight;
    }
    
    public int GetID()
    {
        if (Id.HasValue)
            return Id.Value;
        if (Name is not null)
        {
            if (NPCID.Search.TryGetId(Name, out int id)) return id;

            if (Terraria.ModLoader.ModContent.TryFind<Terraria.ModLoader.ModNPC>(Name, out Terraria.ModLoader.ModNPC modNPC))
                return modNPC.Type;
        }
        return NPCID.FairyCritterGreen;
    }

    [JsonInclude]
    private string Name;
    [JsonInclude]
    private int? Id;

    [JsonInclude]
    public float Weight;
}