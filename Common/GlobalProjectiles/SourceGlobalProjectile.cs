using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalProjectiles
{
    public class SourceGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public Item itemSource = null;

        public override void SetDefaults(Projectile projectile)
        {
            // Allowing unlimited minions to be spawned, intended to allow the player to use multiple skill items that summon at once
            projectile.minionSlots = 0;
        }

        // Set the item source of a projectile when it spawns, to assist in tracking
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is IEntitySource_WithStatsFromItem entitySource)
            {
                itemSource = entitySource.Item;
            }
        }
    }
}
