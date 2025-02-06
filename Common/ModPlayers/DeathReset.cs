using System.Collections.Generic;
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
		foreach (NPC npc in Main.ActiveNPCs)
			npc.StrikeInstantKill(); //Kill all NPCs so they aren't re-added to respawn buffer
		foreach (Item item in Main.ActiveItems)
			item.TurnToAir(true); //Turn all items to air, so player and NPC drops don't remain
		foreach (Projectile projectile in Main.ActiveProjectiles)
			projectile.Kill(); //Disable any tombstones or what-have-you
		NPCRoomSpawner.ResetSpawns();
		WorldPylonSystem.ResetPylons();

		//Wipes the map data for the current session, but it doesn't like trying to load areas that you've explored this session, and the map will be restored next time you load the world
		//Main.clearMap = true;
    }



	public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
	{
		return new Item[]
			{
				new Item(Terraria.ID.ItemID.CopperShortsword), //Weapon Slot 1
				new Item(Terraria.ID.ItemID.WoodenBow), //Weapon Slot 2
				new Item(0, 0), //Skill Slot 1 (idk if this'll keep it open I hope it does tho)
				new Item(0, 0), //Skill Slot 2
				new Item(Terraria.ID.ItemID.LesserHealingPotion, 2), //Potion Slot
			};
	}
	public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
	{
		itemsByMod["Terraria"].Clear();
	}
}
