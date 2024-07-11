using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace TerrariaCells.Common.ModPlayers
{
	//ModPlayer handling health and regeneration aspects
	public class Regenerator : ModPlayer
	{
		private const float STAGGER_POTENCY = 3f;
		private const float INV_STAGGER_POTENCY = 1f / STAGGER_POTENCY;

		private int damageBuffer;
		private int damageTime;
		private float antiRegen;

		public float TimeAmplitude => damageBuffer * INV_STAGGER_POTENCY;
		public float MaxTime => damageBuffer * STAGGER_POTENCY;
		private int DamageLeft => (int)(-MathF.Sqrt(TimeAmplitude * damageTime) + damageBuffer);

		//Mathematics used for Damage Staggering:
			//Damage Left = -sqrt(TimeAmplitude * damageTime) + damageBuffer
			//Damage per Tick = -TimeAmplitude / (2 * sqrt(TimeAmplitude * damageTime))
		//Damage Left approaches 0 when damageTime reaches MaxTime

		public void SetStaggerDamage(int value)
		{
			damageBuffer = value;
			if (damageBuffer < 0)
			{
				damageBuffer = 0;
			}
			damageTime = 0;
		}
		public void AdjustStaggerDamage(int value)
		{
			damageBuffer = DamageLeft + value;
			if (damageBuffer < 0)
			{
				damageBuffer = 0;
			}
			damageTime = 0;
		}

		//Disable health regeneration
		public override void NaturalLifeRegen(ref float regen)
		{
			regen = 0;
		}

		public override void UpdateBadLifeRegen()
		{
			if (damageBuffer > 0)
			{
				UpdateDamageBuffer();
			}
		}
		//Run damage stagger calcs
		private void UpdateDamageBuffer()
		{
			damageTime++;
			float timeAmp = TimeAmplitude;
			float sqrt = MathF.Sqrt(timeAmp * damageTime);
			antiRegen += (timeAmp / (2f * sqrt));
			int lifeDamage = (int)MathF.Floor(antiRegen);
			if (lifeDamage != 0)
			{
				Player.statLife -= lifeDamage;
				antiRegen -= lifeDamage;
				CheckDead();
			}
			if (damageTime > MaxTime)
			{
				//Assume antiRegen will never be 0, knock off 1 extra health to mark what would have been
				antiRegen = 0;
				Player.statLife--;
				CheckDead();
				damageTime = 0;
				damageBuffer = 0;
			}
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			int damageTaken = info.Damage;
			Player.statLife += damageTaken; //Prevent player from taking instant direct damage
			if (damageTaken > Player.statLife)
			{
				//Oneshot protection
				int oneShotTolerance = (int)(Player.statLifeMax2 * 0.02f);
				if (Player.statLife > Player.statLifeMax2 - oneShotTolerance)
				{
					Player.statLife = Main.rand.Next(1, oneShotTolerance);
					SetStaggerDamage(0);
					return;
				}
			}
			AdjustStaggerDamage(damageTaken+1);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (damageBuffer > 0)
			{
				AdjustStaggerDamage(-damageDone / 2); //Health/rally recovery
			}
		}

		//Make sure player dies when they hit <=0 health
		private void CheckDead(PlayerDeathReason reason = null)
		{
			if (Player.statLife <= 0)
			{
				reason ??= PlayerDeathReason.ByCustomReason($"{Player.name} was beheaded.");

				Player.KillMe(reason, 1, 0);
			}
		}
	}
}
