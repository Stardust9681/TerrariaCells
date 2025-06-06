using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.GlobalProjectiles;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.UI;
using TerrariaCells.Content.WeaponAnimations;

namespace TerrariaCells.Common.GlobalItems
{
    public class VanillaReworksGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item == null || item.type <= ItemID.None)
                return;

            //make swords that are juuust too fast for the animation to look right. slowed down a given a bit of damage
            if (item.useStyle == ItemUseStyleID.Swing && !item.noMelee && item.DamageType == DamageClass.Melee && item.useTime == 15)
            {
                item.useTime += 5;
                item.damage = (int)(item.damage * 1.05f);
            }
            // CHANGE DEFAULT ITEM STATS HERE
            switch (item.type)
            {
                // RANGED WEAPONS
                // Guns
                case ItemID.PhoenixBlaster:
                    item.damage = 12;
                    item.useTime = 14;
                    item.value = 1000;
                    break;
                case ItemID.Minishark:
                    // dps ~60, with reloading ~90, tapers off to 70
                    item.damage = 6;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.SniperRifle:
                    item.damage = 50;
                    item.useTime = 25;
                    item.value = 1000;
                    break;
                case ItemID.OnyxBlaster:
                    // Change its damage in WeaponHoldoutify.cs, no idea why is it there but I don't want to break it
                    item.damage = 8;
                    item.useTime = 48;
                    item.value = 1000;
                    break;
                // Bows
                case ItemID.WoodenBow:
                    item.damage = 6;
                    break;
                case ItemID.PulseBow:
                    item.damage = 13;
                    item.useTime = 23;
                    item.value = 1000;
                    break;
                case ItemID.IceBow:
                    item.damage = 13;
                    item.useTime = 16;
                    //item.shootSpeed -= 2f;
                    item.value = 1000;
                    break;
                case ItemID.PlatinumBow:
                    item.damage = 20;
                    item.value = 1000;
                    break;
                // Launchers
                case ItemID.Toxikarp:
                    item.damage = 2;
                    item.useTime = 12;
                    item.value = 1000;
                    break;
                case ItemID.RocketLauncher:
                    item.damage = 10;
                    item.useTime = 30;
                    item.value = 1000;
                    break;
                case ItemID.StarCannon:
                    item.damage = 15;
                    item.useTime = 10;
                    item.useAnimation = 30;
                    item.reuseDelay = 15;
                    item.value = 1000;
                    return;
                case ItemID.GrenadeLauncher:
                    item.damage = 80;
                    item.useTime = 70;
                    item.value = 1000;
                    break;
                // Other
                case ItemID.AleThrowingGlove:
                    item.damage = 20;
                    item.value = 1000;
                    break;

                // MELEE
                // Swords
                case ItemID.FieryGreatsword:
                    item.damage = 20;
                    item.useTime = 30;
                    item.value = 1000;
                    break;
                case ItemID.Starfury:
                    item.damage = 8;
                    item.useTime = 20;
                    item.value = 1000;
                    break;
                case ItemID.PlatinumBroadsword:
                    item.damage = 20;
                    item.value = 1000;
                    break;
                case ItemID.Gladius:
                    item.damage = 8;
                    item.value = 1000;
                    break;
                case ItemID.Katana:
                    item.damage = 8;
                    item.useTime = 20;
                    item.value = 1000;
                    break;
                case ItemID.SawtoothShark:
                    item.axe = 0;
                    item.value = 1000;
                    break;

                // MAGE
                case ItemID.EmeraldStaff:
                case ItemID.RubyStaff:
                    item.damage = 7;
                    item.mana = 8;
                    item.useTime = 18;
                    item.knockBack = 0f;
                    item.shootSpeed = 10;
                    item.value = 1000;
                    break;
                case ItemID.InfernoFork:
                    item.damage = 15;
                    item.mana = 80;
                    item.useTime = 45;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.StaffofEarth:
                    item.damage = 120;
                    item.mana = 100;
                    item.useTime = 45;
                    item.knockBack = 10f;
                    item.value = 1000;
                    break;
                case ItemID.LaserRifle:
                    item.damage = 5;
                    item.mana = 3;
                    item.useTime = 8;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.VenomStaff:
                    item.damage = 8;
                    item.mana = 40;
                    item.useTime = 30;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.BookofSkulls:
                    item.damage = 40;
                    item.mana = 40;
                    item.useTime = 20;
                    item.knockBack = 0f;
                    item.shootSpeed = 12;
                    item.value = 1000;
                    break;

                // SUMMON
                // Staffs
                case ItemID.ClingerStaff:
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
            }

            Dictionary<string, int[]> chestLootTables = ModContent.GetContent<ChestLootSpawner>().First().ChestLootTables;
            foreach (var key in chestLootTables.Keys)
            {
                if (chestLootTables[key].Contains(item.type))
                {
                    switch (key)
                    {
                        case "1":
                            // multiplied by 5 so that I can input sell price instead of buy price
                            item.value = 5 * 500;
                            break;
                        case "19":
                            item.value = 5 * 5000;
                            break;
                        case "20":
                            item.value = 5 * 10000;
                            break;
                        case "43":
                            item.value = 5 * 4000;
                            break;
                    }
                    break;
                }
            }

            item.useAnimation = item.useTime;
            if (item.DamageType.CountsAsClass(DamageClass.Ranged))
                item.knockBack = 0;

            // Use color rarities to indicate item category:
            // Weapons(Red)
            // Skills(Green)
            // Armor(Blue)
            // Healing potions(Amber)
            // Accessories(Yellow)
            // Large gems(Light Purple)

            try
            {
                switch (InventoryManager.GetItemCategorization(item.type))
                {
                    case TerraCellsItemCategory.Weapon:
                        item.rare = ItemRarityID.Red; // or custom rarity ID
                        break;
                    case TerraCellsItemCategory.Skill:
                        item.rare = ItemRarityID.Green;
                        break;
                    case TerraCellsItemCategory.Potion:
                        item.rare = ItemRarityID.Quest; // Amber-like
                        break;
                    case TerraCellsItemCategory.Storage:
                        switch (InventoryManager.GetStorageItemSubcategorization(item.type))
                        {
                            case StorageItemSubcategorization.Accessory:
                                item.rare = ItemRarityID.Yellow;
                                break;
                            case StorageItemSubcategorization.Armor:
                                item.rare = ItemRarityID.Blue;
                                break;
                            case StorageItemSubcategorization.Coin:
                                item.rare = ItemRarityID.White;
                                break;
                            default:
                                item.rare = ItemRarityID.LightPurple;
                                break;
                                //default:
                                //    item.rare = ItemRarityID.LightRed; // Large gems or other Storage items
                                //    break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn($"Failed to assign rarity for item {item?.Name} (type {item?.type}): {ex.Message}");
            }
        }

        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.type == ItemID.FieryGreatsword && target.HasBuff(BuffID.Oiled))
            {
                modifiers.SetCrit();
                Projectile.NewProjectileDirect(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ProjectileID.Volcano, item.damage, modifiers.GetKnockback(item.knockBack), player.whoAmI, ai1: 1);
            }
            if (item.type == ItemID.Gladius && (target.HasBuff(BuffID.Poisoned) || target.HasBuff(BuffID.BloodButcherer)))
            {
                modifiers.SetCrit();
                Projectile.NewProjectileDirect(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ProjectileID.GladiusStab, item.damage, item.knockBack, player.whoAmI, ai1: 1);
            }
            if (item.type == ItemID.Katana && player.GetModPlayer<WeaponPlayer>().swingType == 0)
            {
                modifiers.SetCrit();
            }
            base.ModifyHitNPC(item, player, target, ref modifiers);
        }

        // Prevents guns from utilizing ammo
        public override bool NeedsAmmo(Item item, Player player)
        {
            return false;
        }

        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            // Disable crits for all weapons by default
            crit = 0;

            // ADD/MODIFY CUSTOM CRIT EFFECTS HERE
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;

            // Iterate through the list of tooltips so we can change vanilla tooltips
            foreach (TooltipLine tooltip in tooltips)
            {
                // CHANGE VANILLA TOOLTIPS HERE
                switch (tooltip.Name)
                {
                    //Tooltips to be hidden
                    // case "Favorite":
                    // case "FavoriteDesc":
                    case "Material":
                    case "Consumable":
                    case "EtherianManaWarning":
                    case "OneDropLogo":
                    case "JourneyResearch":
                        tooltip.Hide();
                        break;

                    case "Knockback":
                        float knockback = player.GetWeaponKnockback(item, item.knockBack);

                        // if (knockback == 0.0)
                        // {
                        tooltip.Hide();

                        // break;
                        // }

                        // knockback = (knockback / 20) * 100;
                        // knockback = MathF.Round(knockback);

                        // tooltip.Text += " (" + knockback + "%)";
                        break;

                    case "Speed":
                        // int tempStat = (int)(item.useAnimation * (1 / player.GetWeaponAttackSpeed(item)));

                        // if (tempStat <= 8)
                        //     tooltip.Text = Lang.tip[6].Value;
                        // else if (tempStat <= 20)
                        //     tooltip.Text = Lang.tip[7].Value;
                        // else if (tempStat <= 25)
                        //     tooltip.Text = Lang.tip[8].Value;
                        // else if (tempStat <= 30)
                        //     tooltip.Text = Lang.tip[9].Value;
                        // else if (tempStat <= 35)
                        //     tooltip.Text = Lang.tip[10].Value;
                        // else if (tempStat <= 45)
                        //     tooltip.Text = Lang.tip[11].Value;
                        // else if (tempStat <= 55)
                        //     tooltip.Text = Lang.tip[12].Value;
                        // else
                        //     tooltip.Text = Lang.tip[13].Value;

                        // float attacksPerSecond = MathF.Round(60 / (float)tempStat, 2);
                        // tooltip.Text += Mod.GetLocalization("Tooltips.AttacksPerSecond").Format(attacksPerSecond);
                        tooltip.Hide();
                        break;
                    case "UseMana":
                        switch (InventoryManager.GetItemCategorization(item.netID))
                        {
                            case TerraCellsItemCategory.Weapon:
                                break;
                            default:
                                tooltip.Hide();
                                break;
                        }
                        break;
                }
            }

            TooltipLine itemCategorizationTooltip = new(Mod, "ItemCategorization", "");
            switch (InventoryManager.GetItemCategorization(item.netID))
            {
                case TerraCellsItemCategory.Default:
                    itemCategorizationTooltip.OverrideColor = LimitedStorageUI.defaultSlotColor;
                    itemCategorizationTooltip.Text = "???";
                    break;
                case TerraCellsItemCategory.Weapon:
                    itemCategorizationTooltip.OverrideColor = LimitedStorageUI.weaponSlotColor;
                    itemCategorizationTooltip.Text = "Weapon";
                    break;
                case TerraCellsItemCategory.Skill:
                    itemCategorizationTooltip.OverrideColor = LimitedStorageUI.skillSlotColor;
                    itemCategorizationTooltip.Text = "Skill";
                    break;
                case TerraCellsItemCategory.Potion:
                    itemCategorizationTooltip.OverrideColor = LimitedStorageUI.potionSlotColor;
                    itemCategorizationTooltip.Text = "Potion";
                    break;
                case TerraCellsItemCategory.Storage:
                    // itemCategorizationTooltip.OverrideColor = LimitedStorageUI.storageSlotColor;
                    // itemCategorizationTooltip.Text = "Storage";
                    break;
                case TerraCellsItemCategory.Pickup:
                    itemCategorizationTooltip.OverrideColor = LimitedStorageUI.defaultSlotColor;
                    itemCategorizationTooltip.Text = "Potion";
                    break;
                default:
                    break;
            }

            int index = tooltips.FindIndex(x => x.Name == "Damage");
            if (index == -1)
            {
                tooltips.Add(itemCategorizationTooltip);
            }
            else
            {
                tooltips.Insert(index, itemCategorizationTooltip);
            }

            //Add tooltips at the end
            switch (item.type)
            {
                case ItemID.SniperRifle:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Inflicts critical hits against bosses"));
                    break;
                case ItemID.FieryGreatsword:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Striking oiled targets causes an explosion, inflicting a critical hit"));
                    break;
                case ItemID.PhoenixBlaster:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "The last 5 shots inflict a critical hit"));
                    break;
                case ItemID.SawtoothShark:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Causes targets to bleed"));
                    break;
                case ItemID.Gladius:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Inflicts a critical hit against poisoned or bleeding targets"));
                    break;
                case ItemID.EmeraldStaff:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Poisons targets"));
                    break;
                case ItemID.RubyStaff:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Lights targets on fire"));
                    break;
                case ItemID.AleThrowingGlove:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Coats targets in oil"));
                    break;
                case ItemID.Minishark:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Inflicts critical hits to nearby enemies"));
                    break;

            }
        }

        public IEnumerable<TooltipLine> GetTooltips(Item item)
        {
            List<TooltipLine> tooltips = [];
            if (DevConfig.Instance.ListCategorizationTooltip)
            {
                TooltipLine itemCategorizationTooltip = new(Mod, "ItemCategorization", "");
                switch (InventoryManager.GetItemCategorization(item.netID))
                {
                    case TerraCellsItemCategory.Default:
                        itemCategorizationTooltip.OverrideColor = LimitedStorageUI.defaultSlotColor;
                        itemCategorizationTooltip.Text = "???";
                        break;
                    case TerraCellsItemCategory.Weapon:
                        itemCategorizationTooltip.OverrideColor = LimitedStorageUI.weaponSlotColor;
                        itemCategorizationTooltip.Text = "Weapon";
                        break;
                    case TerraCellsItemCategory.Skill:
                        itemCategorizationTooltip.OverrideColor = LimitedStorageUI.skillSlotColor;
                        itemCategorizationTooltip.Text = "Skill";
                        break;
                    case TerraCellsItemCategory.Potion:
                        itemCategorizationTooltip.OverrideColor = LimitedStorageUI.potionSlotColor;
                        itemCategorizationTooltip.Text = "Potion";
                        break;
                    case TerraCellsItemCategory.Storage:
                        // itemCategorizationTooltip.OverrideColor = LimitedStorageUI.storageSlotColor;
                        // itemCategorizationTooltip.Text = "Storage";
                        break;
                    case TerraCellsItemCategory.Pickup:
                        itemCategorizationTooltip.OverrideColor = LimitedStorageUI.defaultSlotColor;
                        itemCategorizationTooltip.Text = "Potion";
                        break;
                    default:
                        break;
                }
                tooltips.Add(itemCategorizationTooltip);
            }
            switch (item.type)
            {
                case ItemID.SniperRifle:
                    tooltips.Add(
                        new TooltipLine(Mod, "Tooltip0", "Inflicts critical hits against bosses")
                    );
                    break;
                case ItemID.FieryGreatsword:
                    tooltips.Add(
                        new TooltipLine(
                            Mod,
                            "Tooltip0",
                            "Striking oiled targets causes an explosion, inflicting a critical hit"
                        )
                    );
                    break;
                case ItemID.PhoenixBlaster:
                    tooltips.Add(
                        new TooltipLine(Mod, "Tooltip0", "The last 5 shots inflict a critical hit")
                    );
                    break;
                case ItemID.SawtoothShark:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Causes targets to bleed"));
                    break;
                case ItemID.Gladius:
                    tooltips.Add(
                        new TooltipLine(
                            Mod,
                            "Tooltip0",
                            "Inflicts a critical hit against poisoned or bleeding targets"
                        )
                    );
                    break;
                case ItemID.EmeraldStaff:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Poisons targets"));
                    break;
                case ItemID.RubyStaff:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Lights targets on fire"));
                    break;
                case ItemID.AleThrowingGlove:
                    tooltips.Add(new TooltipLine(Mod, "Tooltip0", "Coats targets in oil"));
                    break;
                case ItemID.Minishark:
                    tooltips.Add(
                        new TooltipLine(Mod, "Tooltip0", "Inflicts critical hits to nearby enemies")
                    );
                    break;
            }
            return tooltips;
        }

        public override void AddRecipes()
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (recipe is null) continue;

                if (
                    !recipe.Disabled && (
                    recipe.HasIngredient(ItemID.CopperCoin)
                    || recipe.HasIngredient(ItemID.SilverCoin)
                    || recipe.HasIngredient(ItemID.GoldCoin)
                    || recipe.HasIngredient(ItemID.PlatinumCoin)))
                    recipe.DisableRecipe();
            }
        }
    }
}