using System;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

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
				case ItemID.FrozenShield: FrozenShield(player, item); break;
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
