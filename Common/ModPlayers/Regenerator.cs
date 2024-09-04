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

using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Reflection;

namespace TerrariaCells.Common.ModPlayers
{
	//ModPlayer handling health and regeneration aspects
	public class Regenerator : ModPlayer
	{
		public override void Load()
		{
			//TODO: Change this to IL Edit
			// Better for crossmod compatability
			// Less redundant coding
			// This is technically improper use of detouring
			On_ResourceDrawSettings.Draw += DrawRallyHealthBar;

			IL_HorizontalBarsPlayerResourcesDisplaySet.Draw += IL_HorizontalBarsPlayerResourcesDisplaySet_Draw;
			IL_HorizontalBarsPlayerResourcesDisplaySet.LifeFillingDrawer += IL_HealthbarTextureSelect;
		}

		public override void Unload()
		{
			On_ResourceDrawSettings.Draw -= DrawRallyHealthBar;

			IL_HorizontalBarsPlayerResourcesDisplaySet.Draw -= IL_HorizontalBarsPlayerResourcesDisplaySet_Draw;
			IL_HorizontalBarsPlayerResourcesDisplaySet.LifeFillingDrawer -= IL_HealthbarTextureSelect;
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

		#region Healthbar
		public const int MAX_HEALTHBAR_SIZE = 800;

		private static void IL_HealthbarTextureSelect(ILContext context)
		{
			log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
			try
			{
				ILCursor cursor = new ILCursor(context);

				ILLabel? IL_001B = null; //IL Instruction 001B (by ilSpy)
				if (!cursor.TryGotoNext(MoveType.Before,
					i => i.MatchLdarg1(), //Int32 elementIndex
					i => i.MatchLdarg0(), //HorizontalBars..DisplaySet self
					i => i.MatchLdfld<HorizontalBarsPlayerResourcesDisplaySet>("_hpFruitCount"), //Int32 self::_hpFruitCount
					i => i.Match(OpCodes.Bge_S, out IL_001B))) //Branch to IL_001B if(elementIndex >= _hpFruitcount)
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				if (IL_001B == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_001B)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}

				cursor.Emit(OpCodes.Ldarg_1); //Int32 elementIndex
											  //bool function for if segment at [elementIndex] should be red or yellow
				cursor.EmitDelegate<Func<int, bool>>((int elementIndex) =>
				{
					Player player = Main.LocalPlayer;
					int segmentsX5WithBonusHealth = Math.Max((player.statLifeMax2 - MAX_HEALTHBAR_SIZE), 0);
					return (elementIndex * 5) >= segmentsX5WithBonusHealth;
				});

				//Create label we can navigate back to later
				// Note for the future: "DefineLabel()" creates a label with no target, thus we must use "MarkLabel(..)" to be able jump back to it
				ILLabel jumpBack = cursor.MarkLabel();

				if (!cursor.TryGotoNext(MoveType.Before,
					i => i.MatchLdarg(4), //Asset<Texture2D> sprite
					i => i.MatchLdarg0(), //HorizontalBars..DisplaySet self
					i => i.MatchLdfld<HorizontalBarsPlayerResourcesDisplaySet>("_hpFillHoney"), //Asset<Texture2D> self::_hpFillHoney
					i => i.MatchStindRef())) //Ref
				{
					//Error finding next set of instructions '\_("/)_/`
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}

				ILLabel honeyFill = cursor.MarkLabel(); //Create label to branch to for our condition
				cursor.GotoLabel(jumpBack, MoveType.Before); //Return to previous label
				cursor.Emit(OpCodes.Brfalse, honeyFill); //Branch to honey texture if previous function is true
				cursor.Emit(OpCodes.Br, IL_001B); //Skip vanilla logic >:3
			}
			catch (Exception x)
			{
				//Something went wrong! :O
				GetInstanceLogger().Error($"Something went wrong with IL Patch: {context.Method.Name}");
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
			}
		}

		private static void IL_HorizontalBarsPlayerResourcesDisplaySet_Draw(ILContext context)
		{
			log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
			try
			{
				ILCursor cursor = new ILCursor(context);

				if (!cursor.TryGotoNext(MoveType.After,
					i => i.MatchCall<HorizontalBarsPlayerResourcesDisplaySet>("PrepareFields"))) //invoke HorizontalBars..DisplaySet::PrepareFields()
				{
					//Couldn't find invokation
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}

				cursor.Emit(OpCodes.Ldarg_0); //HorizontalBars..DisplaySet self

				cursor.EmitDelegate((HorizontalBarsPlayerResourcesDisplaySet self) =>
				{
					Player localPlayer = Main.LocalPlayer;

					int numSegments = Math.Clamp(localPlayer.statLifeMax2 / 20, 5, MAX_HEALTHBAR_SIZE / 20);
					typeof(HorizontalBarsPlayerResourcesDisplaySet).GetField("_hpSegmentsCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, numSegments);
				});
			}
			catch (Exception x)
			{
				GetInstanceLogger().Error($"Something went wrong with IL Patch: {context.Method.Name}");
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
			}
		}

		private static void DrawRallyHealthBar(On_ResourceDrawSettings.orig_Draw orig, ref ResourceDrawSettings self, SpriteBatch spriteBatch, ref bool isHovered)
		{
			Player player = Main.LocalPlayer;
			Regenerator modPlayer = player.GetModPlayer<Regenerator>();
			if (modPlayer.damageBuffer <= 0)
			{
				orig.Invoke(ref self, spriteBatch, ref isHovered);
				return;
			}

			int segmentSize = player.statLifeMax2 <= MAX_HEALTHBAR_SIZE ? 20 : ((player.statLifeMax2 * 20) / MAX_HEALTHBAR_SIZE);
			if (segmentSize == 0)
				segmentSize = 1;
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
				if (i == 0)
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
		#endregion

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
			float incrementValue = (timeAmp / (2f * sqrt));
			antiRegen += incrementValue;
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