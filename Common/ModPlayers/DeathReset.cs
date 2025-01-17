using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;

namespace TerrariaCells.Common.ModPlayers;

public class DeathReset : ModPlayer, IEntitySource
{
    public string Context => "TerrariaCells.Common.ModPlayers.DeathReset";

    public override void Kill(
        double damage,
        int hitDirection,
        bool pvp,
        PlayerDeathReason damageSource
    )
    {
        if (!DevConfig.Instance.DropItems)
        {
            return;
        }
        foreach ((int itemslot, TerraCellsItemCategory _) in InventoryManager.slotCategorizations)
        {
            Entity.DropItem(this, Entity.Center, ref Entity.inventory[itemslot]);
        }
        Entity.DropItem(this, Entity.Center, ref Entity.inventory[50]);
        Entity.DropItem(this, Entity.Center, ref Entity.inventory[51]);
        Entity.DropItem(this, Entity.Center, ref Entity.inventory[52]);
        Entity.DropItem(this, Entity.Center, ref Entity.inventory[53]);
        Entity.DropItem(this, Entity.Center, ref Entity.inventory[58]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[0]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[1]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[2]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[3]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[4]);
        Entity.DropItem(this, Entity.Center, ref Entity.armor[5]);
    }

    public override void OnRespawn()
    {
		ModContent.GetInstance<TeleportTracker>().Reset();
		ModContent.GetInstance<ClickedHeartsTracker>().Reset();
		ModContent.GetInstance<ChestLootSpawner>().Reset();
		NPCRoomSpawner.ResetSpawns();
		WorldPylonSystem.ResetPylons();
        foreach (Item item in Main.ActiveItems) {
            item.TurnToAir(true);
        }
    }
}
