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
    [JsonIgnore]
    public int SetID { get => setID.Value; private set => setID = value; }

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
    private (int, float)[] WIdPool;

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

            foreach (var (id, w) in WIdPool) {
                rand2.Add(id, w);
            }

            return SetID = rand2.Get();
        }
        return SetID = NPCID.FairyCritterBlue;
    }
}