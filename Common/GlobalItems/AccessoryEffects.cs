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
		public override void UpdateAccessory(Item item, Player player, bool hideVisual)
		{
			switch (item.type)
			{
				case ItemID.FastClock: FastClock(player); break;
				case ItemID.BandofRegeneration: BandOfRegeneration(player); break;
				case ItemID.FrozenTurtleShell: FrozenShield(player, item); break;
				case ItemID.ObsidianShield: ObsidianShield(player); break;
				case ItemID.ThePlan: ThePlan(player); break;
				case ItemID.FeralClaws: FeralClaws(player); break;
				case ItemID.Nazar: Nazar(player); break;
				case ItemID.SharkToothNecklace: SharktoothNecklace(player); break;
				case ItemID.BerserkerGlove: BerserkerGlove(player); break;
				case ItemID.ReconScope: ReconScope(player); break;
				case ItemID.BallOfFuseWire: FuseKitten(player); break;
				case ItemID.ChlorophyteDye: ChlorophyteCoating(player); break;
				case ItemID.AmmoBox: AmmoBox(player); break;
				case ItemID.StalkersQuiver: StalkerQuiver(player); break;
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (item.type == ItemID.Nazar)
			{
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Melee attacks restore 20 mana ");
                tooltips.Add(line);
            }
            if (item.type == ItemID.ArcaneFlower)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Increases damage and mana cost by 50%");
                tooltips.Add(line);
            }
            if (item.type == ItemID.FrozenTurtleShell)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "If you would die, instead survive with 1 HP. Consumed on use");
                tooltips.Add(line);
            }
            if (item.type == ItemID.BallOfFuseWire)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Explosions are more powerful");
                tooltips.Add(line);
            }
            if (item.type == ItemID.StalkersQuiver)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Arrow attacks create spectral arrows to hunt the target");
                tooltips.Add(line);
            }
            if (item.type == ItemID.BandofRegeneration)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Killing an enemy restores 1% of your max HP");
                tooltips.Add(line);
            }
            if (item.type == ItemID.FastClock)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Killing an enemy provides a temporary speed buff");
                tooltips.Add(line);
            }
            if (item.type == ItemID.ChlorophyteDye)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Bullets and Arrows are covered with chlorophyte");
                tooltips.Add(line);
            }
            if (item.type == ItemID.NaturesGift)
            {
                tooltips.RemoveAll(x => x.Name.StartsWith("Tooltip") && x.Mod == "Terraria");

                TooltipLine line = new TooltipLine(Mod, "Tooltip0", "Reduces mana cost by 25% ");
                tooltips.Add(line);
            }
        }

		private void CelestialStone(Player player)
		{
			//var modPlayer = player.GetModPlayer<SkillPlayer>();
			//modPlayer.skillOne_cooldown -= 30;
			//modPlayer.skillTwo_cooldown -= 30;
		}

		private void FastClock(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().fastClock = true;
		}
		private void BandOfRegeneration(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().bandOfRegen = true;
		}
		private void FrozenShield(Player player, Item frozenShield)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().frozenShieldItem = frozenShield;
		}
		private void ObsidianShield(Player player)
		{
			player.statDefense += 4; //Obs Shield already gives +2 and KB Immunity
		}
		private void ThePlan(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().thePlan = true;
		}
		private void FeralClaws(Player player)
		{
			player.GetAttackSpeed(DamageClass.Melee) += 0.28f; //Feral Claw already gives +0.12f
		}
		private void Nazar(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().nazar = true;
		}
		private void SharktoothNecklace(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().sharktooth = true;
		}
		private void BerserkerGlove(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().bersGlove = true;
		}
		private void ReconScope(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().reconScope = true;
		}
		private void FuseKitten(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().fuseKitten = true;
		}
		private void ChlorophyteCoating(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().chlorophyteCoating = true;
		}
		private void AmmoBox(Player player)
		{
			//Need Weapons branch, and idk how they have this set up
		}
		private void StalkerQuiver(Player player)
		{
			player.GetModPlayer<ModPlayers.AccessoryPlayer>().stalkerQuiver = true;
		}

		public override void GrabRange(Item item, Player player, ref int grabRange)
		{
			if (player.manaMagnet)
				grabRange = 15 * 16;
		}
	}
}
