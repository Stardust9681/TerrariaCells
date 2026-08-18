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
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.GlobalProjectiles;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.UI;
using TerrariaCells.Common.Utilities;
using TerrariaCells.Content.WeaponAnimations;

namespace TerrariaCells.Common.GlobalItems
{
    public partial class VanillaReworksGlobalItem : GlobalItem
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
                    item.damage = 8;
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
                    item.knockBack = 2f;
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
                case ItemID.AmberStaff:
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

            // Use color rarities to indicate item category:
            // Weapons(Red)
            // Skills(Green)
            // Armor(Blue)
            // Healing potions(Amber)
            // Accessories(Yellow)
            // Large gems(Light Purple)
            switch (ItemsJson.Instance.Category.GetValueOrDefault(item.type))
            {
                case ItemsJson.ItemCategory.Weapons:
                    item.value = 5 * 5_00;
                    item.rare = ItemRarityID.Red;
                    break;
                case ItemsJson.ItemCategory.Abilities:
                    item.value = 5 * 50_00;
                    item.rare = ItemRarityID.Green;
                    break;
                case ItemsJson.ItemCategory.Accessories:
                    item.value = 5 * 1_00_00;
                    item.rare = ItemRarityID.Yellow;
                    break;
                case ItemsJson.ItemCategory.Armor:
                    item.value = 5 * 40_00;
                    item.rare = ItemRarityID.Blue;
                    break;
                case ItemsJson.ItemCategory.Potions:
                    item.value = 5 * 40_00;
                    item.rare = ItemRarityID.Cyan;
                    break;
                default:
                    if (!item.IsACoin) item.rare = ItemRarityID.LightPurple;
                    else item.rare = ItemRarityID.White;
                    break;
            }

            item.useAnimation = item.useTime;
            if (!item.DamageType.CountsAsClass(DamageClass.Melee))
                item.knockBack = 0;

            SetNameOverrides(item);
        }

        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            switch (item.type)
            {
                case ItemID.FieryGreatsword:
                    if (target.HasBuff(BuffID.Oiled))
                    {
                        modifiers.SetCrit();
                        Projectile.NewProjectileDirect(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ProjectileID.Volcano, item.damage, modifiers.GetKnockback(item.knockBack), player.whoAmI, ai1: 1);
                    }
                    break;
                case ItemID.Gladius:
                    if (target.HasBuff(BuffID.Poisoned) || target.HasBuff(BuffID.Bleeding))
                    {
                        modifiers.SetCrit();
                        Projectile.NewProjectileDirect(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ProjectileID.GladiusStab, item.damage, item.knockBack, player.whoAmI, ai1: 1);
                    }
                    break;
                case ItemID.Katana:
                    if (player.GetModPlayer<WeaponPlayer>().swingType == 0)
                    {
                        modifiers.SetCrit();
                    }
                    break;
                case ItemID.FalconBlade:
                    if (player.moveSpeed > 1.25f)
                    {
                        modifiers.SetCrit();
                    }
                    break;
            }
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

        public override void Load()
        {
            LoadLocalization();
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