using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Content.WeaponAnimations;

namespace TerrariaCells.Common.GlobalItems
{
    public class ArmorEffects : GlobalItem
    {
        public override void Load()
        {
            On_Player.GrantArmorBenefits += On_Player_GrantArmorBenefits;
        }
        public override void Unload()
        {
            On_Player.GrantArmorBenefits -= On_Player_GrantArmorBenefits;
        }

        public override void SetDefaults(Item item)
        {
            switch (item.type)
            {
                case ItemID.NinjaHood:
                case ItemID.NinjaShirt:
                case ItemID.NinjaPants:
                case ItemID.JungleHat:
                case ItemID.JungleShirt:
                case ItemID.JunglePants:
                case ItemID.NecroHelmet:
                case ItemID.NecroBreastplate:
                case ItemID.NecroGreaves:
                case ItemID.MoltenHelmet:
                case ItemID.MoltenBreastplate:
                case ItemID.MoltenGreaves:
                    item.defense = 0;
                    break;
            }
        }

        private void On_Player_GrantArmorBenefits(On_Player.orig_GrantArmorBenefits orig, Player player, Item item)
        {
            ArmorPlayer modPlayer = player.GetModPlayer<ArmorPlayer>();
            switch (item.type)
            {
                // Ninja Armor
                case ItemID.NinjaHood:
                    modPlayer.ninjaHood = true;
                    player.GetDamage(DamageClass.Generic) += 0.15f;
                    break;
                case ItemID.NinjaShirt:
                    modPlayer.ninjaShirt = true;
                    break;
                case ItemID.NinjaPants:
                    modPlayer.ninjaPants = true;
                    player.moveSpeed += 0.15f;
                    break;

                // Jungle Armor
                case ItemID.JungleHat:
                    modPlayer.jungleHat = true;
                    player.GetDamage(DamageClass.Magic) += 0.2f;
                    break;
                case ItemID.JungleShirt:
                    modPlayer.jungleShirt = true;
                    //picking up mana stars reduces skill cooldowns by 1/2 second
                    break;
                case ItemID.JunglePants:
                    modPlayer.junglePants = true;
                    player.moveSpeed += 0.1f;
                    player.GetDamage(DamageClass.Magic) += 0.1f;
                    break;

                // Necro Armor
                case ItemID.NecroHelmet:
                    modPlayer.necroHelmet = true;
                    player.GetDamage(DamageClass.Ranged) += 0.2f;
                    break;
                case ItemID.NecroBreastplate:
                    modPlayer.necroBreastplate = true;
                    //killing an enemy spawns baby spiders, which attack nearby enemies
                    break;
                case ItemID.NecroGreaves:
                    modPlayer.necroGreaves = true;
                    player.moveSpeed += 0.1f;
                    //the last bullet in a magazine deals 50% more damage
                    break;

                // Molten Armor
                case ItemID.MoltenHelmet:
                    modPlayer.moltenHelmet = true;
                    player.GetDamage(DamageClass.Melee) += 0.2f;
                    break;
                case ItemID.MoltenBreastplate:
                    modPlayer.moltenBreastplate = true;
                    //-20% damage taken
                    //upon taking damage, all nearby enemies are lit on fire
                    break;
                case ItemID.MoltenGreaves:
                    modPlayer.moltenGreaves = true;
                    player.moveSpeed += 0.1f;
                    //leave a trail of flames that ignites enemies (hellfire treads, but functional)
                    break;

                default:
                    orig.Invoke(player, item);
                    break;
            }

            if (modPlayer.necroArmorSet)
            {
                Gun.StaticReloadTimeMult = 0.5f;
                Bow.StaticChargeTimeMult = 0.5f;
            }
            else
            {
                Gun.StaticReloadTimeMult = 1f;
                Bow.StaticChargeTimeMult = 1f;
            }
        }

        public IEnumerable<TooltipLine> GetTooltips(Item item)
        {
            ArmorPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ArmorPlayer>();
            if (modPlayer.ninjaArmorSet)
            {
                return item.type switch
                {
                    ItemID.NinjaHood => [
                        new(Mod, "Tooltip0", "15% increased damage"),
                        new (Mod, "Tooltip1", "Set bouns: Become immune after striking an enemy")
                    ],
                    ItemID.NinjaShirt => [
                        new(Mod, "Tooltip0", "Allows the player to dash"),
                        new (Mod, "Tooltip1", "Set bouns: Become immune after striking an enemy")
                    ],
                    ItemID.NinjaPants => [
                        new(Mod, "Tooltip0", "15% increased movement speed"),
                        new (Mod, "Tooltip1", "Set bouns: Become immune after striking an enemy")
                    ],
                    _ => []
                };
            }
            if (modPlayer.jungleArmorSet)
            {
                return item.type switch
                {
                    ItemID.JungleHat => [
                        new(Mod, "Tooltip0", "20% increased magic damage"),
                        new(Mod, "Tooltip1", "Set bonus: Killing an enemy reduces mana cost by 100% for 3 seconds")
                    ],
                    ItemID.JungleShirt => [
                        new(Mod, "Tooltip0", "Picking up mana stars reduces skill cooldowns by 1/2 second"),
                        new(Mod, "Tooltip1", "Set bonus: Killing an enemy reduces mana cost by 100% for 3 seconds")
                    ],
                    ItemID.JunglePants => [
                        new(Mod, "Tooltip0", "10% increased movement speed"),
                        new(Mod, "Tooltip1", "10% increased magic damage"),
                        new(Mod, "Tooltip2", "Set bonus: Killing an enemy reduces mana cost by 100% for 3 seconds")
                    ],
                    _ => []
                };
            }
            if (modPlayer.necroArmorSet)
            {
                return item.type switch
                {
                    ItemID.NecroHelmet => [
                        new(Mod, "Tooltip0", "20% increased ranged damage"),
                        new(Mod, "Tooltip1", "Set bouns: Bows charge twice as fast,\nguns reload in half the time")
                    ],
                    ItemID.NecroBreastplate => [
                        new(Mod, "Tooltip0", "Killing an enemy spawns baby spiders, which attack nearby enemies"),
                        new(Mod, "Tooltip1", "Set bouns: Bows charge twice as fast,\nguns reload in half the time")
                    ],
                    ItemID.NecroGreaves => [
                        new(Mod, "Tooltip0", "10% increased movement speed"),
                        new(Mod, "Tooltip1", "The last bullet in a magazine deals 50% more damage"),
                        new(Mod, "Tooltip2", "Set bouns: Bows charge twice as fast,\nguns reload in half the time")
                    ],
                    _ => []
                };
            }
            if (modPlayer.moltenArmorSet)
            {
                return item.type switch
                {
                    ItemID.MoltenHelmet => [
                        new(Mod, "Tooltip0", "20% increased melee damage"),
                        new(Mod, "Tooltip1", "Set bonus: All fire debuffs are upgraded to Hellfire, with increased damage and duration")
                    ],
                    ItemID.MoltenBreastplate => [
                        new(Mod, "Tooltip0", "Reduces damage taken by 20%"),
                        new(Mod, "Tooltip1", "Upon taking damage, all nearby enemies are lit on fire"),
                        new(Mod, "Tooltip2", "Set bonus: All fire debuffs are upgraded to Hellfire, with increased damage and duration")
                    ],
                    ItemID.MoltenGreaves => [
                        new(Mod, "Tooltip0", "10% increased movement speed"),
                        new(Mod, "Tooltip1", "Leaves a trail of flames in your wake that ignites enemies"),
                        new(Mod, "Tooltip2", "Set bonus: All fire debuffs are upgraded to Hellfire, with increased damage and duration")
                    ],
                    _ => []
                };
            }

            return item.type switch
            {
                ItemID.NinjaHood => [new(Mod, "Tooltip0", "15% increased damage")],
                ItemID.NinjaShirt => [new(Mod, "Tooltip0", "Allows the player to dash")],
                ItemID.NinjaPants => [new(Mod, "Tooltip0", "15% increased movement speed")],
                ItemID.JungleHat => [new(Mod, "Tooltip0", "20% increased magic damage")],
                ItemID.JungleShirt => [new(Mod, "Tooltip0", "Picking up mana stars reduces skil cooldowns by 1/2 second")],
                ItemID.JunglePants => [
                    new(Mod, "Tooltip0", "10% increased movement speed"),
                    new(Mod, "Tooltip1", "10% increased magic damage"),
                    ],
                ItemID.NecroHelmet => [new(Mod, "Tooltip0", "20% increased ranged damage")],
                ItemID.NecroBreastplate => [new(Mod, "Tooltip0", "Killing an enemy spawns baby spiders, which attack nearby enemies")],
                ItemID.NecroGreaves => [
                    new(Mod, "Tooltip0", "10% increased movement speed"),
                    new(Mod, "Tooltip1", "The last bullet in a magazine deals 50% more damage"),
                    ],
                ItemID.MoltenHelmet => [new(Mod, "Tooltip0", "20% increased melee damage")],
                ItemID.MoltenBreastplate => [
                    new(Mod, "Tooltip0", "Reduces damage taken by 20%"),
                    new(Mod, "Tooltip1", "Upon taking damage, all nearby enemies are lit on fire"),
                    ],
                ItemID.MoltenGreaves => [
                    new(Mod, "Tooltip0", "10% increased movement speed"),
                    new(Mod, "Tooltip1", "Leaves a trail of flames in your wake that ignites enemies"),
                    ],
                _ => [],
            };
        }

        public override bool OnPickup(Item item, Player player)
        {
            if (Main.LocalPlayer.GetModPlayer<ArmorPlayer>().jungleShirt && (item.type == ItemID.Star || item.type == ItemID.SoulCake || item.type == ItemID.SugarPlum))
            {
                foreach (KeyValuePair<int, SkillSlotData> slotInfo in SkillModPlayer.SkillSlots)
                {
                    if (slotInfo.Value.cooldownTimer > 0)
                    {
                        slotInfo.Value.cooldownTimer -= 30;
                    }
                }
            }
            return base.OnPickup(item, player);
        }
    }
}
