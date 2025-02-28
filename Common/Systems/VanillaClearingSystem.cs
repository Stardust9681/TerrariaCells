using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace TerrariaCells.Common.Systems {
    public class VanillaClearingSystem : ModSystem {
        static readonly HashSet<int> BuffsToClear = [
        ];
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
        public override void Load() {
            On_NPC.UpdateNPC_BuffSetFlags += this.ClearBuffs;
            On_Player.GrantArmorBenefits += this.ClearAccessories;
        }

        private void ClearAccessories(On_Player.orig_GrantArmorBenefits orig, Player self, Item armorPiece) {
            if (!AccessoriesToClear.Contains(armorPiece.type)) {
                orig(self, armorPiece);
            }
        }

        private void ClearBuffs(On_NPC.orig_UpdateNPC_BuffSetFlags orig, NPC self, bool lowerBuffTime) {
            for (int i = 0; i < NPC.maxBuffs; i++) {
                if (BuffsToClear.Contains(self.buffType[i])) {
                    // negative buffs are ignored by the original method
                    self.buffType[i] = -self.buffType[i];
                    if (lowerBuffTime) {
                        // this would get skipped inside the method otherwise
                        self.buffTime[i]--;
                    }
                }
            }
            orig(self, lowerBuffTime);
            // restore the original buffs after we've run orig
            for (int i = 0; i < NPC.maxBuffs; i++) {
                if (BuffsToClear.Contains(-self.buffType[i])) {
                    self.buffType[i] = -self.buffType[i];
                }
            }
        }
    }
}