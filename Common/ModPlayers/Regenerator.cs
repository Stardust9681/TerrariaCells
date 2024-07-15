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
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.CodeAnalysis.Text;

namespace TerrariaCells.Common.ModPlayers
{
	//ModPlayer handling health and regeneration aspects
	public class Regenerator : ModPlayer
	{
		public const float STAGGER_POTENCY = 3f;
		public const float INV_STAGGER_POTENCY = 1f / STAGGER_POTENCY;
		public const string BAR_HEALTH_FILL1 = "Images\\UI\\PlayerResourceSets\\HorizontalBars\\HP_Fill";
		public const string BAR_HEALTH_FILL2 = "Images\\UI\\PlayerResourceSets\\HorizontalBars\\HP_Fill_Honey";

		private int damageBuffer;
		private int damageTime;
		private float antiRegen;

		private float TimeAmplitude => damageBuffer * INV_STAGGER_POTENCY;
		private float MaxTime => damageBuffer * STAGGER_POTENCY;
		private int DamageLeft => (int)(-MathF.Sqrt(TimeAmplitude * damageTime) + damageBuffer);

		//Mathematics used for Damage Staggering:
		//Damage Left = -sqrt(TimeAmplitude * damageTime) + damageBuffer
		//Damage per Tick = -TimeAmplitude / (2 * sqrt(TimeAmplitude * damageTime))
		//Damage Left approaches 0 when damageTime reaches MaxTime

		public override void Load()
		{
			On_ResourceDrawSettings.Draw += On_ResourceDrawSettings_Draw;
		}
		public override void Unload()
		{
			On_ResourceDrawSettings.Draw -= On_ResourceDrawSettings_Draw;
		}

		//Detour for Health Bar drawing
		//Handles health indicator for Rally Mechanih
		private void On_ResourceDrawSettings_Draw(On_ResourceDrawSettings.orig_Draw orig, ref ResourceDrawSettings self, SpriteBatch spriteBatch, ref bool isHovered)
		{
			Player player = Main.LocalPlayer;
			Regenerator modPlayer = player.GetModPlayer<Regenerator>();
			if (modPlayer.damageBuffer <= 0)
			{
				orig.Invoke(ref self, spriteBatch, ref isHovered);
				return;
			}

			//Assuming that 20 segments is our cap here
			int segmentSize = player.statLife <= 400 ? 20 : (player.statLife <= 500 ? 25 : (Player.statLife / 20));
			int endPoint = player.statLife - modPlayer.DamageLeft;
			if (endPoint < 0)
				endPoint = 0;
			int damageInSegment = endPoint % segmentSize;
			int damageSegmentIndex = endPoint / segmentSize;

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

				if (elementIndex > damageSegmentIndex)
				{
					float alpha = (MathF.Cos((float)Main.timeForVisualEffects * 0.314f) * 0.25f) + 0.75f;

					spriteBatch.Draw(asset.Value, vector, rectangle2, Color.White * alpha, 0f, vector2, scale, SpriteEffects.None, 0f);
				}
				else if (elementIndex == damageSegmentIndex)
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
				else
				{
					spriteBatch.Draw(asset.Value, vector, rectangle2, Color.White, 0f, vector2, scale, SpriteEffects.None, 0f);
				}
				value += self.OffsetPerDraw + (rectangle2.Size() * self.OffsetPerDrawByTexturePercentile);
			}
		}

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
		//Run damage stagger calcs : split into its own function so it can be moved more easily or whatever.
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
		internal void RallyHeal(int amount)
		{
			if (damageBuffer > 0)
			{
				amount /= 2;
				Player.HealEffect(amount);
				AdjustStaggerDamage(-amount);
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