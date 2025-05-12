using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalBuffs {
    internal class Endurance : GlobalBuff {public override void ModifyBuffText(int type, ref string buffName, ref string tip, ref int rare) {
            if (type != BuffID.Endurance) {
                return;
            }
            tip = "10% reduced damage taken";
        }
    }
}
