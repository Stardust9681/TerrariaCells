using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;

namespace TerrariaCells.Common.Systems;

public class ChestLootSpawner : ModSystem, IEntitySource
{
    Dictionary<string, int[]> ChestLootTables;


    public List<int> lootedChests = [];

    public string Context => "TerrariaCells.ChestLootSpawner.OnChestOpen";

    public override void SetStaticDefaults()
    {
        On_Player.OpenChest += OnChestOpen;

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

    public void OnChestOpen(On_Player.orig_OpenChest orig, Player self, int x, int y, int newChest)
    {
        using (Stream stream = Mod.GetFileStream("chest loot tables.json"))
        {
            var buf = new byte[stream.Length];
            stream.Read(buf);
            ChestLootTables = JsonSerializer.Deserialize<Dictionary<string, int[]>>(buf);
        }

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

        Main.NewText(tileFrame);

        if (DevConfig.Instance.EnableChestChanges)
        {
            Main.chest[newChest].frame = 2;
            Main.chest[newChest].frameCounter = 10;
            self.chest = -1;


            if (isNewChest)
            {


                if (ChestLootTables.TryGetValue(tileFrame, out int[] value))
                {
                    if (value.Length != 0)
                    {
                        Item.NewItem(
                            this,
                            new Point16(x, y).ToWorldCoordinates(), 0, 0,
                            value[Main.rand.Next(value.Length)]
                        );
                    }
                }
                else
                {
                    Main.NewText("Could not find chest in loot tables: " + tileFrame);
                    // Item.NewItem(
                    //     this,
                    //     new Point16(x, y).ToWorldCoordinates(), 0, 0,
                    //     InventoryManager.GetRandomItem(TerraCellsItemCategory.Weapon)

                    // );
                }
            }
        }



        if (isNewChest)
        {
            lootedChests.Add(newChest);
        }
    }
}
