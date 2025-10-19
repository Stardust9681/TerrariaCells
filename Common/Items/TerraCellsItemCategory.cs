/*
namespace TerrariaCells.Common.Items;

public enum TerraCellsItemCategory
{
    /// <summary>
    /// This item has a category
    /// </summary>
    Default = 0,

    /// <summary>
    /// This item can be stored in a weapon slot
    /// </summary>
    Weapon = 1,

    /// <summary>
    /// This item can be stored in a skill slot
    /// </summary>
    Skill = 2,

    /// <summary>
    /// This item can be stored in the potion slot
    /// </summary>
    Potion = 3,

    /// <summary>
    /// This item does not fit into other catagories and mostly stays in the storage area
    /// </summary>
    Storage = 4,

    /// <summary>
    /// This is treated as an item, but does not go into the inventory when picked up. Example: hearts, mana stars, nebula buffs
    /// 
    /// This is also what powerups are categorized as
    /// </summary>
    Pickup = 5,
}

public static class Categorization
{
    public static TerraCellsItemCategory FromString(string value)
    {
        return value.ToLower() switch
        {
            "default" => TerraCellsItemCategory.Default,
            "weapon" => TerraCellsItemCategory.Weapon,
            "skill" => TerraCellsItemCategory.Skill,
            "potion" => TerraCellsItemCategory.Potion,
            "storage" => TerraCellsItemCategory.Storage,
            "armor" => TerraCellsItemCategory.Storage,
            "accessory" => TerraCellsItemCategory.Storage,
            "coin" => TerraCellsItemCategory.Storage,
            "pickup" => TerraCellsItemCategory.Pickup,
            "powerup" => TerraCellsItemCategory.Pickup,
            _ => throw new System.NotImplementedException(value),
        };
    }

    public static StorageItemSubcategorization SubcategorizationFromString(string value)
    {
        return value.ToLower() switch
        {
            "accessory" => StorageItemSubcategorization.Accessory,
            "armor" => StorageItemSubcategorization.Armor,
            "coin" => StorageItemSubcategorization.Coin,
            _ => StorageItemSubcategorization.None,
        };

    }
}

public enum StorageItemSubcategorization
{
    None = 0,
    Armor = 1,
    Accessory = 2,

    /// <summary>
    /// This item should actually avoid storage altogether and go into its own slot
    /// </summary>
    Coin = 3,
}
*/