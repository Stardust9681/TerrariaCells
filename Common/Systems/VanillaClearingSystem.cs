using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.Systems
{
    public class VanillaClearingSystem : ModSystem
	{
        static readonly HashSet<int> AccessoriesToClear = [
            ItemID.CelestialMagnet,
            ItemID.NaturesGift,
            ItemID.ArcaneFlower,
            ItemID.ManaRegenerationBand,
            ItemID.MagicCuffs,
            ItemID.StalkersQuiver,
            ItemID.AmmoBox,
            ItemID.ChlorophyteDye,
            ItemID.BallOfFuseWire,
            ItemID.SniperScope,
            ItemID.ReconScope,
            ItemID.BerserkerGlove,
            ItemID.SharkToothNecklace,
            ItemID.Nazar,
            ItemID.FeralClaws,
            ItemID.ThePlan,
            ItemID.ObsidianShield,
            ItemID.FrozenTurtleShell,
            ItemID.BandofRegeneration,
            ItemID.FastClock,
            ItemID.CelestialStone,
        ];
        public override void Load()
		{
            On_Player.GrantArmorBenefits += this.ClearAccessories;
        }
		//Technically they're automatically unloaded now, still prefer to manually unload
		public override void Unload()
		{
			On_Player.GrantArmorBenefits -= ClearAccessories;
		}

		private void ClearAccessories(On_Player.orig_GrantArmorBenefits orig, Player self, Item armorPiece)
		{
            if (!AccessoriesToClear.Contains(armorPiece.type))
                orig.Invoke(self, armorPiece);
        }
    }
}