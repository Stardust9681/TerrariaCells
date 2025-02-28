using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MonoMod;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.DataStructures;

namespace TerrariaCells.Common.ModPlayers
{
	public class HurtPlayer : ModPlayer
	{
        public int timeSinceLastHurt = 0;
        public override void PreUpdate()
        {
            timeSinceLastHurt++;
            base.PreUpdate();
        }
        public override void OnHurt(Player.HurtInfo info)
        {
            timeSinceLastHurt = 0;
            base.OnHurt(info);
        }
    }
}