using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Localization;
using System.Collections.Generic;

namespace TerrariaCells.Common.GlobalItems
{
	public class AccessoryEffects : GlobalItem
	{
		public override void Load()
		{
			On_Player.ApplyEquipFunctional += On_Player_ApplyEquipFunctional;
		}
		public override void Unload()
		{
			On_Player.ApplyEquipFunctional -= On_Player_ApplyEquipFunctional;
		}

		public override void SetDefaults(Item item)
		{
			int[] NewAccessoryTypes = new int[] {
				ItemID.BallOfFuseWire, ItemID.ChlorophyteDye,
			};

			if (NewAccessoryTypes.Contains(item.type))
			{
				item.DefaultToAccessory(item.width, item.height);
				item.maxStack = 1;
			}

			switch (item.type)
			{
				case ItemID.ChlorophyteDye:
					item.SetNameOverride("Chlorophyte Coating");
					item.dye = 0;
					item.glowMask = -1;
					break;
				case ItemID.BallOfFuseWire:
					item.shoot = ProjectileID.None;
					item.buffType = 0;
					item.useAnimation = 0;
					item.useTime = 0;
					item.useStyle = ItemUseStyleID.None;
					item.UseSound = null;
					break;
				case ItemID.BerserkerGlove:
					item.defense = 0;
					break;
				case ItemID.ObsidianShield:
					item.defense = 6;
					break;
			}
			if (item.type == ItemID.ChlorophyteDye)
			{
				item.SetNameOverride("Chlorophyte Coating");
				item.dye = 0;
				item.glowMask = -1;
			}
			if (item.type == ItemID.BallOfFuseWire)
			{
				item.shoot = ProjectileID.None;
				item.buffType = 0;
				item.useAnimation = 0;
				item.useTime = 0;
				item.useStyle = ItemUseStyleID.None;
				item.UseSound = null;
			}
		}

		private void On_Player_ApplyEquipFunctional(On_Player.orig_ApplyEquipFunctional orig, Player player, Item item, bool hideVisual)
		{
			ModPlayers.AccessoryPlayer modPlayer = player.GetModPlayer<ModPlayers.AccessoryPlayer>();
			switch (item.type)
			{
				case ItemID.FastClock:
					modPlayer.fastClock = true;
					break;
				case ItemID.BandofRegeneration:
					modPlayer.bandOfRegen = true;
					break;
				case ItemID.FrozenTurtleShell:
					modPlayer.frozenShieldItem = item;
					break;
				case ItemID.ObsidianShield:
					player.noKnockback = true;
					break;
				case ItemID.ThePlan:
					modPlayer.thePlan = true;
					break;
				case ItemID.CelestialStone:
					modPlayer.celestialStone = true;
					break;

				case ItemID.FeralClaws:
					player.GetAttackSpeed(DamageClass.Melee) += 0.4f;
					break;
				case ItemID.Nazar:
					modPlayer.nazar = true;
					break;
				case ItemID.BerserkerGlove:
					modPlayer.bersGlove = true;
					break;

				case ItemID.ReconScope:
					modPlayer.reconScope = true;
					break;
				case ItemID.BallOfFuseWire:
					modPlayer.fuseKitten = true;
					break;
				case ItemID.ChlorophyteDye:
					modPlayer.chlorophyteCoating = true;
					break;
				//case ItemID.AmmoBox: break;
				case ItemID.StalkersQuiver:
					modPlayer.stalkerQuiver = true;
					break;

				case ItemID.ArcaneFlower:
					player.GetDamage(DamageClass.Magic) += 0.50f;
					player.manaCost += 0.5f;
					break;
				case ItemID.ManaRegenerationBand:
					player.manaRegenDelayBonus += 4f;
					player.manaRegenBonus += 50;
					break;
				case ItemID.NaturesGift:
					//I know suggestion is 25%, I'm going 33% because you're sacrificing SO MUCH for this boost to mana cost of all things
					player.manaCost -= 0.33f;
					break;
				case ItemID.MagicCuffs:
					modPlayer.magicCuffs = true;
					break;
				default:
					orig.Invoke(player, item, hideVisual);
					break;
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			int ttCounter = 0;
			void AddToolTip(string text)
			{
				if(ttCounter == 0)
                    tooltips.ForEach(x =>
                    {
                        if (x.Name.StartsWith("Tooltip") && x.Mod == "Terraria")
                        {
                            x.Hide();
                        }
                    });

                tooltips.Add(new TooltipLine(Mod, $"AccessoryTip{ttCounter++}", text));
            }
            switch (item.type)
            {
                case ItemID.Nazar:
                    AddToolTip("Melee attacks restore 20 mana");
                    break;
                case ItemID.ArcaneFlower:
                    AddToolTip("50% increased magic damage");
                    AddToolTip("50% increased mana cost");
                    break;
                case ItemID.FrozenTurtleShell:
                    AddToolTip("If you would die, instead survive with 1 HP");
                    AddToolTip("Consumed on use");
                    break;
                case ItemID.BallOfFuseWire:
                    AddToolTip("Explosions are much more powerful");
                    break;
                case ItemID.StalkersQuiver:
                    AddToolTip("Arrow attacks cause spectral arrows to attack the target");
                    break;
                case ItemID.BandofRegeneration:
                    AddToolTip("Killing an enemy restores 1% of your max HP");
                    break;
                case ItemID.FastClock:
                    AddToolTip("Killing an enemy increases your speed briefly");
                    break;
                case ItemID.ChlorophyteDye:
                    AddToolTip("Bullets and Arrows become coated in chlorophyte");
                    break;
                case ItemID.NaturesGift:
                    AddToolTip("25% reduced mana cost");
                    break;
                case ItemID.BerserkerGlove:
                    AddToolTip("4% increased damage on successive melee attacks");
                    break;
                case ItemID.FeralClaws:
                    AddToolTip("40% increased melee attack speed");
                    break;
                case ItemID.ThePlan:
                    AddToolTip("50% increased damage against healthy enemies");
                    break;
                case ItemID.ReconScope:
                    AddToolTip("30% increased damage when no enemies are nearby");
                    break;
				case ItemID.CelestialStone:
					AddToolTip("Critical Strikes reduce the cooldown of your abilities by 0.5 sec");
					break;
                default:
                    return;
            }
        }

        public IEnumerable<TooltipLine> GetTooltips(Item item)
        {
            return item.type switch
            {
				ItemID.CelestialStone => new TooltipLine[] {
					new TooltipLine(Mod, "Tooltip0", "Critical Strikes reduce the cooldown of your abilities by 0.5 sec"),
				},
                ItemID.Nazar => [new(Mod, "Tooltip0", "Melee attacks restore 20 mana")],
                ItemID.ArcaneFlower =>
                [
                    new(Mod, "Tooltip0", "50% increased magic damage"),
                    new(Mod, "Tooltip1", "50% increased mana cost"),
                ],
                ItemID.FrozenTurtleShell =>
                [
                    new(Mod, "Tooltip0", "If you would die, instead survive with 1 HP"),
                    new(Mod, "Tooltip1", "Consumed on use"),
                ],
                ItemID.BallOfFuseWire =>
                [
                    new(Mod, "Tooltip0", "Explosions are much more powerful"),
                ],
                ItemID.StalkersQuiver =>
                [
                    new(
                        Mod,
                        "Tooltip0",
                        "Arrow attacks cause spectral arrows to attack the target"
                    ),
                ],
                ItemID.BandofRegeneration =>
                [
                    new(Mod, "Tooltip0", "Killing an enemy restores 1% of your max HP"),
                ],
                ItemID.FastClock =>
                [
                    new(Mod, "Tooltip0", "Killing an enemy increases your speed briefly"),
                ],
                ItemID.ChlorophyteDye =>
                [
                    new(Mod, "Tooltip0", "Bullets and Arrows become coated in chlorophyte"),
                ],
                ItemID.NaturesGift => [new(Mod, "Tooltip0", "25% reduced mana cost")],
                ItemID.BerserkerGlove =>
                [
                    new(Mod, "Tooltip0", "4% increased damage on successive melee attacks"),
                ],
                ItemID.FeralClaws => [new(Mod, "Tooltip0", "40% increased melee attack speed")],
                ItemID.ThePlan =>
                [
                    new(Mod, "Tooltip0", "50% increased damage against healthy enemies"),
                ],
                ItemID.ReconScope =>
                [
                    new(Mod, "Tooltip0", "30% increased damage when no enemies are nearby"),
                ],
                ItemID.CelestialMagnet => [new(Mod, "Tooltip0", "Increased mana pickup range")],
                ItemID.ManaRegenerationBand =>
                [
                    new(Mod, "Tooltip0", "Significantly increased mana regeneration"),
                ],
                ItemID.MagicCuffs => [new(Mod, "Tooltip0", "Gain mana when taking damage")],
                ItemID.ObsidianShield =>
                [
                    new(Mod, "Tooltip0", "Immunity to knockback and fiery hot tiles"),
                ],
                _ => [],
            };
        }

		public override void GrabRange(Item item, Player player, ref int grabRange)
		{
			if ((item.type == ItemID.Star || item.type == ItemID.SoulCake || item.type == ItemID.SugarPlum) && player.manaMagnet)
				grabRange = 15 * 16;
		}
	}
}
