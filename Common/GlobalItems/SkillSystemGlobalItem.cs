using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace TerrariaCells.Common.GlobalItems
{

    public class SkillItemData
    {
        public float cooldownTime = 60;
        public float skillDuration = -1;
        public string tooltip;

        public bool useInSolid = true;
        public bool lineOfSight = false;

        public SkillItemData() { }

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

    public class SkillSlotData
    {
        public float cooldownTimer = 0;
        public float cooldownTotal = 60;

        public ModKeybind keybind = null;

        public SkillSlotData() { }

        public SkillSlotData(ModKeybind keybind)
        {
            this.keybind = keybind;
        }
    }

    public class SkillModPlayer : ModPlayer
    {
        // Slot index and slot data
        public static Dictionary<int, SkillSlotData> SkillSlots = new Dictionary<int, SkillSlotData>();

        // ItemID and skill data
        public static Dictionary<int, SkillItemData> SkillItems = new Dictionary<int, SkillItemData>();

        public override void SetStaticDefaults()
        {

            // DEFINE WHICH SLOTS ARE FOR SKILLS
            ModKeybind skillKeybind1 = KeybindLoader.RegisterKeybind(Mod, "First Skill", "Q");
            AddSkillSlotWithKeybind(2, skillKeybind1);
            ModKeybind skillKeybind2 = KeybindLoader.RegisterKeybind(Mod, "Second Skill", "E");
            AddSkillSlotWithKeybind(3, skillKeybind2);


            // DEFINE WHICH ITEMS ARE SKILLS
            SkillItems.Add(ItemID.StormTigerStaff, new SkillItemData(1800, 600));
            SkillItems.Add(ItemID.StardustDragonStaff, new SkillItemData(1800, 600));

            SkillItems.Add(ItemID.ClingerStaff, new SkillItemData(600, 300));
            SkillItems[ItemID.ClingerStaff].useInSolid = false;
            SkillItems[ItemID.ClingerStaff].lineOfSight = true;

            SkillItems.Add(ItemID.ToxicFlask, new SkillItemData(480));

            SkillItems.Add(ItemID.StaffoftheFrostHydra, new SkillItemData(1800, 600));
            SkillItems[ItemID.StaffoftheFrostHydra].useInSolid = false;

            SkillItems.Add(ItemID.DD2ExplosiveTrapT1Popper, new SkillItemData(480, 300));
            SkillItems[ItemID.DD2ExplosiveTrapT1Popper].useInSolid = false;
            SkillItems[ItemID.DD2ExplosiveTrapT1Popper].lineOfSight = true;

            SkillItems.Add(ItemID.MolotovCocktail, new SkillItemData(480));
            SkillItems.Add(ItemID.BouncyDynamite, new SkillItemData(480));
            SkillItems.Add(ItemID.BouncingShield, new SkillItemData(360));
            SkillItems.Add(ItemID.SnowballLauncher, new SkillItemData(1200, 480));

        }

        internal int originalSelectedItem;
        internal bool autoRevertSelectedItem = false;
        internal bool pendingQuickUse = false;

        public void QuickUseItemAt(int index, bool use = true)
        {

            if (SkillSlots.ContainsKey(index))
            {
                SkillSlotData slotData = SkillSlots[index];

                if (slotData.cooldownTimer > 0)
                {
                    return;
                }
            }

            if (Player.selectedItem == index)
            {
                Player.controlUseItem = true;
                return;
            }

            if (!autoRevertSelectedItem && Player.selectedItem != index && Player.inventory[index].type != 0)
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

                    // Reduce cooldown timer for skill slot, if above 0
                    if (slotInfo.Value.cooldownTimer > 0)
                    {
                        slotInfo.Value.cooldownTimer -= 1;
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
        public void StopSkill(Item item)
        {

            if (item.buffType != 0)
            {
                if (Main.LocalPlayer.HasBuff(item.buffType))
                {

                    // Clear the buff, to ensure it is removed regardless of the status of the following projectile checks - those are mostly for visually disposing of the summoned "minions"
                    Main.LocalPlayer.ClearBuff(item.buffType);

                    // Search for the projectile associated with the item, if there is one
                    if (item.shoot != ProjectileID.None)
                    {
                        bool hidden = false;

                        foreach (Projectile projectile in Main.projectile)
                        {

                            if (projectile.type == item.shoot && projectile.owner == Main.myPlayer || hidden && projectile.minion == true)
                            {
                                Projectile actualProjectile = projectile;

                                // Look for the secondary minions spawned instead, if the projectile spawned is hidden
                                if (projectile.hide == true)
                                {
                                    hidden = true;
                                    continue;
                                }

                                // Create an effect at the position of the found "projectile" (probably a summon)
                                Explosion(new Vector2(projectile.position.X + (projectile.Size.X / 2), projectile.position.Y + (projectile.Size.Y / 2)), 24);
                            }

                        }
                    }

                }

            }
            else if (item.createTile != -1) // Destroy any tiles created by the skill item, typically sentry turrets
            {
                Vector2 playerPos = Main.LocalPlayer.position / 16f;

                for (int i = (int)playerPos.X - 90; i < (int)playerPos.X + 90; i++)
                {
                    for (int j = (int)playerPos.Y - 90; j < (int)playerPos.Y + 90; j++)
                    {
                        Point point = new Point(i, j);

                        Tile tile = Main.tile[point];

                        if (tile.TileType == item.createTile)
                        {
                            //tile.ClearTile(); // clears individual tiles

                            Explosion(new Vector2(point.X * 16, point.Y * 16), 48);

                            WorldGen.KillTile(point.X, point.Y, false, false, true); // Works better but drops an item, can be prevented with a GlobalTile.Drop()
                            return;
                        }
                    }
                }
            }
            else // checking for sentry == true gets things like the frosty hydra but not the clinger staff, which seems to have no differentiating factors from the toxic flask
            {
                // Clean up any remaining projectiles
                foreach (Projectile projectile in Main.projectile)
                {
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
        private void Explosion(Vector2 position, int size)
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
            if (SkillSlots.ContainsKey(inventorySlot))
            {
                return SkillSlots[inventorySlot];
            }
            else
            {
                return null;
            }
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

        /// <summary>
        /// Adds the given inventory slot id to the dictonary of skill slots, giving it skill slot functionality
        /// </summary>
        /// <param name="inventorySlot"></param>
        public void AddSkillSlot(int inventorySlot)
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
        public void AddSkillSlotWithKeybind(int inventorySlot, ModKeybind keybind)
        {
            SkillSlotData data = GetSkillSlotData(inventorySlot);

            if (data == null)
            {
                SkillSlotData newData = new SkillSlotData(keybind);

                SkillSlots.Add(inventorySlot, newData);
            }
        }

        /// <summary>
        /// Return true if the given item is in an inventory slot which has been designated as a skill slot
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

        public static int GetSkillSlotIndex(Item item)
        {
            foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillSlots)
            {
                if (Main.LocalPlayer.inventory[slotInfo.Key] == item)
                {
                    return slotInfo.Key;
                }
            }

            return -1;
        }

    }



    public class SkillSystemGlobalItem : GlobalItem
    {

        public bool onCooldown = false; //// is equivalent to (SkillModPlayer.GetSkillSlotData(item).cooldownTimer > 0)

        public override bool AppliesToEntity(Item item, bool lateInstantiation)
        {
            return lateInstantiation && SkillModPlayer.SkillItems.ContainsKey(item.type);
        }

        public override bool InstancePerEntity => true;

        public override void SetDefaults(Item item)
        {

            // MAKE CHANGES TO SKILL ITEMS PROPERTIES HERE
            switch (item.type)
            {
                case ItemID.StormTigerStaff:
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
            // Hook to prevent items from being picked up while on the skill slot is on cooldown
            On_ItemSlot.PickItemMovementAction += ItemSlot_PickItemMovementAction;

            // Hook to check for tile placement, which ensures skill items that place tiles (ie. turrets) do not get put on cooldown until the tile is succesfully placed
            On_Player.PlaceThing_Tiles_PlaceIt += On_Player_PlaceThing_Tiles_PlaceIt;

            // Hooks to allow DD2 turrets at all times
            On_Projectile.TurretShouldPersist += On_Projectile_TurretShouldPersist;
            On_Player.ItemCheck_CheckCanUse += On_Player_ItemCheck_CheckCanUse;
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


        // Redirection of Terraria function that runs while the mouse is hovering over the inventory
        private int ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item item)
        {
            // Prevent items on cooldown from being picked up in the inventory
            if (SkillModPlayer.SkillSlots.TryGetValue(slot, out SkillSlotData slotData))
            {
                if (slotData.cooldownTimer > 0)
                {
                    return -1;
                }

            }


            //  Prevent two of the same item from being placed in skill slots at the same time
            foreach (int invSlot in SkillModPlayer.SkillSlots.Keys)
            {
                if (!SkillModPlayer.SkillSlots.Keys.Contains(slot))
                {
                    continue;
                }

                if (inv[invSlot].type == item.type)
                {
                    return -1;
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

                /*
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

        public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (!SkillModPlayer.IsInSlot(item) || !onCooldown)
            {
                return;
            }

            SkillSlotData data = SkillModPlayer.GetSkillSlotData(item);

            // Cooldown item slot indicator
            spriteBatch.Draw(TextureAssets.InventoryBack.Value,
                position: new Vector2(position.X, position.Y),
                sourceRectangle: new Rectangle(0, 0, 52, (int)(52 * ((float)data.cooldownTimer / data.cooldownTotal))),
                color: new Color(15, 15, 15, 128),
                rotation: 3.14159f,
                origin: new Vector2(26, 26),
                scale: new Vector2(Main.inventoryScale, Main.inventoryScale),
                SpriteEffects.None,
                layerDepth: 0f);

            // Cooldown countdown text display
            string currentCooldown = MathF.Ceiling(data.cooldownTimer / 60).ToString();
            //spriteBatch.DrawString(FontAssets.DeathText.Value, currentCooldown.ToString(), position + new Vector2(-13f, 2f), Color.White, 0, origin: new Vector2(0, 0), Main.inventoryScale * 0.35f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(FontAssets.DeathText.Value, currentCooldown.ToString(), position + new Vector2(0f, 4f), Color.White, 0, origin: new Vector2(10f * (currentCooldown.Length), 20), Main.inventoryScale * 0.5f, SpriteEffects.None, 0f);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            SkillItemData skillData = SkillModPlayer.SkillItems[item.type];

            if (skillData == null)
            {
                return;
            }

            TooltipLine skillTooltip = new TooltipLine(Mod, "SkillTooltip", skillData.tooltip);

            // Iterate backwards through the list of tooltips so we can change it while we iterate through
            for (int i = tooltips.Count - 1; i >= 0; i--)
            {
                // Alter vanilla tooltips here
                switch (tooltips[i].Name)
                {
                    case "ItemName": // Change all skills to have a light green name color
                        tooltips[i].Text = "[Skill] " + tooltips[i].Text;

                        Color amberColor = new Color(255, 175, 0);
                        tooltips[i].OverrideColor = amberColor;
                        break;
                    case "Tooltip0": // If the skill has a custom tooltip description, replace the vanilla tooltip with it
                        if (!string.IsNullOrEmpty(skillData.tooltip))
                        {
                            tooltips.Remove(tooltips[i]);
                            tooltips.Add(skillTooltip);
                        }
                        break;
                    case "Tooltip1": // Remove additional tooltips, if a custom one is available
                        if (!string.IsNullOrEmpty(skillData.tooltip))
                        {
                            tooltips.Remove(tooltips[i]);
                        }
                        break;
                    case "Tooltip2": // Remove additional tooltips, if a custom one is available
                        if (!string.IsNullOrEmpty(skillData.tooltip))
                        {
                            tooltips.Remove(tooltips[i]);
                        }
                        break;
                    case "Material": // Remove the Material tag in the item tooltip
                        tooltips.Remove(tooltips[i]);
                        break;
                    case "Speed":
                        tooltips.Remove(tooltips[i]);
                        break;
                }

            }

            TooltipLine cooldownTooltip = new TooltipLine(Mod, "SkillCooldown", Mod.GetLocalization("Tooltips.Cooldown").Value + ": " + skillData.cooldownTime);
            cooldownTooltip.OverrideColor = Color.LightCyan;
            tooltips.Add(cooldownTooltip);

            if (skillData.skillDuration != -1)
            {
                TooltipLine summonTooltip = new TooltipLine(Mod, "SkillDuration", Mod.GetLocalization("Tooltips.SummonTime").Value + ": " + skillData.skillDuration);
                summonTooltip.OverrideColor = Color.LightCyan;
                tooltips.Add(summonTooltip);
            }

        }
    }
}
