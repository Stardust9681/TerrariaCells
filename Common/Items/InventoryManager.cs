using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;

namespace TerrariaCells.Common.Items;

/// Handles the logic behind locking inventory slots
[Autoload(Side = ModSide.Both)]
public class InventoryManager : ModSystem, IEntitySource
{
    public string Context => "TerraCells Inventory Manager";

    const int INVENTORY_SLOT_COUNT = 5;
    const int WEAPON_SLOT_1 = 0;
    const int WEAPON_SLOT_2 = 1;
    const int SKILL_SLOT_1 = 2;
    const int SKILL_SLOT_2 = 3;
    const int POTION_SLOT = 4;
    const int STORAGE_SLOT_1 = 10;
    const int STORAGE_SLOT_2 = 11;
    const int STORAGE_SLOT_3 = 12;
    const int STORAGE_SLOT_4 = 13;

    internal static readonly (int, TerraCellsItemCategory)[] slotCategorizations =
    [
        (0, TerraCellsItemCategory.Weapon),
        (1, TerraCellsItemCategory.Weapon),
        (2, TerraCellsItemCategory.Skill),
        (3, TerraCellsItemCategory.Skill),
        (4, TerraCellsItemCategory.Potion),
        (10, TerraCellsItemCategory.Storage),
        (11, TerraCellsItemCategory.Storage),
        (12, TerraCellsItemCategory.Storage),
        (13, TerraCellsItemCategory.Storage),
    ];

    /// <summary>
    /// Gets the categorization for a given item.
    /// If the item implements ITerraCellsCategorization, it uses that.
    /// Otherwise, it checks the list of categorizations for vanilla items.
    /// If nothing is found, it returns TerraCellsItemCategory.Default
    /// <summary>
    public static TerraCellsItemCategory GetItemCategorization(Item item) =>
        item.ModItem is ITerraCellsCategorization categorization
            ? categorization.Category
            : VanillaItemCategorizations.GetValueOrDefault(
                (short)item.netID,
                TerraCellsItemCategory.Default
            );

    public static TerraCellsItemCategory GetItemCategorization(int type) =>
        ModContent.GetModItem(type) is ITerraCellsCategorization categorization
            ? categorization.Category
            : VanillaItemCategorizations.GetValueOrDefault(
                (short)type,
                TerraCellsItemCategory.Default
            );

    public static StorageItemSubcategorization GetStorageItemSubcategorization(Item item) =>
        GetItemCategorization(item) is TerraCellsItemCategory.Storage
            ? StorageSubcategorizations.GetValueOrDefault(
                (short)item.netID,
                StorageItemSubcategorization.None
            )
            : StorageItemSubcategorization.None;

    public static StorageItemSubcategorization GetStorageItemSubcategorization(int type) =>
        GetItemCategorization(type) is TerraCellsItemCategory.Storage
            ? StorageSubcategorizations.GetValueOrDefault(
                (short)type,
                StorageItemSubcategorization.None
            )
            : StorageItemSubcategorization.None;

    public static int GetRandomItem(TerraCellsItemCategory category)
    {
        while (true)
        {
            int id = (int)(Main.rand.NextFloat() * 3400);
            if (GetItemCategorization(id) == category)
            {
                return id;
            }
        }
        ;
    }

    public static readonly Dictionary<string, short> ItemIDNames = typeof(ItemID)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
        .ToDictionary(x => x.Name)
        .Select(x => KeyValuePair.Create(x.Key, (short)x.Value.GetRawConstantValue()))
        .ToDictionary();

    public static readonly Dictionary<short, string> ItemNameIDs = ItemIDNames
        .Select(x => KeyValuePair.Create(x.Value, x.Key))
        .ToDictionary();

    /// <summary>
    /// A list of all of categorizations for vanilla items, including those that get reworked.
    ///
    /// DOES NOT CONTAIN MODDED ITEMS. It only contains categorizations for items with a vanilla ItemID.
    /// Categorization for modded items are contained within their own classes, using ITerraCellsCategorization
    /// <summary>
    private static Dictionary<short, TerraCellsItemCategory> VanillaItemCategorizations = [];

    private static Dictionary<short, StorageItemSubcategorization> StorageSubcategorizations = [];

    public override void Load()
    {
        Dictionary<string, string> deserialized;

        deserialized = JsonSerializer.Deserialize<Dictionary<string, string>>(
            Mod.GetFileBytes("categorizations.json")
        );

        foreach (
            KeyValuePair<string, string> item in deserialized.Where(x =>
                !ItemIDNames.ContainsKey(x.Key)
            )
        )
        {
            deserialized.Remove(item.Key);
            Mod.Logger.Error(
                "Could not find the "
                    + item.Value
                    + " with the ID of "
                    + item.Key
                    + " among the vanilla ItemID's."
            );
        }

        VanillaItemCategorizations = deserialized
            .Select(x =>
                KeyValuePair.Create(ItemIDNames[x.Key], Categorization.FromString(x.Value))
            )
            .ToDictionary();

        StorageSubcategorizations = VanillaItemCategorizations
            .Where(x => x.Value == TerraCellsItemCategory.Storage)
            .Select(x =>
                KeyValuePair.Create(
                    x.Key,
                    Categorization.SubcategorizationFromString(deserialized[ItemNameIDs[x.Key]])
                )
            )
            .ToDictionary();

        On_Player.CanAcceptItemIntoInventory += FilterPickups;
        On_Player.GetItem_FillIntoOccupiedSlot += On_Player_GetItem_FillIntoOccupiedSlot;
    }

    private bool On_Player_GetItem_FillIntoOccupiedSlot(On_Player.orig_GetItem_FillIntoOccupiedSlot orig, Player self, int plr, Item newItem, GetItemSettings settings, Item returnItem, int i)
    {
        bool origResult = orig.Invoke(self, plr, newItem, settings, returnItem, i);
        if (i is not (>= WEAPON_SLOT_1 and <= POTION_SLOT) or (>= STORAGE_SLOT_1 and <= STORAGE_SLOT_4))
            return false;
        return origResult;
    }

    public override void PostUpdatePlayers()
    {
        if (DevConfig.Instance.EnableInventoryLock)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                foreach (Player player in Main.ActivePlayers)
                {
                    SortInventory(player);

                    if (player.selectedItem < INVENTORY_SLOT_COUNT)
                        continue;

                    if (player.selectedItem > INVENTORY_SLOT_COUNT + 2)
                        player.selectedItem = 0;
                    else
                        player.selectedItem = INVENTORY_SLOT_COUNT - 1;
                }
            }
            else
            {
                SortInventory(Main.LocalPlayer);

                if (Main.LocalPlayer.selectedItem < INVENTORY_SLOT_COUNT)
                    return;

                if (Main.LocalPlayer.selectedItem > INVENTORY_SLOT_COUNT + 2)
                    Main.LocalPlayer.selectedItem = 0;
                else
                    Main.LocalPlayer.selectedItem = INVENTORY_SLOT_COUNT - 1;
            }
        }
    }

    public void SortInventory(Terraria.Player player)
    {
        // var openSlots = 4;
        // for (int i = 10; i < 14; i++)
        // {
        //     if (!player.inventory[i].IsAir)
        //     {
        //         openSlots--;
        //     }
        // }
        for (int i = 5; i < 50; i++)
        {
            if (i >= 10 & i < 14)
            {
                continue;
            }
            if (
                DoesStorageItemGoIntoRegularInventory(
                    GetStorageItemSubcategorization(player.inventory[i])
                )
            )
            {
                for (int i2 = 10; i2 < 14; i2++)
                {
                    if (player.inventory[i2].IsAir)
                    {
                        Utils.Swap(ref player.inventory[i], ref player.inventory[i2]);
                        break;
                    }
                }
            }
            // if (openSlots == 0)
            // {
            //     player.DropItem(this, player.position, ref player.inventory[i]);
            //     continue;
            // }
            // switch (GetItemCategorization(player.inventory[i]))
            // {
            //     case TerraCellsItemCategory.Default:
            //         continue;
            //     case TerraCellsItemCategory.Storage:
            //         if (
            //             GetStorageItemSubcategorization(player.inventory[i])
            //             == StorageItemSubcategorization.Coin
            //         )
            //         {
            //             player.GetItem(0, player.inventory[i], GetItemSettings.PickupItemFromWorld);
            //             continue;
            //         }
            //         break;
            // }
        }

        foreach ((int, TerraCellsItemCategory) slotCategory in slotCategorizations)
        {
            Item item = player.inventory[slotCategory.Item1];
            if (slotCategory.Item2 == TerraCellsItemCategory.Storage && item.IsAir) { }
            else if (
                GetItemCategorization(item) != slotCategory.Item2
                && slotCategory.Item2 != TerraCellsItemCategory.Storage
            )
            {
                MoveItemToItsDedicatedCategory(player, item, slotCategory.Item1);
            }
        }
    }

    public bool MoveItemToItsDedicatedCategory(
        Terraria.Player player,
        Item item,
        int previousInventorySlot
    )
    {
        switch (GetItemCategorization(item))
        {
            case TerraCellsItemCategory.Default:
                for (int i = 14; i <= 49; i++)
                {
                    if (player.inventory[i].IsAir)
                    {
                        player.inventory[i] = item.Clone();
                        player.inventory[previousInventorySlot].TurnToAir();

                        return true;
                    }
                }
                player.DropItem(this, new Vector2(), ref item);
                return false;

            case TerraCellsItemCategory.Weapon:
                if (WeaponsSlotsFull(player, item))
                {
                    for (int i = 10; true; i++)
                    {
                        if (player.inventory[i].IsAir)
                        {
                            player.inventory[i] = item.Clone();
                            break;
                        }
                        if (i >= 13)
                        {
                            player.DropItem(this, new Vector2(), ref item);
                            return false;
                        }
                    }
                }
                else if (player.inventory[WEAPON_SLOT_1].IsAir)
                {
                    player.inventory[WEAPON_SLOT_1] = item.Clone();
                }
                else
                {
                    player.inventory[WEAPON_SLOT_2] = item.Clone();
                }
                player.inventory[previousInventorySlot].TurnToAir();
                return true;
            case TerraCellsItemCategory.Skill:
                if (SkillsSlotsFull(player, item))
                {
                    for (int i = 10; true; i++)
                    {
                        if (player.inventory[i].IsAir)
                        {
                            player.inventory[i] = item.Clone();
                            break;
                        }
                        if (i >= 13)
                        {
                            player.DropItem(this, new Vector2(), ref item);
                            return false;
                        }
                    }
                }
                else if (player.inventory[SKILL_SLOT_1].IsAir)
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
                if (PotionSlotFull(player, item))
                {
                    for (int i = 10; true; i++)
                    {
                        if (player.inventory[i].IsAir)
                        {
                            break;
                        }
                        if (i >= 13)
                        {
                            player.DropItem(this, new Vector2(), ref item);
                            return false;
                        }
                    }
                }
                player.inventory[POTION_SLOT] = item.Clone();
                player.inventory[previousInventorySlot].TurnToAir();
                return true;
            case TerraCellsItemCategory.Storage:
                // if (GetStorageItemSubcategorization(item) == StorageItemSubcategorization.Coin)
                // {
                //     return true;
                // }
                for (int i = 10; true; i++)
                {
                    if (player.inventory[i].IsAir)
                    {
                        player.inventory[i] = item.Clone();
                        return true;
                    }
                    if (i >= 13)
                    {
                        player.DropItem(this, new Vector2(), ref item);
                        return false;
                    }
                }
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
                player.DropItem(this, new Vector2(), ref item);
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
        Terraria.Player player,
        Item item
    )
    {
        bool origResult = orig.Invoke(player, item);
        if (!origResult)
            return origResult;
        if (!DevConfig.Instance.EnableInventoryLock)
        {
            return origResult;
        }

        return GetItemCategorization(item) switch
        {
            TerraCellsItemCategory.Default => true,
            TerraCellsItemCategory.Pickup => true,
            TerraCellsItemCategory.Weapon => !WeaponsSlotsFull(player, item) | !StorageSlotsFull(player, item),
            TerraCellsItemCategory.Skill => !SkillsSlotsFull(player, item) | !StorageSlotsFull(player, item),
            TerraCellsItemCategory.Potion => !PotionSlotFull(player, item) | !StorageSlotsFull(player, item),
            TerraCellsItemCategory.Storage => !StorageSlotsFull(player, item)
                | !DoesStorageItemGoIntoRegularInventory(GetStorageItemSubcategorization(item)),
            _ => throw new System.Exception(
                "Missing Item category check (I hate runtime exceptions but i cant think of a better solution atm)"
            ),
        };
    }

    public static bool DoesStorageItemGoIntoRegularInventory(
        StorageItemSubcategorization subcategorization
    )
    {
        return subcategorization switch
        {
            // StorageItemSubcategorization.Armor => true,
            // StorageItemSubcategorization.Accessory => true,
            StorageItemSubcategorization.Coin => false,
            _ => true,
        };
    }

    private static bool CanAcceptNewItem(Player player, int slot, Item newItem)
    {
        Item slotItem = player.inventory[slot];
        return CanAcceptNewItem(slotItem, newItem);
    }

    private static bool CanAcceptNewItem(Item slot, Item newItem)
    {
        if (slot.IsAir) return true;
        if (slot.type == newItem.type) return slot.stack < slot.maxStack;
        return false;
    }

    /// <summary>
    /// Checks the two inventory slots that are used for weapons, and returns true if both are occupied.
    /// </summary>
    public static bool WeaponsSlotsFull(Player player, Item pickup) =>
        !CanAcceptNewItem(player, WEAPON_SLOT_1, pickup) && !CanAcceptNewItem(player, WEAPON_SLOT_2, pickup);

    public static bool SkillsSlotsFull(Player player, Item pickup) =>
        !CanAcceptNewItem(player, SKILL_SLOT_1, pickup) && !CanAcceptNewItem(player, SKILL_SLOT_2, pickup);

    public static bool PotionSlotFull(Player player, Item pickup) =>
        !CanAcceptNewItem(player, POTION_SLOT, pickup);

    public static bool StorageSlotsFull(Player player, Item pickup) =>
        !CanAcceptNewItem(player, STORAGE_SLOT_1, pickup)
        && !CanAcceptNewItem(player, STORAGE_SLOT_2, pickup)
        && !CanAcceptNewItem(player, STORAGE_SLOT_3, pickup)
        && !CanAcceptNewItem(player, STORAGE_SLOT_4, pickup);

    public static bool AccessorySlotsFull(Player player, Item pickup) =>
        CanAcceptNewItem(player.armor[3], pickup) && CanAcceptNewItem(player.armor[4], pickup);

}
