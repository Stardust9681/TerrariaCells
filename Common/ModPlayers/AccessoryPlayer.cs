using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using MonoMod;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Terraria.DataStructures;

namespace TerrariaCells.Common.ModPlayers
{
	public class AccessoryPlayer : ModPlayer
	{
		public bool fastClock; //+30% speed on killing an enemy
		private int fastClockTimer = 0;
		public bool bandOfRegen; //+1% health on killing an enemy
		private bool frozenShield; //Saved from lethal damage once (consumed on use)
		public Item? frozenShieldItem;
		public bool thePlan; //+50% damage vs enemies with >90% hp
		public bool nazar; //+20 mana on melee hit
		public bool sharktooth; //Inflict bleed on hit
		public bool bersGlove; //+4% damage for "consecutive" melee hits
		private int bersTimer;
		private int bersCounter;
		public bool reconScope; //+30% damage when no enemies nearby
		public bool fuseKitten; //[Unused] Extra rocket explosion damage/radius
		public bool chlorophyteCoating; //[Unused] Bullets and arrows become chlorophyte
		public bool stalkerQuiver; //Summons a spectral arrow to hit targets hit by your arrows (deals 50% original damage)

		public override void Load()
		{
			IL_Player.OnHurt_Part2 += IL_Player_OnHurt_Part2;
			//IL_Player.ApplyEquipFunctional += IL_Player_ApplyEquipFunctional;
		}
		public override void Unload()
		{
			IL_Player.OnHurt_Part2 -= IL_Player_OnHurt_Part2;
			//IL_Player.ApplyEquipFunctional -= IL_Player_ApplyEquipFunctional;
		}
		private void IL_Player_OnHurt_Part2(ILContext context)
		{
			log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
			try
			{
				ILCursor cursor = new ILCursor(context);

				//===== NOTE =====
				//Magic Cuffs needs to either be IL or Detour (no orig), due to them restoring mana on hit in vanilla
				//================

				ILLabel? IL_0176 = null; //IL instruction 0176 (by ilSpy)
				if (!cursor.TryGotoNext(
					MoveType.After,
					n => n.MatchLdarg0(), //Player self
					n => n.MatchLdfld<Player>("magicCuffs"), //bool self::magicCuffs
					n => n.Match(OpCodes.Brfalse_S, out IL_0176) //Branch to IL_0176 if(!magicCuffs)
					))
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				if (IL_0176 == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_0176)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}

				cursor.Emit(OpCodes.Ldarg_0); //Player self
				cursor.EmitDelegate<Action<Player>>((Player player) =>
				{
					player.statMana += 100;
					if (player.statMana > player.statManaMax2)
						player.statMana = player.statManaMax2;
					player.ManaEffect(100);
				});
				cursor.Emit(OpCodes.Br, IL_0176); //Branch to IL_0176 (skip vanilla logics)
			}
			catch (Exception x)
			{
				//Something went wrong! :O
				GetInstanceLogger().Error($"Something went wrong with IL Patch: {context.Method.Name}");
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
			}
		}
		//[Unused]
		private void IL_Player_ApplyEquipFunctional(ILContext context)
		{
			log4net.ILog GetInstanceLogger() => ModContent.GetInstance<TerrariaCells>().Logger;
			try
			{
				ILCursor cursor = new ILCursor(context);

				//End of switch(currentItem.type)
				ILLabel? IL_0543 = null; //IL instruction 0543 (by ilSpy)
				#region Arcane Flower
				if (!cursor.TryGotoNext(
					MoveType.Before,
					n => n.MatchLdarg0(),
					n => n.MatchLdcI4(1),
					n => n.MatchStfld<Player>("manaFlower"),
					n => n.MatchLdarg0(),
					n => n.MatchLdarg0(),
					n => n.MatchLdfld<Player>("manaCost"),
					n => n.MatchLdcR4(0.08f),
					n => n.MatchSub(),
					n => n.MatchStfld<Player>("manaCost"),
					n => n.MatchLdarg0(),
					n => n.MatchLdarg0(),
					n => n.MatchLdfld<Player>("aggro"),
					n => n.MatchLdcI4(400),
					n => n.MatchSub(),
					n => n.MatchStfld<Player>("aggro"),
					n => n.MatchBr(out IL_0543)))
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				if (IL_0543 == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_0543)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}
				#endregion
				ILLabel sw_ArcaneFlower = cursor.MarkLabel();

				ILLabel? IL_0DAB = null; //IL instruction 0DAB (by ilSpy) (0-dab moment)
				#region Mana Regeneration Band
				if (!cursor.TryGotoNext(
					MoveType.After,
					n => n.MatchLdarg1(),
					n => n.MatchLdfld<Item>("type"),
					n => n.MatchLdcI4(ItemID.ManaRegenerationBand),
					n => n.Match(OpCodes.Bne_Un_S, out IL_0DAB)))
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				if (IL_0DAB == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_0DAB)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}
				#endregion
				ILLabel if_ManaRegenBand = cursor.MarkLabel();

				ILLabel? IL_11CE = null; //IL instruction 11CE (by ilSpy)
				#region Nature's Gift
				if (!cursor.TryGotoNext(
					MoveType.After,
					n => n.MatchLdarg1(),
					n => n.MatchLdfld<Item>("type"),
					n => n.MatchLdcI4(ItemID.NaturesGift),
					n => n.Match(OpCodes.Bne_Un_S, out IL_11CE)))
				{
					//Couldn't match given instructions, perform no further edits
					GetInstanceLogger().Error($"Couldn't match IL Patch: {context.Method.Name} @ {cursor.Index}");
					return;
				}
				if (IL_11CE == null)
				{
					//Matched correctly but didn't get Label ???
					GetInstanceLogger().Error($"IL Label {nameof(IL_11CE)} not found in IL Patch {context.Method.Name} @ {cursor.Index}");
					return;
				}
				#endregion
				ILLabel if_NaturesGift = cursor.MarkLabel();

				cursor.GotoLabel(sw_ArcaneFlower);
				cursor.EmitLdarg0();
				cursor.EmitDelegate<Action<Player>>((Player player) =>
				{
					player.manaCost += 0.5f;
					player.GetDamage(DamageClass.Magic) += 0.5f;
				});
				cursor.EmitBr(IL_0543);

				cursor.GotoLabel(if_ManaRegenBand);
				cursor.EmitLdarg0();
				cursor.EmitDelegate<Action<Player>>((Player player) =>
				{
					player.statManaMax2 += 20;
					//Basically doubled the regen boost that Mana Regeneration Band gives
					player.manaRegenDelayBonus += 4f;
					player.manaRegenBonus += 50;
				});
				cursor.EmitBr(IL_0DAB);

				cursor.GotoLabel(if_NaturesGift);
				cursor.EmitLdarg0();
				cursor.EmitDelegate<Action<Player>>((Player player) =>
				{
					player.manaCost -= 0.33f; //I know suggestion is 25%, I'm going 33% because you're sacrificing SO MUCH for this boost to mana cost of all things
				});
				cursor.EmitBr(IL_11CE);
			}
			catch (Exception x)
			{
				//Something went wrong! :O
				GetInstanceLogger().Error($"Something went wrong with IL Patch: {context.Method.Name}");
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), context);
			}
		}

		public override void ResetEffects()
		{
			if (!fastClock)
				fastClockTimer = 0;
			if (!bersGlove)
			{
				bersCounter = 0;
				bersTimer = 0;
			}
			if (bersTimer > 0) bersTimer--;
			else bersCounter = 0;
			if (Player.immuneTime < 1)
				frozenShield = false;
			else if (frozenShield)
			{
				Player.iceBarrier = true;
				Lighting.AddLight(Player.Center, 0.1f, 0.2f, 0.45f);
				Player.iceBarrierFrameCounter++;
				if (Player.iceBarrierFrameCounter > 2)
				{
					Player.iceBarrierFrameCounter = 0;
					Player.iceBarrierFrame++;
					if (Player.iceBarrierFrame >= 12)
					{
						Player.iceBarrierFrame = 0;
					}
				}
			}
			fastClock = false;
			bandOfRegen = false;
			frozenShieldItem = null;
			thePlan = false;
			nazar = false;
			sharktooth = false;
			bersGlove = false;
			reconScope = false;
			fuseKitten = false;
			chlorophyteCoating = false;

			stalkerQuiver = false;
		}
		public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (chlorophyteCoating)
			{
				if (GlobalProjectiles.AccessoryProjModifier.IsAnArrow(type))
					type = ProjectileID.ChlorophyteArrow;
				if (GlobalProjectiles.AccessoryProjModifier.IsABullet(type))
					type = ProjectileID.ChlorophyteBullet;
			}
		}
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
		{
			if (frozenShieldItem != null)
			{
				Player.statLife = 1;
				playSound = false;
				genDust = false;
				Player.immuneTime = 4 * 60; //4 sec
				frozenShield = true;
				int itemIndex = -1;
				foreach (Item item in Player.armor[13..19])
				{
					itemIndex++;
					if (item.Equals(frozenShieldItem))
						break;
				}
				frozenShieldItem.TurnToAir();
				Player.UpdateVisibleAccessory(itemIndex, frozenShieldItem);
				int dustCount = 14 + Main.rand.Next(7);
				for (int i = 0; i < dustCount; i++)
				{
					Dust dust = Dust.NewDustDirect(Player.Center + (Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 8), 1, 1, DustID.FrostHydra);
					dust.noGravity = true;
					dust.velocity = Player.Center.DirectionTo(dust.position) * Main.rand.NextFloat(3f, 5f);
					dust.scale = Main.rand.NextFloat(0.9f, 1.1f);
				}
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27);
				Player.GetModPlayer<Regenerator>().SetStaggerDamage(0);
				return false;
			}
			return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genDust, ref damageSource);
		}
		public override void PostUpdateBuffs()
		{
			if (fastClockTimer > 0)
			{
				fastClockTimer--;
				Player.moveSpeed += 0.3f;
			}
			if (reconScope)
			{
				bool anyNearbyEnemies = false;
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					if (!Main.npc[i].active) continue;
					NPC npc = Main.npc[i];
					if (MathF.Abs(npc.position.X - Player.position.X) + MathF.Abs(npc.position.X - Player.position.X) < 6 * 16)
					{
						anyNearbyEnemies = true;
						break;
					}
				}
				if (!anyNearbyEnemies)
					Player.GetDamage(DamageClass.Generic) += 0.3f;
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (thePlan && modifiers.DamageType.CountsAsClass(DamageClass.Melee))
			{
				if (target.life > (int)(target.lifeMax * 0.9f)) modifiers.SourceDamage += 0.5f;
			}
			if (bersGlove)
			{
				modifiers.FinalDamage += (bersCounter * 0.04f);
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (stalkerQuiver)
			{
				if (proj.arrow && proj.type != ProjectileID.PhantasmArrow)
				{
					Vector2 pos = target.Center + (Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 320f);
					Projectile newProj = Projectile.NewProjectileDirect(proj.GetSource_OnHit(target), pos, pos.DirectionTo(target.Center) * 4f, ProjectileID.PhantasmArrow, damageDone / 2, 0f, proj.owner, target.whoAmI);
					//newProj.tileCollide = false;
				}
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target.life - damageDone < 1) OnKill(target, hit, damageDone);
			if (hit.DamageType.CountsAsClass(DamageClass.Melee))
			{
				if (nazar)
				{
					Player.statMana += 20;
					Player.ManaEffect(20);
				}
				if (sharktooth)
				{
					//Still need Bleed
					target.AddBuff(BuffID.Bleeding, 360); //Replace with modded debuff? 6 sec duration.
				}
				if (bersGlove)
				{
					bersCounter++;
					if (bersCounter > 50)
						bersCounter = 50;
					bersTimer = 150; //2.5 sec
				}
			}
		}

		private void OnKill(NPC npc, NPC.HitInfo hit, int damage)
		{
			if(fastClock) fastClockTimer = 5 * 60;
			if (bandOfRegen) Player.Heal((int)(Player.statLifeMax2 * 0.01f));
		}
	}
}
