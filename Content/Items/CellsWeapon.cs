using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TerrariaCells.Content.Items
{
	//A lot of the DC weapons seemed to have charge up effects, going to use this for that
	//Base class for mod weapons
	public abstract class CellsWeapon : ModItem
	{
		protected int chargeMin;
		protected int chargeMax;

		protected virtual void SafeSetDefaults() { }
		public override void SetDefaults()
		{
			SafeSetDefaults();
			Item.channel = true;
			Item.autoReuse = true;
		}

		protected virtual bool SafeCanUseItem(Player player) { return true; }
		public override bool CanUseItem(Player player)
		{
			bool val = SafeCanUseItem(player);
			if(val)
				player.GetModPlayer<Common.ModPlayers.WeaponsManager>().UseItem = this;
			return val;
		}
	}
}
