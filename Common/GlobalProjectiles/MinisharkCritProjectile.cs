using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.ID;
using Microsoft.Xna.Framework;
using TerrariaCells.Common.GlobalProjectiles;

namespace TerrariaCells.Common.GlobalProjectiles{
	public class MinisharkCritProjectile : GlobalProjectile {
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers mods) {
			if (projectile.GetGlobalProjectile<SourceGlobalProjectile>().itemSource.type == ItemID.Minishark) {
				double tilesForCrit = 3.0f;
				double coordsForCrit = tilesForCrit * 16;
				// 600 is the default projectile lifetime
				double timeAlive = 600 - projectile.timeLeft; // ticks
				double speedX = projectile.oldVelocity.X;
				double speedY = projectile.oldVelocity.Y;
				double speed = Math.Sqrt(speedX * speedX + speedY * speedY); // coords per tick
				if (timeAlive * speed < coordsForCrit)
					projectile.CritChance = 100;
			}
		}
	}
}
