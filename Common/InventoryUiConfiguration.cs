using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TerrariaCells.Common
{
    public class InventoryUiConfiguration : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue(true)]
        public bool EnableInventoryLock;
    }
}
