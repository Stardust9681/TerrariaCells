using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common;

public class InventoryUiConfiguration : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    /// <summary>
    /// Effectively controls whether this mod affects the interface.
    /// </summary>
    [DefaultValue(true)]
    public bool EnableInventoryChanges;

    /// <summary>
    /// Since the default inventory is used and manipulated in this mod, you can disable that behaviour here if you wish.
    /// </summary>
    [DefaultValue(true)]
    public bool EnableInventoryLock;

    /// <summary>
    /// Disables the interfaces that show the inventory.
    ///
    /// Note that this disables the functionality of the visible inventory as well.
    /// </summary>
    [DefaultValue(true)]
    public bool HideVanillaInventory;
}
