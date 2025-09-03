using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers
{
    public class BuffPlayer : ModPlayer
    {
        public readonly Dictionary<int, int?> ReplaceBuffWith = new Dictionary<int, int?>();
        public override void ResetEffects()
        {
            foreach (int key in ReplaceBuffWith.Keys)
            {
                ReplaceBuffWith[key] = null;
            }
        }
        public int GetBuffToApply(int buffID, ref int time, ref int stacks)
        {
            if (ReplaceBuffWith.TryGetValue(buffID, out int? newID) && newID is not null)
            {
                switch ((buffID, newID))
                {
                    case (BuffID.OnFire, BuffID.OnFire3):
                        time = (int)(time * 1.5f);
                        break;
                }
                return newID.Value;
            }
            return buffID;
        }
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (Player.HasBuff(BuffID.MagicPower))
            {
                mult *= 0;
            }
        }
    }
}
