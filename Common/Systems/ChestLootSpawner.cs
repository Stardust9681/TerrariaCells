using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;

namespace TerrariaCells.Common.Systems
{
    public class ChestLootSpawner : ModSystem, IEntitySource
    {
        public List<int> lootedChests = [];

        public string Context => "TerrariaCells.ChestLootSpawner.OnChestOpen";

        public override void SetStaticDefaults()
        {
            On_Player.OpenChest += OnChestOpen;
            On_Player.UpdateDead += OnUpdateDead;
        }

        public void OnUpdateDead(On_Player.orig_UpdateDead orig, Player self)
        {
            if (DevConfig.Instance.EnableChestChanges)
            {
                foreach (int chest in lootedChests)
                {
                    // Main.chest[chest].frame = 0;
                    // Main.chest[chest].frameCounter = 0;
                }
            }
            lootedChests.Clear();

        }

        public void OnChestOpen(On_Player.orig_OpenChest orig, Player self, int x, int y, int newChest)
        {
            bool isNewChest = !lootedChests.Contains(newChest);

            if (DevConfig.Instance.EnableChestChanges)
            {
                // Main.chest[newChest].frame = 2;
                // Main.chest[newChest].frameCounter = 10;
                self.chest = -1;

                // if (isNewChest)
                // {



                Item.NewItem(
                    this,
                    new Point16(x, y).ToWorldCoordinates(), 0, 0,
                    InventoryManager.GetRandomItem(TerraCellsItemCategory.Weapon)
                );
                // }

            }

            if (isNewChest)
            {
                lootedChests.Add(newChest);
            }
        }
    }
}
