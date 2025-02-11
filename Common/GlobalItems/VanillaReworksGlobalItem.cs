using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Content.WeaponAnimations;

namespace TerrariaCells.Common.GlobalItems
{
    public class VanillaReworksGlobalItem : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
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
                    item.damage = 20;
                    item.useTime = 14;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.Minishark:
                    // dps ~60, with reloading ~90, tapers off to 70
                    item.damage = 10;
                    item.knockBack = 0f;
                    item.value = 1000;
					break;
                case ItemID.SniperRifle:
                    item.damage = 60;
                    item.useTime = 25;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.OnyxBlaster:
                    // Change its damage in WeaponHoldoutify.cs, no idea why is it there but I don't want to break it
                    item.damage = 13;
                    item.useTime = 48;
                    item.value = 1000;
                    break;
                // Bows
                case ItemID.PulseBow:
                    item.damage = 15;
                    item.useTime = 23;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.IceBow:
                    item.damage = 15;
                    item.useTime = 16;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                // Launchers
                case ItemID.Toxikarp:
                    item.damage = 2;
                    item.useTime = 12;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.RocketLauncher:
                    item.damage = 10;
                    item.useTime = 30;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                case ItemID.StarCannon:
                    item.damage = 15;
                    item.useTime = 10;
                    item.useAnimation = 30;
                    item.reuseDelay = 15;
                    item.knockBack = 0f;
                    item.value = 1000;
					return;
                case ItemID.GrenadeLauncher:
                    item.damage = 80;
                    item.useTime = 70;
                    item.knockBack = 0f;
                    item.value = 1000;
                    break;
                // Other
                case ItemID.AleThrowingGlove:
                    item.damage = 20;
                    item.knockBack = 0f;
                    item.value = 1000;
					break;

                // MELEE
                // Swords
                case ItemID.FieryGreatsword:
                    item.damage = 30;
                    item.useTime = 30;
                    item.value = 1000;
                    break;
                case ItemID.Starfury:
                    item.damage = 8;
                    item.useTime = 20;
                    item.value = 1000;
                    break;
                // MAGE
                case ItemID.EmeraldStaff:
                    item.damage = 15;
                    item.mana = 5;
                    item.useTime = 18;
                    item.knockBack = 0f;
                    item.shootSpeed = 50;
                    break;
                case ItemID.InfernoFork:
                    item.damage = 15;
                    item.mana = 80;
                    item.useTime = 45;
                    item.knockBack = 0f;
                    break;
                case ItemID.StaffofEarth:
                    item.damage = 120;
                    item.mana = 100;
                    item.useTime = 45;
                    item.knockBack = 10f;
                    break;
                case ItemID.LaserRifle:
                    item.damage = 5;
                    item.mana = 3;
                    item.useTime = 8;
                    item.knockBack = 0f;
                    break;
                case ItemID.VenomStaff:
                    item.damage = 8;
                    item.mana = 40;
                    item.useTime = 30;
                    item.knockBack = 0f;
                    break;
                case ItemID.BookofSkulls: 
                    item.damage = 40;
                    item.mana = 40;
                    item.useTime = 20;
                    item.knockBack = 0f;
                    item.shootSpeed = 12;
                    break;
                // SUMMON
                // Staffs
                case ItemID.ClingerStaff:
                    item.knockBack = 0f;
                    break;
            }
            
			item.useAnimation = item.useTime;
        }

        public override void ModifyHitNPC(Item item, Player player, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (item.type == ItemID.FieryGreatsword && target.HasBuff(BuffID.Oiled))
            {
                modifiers.SetCrit();
                Projectile.NewProjectileDirect(player.GetSource_OnHit(target), target.Center, Vector2.Zero, ProjectileID.Volcano, item.damage, 5, player.whoAmI, ai1: 1);
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
                    case "Material": // Remove the Material tag in the item tooltip
                        tooltip.Hide();
                        break;
                    case "Knockback":
                        float knockback = player.GetWeaponKnockback(item, item.knockBack);

                        knockback = (knockback / 20) * 100;
                        knockback = MathF.Round(knockback);

                        tooltip.Text += " (" + knockback + "%)";
                        break;
                    case "Speed":

                        int tempStat = (int)(item.useAnimation * (1 / player.GetWeaponAttackSpeed(item)));

                        if (tempStat <= 8)
                            tooltip.Text = Lang.tip[6].Value;
                        else if (tempStat <= 20)
                            tooltip.Text = Lang.tip[7].Value;
                        else if (tempStat <= 25)
                            tooltip.Text = Lang.tip[8].Value;
                        else if (tempStat <= 30)
                            tooltip.Text = Lang.tip[9].Value;
                        else if (tempStat <= 35)
                            tooltip.Text = Lang.tip[10].Value;
                        else if (tempStat <= 45)
                            tooltip.Text = Lang.tip[11].Value;
                        else if (tempStat <= 55)
                            tooltip.Text = Lang.tip[12].Value;
                        else
                            tooltip.Text = Lang.tip[13].Value;

                        float attacksPerSecond = MathF.Round(60 / (float)tempStat, 2);
                        tooltip.Text += Mod.GetLocalization("Tooltips.AttacksPerSecond").Format(attacksPerSecond);
                        break;
                    case "UseMana":
                        tooltip.Hide();
                        break;
                    case "EtherianManaWarning":
                        tooltip.Hide();
                        break;
                    case "OneDropLogo":
                        tooltip.Hide();
                        break;
                        /*
                    case "Favorite":
                        tooltip.Hide();
                        break;
                    case "FavoriteDesc":
                        tooltip.Hide();
                        break;
                        */
                }

            }

            /*
            // FOR TESTING
            Mod.Logger.Debug("Tooltip for " + item.Name);
            foreach (TooltipLine tooltip in tooltips)
            {
                Mod.Logger.Debug(tooltip.Name + " : " + tooltip.Text);
            }
            */
        }
    }
}
