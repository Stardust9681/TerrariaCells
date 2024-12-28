using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using TerrariaCells.Common.ModPlayers;

namespace TerrariaCells.Common.GlobalTiles {
	public class ClickcableHeartsGlobalTile : GlobalTile {
		public override void RightClick(int i, int j, int type) {
			if (type == TileID.Heart) {
				WorldGen.KillTile(i, j);
				Main.player[Main.myPlayer].GetModPlayer<LifeModPlayer>().extraHealth += 20;
			} else {
				base.RightClick(i, j, type);
			}

		}
		public override bool CanDrop(int i, int j, int type) {
			if (type == TileID.Heart) {
				return false;
			} else {
				return base.CanDrop(i, j, type);
			}
		}
	}
}
