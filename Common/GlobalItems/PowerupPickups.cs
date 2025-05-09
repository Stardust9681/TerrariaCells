
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems;

public class PowerupPickups : GlobalItem {
    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        switch (item.type) {
            case ItemID.CloudinaBottle:
                // item.shimmerWet = true;
                item.shimmered = true;
                // item.shimmerTime = 0;
                break;
        }
    }
}