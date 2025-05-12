
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems;

public class PowerupPickups : GlobalItem {
    /// <summary>
    /// Position of the Brain of Cthulhu spawn point. Should be set when BoC spawns. 
    /// 
    /// Used for calculating how much velocity to give to the Cloud in a Bottle drop.
    /// </summary>
    public static Vector2? brainOfCthuluSpawnPoint;

    public override void SetStaticDefaults()
    {
        brainOfCthuluSpawnPoint = null; 
    }

    public override void Update(Item item, ref float gravity, ref float maxFallSpeed)
    {
        switch (item.type) {
            case ItemID.CloudinaBottle:
                item.shimmered = true;
                if (brainOfCthuluSpawnPoint.HasValue) {
                    item.velocity = (brainOfCthuluSpawnPoint.Value - item.Center) * 0.1f;
                }
                break;
        }
    }
}