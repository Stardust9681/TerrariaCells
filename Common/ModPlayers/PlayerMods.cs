using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace TerrariaCells.Common.ModPlayers
{
	//Not sure what to name this one
	//Some really basic stuff, movement (fall damage immunity, cloud jump), and removing certain buffs (namely, from nearby tiles)
	public class PlayerMods : ModPlayer
	{
		public override void ResetEffects()
		{
			Player.buffImmune[BuffID.MonsterBanner] = true;
			Player.buffImmune[BuffID.Sunflower] = true;
			Player.buffImmune[BuffID.Campfire] = true;
			Player.buffImmune[BuffID.Honey] = true;
			Player.buffImmune[BuffID.HeartLamp] = true;
			Player.buffImmune[BuffID.StarInBottle] = true;
			Player.buffImmune[BuffID.CatBast] = true;
			Player.buffImmune[BuffID.PeaceCandle] = true;
			Player.buffImmune[BuffID.WaterCandle] = true;
			Player.buffImmune[BuffID.ShadowCandle] = true;

			Player.noFallDmg = true;

            if(Player.GetModPlayer<MetaPlayer>().CloudJump)
			    Player.GetJumpState(VanillaExtraJump.CloudInABottle).Enable();
		}
	}
}
