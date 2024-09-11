using System.Collections.Generic;
using Stubble.Core.Contexts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaCells.Common;

namespace TerrariaCells.Content.UI;

[Autoload(Side = ModSide.Server)]
public class InventoryManager : ModSystem, IEntitySource
{
    public string Context => "TerraCells Inventory Manager";

    const int INVENTORY_SLOT_COUNT = 6;
    const int WEAPON_SLOT_1 = 0;
    const int WEAPON_SLOT_2 = 1;
    const int SKILL_SLOT_1 = 2;
    const int SKILL_SLOT_2 = 3;
    const int POTION_SLOT = 4;
    const int STORAGE_SLOT = 5;

    private static readonly (int, TerraCellsItemCategory)[] slotCategories =
    [
        (0, TerraCellsItemCategory.Weapon),
        (1, TerraCellsItemCategory.Weapon),
        (2, TerraCellsItemCategory.Skill),
        (3, TerraCellsItemCategory.Skill),
        (4, TerraCellsItemCategory.Potion),
        (5, TerraCellsItemCategory.Storage),
    ];

    InventoryUiConfiguration config;

    private static readonly Dictionary<short, TerraCellsItemCategory> VanillaItemCategories =
        new()
        {
            { ItemID.GoldBroadsword, TerraCellsItemCategory.Weapon },
            { ItemID.FlowerofFire, TerraCellsItemCategory.Weapon },
            { ItemID.DirtBlock, TerraCellsItemCategory.Skill },
            { ItemID.Wood, TerraCellsItemCategory.Skill },
            { ItemID.LesserHealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.HealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.GreaterHealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.SuperHealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.GoldHelmet, TerraCellsItemCategory.Storage },
            { ItemID.GoldChainmail, TerraCellsItemCategory.Storage },
            { ItemID.GoldGreaves, TerraCellsItemCategory.Storage },
            { ItemID.JungleHat, TerraCellsItemCategory.Storage },
            { ItemID.JungleShirt, TerraCellsItemCategory.Storage },
            { ItemID.JunglePants, TerraCellsItemCategory.Storage },
            { ItemID.LargeAmethyst, TerraCellsItemCategory.Storage },
            { ItemID.None, TerraCellsItemCategory.Default },
        };

    private static TerraCellsItemCategory CategoryOfItem(Item item) =>
        item is ITerraCellsCategorization categorization
            ? categorization.Category
            : VanillaItemCategories.GetValueOrDefault(
                (short)item.netID,
                TerraCellsItemCategory.Default
            );

    public override void Load()
    {
        config = (InventoryUiConfiguration)Mod.GetConfig("InventoryUiConfiguration");
        if (config == null)
        {
            Logging.PublicLogger.Error("Missing Inventory/UI Config! (This is a dev issue)");
            return;
        }

        if (config.EnableInventoryLock)
        {
            On_Player.CanAcceptItemIntoInventory += new(FilterPickups);
        }
    }

    public override void PreUpdateItems()
    {
        if (config.EnableInventoryChanges && config.EnableInventoryLock)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.selectedItem < INVENTORY_SLOT_COUNT)
                    return;

                if (
                    player.selectedItem
                    > INVENTORY_SLOT_COUNT - 1 + (10 - INVENTORY_SLOT_COUNT) / 2
                )
                    player.selectedItem = INVENTORY_SLOT_COUNT - 1;
                else
                    player.selectedItem = 0;
            }
        }
    }

    public override void PostUpdateItems()
    {
        if (config.EnableInventoryChanges && config.EnableInventoryLock)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                SortInventory(player);
            }
        }
    }

    public void SortInventory(Player player)
    {
        foreach ((int, TerraCellsItemCategory) slotCategory in slotCategories)
        {
            Item item = player.inventory[slotCategory.Item1];
            if (CategoryOfItem(item) != slotCategory.Item2)
            {
                MoveItemToItsDedicatedCategory(player, item, slotCategory.Item1);
            }
        }
    }

    public bool MoveItemToItsDedicatedCategory(Player player, Item item, int previousInventorySlot)
    {
        switch (CategoryOfItem(item))
        {
            case TerraCellsItemCategory.Default:
                for (int i = 6; i <= 49; i++)
                {
                    if (player.inventory[i].IsAir)
                    {
                        player.inventory[i] = item.Clone();
                        player.inventory[previousInventorySlot].TurnToAir();

                        return true;
                    }
                }
                player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                return false;

            case TerraCellsItemCategory.Weapon:
                if (IsWeaponsSlotsFull(player))
                {
                    player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                    return false;
                }
                else if (player.inventory[WEAPON_SLOT_1].IsAir)
                {
                    player.inventory[WEAPON_SLOT_1] = item.Clone();
                }
                else
                {
                    player.inventory[WEAPON_SLOT_2] = item.Clone();
                }
                // player.inventory[previousInventorySlot].TurnToAir();

                return true;
            case TerraCellsItemCategory.Skill:
                if (IsSkillsSlotsFull(player))
                {
                    player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                    return false;
                }
                if (player.inventory[SKILL_SLOT_1].IsAir)
                {
                    player.inventory[SKILL_SLOT_1] = item.Clone();
                }
                else
                {
                    player.inventory[SKILL_SLOT_2] = item.Clone();
                }
                player.inventory[previousInventorySlot].TurnToAir();
                return true;
            case TerraCellsItemCategory.Potion:
                if (IsPotionSlotFull(player))
                {
                    player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                }
                player.inventory[POTION_SLOT] = item.Clone();
                player.inventory[previousInventorySlot].TurnToAir();
                return true;
            case TerraCellsItemCategory.Storage:
                if (IsStorageSlotFull(player))
                {
                    player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                }
                player.inventory[POTION_SLOT] = item.Clone();
                player.inventory[previousInventorySlot].TurnToAir();
                return true;
            default:
                for (int i = 6; i <= 49; i++)
                {
                    if (player.inventory[i].IsAir)
                    {
                        player.inventory[i] = item.Clone();
                        player.inventory[previousInventorySlot].TurnToAir();
                        return true;
                    }
                }
                player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                return false;
        }
    }

    /// <summary>
    /// Filters items that are to be picked up into the inventory.
    ///
    /// If the item has a categorization (implements TerraCellsItemCategory) then it will be checked against the given categorization of the inventory.
    /// If that section is full, the item will not be picked up.
    /// </summary>
    private static bool FilterPickups(
        On_Player.orig_CanAcceptItemIntoInventory orig,
        Player player,
        Item item
    )
    {
        return CategoryOfItem(item) switch
        {
            TerraCellsItemCategory.Default => false,
            TerraCellsItemCategory.Weapon => !IsSkillsSlotsFull(player),
            TerraCellsItemCategory.Skill => !IsSkillsSlotsFull(player),
            TerraCellsItemCategory.Potion => !IsPotionSlotFull(player),
            TerraCellsItemCategory.Storage => !IsStorageSlotFull(player),
            _ => orig.Invoke(player, item),
        };
    }

    /// <summary>
    /// Checks the two inventory slots that are used for weapons, and returns true if both are occupied.
    /// </summary>
    public static bool IsWeaponsSlotsFull(Player player) =>
        !player.inventory[WEAPON_SLOT_1].IsAir && !player.inventory[WEAPON_SLOT_2].IsAir;

    public static bool IsSkillsSlotsFull(Player player) =>
        !player.inventory[SKILL_SLOT_1].IsAir && !player.inventory[SKILL_SLOT_2].IsAir;

    public static bool IsPotionSlotFull(Player player) => !player.inventory[POTION_SLOT].IsAir;

    public static bool IsStorageSlotFull(Player player) => !player.inventory[STORAGE_SLOT].IsAir;
}
