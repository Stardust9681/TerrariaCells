using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace TerrariaCells.Common.ModPlayers
{
	public class MovementPlayer : ModPlayer
	{
		public override void ResetEffects()
		{
			Player.noFallDmg = true;
			//Un-comment if we want to give player Cloud Jump
			//Player.GetJumpState(VanillaExtraJump.CloudInABottle).Enable();
		}
	}
}
