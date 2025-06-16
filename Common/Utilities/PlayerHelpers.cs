using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariaCells.Common.Utilities
{
    public static class PlayerHelpers
    {
        public static bool IsNewWorld(this Terraria.Player player)
        {
            for (int i = 0; i < 200; i++)
            {
                if (player.spN[i] is null) return true;

                if (player.spN[i]?.Equals(Terraria.Main.worldName) == true && player.spI[i] == Terraria.Main.worldID) return false;
            }
            return true;
        }
    }
}
