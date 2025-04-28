using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Common.Systems;

public class ChestLootSpawner : ModSystem, IEntitySource
{
    public Dictionary<string, int[]> ChestLootTables;

    public List<int> lootedChests = [];

    public string Context => "TerrariaCells.ChestLootSpawner.OnChestOpen";

    public override void Load()
    {
        using Stream stream = Mod.GetFileStream("chest loot tables.json");
        var buf = new byte[stream.Length];
        stream.Read(buf);
        ChestLootTables = JsonSerializer.Deserialize<Dictionary<string, int[]>>(buf);
    }

    public override void SetStaticDefaults()
    {
        On_Player.OpenChest += OnChestOpen;
    }

    public override void Unload()
    {
        On_Player.OpenChest -= OnChestOpen;
    }

    public override void OnWorldLoad()
    {
        lootedChests.Clear();
    }

    public void Reset()
    {
        if (DevConfig.Instance.EnableChestChanges)
        {
            foreach (int chest in lootedChests)
            {
                Main.chest[chest].frame = 0;
                Main.chest[chest].frameCounter = 0;
            }
        }
        lootedChests.Clear();
    }
    // This is executed on the server or singleplayer
    public override void PostUpdateWorld()
    {
        if (Main.netMode != NetmodeID.SinglePlayer) return;
        foreach (int chest in lootedChests)
        {
            if (Main.chest[chest] == null)
            {
                continue;
            }
            Main.chest[chest].frame = 2;
            Main.chest[chest].frameCounter = 10;
        }
    }
    // MULTIPLAYER ONLY
    public void OpenChest(int x, int y, int newChest)
    {
        bool isNewChest = !lootedChests.Contains(newChest);

        Tile tile = Main.tile[x, y];

        string tileFrameX = (tile.TileFrameX / 36).ToString();
        string tileFrameY = tile.TileFrameY.ToString();
        if (tileFrameY == "0")
        {
            tileFrameY = "";
        }
        else
        {
            tileFrameY = "/" + tileFrameY;
        }
        string tileFrame = tileFrameX + tileFrameY;

        Mod.Logger.Info("Chest opened: " + tileFrame);

        if (DevConfig.Instance.EnableChestChanges)
        {

            if (isNewChest)
            {
                int length = ChestLootTables[tileFrame].Length;
                if (length > 0)
                {
                    int i = Item.NewItem(
                        this,
                        new Point16(x, y).ToWorldCoordinates(),
                        0,
                        0,
                        ChestLootTables[tileFrame][Main.rand.Next(length)]
                    );
                    Item item = Main.item[i];

                    if (item.TryGetGlobalItem<TierSystemGlobalItem>(out var tierSystem))
                    {
                        int level = Mod.GetContent<TeleportTracker>().First().level;
                        tierSystem.SetLevel(item, level);
                    }
                    FunkyModifierItemModifier.Reforge(item);
                }
            }
        }

        if (isNewChest)
        {
            lootedChests.Add(newChest);
        }
    }
    // SINGLEPLAYER ONLY
    public void OnChestOpen(On_Player.orig_OpenChest orig, Player self, int x, int y, int newChest)
    {
        // using (Stream stream = Mod.GetFileStream("chest loot tables.json"))
        // {
        //     var buf = new byte[stream.Length];
        //     stream.Read(buf);
        //     ChestLootTables = JsonSerializer.Deserialize<Dictionary<string, int[]>>(buf);
        // }

        bool isNewChest = !lootedChests.Contains(newChest);

        Tile tile = Main.tile[x, y];

        string tileFrameX = (tile.TileFrameX / 36).ToString();
        string tileFrameY = tile.TileFrameY.ToString();
        if (tileFrameY == "0")
        {
            tileFrameY = "";
        }
        else
        {
            tileFrameY = "/" + tileFrameY;
        }
        string tileFrame = tileFrameX + tileFrameY;

        Mod.Logger.Info("Chest opened: " + tileFrame);

        if (DevConfig.Instance.EnableChestChanges)
        {
            self.chest = -1;

            if (isNewChest)
            {
                int length = ChestLootTables[tileFrame].Length;
                if (length > 0)
                {
                    Item item = Main.item[
                        Item.NewItem(
                        this,
                        new Point16(x, y).ToWorldCoordinates(),
                        0,
                        0,
                        ChestLootTables[tileFrame][Main.rand.Next(length)]
                        )
                    ];
                    if (item.TryGetGlobalItem<TierSystemGlobalItem>(out var tierSystem))
                    {
                        int level = Mod.GetContent<TeleportTracker>().First().level;
                        tierSystem.SetLevel(item, level);
                    }
                    FunkyModifierItemModifier.Reforge(item);
                }
            }
        }

        if (isNewChest)
        {
            lootedChests.Add(newChest);
        }
    }
}
