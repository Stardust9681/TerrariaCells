using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.RuntimeDetour;
using Stubble.Core.Contexts;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using TerrariaCells.Common;

namespace TerrariaCells.Content.UI;

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

    private static readonly (int, TerraCellsItemCategory)[] slotCategorizations =
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

    static InventoryUiConfiguration config;

    bool pickupLock = false;

    /// <summary>
    /// Gets the categorization for a given item.
    /// If the item implements ITerraCellsCategorization, it uses that.
    /// Otherwise, it checks the list of categorizations for vanilla items.
    /// If nothing is found, it returns TerraCellsItemCategory.Default
    /// <summary>
    public static TerraCellsItemCategory GetItemCategorization(Item item) =>
        item is ITerraCellsCategorization categorization
            ? categorization.Category
            : VanillaItemCategorizations.GetValueOrDefault(
                (short)item.netID,
                TerraCellsItemCategory.Default
            );

    /// <summary>
    /// A list of all of categorizations for vanilla items, including those that get reworked.
    /// 
    /// DOES NOT CONTAIN MODDED ITEMS. It only contains categorizations for items with a vanilla ItemID.
    /// Categorization for modded items are contained within their own classes, using ITerraCellsCategorization
    /// <summary>
    private static readonly Dictionary<short, TerraCellsItemCategory> VanillaItemCategorizations =
        new()
        {
            { ItemID.PhoenixBlaster, TerraCellsItemCategory.Weapon }, //219
            { ItemID.SniperRifle, TerraCellsItemCategory.Weapon }, //1254
            { ItemID.OnyxBlaster, TerraCellsItemCategory.Weapon }, //
            { ItemID.RocketLauncher, TerraCellsItemCategory.Weapon }, //759
            { ItemID.StarCannon, TerraCellsItemCategory.Weapon },
            { ItemID.PulseBow, TerraCellsItemCategory.Weapon },
            { ItemID.IceBow, TerraCellsItemCategory.Weapon },
            { ItemID.Toxikarp, TerraCellsItemCategory.Weapon },
            { ItemID.Minishark, TerraCellsItemCategory.Weapon },
            { ItemID.GrenadeLauncher, TerraCellsItemCategory.Weapon },
            { ItemID.VolcanoLarge, TerraCellsItemCategory.Weapon },
            { ItemID.Starfury, TerraCellsItemCategory.Weapon },
            { ItemID.Excalibur, TerraCellsItemCategory.Weapon },
            { ItemID.NightsEdge, TerraCellsItemCategory.Weapon },
            { ItemID.TerraBlade, TerraCellsItemCategory.Weapon },
            { ItemID.ThunderSpear, TerraCellsItemCategory.Weapon }, // storm spear
            { ItemID.FetidBaghnakhs, TerraCellsItemCategory.Weapon },
            { ItemID.GolemFist, TerraCellsItemCategory.Weapon },
            { ItemID.Sunfury, TerraCellsItemCategory.Weapon },
            { ItemID.DemonScythe, TerraCellsItemCategory.Weapon },
            { ItemID.Flamelash, TerraCellsItemCategory.Weapon },
            { ItemID.ShadowbeamStaff, TerraCellsItemCategory.Weapon },
            { ItemID.VenomStaff, TerraCellsItemCategory.Weapon },
            { ItemID.FlowerofFrost, TerraCellsItemCategory.Weapon },
            { ItemID.HeatRay, TerraCellsItemCategory.Weapon },
            { ItemID.WaterBolt, TerraCellsItemCategory.Weapon },
            { ItemID.EmeraldStaff, TerraCellsItemCategory.Weapon },
            { ItemID.LaserRifle, TerraCellsItemCategory.Weapon },
            { ItemID.SharpTears, TerraCellsItemCategory.Weapon }, // bloodthorn
            { ItemID.BubbleGun, TerraCellsItemCategory.Weapon },
            // Skills
            { ItemID.MolotovCocktail, TerraCellsItemCategory.Skill },
            { ItemID.StormTigerStaff, TerraCellsItemCategory.Skill },
            { ItemID.ClingerStaff, TerraCellsItemCategory.Skill },
            { ItemID.ToxicFlask, TerraCellsItemCategory.Skill },
            { ItemID.AleThrowingGlove, TerraCellsItemCategory.Skill },
            { ItemID.DD2ExplosiveTrapT1Popper, TerraCellsItemCategory.Skill }, //explosive cane rod
            { ItemID.BouncyDynamite, TerraCellsItemCategory.Skill },
            { ItemID.MedusaHead, TerraCellsItemCategory.Skill },
            // Potions
            { ItemID.HealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.LesserHealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.GreaterHealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.SuperHealingPotion, TerraCellsItemCategory.Potion },
            { ItemID.ManaPotion, TerraCellsItemCategory.Potion },
            { ItemID.LesserManaPotion, TerraCellsItemCategory.Potion },
            { ItemID.GreaterManaPotion, TerraCellsItemCategory.Potion },
            { ItemID.SuperManaPotion, TerraCellsItemCategory.Potion },
            { ItemID.IronskinPotion, TerraCellsItemCategory.Potion },
            { ItemID.SwiftnessPotion, TerraCellsItemCategory.Potion },
            { ItemID.RegenerationPotion, TerraCellsItemCategory.Potion },
            { ItemID.RedPotion, TerraCellsItemCategory.Potion },
            // Storage
            // Armor
            { ItemID.NinjaHood, TerraCellsItemCategory.Storage },
            { ItemID.NinjaShirt, TerraCellsItemCategory.Storage },
            { ItemID.NinjaPants, TerraCellsItemCategory.Storage },
            { ItemID.GoldHelmet, TerraCellsItemCategory.Storage },
            { ItemID.GoldChainmail, TerraCellsItemCategory.Storage },
            { ItemID.GoldGreaves, TerraCellsItemCategory.Storage },
            // Accessories
            { ItemID.CelestialMagnet, TerraCellsItemCategory.Storage },
            { ItemID.NaturesGift, TerraCellsItemCategory.Storage },
            { ItemID.ArcaneFlower, TerraCellsItemCategory.Storage },
            { ItemID.BandofStarpower, TerraCellsItemCategory.Storage },
            { ItemID.MagicCuffs, TerraCellsItemCategory.Storage },
            { ItemID.BerserkerGlove, TerraCellsItemCategory.Storage },
            { ItemID.SharkToothNecklace, TerraCellsItemCategory.Storage },
            { ItemID.Nazar, TerraCellsItemCategory.Storage },
            { ItemID.FeralClaws, TerraCellsItemCategory.Storage },
            { ItemID.ThePlan, TerraCellsItemCategory.Storage },
            { ItemID.ObsidianShield, TerraCellsItemCategory.Storage },
            { ItemID.FrozenTurtleShell, TerraCellsItemCategory.Storage },
            { ItemID.BandofRegeneration, TerraCellsItemCategory.Storage },
            { ItemID.FastClock, TerraCellsItemCategory.Storage },
            { ItemID.CelestialStone, TerraCellsItemCategory.Storage },
            //
            { ItemID.None, TerraCellsItemCategory.Default }

        };

    public override void Load()
    {
        config = (InventoryUiConfiguration)Mod.GetConfig("InventoryUiConfiguration");
        if (config == null)
        {
            Logging.PublicLogger.Error("Missing Inventory/UI Config! (This is a dev issue)");
            return;
        }

        if (!pickupLock & config.EnableInventoryLock)
        {
            pickupLock = true;
            On_Player.CanAcceptItemIntoInventory += new(FilterPickups);
        }
        else if (pickupLock & !config.EnableInventoryLock)
        {
            pickupLock = false;
            On_Player.CanAcceptItemIntoInventory -= new(FilterPickups);
        }
    }

    public override void PreUpdateItems()
    {
        if (config.EnableInventoryLock)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                if (player.selectedItem < INVENTORY_SLOT_COUNT)
                    return;

                if (player.selectedItem > INVENTORY_SLOT_COUNT - 1)
                    player.selectedItem = INVENTORY_SLOT_COUNT - 1;
                else
                    player.selectedItem = 0;
            }
        }
        if (config.EnableInventoryLock)
        {
            foreach (Player player in Main.ActivePlayers)
            {
                SortInventory(player);
            }
        }
    }

    public override void PostUpdateItems() { }

    public void SortInventory(Player player)
    {
        foreach ((int, TerraCellsItemCategory) slotCategory in slotCategorizations)
        {
            Item item = player.inventory[slotCategory.Item1];
            if (
                GetItemCategorization(item) != slotCategory.Item2
                && slotCategory.Item2 != TerraCellsItemCategory.Storage
            )
            {
                MoveItemToItsDedicatedCategory(player, item, slotCategory.Item1);
            }
        }
    }

    public bool MoveItemToItsDedicatedCategory(Player player, Item item, int previousInventorySlot)
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
                player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                return false;

            case TerraCellsItemCategory.Weapon:
                if (WeaponsSlotsFull(player))
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
                            player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
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
                if (SkillsSlotsFull(player))
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
                            player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
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
                if (PotionSlotFull(player))
                {
                    for (int i = 10; true; i++)
                    {
                        if (player.inventory[i].IsAir)
                        {
                            break;
                        }
                        if (i >= 13)
                        {
                            player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
                            return false;
                        }
                    }
                }
                player.inventory[POTION_SLOT] = item.Clone();
                player.inventory[previousInventorySlot].TurnToAir();
                return true;
            case TerraCellsItemCategory.Storage:
                for (int i = 10; true; i++)
                {
                    if (player.inventory[i].IsAir)
                    {
                        player.inventory[i] = item.Clone();
                        return true;
                    }
                    if (i >= 13)
                    {
                        player.DropItem(this, new Microsoft.Xna.Framework.Vector2(), ref item);
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
        return GetItemCategorization(item) switch
        {
            TerraCellsItemCategory.Default => false,
            TerraCellsItemCategory.Weapon => !WeaponsSlotsFull(player) | !StorageSlotsFull(player),
            TerraCellsItemCategory.Skill => !SkillsSlotsFull(player) | !StorageSlotsFull(player),
            TerraCellsItemCategory.Potion => !PotionSlotFull(player) | !StorageSlotsFull(player),
            TerraCellsItemCategory.Storage => !StorageSlotsFull(player),
            _ => !config.EnableInventoryLock,
        };
    }

    /// <summary>
    /// Checks the two inventory slots that are used for weapons, and returns true if both are occupied.
    /// </summary>
    public static bool WeaponsSlotsFull(Player player) =>
        !player.inventory[WEAPON_SLOT_1].IsAir && !player.inventory[WEAPON_SLOT_2].IsAir;

    public static bool SkillsSlotsFull(Player player) =>
        !player.inventory[SKILL_SLOT_1].IsAir && !player.inventory[SKILL_SLOT_2].IsAir;

    public static bool PotionSlotFull(Player player) => !player.inventory[POTION_SLOT].IsAir;

    public static bool StorageSlotsFull(Player player) =>
        !player.inventory[STORAGE_SLOT_1].IsAir
        && !player.inventory[STORAGE_SLOT_2].IsAir
        && !player.inventory[STORAGE_SLOT_3].IsAir
        && !player.inventory[STORAGE_SLOT_4].IsAir;

    public static bool AccessorySlotsFull(Player player) =>
        !player.armor[3].IsAir && !player.armor[4].IsAir;
}
