using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using TerrariaCells.Common.Configs;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.ModPlayers;

namespace TerrariaCells.Common.Systems;

public class ChestLootSpawner : ModSystem, IEntitySource
{
    //public Dictionary<string, int[]> ChestLootTables;

    public List<int> lootedChests = [];

    public string Context => "TerrariaCells.ChestLootSpawner.OnChestOpen";

    public override void Load()
    {
        ///See <see cref="ModPlayers.RewardPlayer."/>
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
    public void OpenChest(int x, int y, int newChest, int fromWho)
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

        //Mod.Logger.Info("Chest opened: " + tileFrame);

        if (DevConfig.Instance.EnableChestChanges)
        {
            if (isNewChest)
            {
                if (ItemsJson.Instance.ChestOverrides.TryGetValue(tileFrame, out var func))
                {
                    int[] loot_ids = Main.player[fromWho].GetModPlayer<MetaPlayer>().GetDropOptions(func.Invoke()).ToArray();
                    if (loot_ids.Length > 0)
                    {
                        int i = Item.NewItem(
                            this,
                            new Point16(x, y).ToWorldCoordinates(),
                            0,
                            0,
                            Main.rand.Next(loot_ids)
                        );
                        Item item = Main.item[i];

                        if (item.TryGetGlobalItem<TierSystemGlobalItem>(out var tierSystem))
                        {
                            int level = Mod.GetContent<TeleportTracker>().First().level;
                            tierSystem.SetLevel(item, level);
                        }

                        FunkyModifierItemModifier.Reforge(item);
                    } else {
                        NPC.NewNPC(this, x * 16, y * 16, NPCID.Firefly);
                    }
                }

                RewardTrackerSystem.UpdateChests_Open(x, y);
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

        //Mod.Logger.Info("Chest opened: " + tileFrame);

        if (DevConfig.Instance.EnableChestChanges)
        {
            self.chest = -1;

            if (isNewChest)
            {
                if (ItemsJson.Instance.ChestOverrides.TryGetValue(tileFrame, out var func))
                {
                    int[] loot_ids = self.GetModPlayer<MetaPlayer>().GetDropOptions(func.Invoke()).ToArray();
                    if (loot_ids.Length > 0)
                    {
                        Item item = Main.item[
                            Item.NewItem(
                            this,
                            new Point16(x, y).ToWorldCoordinates(),
                            0,
                            0,
                            Main.rand.Next(loot_ids)
                            )
                        ];

                        if (item.TryGetGlobalItem<TierSystemGlobalItem>(out var tierSystem))
                        {
                            int level = Mod.GetContent<TeleportTracker>().First().level;
                            tierSystem.SetLevel(item, level);
                        }

                        FunkyModifierItemModifier.Reforge(item);
                    }
                } else {
                    NPC.NewNPC(this, (x + 1) * 16, y * 16, NPCID.Firefly);
                }

                RewardTrackerSystem.UpdateChests_Open(x, y, self);

                lootedChests.Add(newChest);
            }
        }
    }
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Add(nameof(lootedChests), lootedChests);
    }
    public override void LoadWorldData(TagCompound tag)
    {
        try
        {
            lootedChests = (List<int>)tag.GetList<int>(nameof(lootedChests)) ?? new List<int>();
        }
        catch (System.Exception x)
        {
            lootedChests = new List<int>();
        }
    }
}