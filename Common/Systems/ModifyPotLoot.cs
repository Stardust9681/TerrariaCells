using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaCells.Content.Tiles.LevelExitPylon;

namespace TerrariaCells.Common.Systems;

public class ModifyPotLoot : GlobalTile, IEntitySource
{
    const string ChestLootPoolsFileName = "pot loot tables.json";
    public string Context => "TerrariaCells.Common.Systems.ModifyPotLoot";
    public Dictionary<int, int[]> potLootPools = [];

    public override void SetStaticDefaults()
    {
        potLootPools = [];

        JsonArray json = JsonSerializer.Deserialize<JsonArray>(
            Encoding.UTF8.GetString(Mod.GetFileBytes(ChestLootPoolsFileName))
        );

        foreach (JsonNode chest in json)
        {
            JsonObject obj = chest.AsObject();
            foreach (JsonNode style in obj["styles"].AsArray())
            {
                int[] lootPool = obj["lootPool"].AsArray().Select(x => (int)x).ToArray();

                potLootPools.Add((int)style, lootPool);
            }
        }
    }

    public override void KillTile(
        int i,
        int j,
        int type,
        ref bool fail,
        ref bool effectOnly,
        ref bool noItem
    )
    {
        if (type != TileID.Pots)
        {
            return;
        }
        noItem = true;

        if (Main.tile[i, j].TileFrameX % 36 != 0)
        {
            return;
        }
        if (Main.tile[i, j].TileFrameY % 36 != 0)
        {
            return;
        }
        Drop(i, j, type);
    }

    public override void Drop(int i, int j, int type)
    {
        int style = 0,
            alt = 0;

        TileObjectData.GetTileInfo(Main.tile[i, j], ref style, ref alt);

        int[] pool = potLootPools[style];

        int itemType = pool[Main.rand.Next(pool.Length)];
        Item item = new(itemType);

        switch (itemType)
        {
            case ItemID.CopperCoin:
                item.stack = Main.rand.Next(2500);
                int stack = item.stack;
                if (stack > item.maxStack)
                {
                    item = new(ItemID.SilverCoin);
                    item.stack = stack / 100;
                }
                break;
            case ItemID.SilverCoin:
                item.stack = Main.rand.Next(26) + 25;
                break;
        }

        Item.NewItem(this, Rectangle.Empty with { X = i * 16, Y = j * 16 }, item);

        // Main.NewText((style, pool.Length, item.Name));
    }
}
