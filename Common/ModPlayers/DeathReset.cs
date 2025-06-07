using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;
using Terraria.ModLoader.IO;

namespace TerrariaCells.Common.ModPlayers;

public class DeathReset : ModPlayer, IEntitySource
{
    public const int MaxRespawnTime = 300; //5 sec

	public string Context => "TerrariaCells.Common.ModPlayers.DeathReset";

    public override void Kill(
		double damage,
		int hitDirection,
		bool pvp,
		PlayerDeathReason damageSource
	)
	{
        Player.RemoveSpawn();
		if (DevConfig.Instance.DropItems)
		{
			// Player.DropItems(); // let me keep my vanity pleas ;-;
			foreach ((int itemslot, TerraCellsItemCategory _) in InventoryManager.slotCategorizations)
			{
				Entity.inventory[itemslot].shimmered = true;
				Entity.DropItem(this, Entity.Center, ref Entity.inventory[itemslot]);
			}
			
			Entity.inventory[50].shimmered = true; Entity.DropItem(this, Entity.Center, ref Entity.inventory[50]); 
			Entity.inventory[51].shimmered = true; Entity.DropItem(this, Entity.Center, ref Entity.inventory[51]); 
			Entity.inventory[52].shimmered = true; Entity.DropItem(this, Entity.Center, ref Entity.inventory[52]); 
			Entity.inventory[53].shimmered = true; Entity.DropItem(this, Entity.Center, ref Entity.inventory[53]); 
			Entity.inventory[58].shimmered = true; Entity.DropItem(this, Entity.Center, ref Entity.inventory[58]); 
			Entity.armor[0].shimmered =      true; Entity.DropItem(this, Entity.Center, ref Entity.armor[0]);      
			Entity.armor[1].shimmered =      true; Entity.DropItem(this, Entity.Center, ref Entity.armor[1]);      
			Entity.armor[2].shimmered =      true; Entity.DropItem(this, Entity.Center, ref Entity.armor[2]);      
			Entity.armor[3].shimmered =      true; Entity.DropItem(this, Entity.Center, ref Entity.armor[3]);      
			Entity.armor[4].shimmered =      true; Entity.DropItem(this, Entity.Center, ref Entity.armor[4]);      
			Entity.armor[5].shimmered =      true; Entity.DropItem(this, Entity.Center, ref Entity.armor[5]);      

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

        Player.respawnTimer = MaxRespawnTime;

		//Reset systems
        // ModContent.GetInstance<TeleportTracker>().Reset();
		ModContent.GetInstance<ClickedHeartsTracker>().Reset();
		ModContent.GetInstance<ChestLootSpawner>().Reset();
		WorldPylonSystem.ResetPylons();
        Player.GetModPlayer<TimerPlayer>().UpdateTimer(TimerPlayer.TimerAction.Stop);
	}

	public override void OnEnterWorld()
	{
        //If the last world the player was in is the world they've just entered
        //Don't do anything
        if(Player.spN[0] is not null
            && Main.worldName.Equals(Player.spN[0])
            && Main.worldID == Player.spI[0])
            return;
        //Don't replace inventories when this is disabled. Whoopsies
        if (!DevConfig.Instance.DropItems)
            return;
        //Set starting inventory
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
        WorldGen.SaveAndQuit();
        return;

		foreach (NPC npc in Main.ActiveNPCs)
			if (!npc.friendly) npc.active = false; //Kill all NPCs so they aren't re-added to respawn buffer
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
        On_Player.SavePlayerFile_Vanilla += On_Player_SavePlayerFile_Vanilla;
        On_Player.CheckSpawn += On_Player_CheckSpawn;
	}

    private bool On_Player_CheckSpawn(On_Player.orig_CheckSpawn orig, int x, int y)
    {
        return true;
    }

    //Changing this in PreSave() hook didn't work :/
    private byte[] On_Player_SavePlayerFile_Vanilla(On_Player.orig_SavePlayerFile_Vanilla orig, Terraria.IO.PlayerFileData playerFile)
    {
        playerFile.Player.ChangeSpawn((int)(playerFile.Player.position.X / 16), (int)(playerFile.Player.Bottom.Y / 16));
        return orig.Invoke(playerFile);
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

public class DeathBoot : ModSystem
{
    public override void Load()
    {
        On_Main.DrawStarsInBackground += On_Main_DrawStarsInBackground;
        On_Main.DrawPrettyStarSparkle += On_Main_DrawPrettyStarSparkle;
    }
    public override void Unload()
    {
        On_Main.DrawStarsInBackground -= On_Main_DrawStarsInBackground;
        On_Main.DrawPrettyStarSparkle -= On_Main_DrawPrettyStarSparkle;
    }

    private void On_Main_DrawPrettyStarSparkle(On_Main.orig_DrawPrettyStarSparkle orig, float opacity, Microsoft.Xna.Framework.Graphics.SpriteEffects dir, Vector2 drawpos, Color drawColor, Color shineColor, float flareCounter, float fadeInStart, float fadeInEnd, float fadeOutStart, float fadeOutEnd, float rotation, Vector2 scale, Vector2 fatness)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
            return;
        if (!Main.LocalPlayer.DeadOrGhost)
        {
            orig.Invoke(opacity, dir, drawpos, drawColor, shineColor, flareCounter, fadeInStart, fadeInEnd, fadeOutStart, fadeOutEnd, rotation, scale, fatness);
        }
    }

    private void On_Main_DrawStarsInBackground(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
            return;
        if (!Main.LocalPlayer.DeadOrGhost)
        {
            orig.Invoke(self, sceneArea, artificial);
        }
    }

    public override void ModifyLightingBrightness(ref float scale)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
            return;
        if (Main.LocalPlayer.DeadOrGhost)
        {
            scale = ((float)Main.LocalPlayer.respawnTimer / (float)DeathReset.MaxRespawnTime);
        }
    }
    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (Main.netMode != NetmodeID.SinglePlayer)
            return;
        if (Main.LocalPlayer.DeadOrGhost)
        {
            float scale = ((float)Main.LocalPlayer.respawnTimer / (float)DeathReset.MaxRespawnTime);
            (byte tileColourA, byte backgroundColourA) = (tileColor.A, backgroundColor.A);
            tileColor *= scale;
            backgroundColor *= scale;
            tileColor.A = tileColourA;
            backgroundColor.A = backgroundColourA;
        }
    }
}