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
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (Player.HasBuff(BuffID.MagicPower))
            {
                mult *= 0;
            }
        }
    }
}
