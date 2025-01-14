using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace TerrariaCells.Common.GlobalItems
{
    /// <summary>
    /// Data object for items that have been defined as skills
    /// Defines the stats and properties of each skill
    /// </summary>
    public class SkillItemData
    {
        // Skill properties
        /// <summary>
        /// Time for slot to go on cooldown when skill is used
        /// </summary>
        public float cooldownTime = 60;
        /// <summary>
        /// Duration before effects of skill are cancelled (ie. summons being unsummoned)
        /// </summary>
        public float skillDuration = -1;
        /// <summary>
        /// Replaces the item tooltip with whatever this string is set to
        /// </summary>
        public string tooltip;

        // Use restrictions
        /// <summary>
        /// Whether the skill can be used while the mouse is in a tile
        /// If false, the skill will fail to activate if the cursor is inside a solid tile when the skill is used
        /// </summary>
        public bool useInSolid = true;
        /// <summary>
        /// Whether the skill can be used while the mouse is not in direct line of sight of the player
        /// If true, the skill will fail to activate if there are any solid tiles intersecting the cast line between the player and cursor when the skill is used
        /// </summary>
        public bool lineOfSight = false;

        public SkillItemData() {}

        public SkillItemData(float cooldown)
        {
            cooldownTime = cooldown;
        }

        public SkillItemData(float cooldown, float duration)
        {
            cooldownTime = cooldown;
            skillDuration = duration;
        }

        public SkillItemData(float cooldown, float duration, string tooltip)
        {
            cooldownTime = cooldown;
            skillDuration = duration;
            this.tooltip = tooltip;
        }
    }

    /// <summary>
    /// Data object for inventory slots that have been defined as skills
    /// Defines per-slot variables for tracking individual cooldowns and other relevant details, such as keybinds
    /// </summary>
    public class SkillSlotData
    {
        public float cooldownTimer = 0; 
        public float cooldownTotal = 0; 

        public ModKeybind keybind = null;

        public SkillSlotData() { }

        public SkillSlotData(ModKeybind keybind)
        {
            this.keybind = keybind;
        }
    }

    /// <summary>
    /// ModPLayer for handling the cooldown logic for each skill slot, as well as non item-specific features
    /// </summary>
    public class SkillModPlayer : ModPlayer
    {
        // Slot index and slot data
        public static Dictionary<int, SkillSlotData> SkillSlots = [];

        // ItemID and skill data
        public static Dictionary<int, SkillItemData> SkillItems = new Dictionary<int, SkillItemData>();

        public override void SetStaticDefaults()
        {

            // DEFINE WHICH SLOTS ARE FOR SKILLS HERE
            ModKeybind skillKeybind1 = KeybindLoader.RegisterKeybind(Mod, "First Skill", "Q");
            AddSkillSlotWithKeybind(2, skillKeybind1);
            ModKeybind skillKeybind2 = KeybindLoader.RegisterKeybind(Mod, "Second Skill", "E");
            AddSkillSlotWithKeybind(3, skillKeybind2);


            // DEFINE WHICH ITEMS ARE SKILLS HERE
            SkillItems.Add(ItemID.StormTigerStaff, new SkillItemData(5400, 1800));
            SkillItems.Add(ItemID.StardustDragonStaff, new SkillItemData(1800, 600));

            SkillItems.Add(ItemID.ClingerStaff, new SkillItemData(2700, 600));
            SkillItems[ItemID.ClingerStaff].useInSolid = false;
            SkillItems[ItemID.ClingerStaff].lineOfSight = true;

            SkillItems.Add(ItemID.ToxicFlask, new SkillItemData(1800));

            SkillItems.Add(ItemID.StaffoftheFrostHydra, new SkillItemData(1800, 600));
            SkillItems[ItemID.StaffoftheFrostHydra].useInSolid = false;

            SkillItems.Add(ItemID.DD2ExplosiveTrapT1Popper, new SkillItemData(1200, 1200));
            SkillItems[ItemID.DD2ExplosiveTrapT1Popper].useInSolid = false;
            SkillItems[ItemID.DD2ExplosiveTrapT1Popper].lineOfSight = true;

            SkillItems.Add(ItemID.MolotovCocktail, new SkillItemData(900));
            SkillItems.Add(ItemID.BouncyDynamite, new SkillItemData(3600));
            SkillItems.Add(ItemID.BouncingShield, new SkillItemData(360));
            SkillItems.Add(ItemID.MedusaHead, new SkillItemData(1800));
            SkillItems.Add(ItemID.WrathPotion, new SkillItemData(4800, 1200));
            SkillItems.Add(ItemID.MagicPowerPotion, new SkillItemData(4800, 1200));
            SkillItems.Add(ItemID.SnowballLauncher, new SkillItemData(1200, 480));

        }

        public override void Load()
        {
            // IL edit for drawing skill slots in green
            IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;
        }

        private void IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(ILContext il)
        {
            try
            {
                // Initialize cursor
                ILCursor c = new ILCursor(il);

                // Find where the entry point of this code will be. This is where flag2 is loaded as a local.
                c.GotoNext(i => i.MatchLdloc(9));
                c.Index++;

                // Emit all required values to stack
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_1); // Inventory array
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_3); // Slot number
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc_S, (byte)7); // Texture value
                c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_2); // Context

                // Emit the delegate (the code)
                c.EmitDelegate<Func<Item[], int, Texture2D, int, Texture2D>>((inv, slot, originalTexture, context) =>
                {

                    if (SkillSlots.ContainsKey(slot))
                    {

                        if (context == 0 || context == 13 && slot != Main.LocalPlayer.selectedItem)
                        {
                            if (inv[slot].favorited)
                            {
                                return (Texture2D)TextureAssets.InventoryBack19;
                            }

                            return (Texture2D)TextureAssets.InventoryBack2;
                        }

                    }

                    return originalTexture;
                });

                // Emit return value
                c.Emit(Mono.Cecil.Cil.OpCodes.Stloc_S, (byte)7);
            }
            catch (Exception e)
            {
                MonoModHooks.DumpIL(Mod, il);
            }
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillSlots)
            {
                // Exit early, if the slot doesn't have a keybind
                if (slotInfo.Value.keybind == null)
                {
                    return;
                }

                // Perform quick use on the item slot, if the keybind has just been pressed
                if (slotInfo.Value.keybind.JustPressed)
                {
                    QuickUseItemAt(slotInfo.Key);
                }
            }
        }

        // Variables for handling quick using items
        internal int originalSelectedItem;
        internal bool autoRevertSelectedItem = false;
        internal bool pendingQuickUse = false;

        public void QuickUseItemAt(int index, bool use = true)
        {

            if (Player.selectedItem == index)
            {
                Player.controlUseItem = true;
                return;
            }

            if (!autoRevertSelectedItem && Player.selectedItem != index && Player.inventory[index].type != ItemID.None)
            {
                originalSelectedItem = Player.selectedItem;
                autoRevertSelectedItem = true;
                Player.selectedItem = index;
                Player.controlUseItem = true;
                if (use && CombinedHooks.CanUseItem(Player, Player.inventory[Player.selectedItem]))
                {
                    if (Player.whoAmI == Main.myPlayer)
                        Player.ItemCheck();
                }
            }
        }

        public override void PostUpdate()
        {
            // Swap back to previously selected item, if an item quick use was just attempted
            if (autoRevertSelectedItem)
            {
                if (Player.itemTime == 0 && Player.itemAnimation == 0)
                {
                    Player.selectedItem = originalSelectedItem;
                    autoRevertSelectedItem = false;
                }
            }
        }

        public override void PreUpdate()
        {
            // Perform update for each skill slot
            foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillSlots)
            {
                Item item = Main.LocalPlayer.inventory[slotInfo.Key];

                // Reduce cooldown timer for skill slot, if above 0
                if (slotInfo.Value.cooldownTimer > 0)
                {
                    slotInfo.Value.cooldownTimer -= 1;
                }

                // If the skill slot has an item in it-
                if (!item.IsAir)
                {

                    // if there is a tile created by this item, attempt to activate it (for turrets and such)
                    if (item.createTile != -1)
                    {
                        Vector2 position = Main.LocalPlayer.position / 16f;

                        // Search a 120x80 tile area around the player's position
                        for (int i = (int)position.X - 60; i < (int)position.X + 60; i++)
                        {
                            for (int j = (int)position.Y - 40; j < (int)position.Y + 40; j++)
                            {
                                Point point = new Point(i, j);

                                // Exit loop if the tile is outside the expected world bounds
                                if (point.X < 1 || point.X > Main.tile.Width || point.Y < 1 || point.Y > Main.tile.Height)
                                {
                                    break;
                                }

                                Tile tile = Main.tile[point];

                                // If the tile matches, attempt to interact with it
                                if (tile.TileType == item.createTile)
                                {
                                    Main.LocalPlayer.tileInteractAttempted = true;
                                    Main.LocalPlayer.TileInteractionsCheck(point.X, point.Y);
                                }
                            }
                        }
                    }

                    // Control cooldown variable for each skill
                    if (item.TryGetGlobalItem<SkillSystemGlobalItem>(out SkillSystemGlobalItem skillItem))
                    {

                        slotInfo.Value.cooldownTotal = SkillItems[item.type].cooldownTime;

                        if (slotInfo.Value.cooldownTimer > 0)
                        {
                            skillItem.onCooldown = true;

                            // If the skill duration is reached, cancel the skill
                            if (slotInfo.Value.cooldownTimer == slotInfo.Value.cooldownTotal - SkillItems[item.type].skillDuration)
                            {
                                StopSkill(item);
                            }

                        }
                        else
                        {
                            skillItem.onCooldown = false;
                        }
                    }


                }
            }
        }

        /// <summary>
        /// Immediately stops the ongoing effects of the skill of any given item, deleting the associated projectile/summons and instantiating a death effect for it
        /// Handles buffs, tiles, and all projectiles
        /// </summary>
        /// <param name="item"></param>
        public static void StopSkill(Item item)
        {

            // If the item applies a buff and the player currently has that buff
            if (item.buffType != 0 && Main.LocalPlayer.HasBuff(item.buffType))
            {

                // Clear the buff, to ensure it is removed regardless of the status of the following projectile checks - those are mostly for visually disposing of the summoned "minions"
                Main.LocalPlayer.ClearBuff(item.buffType);

                // Search for the projectile associated with the item, if there is one
                if (item.shoot != ProjectileID.None)
                {
                    // Track whether the initial projectile fired was invisible
                    bool hidden = false;

                    // Track the current projectile index to explode on
                    int projectileTargetIndex = 0;

                    // Loop over all projectiles in-game
                    for (int i = 0; i < Main.projectile.Length; i++)
                    {
                        // Projectile ref
                        Projectile projectile = Main.projectile[i];

                        // Skip inactive projectiles
                        if (!projectile.active)
                        {
                            continue;
                        }

                        // Projectile match found or searching after initial hidden projectile
                        if (projectile.type == item.shoot && projectile.owner == Main.myPlayer || hidden)
                        {

                            // Exit loop, if the projectile was hidden and there are no more sub-minions to search
                            if (hidden && !projectile.minion)
                            {
                                break;
                            }

                            // Continue searching for the secondary minions spawned, if the initial projectile spawned was hidden, skip if already triggered
                            if (!hidden && projectile.hide == true)
                            {
                                projectileTargetIndex = i;
                                hidden = true;
                                continue;
                            }

                            // Set projectile to explode on and exit loop
                            projectileTargetIndex = i;
                            break;
                        }

                    }

                    // Ensure that a valid projectile has been found to explode on
                    if (projectileTargetIndex != 0)
                    {
                        // Create an effect at the position of the found "projectile" (probably a summon)
                        Projectile projectile = Main.projectile[projectileTargetIndex];

                        // Projectile destruction effect
                        Explosion(new Vector2(projectile.position.X + (projectile.Size.X / 2), projectile.position.Y + (projectile.Size.Y / 2)), 24);
                    }
                }

            }
            else if (item.createTile != -1) // Destroy any tiles created by the skill item, typically sentry turrets
            {
                // Get player position, in tiles(16x16)
                Vector2 playerPos = Main.LocalPlayer.position / 16f;

                // Search tiles in an area around the player
                for (int i = (int)playerPos.X - 90; i < (int)playerPos.X + 90; i++)
                {
                    for (int j = (int)playerPos.Y - 90; j < (int)playerPos.Y + 90; j++)
                    {
                        Point point = new Point(i, j);

                        Tile tile = Main.tile[point];

                        // Tile match found!
                        if (tile.TileType == item.createTile)
                        {
                            // Destruction effect
                            Explosion(new Vector2(point.X * 16, point.Y * 16), 48);

                            //tile.ClearTile(); // clears individual tiles

                            // Destroy tile found
                            WorldGen.KillTile(point.X, point.Y, false, false, true); // drops an item, prevented with a GlobalTile.CanDrop() override
                            return;
                        }
                    }
                }
            }
            else // checking for sentry == true gets things like the frosty hydra but not the clinger staff, which seems to have no differentiating factors from the toxic flask (and other projectile items)
            {

                // Clean up any remaining projectiles
                foreach (Projectile projectile in Main.projectile)
                {
                    // Skip inactive projectiles
                    if (!projectile.active)
                    {
                        continue;
                    }

                    // Projectile match found!
                    if (projectile.type == item.shoot && projectile.owner == Main.myPlayer)
                    {
                        if (item.sentry == true) // Only use an effect on death if it is a sentry, others (ie. clinger staff) look a bit strange because the projectile position isn't representative of the spells AOE
                        {
                            Explosion(new Vector2(projectile.position.X + (projectile.Size.X / 2), projectile.position.Y + (projectile.Size.Y / 2)), 24);
                        }

                        // Projectile is kill
                        projectile.Kill();
                    }
                }
            }

        }

        /// <summary>
        /// Placeholder effect function that generates a simple explosion particle effect
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public static void Explosion(Vector2 position, int size)
        {

            for (int i = 0; i < 8; i++)
            {
                int dust = Dust.NewDust(position, size, size, DustID.Smoke, 0f, 0f, 100, default, 1.7f);
                Main.dust[dust].velocity *= 1.4f;
            }

            for (int j = 0; j < 12; j++)
            {
                int dust = Dust.NewDust(position, size, size, DustID.Torch, 0f, 0f, 100, default, 2.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 5f;
                dust = Dust.NewDust(position, size, size, DustID.Torch, 0f, 0f, 100, default, 1.6f);
                Main.dust[dust].velocity *= 3f;
            }
        }

        /// <summary>
        /// Returns the data object for a skill slot, given the inventory slot id
        /// Will return null- if the given slot if isn't a skill slot
        /// </summary>
        /// <param name="inventorySlot"></param>
        /// <returns></returns>
        public static SkillSlotData GetSkillSlotData(int inventorySlot)
        {
            // if (SkillSlots.ContainsKey(inventorySlot))
            // {

            if (SkillSlots.TryGetValue(inventorySlot, out SkillSlotData slotData))
            {
                return slotData;
            }

            // }

            return null;

        }

        /// <summary>
        /// Returns the data object for a skill slot, given the item you are searching for
        /// Will return null- if the given item is not in a skill slot
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static SkillSlotData GetSkillSlotData(Item item)
        {
            foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillSlots)
            {
                if (Main.LocalPlayer.inventory[slotInfo.Key] == item)
                {
                    return slotInfo.Value;
                }
            }

            return null;
        }

        public static void RemoveSkillSlot(int inventorySlot)
        {
            if (SkillSlots.ContainsKey(inventorySlot))
            {
                SkillSlots.Remove(inventorySlot);
            }
        }

        /// <summary>
        /// Adds the given inventory slot id to the dictonary of skill slots, giving it skill slot functionality
        /// </summary>
        /// <param name="inventorySlot"></param>
        public static void AddSkillSlot(int inventorySlot)
        {
            SkillSlotData data = GetSkillSlotData(inventorySlot);

            if (data == null)
            {
                SkillSlotData newData = new SkillSlotData();

                SkillSlots.Add(inventorySlot, newData);
            }
        }

        /// <summary>
        /// Adds the given inventory slot id to the dictonary of skill slots, giving it skill slot functionality
        /// An additional argument can be used to set the keybind for quick use of the skill slot
        /// </summary>
        /// <param name="inventorySlot"></param>
        /// <param name="keybind"></param>
        public static void AddSkillSlotWithKeybind(int inventorySlot, ModKeybind keybind)
        {
            SkillSlotData data = GetSkillSlotData(inventorySlot);

            if (data == null)
            {
                SkillSlotData newData = new SkillSlotData(keybind);

                SkillSlots.Add(inventorySlot, newData);
            }
        }

        /// <summary>
        /// Return true if the given item is in an inventory slot which has been designated as a skill slot (Must be the same instance, clones will always return false)
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsInSlot(Item item)
        {

            foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillSlots)
            {
                if (Main.LocalPlayer.inventory[slotInfo.Key] == item)
                {
                    return true;
                }
            }

            return false;
        }

        public static int? GetSkillSlotIndex(Item item)
        {
            foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillSlots)
            {
                if (Main.LocalPlayer.inventory[slotInfo.Key] == item)
                {
                    return slotInfo.Key;
                }
            }

            return null;
        }

    }

    /// <summary>
    /// GlobalItem applying skill functionality to all items defined as skills (in the SkillModPlayer SkillItems dictionary)
    /// Includes defaults for all skill items, skill item hooks (ie. prevent skill items from being dropped), and general skill functionality
    /// </summary>
    public class SkillSystemGlobalItem : GlobalItem
    {

        public Item instance;
        public bool onCooldown = false; //// is equivalent to (SkillModPlayer.GetSkillSlotData(item).cooldownTimer > 0)

        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            return lateInstantiation && SkillModPlayer.SkillItems.ContainsKey(item.type);
        }

        public override bool InstancePerEntity => true;

        public override void SetDefaults(Item item)
        {
            instance = item;

            // MAKE CHANGES TO SKILL ITEM PROPERTIES HERE
            switch (item.type)
            {
                case ItemID.StormTigerStaff:
                    item.damage = 20;
                    break;
                case ItemID.StardustDragonStaff:
                    break;
                case ItemID.ClingerStaff:
                    break;
                case ItemID.ToxicFlask:
                    break;
                case ItemID.StaffoftheFrostHydra:
                    item.knockBack = 0;
                    break;
                case ItemID.DD2ExplosiveTrapT1Popper:
                    break;
                case ItemID.MolotovCocktail:
                    item.consumable = false;
                    item.maxStack = 1;
                    break;
                case ItemID.BouncyDynamite:
                    item.consumable = false;
                    item.maxStack = 1;
                    break;
                case ItemID.BouncingShield:

                    break;
                case ItemID.SnowballLauncher:
                    item.consumable = false;
                    item.maxStack = 1;

                    break;
            }
        }

        public override void Load()
        {

            // Hook to prevent items from being picked up while the skill slot is on cooldown
            On_ItemSlot.PickItemMovementAction += ItemSlot_PickItemMovementAction;

            // Hook to check for tile placement, which ensures skill items that place tiles (ie. turrets) do not get put on cooldown until the tile is succesfully placed
            On_Player.PlaceThing_Tiles_PlaceIt += On_Player_PlaceThing_Tiles_PlaceIt;

            // Hooks to allow DD2 turrets at all times
            On_Projectile.TurretShouldPersist += On_Projectile_TurretShouldPersist;
            On_Player.ItemCheck_CheckCanUse += On_Player_ItemCheck_CheckCanUse;

            // Hooks to prevent skill items from being dropped while on cooldown
            On_Player.DropSelectedItem += On_Player_DropSelectedItem;

            // Hooks to prevent non-skill items from being picked up into a skill slot
            On_Player.GetItem_FillEmptyInventorySlot += On_Player_GetItem_FillEmptyInventorySlot;


        }

        private bool On_Player_GetItem_FillEmptyInventorySlot(On_Player.orig_GetItem_FillEmptyInventorySlot orig, Player self, int plr, Item newItem, GetItemSettings settings, Item returnItem, int i)
        {

            if (SkillModPlayer.SkillSlots.ContainsKey(i) && !SkillModPlayer.SkillItems.ContainsKey(newItem.type))
            {
                return false;
            }
            else
            {
                return orig(self, plr, newItem, settings, returnItem, i);
            }
        }

        private void On_Player_DropSelectedItem(On_Player.orig_DropSelectedItem orig, Player self)
        {

            if (SkillModPlayer.SkillSlots.TryGetValue(Main.LocalPlayer.selectedItem, out SkillSlotData slotData))
            {
                if (slotData.cooldownTimer > 0)
                {
                    return;
                }

            }

            orig(self);
        }

        private bool On_Player_ItemCheck_CheckCanUse(On_Player.orig_ItemCheck_CheckCanUse orig, Player self, Item sItem)
        {
            if (ProjectileID.Sets.IsADD2Turret[sItem.shoot] && CanUseItem(sItem, Main.LocalPlayer))
            {
                return true;
            }

            return orig(self, sItem);

        }

        private bool On_Projectile_TurretShouldPersist(On_Projectile.orig_TurretShouldPersist orig, Projectile self)
        {
            return true;
        }

        private TileObject On_Player_PlaceThing_Tiles_PlaceIt(On_Player.orig_PlaceThing_Tiles_PlaceIt orig, Player self, bool newObjectType, TileObject data, int tileToCreate)
        {

            if (SkillModPlayer.SkillItems.ContainsKey(Main.LocalPlayer.HeldItem.type) && Main.LocalPlayer.HeldItem.createTile == tileToCreate)
            {

                SkillSlotData skillData = SkillModPlayer.GetSkillSlotData(Main.LocalPlayer.HeldItem);
                skillData.cooldownTimer = skillData.cooldownTotal;
            }

            return orig(self, newObjectType, data, tileToCreate);
        }

        private int ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item item)
        {

            if (context == 0)
            {
                if (SkillModPlayer.SkillSlots.TryGetValue(slot, out SkillSlotData slotData))
                {
                    if (slotData.cooldownTimer > 0)
                    {
                        return -1;
                    }

                }

                if (item.type != ItemID.None)
                {
                    if (SkillModPlayer.SkillSlots.ContainsKey(slot) && !SkillModPlayer.SkillItems.ContainsKey(item.type))
                    {
                        return -1;
                    }

                    //  Prevent two of the same item from being placed in skill slots at the same time
                    foreach (int invSlot in SkillModPlayer.SkillSlots.Keys)
                    {
                        if (!SkillModPlayer.SkillSlots.ContainsKey(slot))
                        {
                            continue;
                        }

                        if (inv[invSlot].type == item.type)
                        {
                            return -1;
                        }
                    }
                }
            }


            return orig(inv, context, slot, item);
        }

        // Prevent skills from reducing mana when they are used
        public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
        {
            reduce -= item.mana;
        }

        public override bool CanUseItem(Item item, Player player)
        {
            if (SkillModPlayer.IsInSlot(item))
            {
                if (onCooldown)
                {
                    return false;
                }

                Point mouseTile = Main.MouseWorld.ToTileCoordinates();

                /* Prevents the user from attempting to place blocks unless able to- only controls animation playing, blocks will not be placed if usually unable
                if (item.createTile != -1)
                {
                    
                    if (!(Main.LocalPlayer.position.X / 16f - (float)Player.tileRangeX - (float)item.tileBoost - (float)Main.LocalPlayer.blockRange <= (float)mouseTile.X) || !((Main.LocalPlayer.position.X + (float)item.width) / 16f + (float)Player.tileRangeX + (float)item.tileBoost - 1f + (float)Main.LocalPlayer.blockRange >= (float)mouseTile.X) || !(Main.LocalPlayer.position.Y / 16f - (float)Player.tileRangeY - (float)item.tileBoost - (float)Main.LocalPlayer.blockRange <= (float)mouseTile.Y) || !((Main.LocalPlayer.position.Y + (float)item.height) / 16f + (float)mouseTile.Y + (float)item.tileBoost - 2f + (float)Main.LocalPlayer.blockRange >= (float)mouseTile.Y))
                    {
                        return false;

                    }
                    
                    if (!TileLoader.CanPlace(mouseTile.X, mouseTile.Y, item.createTile))
                    {
                        return false;
                    }
                }
                */


                if (!SkillModPlayer.SkillItems[item.type].useInSolid && WorldGen.SolidTile(Main.tile[mouseTile]))
                {
                    return false;
                }

                if (SkillModPlayer.SkillItems[item.type].lineOfSight && !Collision.CanHitLine(Main.LocalPlayer.position, Main.LocalPlayer.width, Main.LocalPlayer.height, new Vector2(Main.MouseWorld.X, Main.MouseWorld.Y), 1, 1))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        public override bool? UseItem(Item item, Player player)
        {
            if (SkillModPlayer.IsInSlot(item))
            {
                if (onCooldown)
                {
                    return false;
                }

                if (!Main.mouseRight && item.createTile == -1) // PSSST - if anyone has a better way to activate the cooldown, that won't trigger when staffs right click and play the swing anim without actually doing their spell... lemme know, so far I have a custom implementation that hooks the tile placement function to ensure that tiles don't start the cooldown until actually placed either. Terraria doesn't actually check succesful placement almost.
                {
                    SkillSlotData data = SkillModPlayer.GetSkillSlotData(item);
                    data.cooldownTimer = data.cooldownTotal;
                }


                return null;
            }

            return false;
        }

        // why is there ui code buried so deep in a GlobalItem file
        // syviery, love what you did for this mod, but whyyyyyyyy
        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            //// IsInSlot is unnecessary here, but does provide extra protection for not showing the cooldown when not equipped

            // if (!SkillModPlayer.IsInSlot(item))
            // {
            //     return;
            // }


            // SkillSlotData data = SkillModPlayer.GetSkillSlotData(item);

            // Show cooldown ui on hotbar
            // if (onCooldown)
            // {
                // Cooldown item slot indicator
                // spriteBatch.Draw(TextureAssets.InventoryBack.Value,
                //     position: new Vector2(position.X, position.Y),
                //     sourceRectangle: new Rectangle(0, 0, 52, (int)(52 * ((float)data.cooldownTimer / data.cooldownTotal))),
                //     color: new Color(15, 15, 15, 128),
                //     rotation: 3.14159f,
                //     origin: new Vector2(26, 26),
                //     scale: new Vector2(Main.inventoryScale, Main.inventoryScale),
                //     SpriteEffects.None,
                //     layerDepth: 0f);

                // Cooldown countdown text display
                // string currentCooldown = MathF.Ceiling(data.cooldownTimer / 60).ToString();

                // float width = FontAssets.DeathText.Value.MeasureString(currentCooldown).X;
                // float textScale = Main.inventoryScale * 0.50f;

                // if (TerrariaCellsConfig.Instance.ShowCooldown)
                // {
                //     ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.DeathText.Value, currentCooldown, position + new Vector2(0f - width / 2f, 0f) * textScale, Color.White, 0, Vector2.Zero, new Vector2(textScale, textScale));
                // }
            // }

            // Show slot keybind, if toggled in settings
            // if (data.keybind != null && TerrariaCellsConfig.Instance.ShowKeybind)
            // {
            //     string text = data.keybind.GetAssignedKeys()[0];
            //     float width = FontAssets.ItemStack.Value.MeasureString(text).X;
            //     float textScale = Main.inventoryScale * 0.75f;
            //     Vector2 textPosition = position + new Vector2(20f - width / 2f, -27f) * textScale;

            //     Color color = Main.inventoryBack;


            //     // Change color when inventory is open or when selected, to match hotbar numbers
            //     if (!Main.playerInventory)
            //     {
            //         color = Color.White;
            //     }
            //     else
            //     {
            //         if (Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem] == instance)
            //         {
            //             color = Color.White;
            //             color.A = 200;
            //             textPosition.Y -= 2;
            //         }
            //     }

            //     ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, text, textPosition, color, new Color(68, 68, 45), 0, Vector2.Zero, new Vector2(textScale, textScale), -1, 2);
            // }

        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            SkillItemData skillData = SkillModPlayer.SkillItems[item.type];

            if (skillData == null)
            {
                return;
            }

            TooltipLine skillTooltip = new TooltipLine(Mod, "SkillTooltip", skillData.tooltip);

            // Iterate through the list of tooltips so we can change vanilla tooltips
            foreach (TooltipLine tooltip in tooltips)
            {
                // Alter vanilla tooltips here
                switch (tooltip.Name)
                {
                    case "ItemName": // Change all skills to have a different name color
                        tooltip.Text = "[Skill] " + tooltip.Text;

                        Color skillColor = new Color(48, 184, 116);
                        tooltip.OverrideColor = skillColor;
                        break;
                    case "Tooltip0": // If the skill has a custom tooltip description, replace the vanilla tooltip with it
                        if (!string.IsNullOrEmpty(skillData.tooltip))
                        {
                            tooltip.Hide();
                            tooltips.Add(skillTooltip);
                        }
                        break;
                    case "Tooltip1": // Remove additional tooltips, if a custom one is available
                        if (!string.IsNullOrEmpty(skillData.tooltip))
                        {
                            tooltip.Hide();
                        }
                        break;
                    case "Tooltip2": // Remove additional tooltips, if a custom one is available
                        if (!string.IsNullOrEmpty(skillData.tooltip))
                        {
                            tooltip.Hide();
                        }
                        break;
                    case "Speed":
                        tooltip.Hide();
                        break;
                }

            }

            // Only show skill-specific tooltips if [shift] is held down
            if (Main.keyState.PressingShift())
            {
                TooltipLine skillTitleTooltip = new TooltipLine(Mod, "SkillTitle", Mod.GetLocalization("Tooltips.SkillStats").Value)
                {
                    OverrideColor = Color.CadetBlue
                };

                tooltips.Add(skillTitleTooltip);

                TooltipLine cooldownTooltip = new TooltipLine(Mod, "SkillCooldown", Mod.GetLocalization("Tooltips.Cooldown").Format(skillData.cooldownTime / 60))
                {
                    OverrideColor = Color.LightCyan
                };
                tooltips.Add(cooldownTooltip);

                if (skillData.skillDuration != -1)
                {
                    TooltipLine summonTooltip = new TooltipLine(Mod, "SkillDuration", Mod.GetLocalization("Tooltips.SummonTime").Format(skillData.skillDuration / 60))
                    {
                        OverrideColor = Color.LightCyan
                    };

                    tooltips.Add(summonTooltip);
                }

                if (!SkillModPlayer.IsInSlot(instance))
                {
                    TooltipLine slotReqTooltip = new TooltipLine(Mod, "SkillSlotReq", "(Item can only be used in a skill slot)")
                    {
                        OverrideColor = Color.PaleVioletRed
                    };

                    tooltips.Add(slotReqTooltip);
                }
            }
            else
            {
                TooltipLine shiftTooltip = new TooltipLine(Mod, "ShiftHint", Mod.GetLocalization("Tooltips.ShiftHint").Value)
                {
                    OverrideColor = Color.CadetBlue
                };

                tooltips.Add(shiftTooltip);
            }

        }

    }
}
