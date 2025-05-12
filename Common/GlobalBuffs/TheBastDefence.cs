using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalBuffs {
    internal class TheBastDefence : GlobalBuff {
        public override void Load() {
            // Hide the buff timer (because this is an unlimited-time buff)
            Main.buffNoTimeDisplay[BuffID.CatBast] = true;
            // Make it not a debuff (only real effect is that you
            // can choose to cancel it, but whatever)
            Main.debuff[BuffID.CatBast] = false;
        }

        public override void Update(int type, Player player, ref int buffIndex) {
            if (type != BuffID.CatBast) {
                return;
            }
            player.statDefense -= 5; // undo Vanilla
            player.endurance += 0.2f; // 20% DR
            player.buffTime[buffIndex] += 1; // make sure it never runs out
        }

        public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare) {
            if (type != BuffID.CatBast) {
                return;
            }
            buffName = "The Bast Defence"; // Vindictive Brit attack >:)
            tip = "20% reduced damage taken";
        }
    }
}
