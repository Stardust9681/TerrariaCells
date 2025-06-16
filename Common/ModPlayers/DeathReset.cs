using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;
using Terraria.ModLoader.IO;
using static TerrariaCells.Common.Utilities.PlayerHelpers;
using TerrariaCells.Common.GlobalItems;

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
        ResetInventory(ResetInventoryContext.Death);

		//Reset mana
		Player.statMana = Player.statManaMax2;

        Player.respawnTimer = MaxRespawnTime;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            //Reset systems
            // ModContent.GetInstance<TeleportTracker>().Reset();
            ModContent.GetInstance<ClickedHeartsTracker>().Reset();
            ModContent.GetInstance<ChestLootSpawner>().Reset();
            WorldPylonSystem.ResetPylons();
            Player.GetModPlayer<RewardPlayer>().UpdateTracker(RewardPlayer.TrackerAction.Stop);
        }
	}

	public override void OnEnterWorld()
	{
        //If the last world the player was in is the world they've just entered
        //Don't do anything
        if (!Player.IsNewWorld())
        {
            return;
        }
        Player.GetModPlayer<RewardPlayer>().UpdateTracker_EnterNewWorld();
        ResetInventory(ResetInventoryContext.NewWorld);
    }

    public override void ModifyScreenPosition()
    {
        if (Player.DeadOrGhost)
        {
            int viewTarget = -1;
            for (int i = 0; i < Main.maxNetPlayers; i++)
            {
                Player test = Main.player[i];
                if (!test.active) continue;
                if (test.DeadOrGhost) continue;
                if (test.whoAmI == Main.myPlayer) continue;
                viewTarget = i;
                break;
            }
            if (viewTarget == -1)
                return;
            Player followPlayer = Main.player[viewTarget];
            Main.screenPosition = followPlayer.Center - (Main.ScreenSize.ToVector2() * 0.5f);
        }
    }

    public override void OnRespawn()
	{
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            WorldGen.SaveAndQuit();
        }
        return;

		foreach (NPC npc in Main.ActiveNPCs)
			if (!npc.friendly) npc.active = false; //Kill all NPCs so they aren't re-added to respawn buffer
		foreach (Item item in Main.ActiveItems)
			item.TurnToAir(true); //Turn all items to air, so player and NPC drops don't remain
		foreach (Projectile projectile in Main.ActiveProjectiles)
			projectile.active = false; //Disable any tombstones or what-have-you

		NPCRoomSpawner.ResetSpawns();
	}

    enum ResetInventoryContext : byte
    {
        NewWorld,
        Death,
    }
    private void ResetInventory(ResetInventoryContext context)
    {
        if (!DevConfig.Instance.DropItems)
            return;
        if (Player.whoAmI != Main.myPlayer)
            return;

        #region Drop Items
        ref Item[] inventory = ref Player.inventory;
        ref Item[] equips = ref Player.armor;
        Vector2 centre = Player.Center;
        IEntitySource playerDeath = Player.GetSource_Death();
        if (context == ResetInventoryContext.NewWorld)
        {
            foreach ((int itemslot, TerraCellsItemCategory _) in InventoryManager.slotCategorizations)
            {
                inventory[itemslot].TurnToAir();
            }

            inventory[50].TurnToAir();
            inventory[51].TurnToAir();
            inventory[52].TurnToAir();
            inventory[53].TurnToAir();
            inventory[58].TurnToAir();
            equips[0].TurnToAir();
            equips[1].TurnToAir();
            equips[2].TurnToAir();
            equips[3].TurnToAir();
            equips[4].TurnToAir();
            equips[5].TurnToAir();
        }
        else if (context == ResetInventoryContext.Death && Main.netMode != NetmodeID.Server)
        {
            foreach ((int itemslot, TerraCellsItemCategory _) in InventoryManager.slotCategorizations)
            {
                inventory[itemslot].shimmered = true;
                Player.DropItem(playerDeath, centre, ref inventory[itemslot]);
            }

            inventory[50].shimmered =   true; Player.DropItem(playerDeath, centre, ref inventory[50]);
            inventory[51].shimmered =   true; Player.DropItem(playerDeath, centre, ref inventory[51]);
            inventory[52].shimmered =   true; Player.DropItem(playerDeath, centre, ref inventory[52]);
            inventory[53].shimmered =   true; Player.DropItem(playerDeath, centre, ref inventory[53]);
            inventory[58].shimmered =   true; Player.DropItem(playerDeath, centre, ref inventory[58]);
            equips[0].shimmered =       true; Player.DropItem(playerDeath, centre, ref equips[0]);
            equips[1].shimmered =       true; Player.DropItem(playerDeath, centre, ref equips[1]);
            equips[2].shimmered =       true; Player.DropItem(playerDeath, centre, ref equips[2]);
            equips[3].shimmered =       true; Player.DropItem(playerDeath, centre, ref equips[3]);
            equips[4].shimmered =       true; Player.DropItem(playerDeath, centre, ref equips[4]);
            equips[5].shimmered =       true; Player.DropItem(playerDeath, centre, ref equips[5]);
        }
        #endregion

        #region Startup Inventory
        Dictionary<int, Item> inv = new Dictionary<int, Item>()
        {
            [0] = new Item(ItemID.CopperShortsword),
            [1] = new Item(ItemID.WoodenBow),
        };
        switch (context)
        {
            case ResetInventoryContext.NewWorld:
                inv[4] = new Item(ItemID.LesserHealingPotion, 2);
                break;
        }

        foreach (KeyValuePair<int, Item> pair in inv)
        {
            Player.inventory[pair.Key] = pair.Value;

            switch (context)
            {
                case ResetInventoryContext.Death:
                    if (Player.inventory[pair.Key].TryGetGlobalItem<TierSystemGlobalItem>(out var tierItem))
                    {
                        //Level + 1, because while they're.. decent, they're not AMAZING, and the weapons should be on-tier
                        tierItem.SetLevel(Player.inventory[pair.Key], ModContent.GetInstance<Systems.TeleportTracker>().level + 1);
                        FunkyModifierItemModifier.Reforge(Player.inventory[pair.Key], tierItem.itemLevel);
                    }
                    break;
            }
        }
        #endregion
    }

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
        byte[] result_orig = orig.Invoke(playerFile);
        playerFile.Player.RemoveSpawn();
        return result_orig;
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

        player.statMana = player.statManaMax;
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