using System.Linq;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Common.ModPlayers;

public class DeathReset : ModPlayer, IEntitySource
{
    public string Context => "TerrariaCells.Common.ModPlayers.DeathReset";

    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        Mod.GetContent<TeleportTracker>().First().Reset();
        Mod.GetContent<ChestLootSpawner>().First().Reset();
        if (!DevConfig.Instance.DropItems)
        {
            return;
        }
        foreach ((int itemslot, TerraCellsItemCategory _) in InventoryManager.slotCategorizations)
        {
            Entity.DropItem(this, Entity.Center, ref Entity.inventory[itemslot]);
        }
        Entity.DropItem(this, Entity.Center, ref Entity.inventory[58]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[0]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[1]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[2]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[3]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[4]);
    }
}
