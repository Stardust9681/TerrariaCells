namespace TerrariaCells.Content.UI;

public enum TerraCellsItemCategory
{
    /// <summary>
    /// This item works with the default properties
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
}
