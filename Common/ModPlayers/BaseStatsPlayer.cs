using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers
{
    public class BaseStatsPlayer : ModPlayer
    {
        public override void PostUpdateMiscEffects()
        {
            Player.manaRegenBonus = -80;
            if (Player.velocity.Length() > 0.01f)
            {
                Player.manaRegenBonus = -34;
            }
            base.PostUpdateMiscEffects();
        }
        public override void PostUpdate()
        {
            base.PostUpdate();
        }
    }
}
