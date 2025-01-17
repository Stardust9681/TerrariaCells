using System;
using Terraria;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;

namespace TerrariaCells.Common.GlobalItems
{
    public class BlockOutOfInventoryItemUsage: GlobalItem
    {
        public override bool CanUseItem(Item item, Player player)
        {
            if (!player.inventory[58].IsAir && DevConfig.Instance.DisableUsingMouseItem ) {
                return false;
            }
            return base.CanUseItem(item, player);
        }
    }
}
