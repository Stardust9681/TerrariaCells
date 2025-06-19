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

    [JsonIgnore]
    public Point Position { get => new (X, Y); private set => (X, Y) = (value.X, value.Y); }

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

            foreach (var wId in WIdPool) {
                rand2.Add(wId.GetID(), wId.Weight);
            }

            return SetID = rand2.Get();
        }
        return SetID = NPCID.FairyCritterBlue;
    }
}
internal struct WeightedID
{
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