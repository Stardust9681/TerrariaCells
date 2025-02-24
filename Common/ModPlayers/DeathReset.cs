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
		if (DevConfig.Instance.DropItems)
		{
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

			//Give default inventory on death
			Item[] startInv = GetStartingItems();
			for (int i = 0; i < startInv.Length; i++)
			{
				Player.inventory[i] = startInv[i].Clone();
				if (startInv[i].IsAir)
					Player.inventory[i].TurnToAir();
			}
		}

		//Reset mana
		Player.statMana = Player.statManaMax2;

		//Reset systems
		ModContent.GetInstance<TeleportTracker>().Reset();
		ModContent.GetInstance<ClickedHeartsTracker>().Reset();
		ModContent.GetInstance<ChestLootSpawner>().Reset();
		WorldPylonSystem.ResetPylons();
	}

    public override void OnEnterWorld()
    {
		foreach ((int itemslot, TerraCellsItemCategory _) in InventoryManager.slotCategorizations)
		{
			Entity.inventory[itemslot].TurnToAir();
		}
		Entity.inventory[50].TurnToAir();
		Entity.inventory[51].TurnToAir();
		Entity.inventory[52].TurnToAir();
		Entity.inventory[53].TurnToAir();
		Entity.inventory[58].TurnToAir();
		Entity.armor[0].TurnToAir();
		Entity.armor[1].TurnToAir();
		Entity.armor[2].TurnToAir();
		Entity.armor[3].TurnToAir();
		Entity.armor[4].TurnToAir();
		Entity.armor[5].TurnToAir();
		Item[] startInv = GetStartingItems();
		for (int i = 0; i < startInv.Length; i++)
		{
			Player.inventory[i] = startInv[i].Clone();
			if (startInv[i].IsAir)
				Player.inventory[i].TurnToAir();
		}
    }

    public override void OnRespawn()
	{
		foreach (NPC npc in Main.ActiveNPCs)
			if(!npc.friendly) npc.active = false; //Kill all NPCs so they aren't re-added to respawn buffer
		foreach (Item item in Main.ActiveItems)
			item.TurnToAir(true); //Turn all items to air, so player and NPC drops don't remain
		foreach (Projectile projectile in Main.ActiveProjectiles)
			projectile.active = false; //Disable any tombstones or what-have-you

		NPCRoomSpawner.ResetSpawns();
	}

	Item[] GetStartingItems() => new Item[]
		{
			new Item(Terraria.ID.ItemID.CopperShortsword), //Weapon Slot 1
			new Item(Terraria.ID.ItemID.WoodenBow), //Weapon Slot 2
			new Item(0, 0), //Skill Slot 1 (idk if this'll keep it open I hope it does tho)
			new Item(0, 0), //Skill Slot 2
			new Item(Terraria.ID.ItemID.LesserHealingPotion, 2), //Potion Slot
		};
	public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath) => GetStartingItems();
	public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
	{
		itemsByMod["Terraria"].Clear();
	}

	public override void Load()
	{
		Terraria.GameContent.UI.States.On_UICharacterCreation.SetupPlayerStatsAndInventoryBasedOnDifficulty += SetupPlayerInfo;
	}
	public override void Unload()
	{
		Terraria.GameContent.UI.States.On_UICharacterCreation.SetupPlayerStatsAndInventoryBasedOnDifficulty -= SetupPlayerInfo;
	}

	//Remove wings and finch staff buff
	private void SetupPlayerInfo(Terraria.GameContent.UI.States.On_UICharacterCreation.orig_SetupPlayerStatsAndInventoryBasedOnDifficulty orig, Terraria.GameContent.UI.States.UICharacterCreation self)
	{
		orig.Invoke(self);

		Player player = (Player)self.GetType().GetField("_player", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(self);
		player.armor[3].TurnToAir();
		player.ClearBuff(216); //Finch, which is for some reason applied at character creation
	}
}
