using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers 
{
	public class LifeModPlayer : ModPlayer {
		public int extraHealth;
		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			StatModifier mod = new();
			mana = mod;
			mod.Flat = (float)extraHealth;
			health = mod;
		}
	}
}
