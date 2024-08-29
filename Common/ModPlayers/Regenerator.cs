using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.ResourceSets;
using ReLogic.Content;

namespace TerrariaCells.Common.ModPlayers
{
	//ModPlayer handling health and regeneration aspects
	public class Regenerator : ModPlayer
	{
		public override void Load()
		{
			On_ResourceDrawSettings.Draw += DrawRallyHealthBar;
		}
		public override void Unload()
		{
			On_ResourceDrawSettings.Draw -= DrawRallyHealthBar;
		}

		public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
		{
			if (mediumCoreDeath)
				return base.AddStartingItems(mediumCoreDeath);
			return new Item[]
				{
					new Item(Terraria.ID.ItemID.LesserHealingPotion, 2),
				};
		}

		#region Rally Heal Mechanic
		public const float STAGGER_POTENCY = 3f;
		public const float INV_STAGGER_POTENCY = 1f / STAGGER_POTENCY;
		public const string BAR_HEALTH_FILL1 = "Images\\UI\\PlayerResourceSets\\HorizontalBars\\HP_Fill";
		public const string BAR_HEALTH_FILL2 = "Images\\UI\\PlayerResourceSets\\HorizontalBars\\HP_Fill_Honey";

		private int damageBuffer;
		private int damageTime;
		private float antiRegen;

		private float TimeAmplitude => damageBuffer * INV_STAGGER_POTENCY; //Used for calculations, opposite of MaxTime
		private float MaxTime => damageBuffer * STAGGER_POTENCY; //Time it will take for damage to stop ticking.
		private int DamageLeft => (int)(-MathF.Sqrt(TimeAmplitude * damageTime) + damageBuffer); //Remaining amount of damage for the player to take

		//Mathematics used for Damage Staggering:
			//Damage Left = -sqrt(TimeAmplitude * damageTime) + damageBuffer
			//Damage per Tick = -TimeAmplitude / (2 * sqrt(TimeAmplitude * damageTime))
		//Damage Left approaches 0 when damageTime reaches MaxTime

		//Detour for Health Bar drawing
		//Handles health indicator for Rally Mechanic
		private void DrawRallyHealthBar(On_ResourceDrawSettings.orig_Draw orig, ref ResourceDrawSettings self, SpriteBatch spriteBatch, ref bool isHovered)
		{
			Player player = Main.LocalPlayer;
			Regenerator modPlayer = player.GetModPlayer<Regenerator>();
			if (modPlayer.damageBuffer <= 0)
			{
				orig.Invoke(ref self, spriteBatch, ref isHovered);
				return;
			}

			//Assuming that 20 segments is our cap here
			int segmentSize = player.statLifeMax2 <= 400 ? 20 : (player.statLifeMax2 <= 500 ? 25 : (Player.statLifeMax2 / 20));
			int endPoint = player.statLife - modPlayer.DamageLeft;
			if (endPoint < 0)
				endPoint = 0;
			int damageInSegment = endPoint % segmentSize;
			int damageSegmentIndex = endPoint / segmentSize;

			//Borrowed and adapted from Terraria
			int elementCount = self.ElementCount;
			Vector2 value = self.TopLeftAnchor;
			Point value2 = Main.MouseScreen.ToPoint();
			int elementIndex = self.ElementIndexOffset - 1;
			for (int i = 0; i < elementCount; i++)
			{
				elementIndex++;
				self.GetTextureMethod.Invoke(elementIndex, self.ElementIndexOffset, self.ElementIndexOffset + elementCount - 1, out Asset<Texture2D> asset, out Vector2 value3, out float scale, out Rectangle? rectangle);
				if (i==0)
				{
					if (!(asset.Name.Equals(BAR_HEALTH_FILL1) || asset.Name.Equals(BAR_HEALTH_FILL2)))
					{
						orig.Invoke(ref self, spriteBatch, ref isHovered);
						return;
					}
				}
				Rectangle rectangle2 = rectangle ?? asset.Frame(1, 1, 0, 0, 0, 0);
				Vector2 vector = value + value3;
				Vector2 vector2 = self.OffsetSpriteAnchor + rectangle2.Size() * self.OffsetSpriteAnchorByTexturePercentile;
				Rectangle rectangle3 = rectangle2;
				rectangle3.X += (int)(vector.X - vector2.X);
				rectangle3.Y += (int)(vector.Y - vector2.Y);
				if (rectangle3.Contains(value2))
				{
					isHovered = true;
				}

				if (elementIndex > damageSegmentIndex) //If entire segment would be lost to damage
				{
					float alpha = (MathF.Cos((float)Main.timeForVisualEffects * 0.314f) * 0.25f) + 0.75f;

					spriteBatch.Draw(asset.Value, vector, rectangle2, Color.White * alpha, 0f, vector2, scale, SpriteEffects.None, 0f);
				}
				else if (elementIndex == damageSegmentIndex) //If segment is split (ex: 20 health, take 10 damage, half of segment should flash)
				{
					int split = 12 - (damageInSegment * 12 / segmentSize);

					Rectangle sourceLeft = rectangle2;
					sourceLeft.Width = split - sourceLeft.X;

					if (sourceLeft.X == 0 && sourceLeft.Width == 1) //Prevents some weird overlapping problems
					{
						spriteBatch.Draw(asset.Value, vector, rectangle2, Color.White, 0f, vector2, scale, SpriteEffects.None, 0f);
					}
					else
					{
						Rectangle sourceRight = rectangle2;
						sourceRight.X = sourceLeft.X + sourceLeft.Width;
						sourceRight.Width = 12 - sourceRight.X;

						float alpha = (MathF.Cos((float)Main.timeForVisualEffects * 0.314f) * 0.25f) + 0.75f;

						spriteBatch.Draw(asset.Value, vector, sourceLeft, Color.White * alpha, 0f, vector2, scale, SpriteEffects.None, 0f);
						vector += sourceLeft.Size() * Vector2.UnitX;
						spriteBatch.Draw(asset.Value, vector, sourceRight, Color.White, 0f, vector2, scale, SpriteEffects.None, 0f);
					}
				}
				else //If segment is unaffected by rally mechanic
				{
					spriteBatch.Draw(asset.Value, vector, rectangle2, Color.White, 0f, vector2, scale, SpriteEffects.None, 0f);
				}
				value += self.OffsetPerDraw + (rectangle2.Size() * self.OffsetPerDrawByTexturePercentile);
			}
		}

		/// <summary>
		/// Set damage stagger to a flat amount. Will discard current amount.
		/// </summary>
		/// <param name="value"></param>
		internal void SetStaggerDamage(int value)
		{
			damageBuffer = value;
			if (damageBuffer < 0)
			{
				damageBuffer = 0;
			}
			damageTime = 0;
		}
		/// <summary>
		/// Adjust damage stagger by some amount +/-
		/// </summary>
		/// <param name="value"></param>
		internal void AdjustStaggerDamage(int value)
		{
			damageBuffer = DamageLeft + value;
			if (damageBuffer < 0)
			{
				damageBuffer = 0;
			}
			damageTime = 0;
		}

		public override void UpdateBadLifeRegen()
		{
			if (damageBuffer > 0)
			{
				UpdateDamageBuffer();
			}
		}
		//Run damage stagger calcs: split into its own function so it can be moved more easily or whatever.
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
			AdjustStaggerDamage(damageTaken);
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
				return;
			RallyHeal(damageDone);
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;
			RallyHeal(damageDone);
		}
		/// <summary>
		/// Handle heal proportion and effect for rally heal
		/// </summary>
		/// <param name="amount"></param>
		internal void RallyHeal(int amount)
		{
			if (damageBuffer > 0)
			{
				amount /= 2;
				Player.HealEffect(amount);
				AdjustStaggerDamage(-amount);
			}
		}
		#endregion

		#region Healing
		public override bool OnPickup(Item item)
		{
			float healthRecovery;
			switch (item.buffType)
			{
				case Terraria.ID.BuffID.WellFed:
					healthRecovery = 0.15f;
					break;
				case Terraria.ID.BuffID.WellFed2:
					healthRecovery = 0.3f;
					break;
				case Terraria.ID.BuffID.WellFed3:
					healthRecovery = 0.45f;
					break;
				default:
					return base.OnPickup(item);
			}
			int healing = (int)(healthRecovery * Player.statLifeMax2);
			healing -= DamageLeft;
			Player.Heal(healing);
			AdjustStaggerDamage(-healing);
			return false;
		}

		//Disable Health Potion CD
		//Game will play quickly, so we don't want 60 sec cooldowns getting in the way
		public override void PreUpdateBuffs()
		{
			Player.ClearBuff(Terraria.ID.BuffID.PotionSickness);
			Player.potionDelay = 0;
		}
		#endregion

		#region Disable Health Regen
		public override void NaturalLifeRegen(ref float regen)
		{
			regen = 0;
			Player.lifeRegenTime = 0;
		}

		public override void UpdateLifeRegen()
		{
			Player.lifeRegen = 0;
		}
		#endregion

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