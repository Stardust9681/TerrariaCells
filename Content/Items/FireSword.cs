using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace TerrariaCells.Content.Items 
{
	public class FireSword : ModItem
	{
		public override void SetDefaults() {
			Item.damage = 50;
			Item.width = 35;
			Item.useTime = 20;
			Item.useStyle = 1;
		}

		public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers mods) {
			if (target.HasBuff(BuffID.OnFire)) {
				mods.SourceDamage *= 2;
			}
		}
	}
}
