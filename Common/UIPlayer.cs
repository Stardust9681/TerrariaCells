using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace TerrariaCells.Common
{
	public class UIPlayer : ModPlayer
	{
		public override void HideDrawLayers(PlayerDrawSet drawInfo)
		{
			PlayerDrawLayers.CaptureTheGem.Hide();
		}
	}
}
