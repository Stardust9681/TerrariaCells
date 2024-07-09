using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace TerrariaCells.Content.Projectiles
{
	//Going to go for heldproj type weapons, using this as the base class.
	//Noted that a lot of the DC equipment tends to be chargeup, which leads itself well for heldproj
	public abstract class CellsProjectile : ModProjectile
	{
		public Player Owner => Main.player[Projectile.owner];
		public int Charge
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		protected virtual void SafeAI() { }
		public sealed override void AI()
		{
			SafeAI();
			Player owner = Owner;
			if (owner.channel)
			{
				Projectile.timeLeft = 2;
				float attackSpeed = owner.GetWeaponAttackSpeed(owner.HeldItem);
				if (attackSpeed == 0)
					attackSpeed = 0.05f;
				owner.itemTime = (int)(owner.itemTimeMax / attackSpeed) + 2;
				owner.itemAnimation = (int)(owner.itemAnimationMax / attackSpeed) + 2;
				if (owner.itemTime < 2)
					owner.itemTime = 2;
				if (owner.itemAnimation < 2)
					owner.itemAnimation = 2;
				owner.heldProj = Projectile.whoAmI;
			}
			Charge++;
		}
	}
}
