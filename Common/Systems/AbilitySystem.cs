using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.UI;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.GameInput;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using MonoMod.Cil;

using TerrariaCells.Common.Utilities;
using static TerrariaCells.Common.Utilities.NumberHelpers;
using static TerrariaCells.Common.Systems.AbilityConditions;

//Genuinely, I'm just using this for anything that I deem sufficiently complex
//Involving multiple parts working together to form one collective piece (a "system of parts" if you will)
namespace TerrariaCells.Common.Systems
{
	//Table of contents because I didn't feel like splitting this up:

	///<see cref="Ability"/> -- structure responsible for loading and storing ability information (Type, Cooldown, Duration, Restrictions)
	///<see cref="PlayerAbility"/> -- structure responsible for tracking ability cooldowns per Player Ability Slot
	///<see cref="AbilityConditions"/> -- Similar to <see cref="Terraria.WorldBuilding.Shapes"/>, contains ability conditions
	///<see cref="AbilityHandler"/> -- ModPlayer responsible for ability handling and updates.
	///<see cref="AbilityEdits"/> -- Detours/IL edits. Also item stats and tooltips.

	//If you want to refactor this, yourself, go for it. Want to split this up into multiple files, redo code, etc, go nuts.
	//Please be sure to use provided player instance in methods when applicable (over 'Main.LocalPlayer')
	//Please be sure to check use of static accessors

	public class Ability
	{
		public static void LoadAbilities()
		{
			RegisterAbility(ItemID.ClingerStaff, new Ability(NumberHelpers.SecToFrames(4) + 30, 10.SecToFrames(), new LineOfSight(), new InSolidTile().Invert()));
			//Doesn't actually get used due to -1 duration (never gets started)...
			//But here's an example of adding extra functionality on ability end.
			RegisterAbility(ItemID.ToxicFlask, new Ability(NumberHelpers.SecToFrames(30)))
				.OnStopAbility += (Ability a, Player p, Item i) => {
					foreach (Projectile proj in Main.ActiveProjectiles)
						if (new int[] { ProjectileID.SporeGas, ProjectileID.SporeGas2, ProjectileID.SporeGas3 }.Contains(proj.type))
							proj.Kill();
				};
			RegisterAbility(ItemID.StaffoftheFrostHydra, new Ability(NumberHelpers.SecToFrames(30), 10.SecToFrames()));
			RegisterAbility(ItemID.DD2ExplosiveTrapT1Popper, new Ability(NumberHelpers.SecToFrames(30), 30.SecToFrames(), new LineOfSight(), new InSolidTile().Invert()));
			RegisterAbility(ItemID.MolotovCocktail, new Ability(NumberHelpers.SecToFrames(15)));
			RegisterAbility(ItemID.BouncyDynamite, new Ability(NumberHelpers.SecToFrames(60)));
			//RegisterAbility(ItemID.BouncingShield, new Ability(NumberHelpers.SecToFrames(6)));
			RegisterAbility(ItemID.MedusaHead, new Ability(NumberHelpers.SecToFrames(30)));
			RegisterAbility(ItemID.SnowballLauncher, new Ability(NumberHelpers.SecToFrames(20), 8.SecToFrames()));
			RegisterAbility(ItemID.ManaPotion, new Ability(NumberHelpers.SecToFrames(30)));
			RegisterAbility(ItemID.WrathPotion, new Ability(NumberHelpers.SecToFrames(80), 20.SecToFrames()));
			RegisterAbility(ItemID.MagicPowerPotion, new Ability(NumberHelpers.SecToFrames(60), 20.SecToFrames()));
			RegisterAbility(ItemID.SwiftnessPotion, new Ability(NumberHelpers.SecToFrames(60), 20.SecToFrames()));
		}

		/// <summary>
		/// <para>Keys = <c>Item Type</c></para>
		/// <para>Values = <c>Ability</c></para>
		/// </summary>
		public static IReadOnlyDictionary<int, Ability> AbilityList => _AbilityRegistry;
		private static Dictionary<int, Ability> _AbilityRegistry = new Dictionary<int, Ability>();
		internal static Ability RegisterAbility(int itemID, Ability ability)
		{
			if (_AbilityRegistry.ContainsKey(itemID))
			{
				ModContent.GetInstance<TerrariaCells>().Logger.Warn($"Key 'itemID' ({itemID}) already has an associated ability");
				return default(Ability);
			}
			_AbilityRegistry.Add(itemID, ability);
			return _AbilityRegistry[itemID];
		}
		public static bool IsAbility(int itemID) => _AbilityRegistry.ContainsKey(itemID);



		public readonly int Cooldown;
		public readonly int Duration;
		public IReadOnlyList<AbilityCondition> Conditions;

		public delegate void On_StopAbility(Ability self, Player player, Item sItem);
		public event On_StopAbility OnStopAbility;

		internal Ability(int cooldown, int duration = -1, params AbilityCondition[] conditions)
		{
			this.Cooldown = cooldown;
			this.Duration = duration;
			this.Conditions = conditions.ToList();
		}

		public bool MeetsConditions(Player player)
		{
			if (!Conditions.Any()) return true;
			return Conditions.All(x => x.ConditionsMet(player));
		}
		internal void StopAbility(Player player, Item sItem)
		{
			if (sItem.shoot != ProjectileID.None)
			{
				int timeLeft = -1;
				int projIndex = -1;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];

					//Skip inactive
					if (!proj.active) continue;
					//Skip other-player owned
					if (proj.owner != player.whoAmI) continue;
					//Skip non-matching
					if (proj.type != sItem.shoot) continue;

					//Find oldest (in case we want to allow multiple of the same ability to be used)
					if (proj.timeLeft < timeLeft || timeLeft == -1)
					{
						projIndex = i;
						timeLeft = proj.timeLeft;
					}
				}

				if (projIndex != -1)
				{
					//Handle minions
					if (sItem.buffType != 0 && player.HasBuff(sItem.buffType))
					{
						player.DelBuff(player.FindBuffIndex(sItem.buffType));
					}

					Projectile proj = Main.projectile[projIndex];
					//VFX vanish
					Terraria.Utils.PoofOfSmoke(proj.Center);
					//For sure, without fail, decimate this projectile
					proj.timeLeft = 2;
					proj.Kill();
					proj.active = false;
				}
			}
			OnStopAbility?.Invoke(this, player, sItem);
		}
	}

	public class PlayerAbility
	{
		public PlayerAbility(int slot, ModKeybind? keybind = null)
		{
			this.Slot = slot;
			this.Keybind = keybind;
			CooldownTimer = 0;
			DurationTimer = 0;
		}
		public delegate void AbilityCooldown(Player player, ref int cooldown);
		public delegate void AbilityDuration(Player player, ref int duration);
		public delegate void AbilityTimer(Player player, ref int timer);
		public static event AbilityCooldown OnApplyCooldown;
		public static event AbilityTimer OnUpdateCooldown;
		public static event AbilityDuration OnApplyDuration;
		public static event AbilityTimer OnUpdateDuration;

		public int CooldownTimer { get; private set; }
		public bool IsOnCooldown => CooldownTimer > 0;
		public int DurationTimer { get; private set; }
		public bool IsAbilityActive => DurationTimer > 0;

		public readonly int Slot = -1;
		public bool ValidSlotIndex => Slot > -1;
		public readonly ModKeybind? Keybind;
		public bool ValidKeybind => Keybind != null;

		public bool TryGetAbility(Player player, out Ability ability)
		{
			ability = default(Ability);
			if (!ValidSlotIndex) return false;
			int itemID = GetAbilityType(player);
			if (!Ability.IsAbility(itemID)) return false;
			return Ability.AbilityList.TryGetValue(itemID, out ability);
		}
		public int GetCooldown(Player player)
		{
			if (!TryGetAbility(player, out Ability ability)) return -1;
			return ability.Cooldown;
		}
		public int GetDuration(Player player)
		{
			if (!TryGetAbility(player, out Ability ability)) return -1;
			return ability.Duration;
		}
		public int GetAbilityType(Player player)
		{
			Item item = player.inventory[Slot];
			if (item.IsAir) return -1;
			return item.type;
		}
		public bool CanUseAbility(Player player)
		{
			return !IsOnCooldown && Ability.IsAbility(GetAbilityType(player));
		}
		public void UseAbility(Player player)
		{
			int timer = GetCooldown(player);
			OnApplyCooldown?.Invoke(player, ref timer);
			CooldownTimer = timer;

			timer = GetDuration(player);
			OnApplyDuration?.Invoke(player, ref timer);
			DurationTimer = timer;
		}
		public bool TryUseAbility(Player player)
		{
			if (!CanUseAbility(player))
				return false;
			UseAbility(player);
			return true;
		}
		public void UpdateTimers(Player player)
		{
			if (IsOnCooldown)
			{
				int newCD = CooldownTimer-1;
				OnUpdateCooldown?.Invoke(player, ref newCD);
				CooldownTimer = newCD;
			}
			if (IsAbilityActive)
			{
				int newDur = DurationTimer-1;
				OnUpdateDuration?.Invoke(player, ref newDur);
				DurationTimer = newDur;
			}
		}
	}

	public static class AbilityConditions
	{
		public abstract class AbilityCondition
		{
			//I'd just use delegates instead
			//EXCEPT I want some way to specify that the condition is to be negated
			//Without just. Having a duplicate function.
			public abstract bool CheckCondition(Player player);
			public bool Inverted;

			public AbilityCondition Invert()
			{
				Inverted = !Inverted;
				return this;
			}
			public bool ConditionsMet(Player player)
			{
				return Inverted ^ CheckCondition(player);
			}
		}

		public class LineOfSight : AbilityCondition
		{
			public override bool CheckCondition(Player player)
				=> Collision.CanHitLine(player.position, player.width, player.height, Main.MouseWorld, 1, 1);
		}
		public class InSolidTile : AbilityCondition
		{
			public override bool CheckCondition(Player player)
				=> WorldGen.SolidTile(Main.tile[Main.MouseWorld.ToTileCoordinates()]);
		}
		public class NearbyEnemy : AbilityCondition
		{
			private float range;
			public NearbyEnemy(float range) : base()
			{
				this.range = range;
			}

			public override bool CheckCondition(Player player)
				=> Main.npc.Any(npc => npc.active && npc.DistanceSQ(player.Center) < MathF.Pow(range, 2));
		}
	}

	public class AbilityHandler : ModPlayer
    {
		public List<PlayerAbility> Abilities;

		private int prevSelectedItem = -1;

		private static ModKeybind Ability1, Ability2;
		public override void Load()
		{
			Ability.LoadAbilities();

			Ability1 = KeybindLoader.RegisterKeybind(Mod, "First Skill", "Q");
			Ability2 = KeybindLoader.RegisterKeybind(Mod, "Second Skill", "E");
		}
		public override void Initialize()
		{
			Abilities = new List<PlayerAbility>()
			{
				new PlayerAbility(2, Ability1),
				new PlayerAbility(3, Ability2),
			};
		}
		public override void Unload()
		{
			Abilities.Clear();
			Abilities = null;
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			foreach (PlayerAbility ability in Abilities)
			{
				if (ability.ValidKeybind && ability.Keybind.JustPressed)
				{
					if (!ability.CanUseAbility(Player))
						continue;

					if (Player.selectedItem == ability.Slot)
					{
						Player.controlUseItem = true;
					}
					else if (prevSelectedItem == -1 && !Player.inventory[ability.Slot].IsAir)
					{
						prevSelectedItem = Player.selectedItem;
						Player.selectedItem = ability.Slot;
						if (CombinedHooks.CanUseItem(Player, Player.inventory[ability.Slot]))
						{
							Player.controlUseItem = true;
							Player.ItemCheck();
						}
					}
				}
			}
		}
		public override void PostUpdate()
		{
			if (prevSelectedItem != -1 && !Player.ItemAnimationEndingOrEnded)
			{
				Player.selectedItem = prevSelectedItem;
				prevSelectedItem = -1;
			}

			foreach (PlayerAbility ability in Abilities)
			{
				bool wasActive = ability.IsAbilityActive;

				ability.UpdateTimers(Player);

				if (wasActive && !ability.IsAbilityActive)
				{
					Item sItem = Player.inventory[ability.Slot];
					Ability.AbilityList[sItem.type].StopAbility(Player, sItem);
				}
			}
		}

		public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
		{
			if (Ability.IsAbility(item.type))
				mult = 0;
		}
		public override bool CanUseItem(Item item)
		{
			//Item passed in is 'Player.inventory[Player.selectedItem]'

			///<see cref="Player.ItemCheck"/> -> <c> Player.ItemCheck_Inner() </c>
			// -> Item item = this.inventory[this.selectedItem]
			// -> bool flag3 = this.ItemCheck_CheckCanUse(item)
			///<see cref="Player.ItemCheck"/> -> <c> Player.ItemCheck_CheckCanUse(Item sItem) </c>
			// -> if(sItem.IsAir || !CombinedHooks.CanUseItem(this, sItem))
			///<see cref="CombinedHooks.CanUseItem(Player, Item)"
			// -> if(PlayerLoader.CanUseItem(player, item))
			///<see cref="PlayerLoader.CanUseItem(Player, Item)"/>
			// -> if(!enumerator.Current.CanUseItem(Item item))

			if (Ability.IsAbility(item.type))
			{
				if (!Abilities.Any(x => x.Slot == Player.selectedItem)) return false;
				return Abilities.First(x => x.Slot == Player.selectedItem).CanUseAbility(Player);
			}
			return base.CanUseItem(item);
		}
		public override void PostItemCheck()
		{
			//Look IDFK this is just what tML does for it okay?
			bool firstUse = Player.ItemAnimationJustStarted;

			Item abilityItem = Player.inventory[Player.selectedItem];
			if (Ability.IsAbility(abilityItem.type) && firstUse)
			{
				if (Abilities.Any(x => x.Slot == Player.selectedItem))
				{
					Abilities.First(x => x.Slot == Player.selectedItem).UseAbility(Player);
				}
			}
		}



		public static bool TryGetSelectedAbility(Player player, out PlayerAbility ability)
		{
			return player.GetModPlayer<AbilityHandler>().TryGetSelectedAbility(out ability);
		}
		public bool TryGetSelectedAbility(out PlayerAbility ability)
		{
			ability = default(PlayerAbility);
			if (!Abilities.Any(x => x.Slot == Player.selectedItem))
				return false;
			ability = Abilities.First(x => x.Slot == Player.selectedItem);
			return true;
		}
	}

	//Detours and IL Edits
	internal class AbilityEdits : GlobalItem
	{
		//Guaranteed that everything this runs on WILL be an ability
		public override bool AppliesToEntity(Item item, bool lateInstantiation)
		{
			return lateInstantiation && Ability.IsAbility(item.type);
		}

		public override void SetDefaults(Item item)
		{
			switch (item.type)
			{
				case ItemID.StormTigerStaff:
					item.damage = 10;
					break;
				case ItemID.ClingerStaff:
					item.damage = 10;
					break;
				case ItemID.ToxicFlask:
					item.damage = 3;
					break;
				case ItemID.StaffoftheFrostHydra:
					item.knockBack = 0;
					break;
				case ItemID.DD2ExplosiveTrapT1Popper:
					item.damage = 50;
					break;
				case ItemID.MolotovCocktail:
					item.maxStack = 1;
					break;
				case ItemID.BouncyDynamite:
					item.maxStack = 1;
					break;
				case ItemID.SnowballLauncher:
					item.maxStack = 1;
					break;
				case ItemID.ManaPotion:
					item.healMana = 200;
					break;
			}
			if (item.buffType != 0 && item.buffTime != 0)
			{
				item.buffTime = Ability.AbilityList[item.type].Duration;
			}
			item.consumable = false;
		}

		public override void Load()
		{
			IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;

			// Hook to prevent items from being picked up while the skill slot is on cooldown
			On_ItemSlot.PickItemMovementAction += ItemSlot_PickItemMovementAction;

			// Hooks to allow DD2 turrets at all times
			On_Projectile.TurretShouldPersist += On_Projectile_TurretShouldPersist;
			On_Player.ItemCheck_CheckCanUse += On_Player_ItemCheck_CheckCanUse;

			// Hooks to prevent skill items from being dropped while on cooldown
			On_Player.DropSelectedItem += On_Player_DropSelectedItem;

			// Hooks to prevent non-skill items from being picked up into a skill slot
			On_Player.GetItem_FillEmptyInventorySlot += On_Player_GetItem_FillEmptyInventorySlot;
		}

		public override void Unload()
		{
			IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;

			// Hook to prevent items from being picked up while the skill slot is on cooldown
			On_ItemSlot.PickItemMovementAction -= ItemSlot_PickItemMovementAction;

			// Hooks to allow DD2 turrets at all times
			On_Projectile.TurretShouldPersist -= On_Projectile_TurretShouldPersist;
			On_Player.ItemCheck_CheckCanUse -= On_Player_ItemCheck_CheckCanUse;

			// Hooks to prevent skill items from being dropped while on cooldown
			On_Player.DropSelectedItem -= On_Player_DropSelectedItem;

			// Hooks to prevent non-skill items from being picked up into a skill slot
			On_Player.GetItem_FillEmptyInventorySlot -= On_Player_GetItem_FillEmptyInventorySlot;
		}

		private static void IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color(ILContext il)
		{
			try
			{
				// Initialize cursor
				ILCursor c = new ILCursor(il);

				// Find where the entry point of this code will be. This is where flag2 is loaded as a local.
				c.GotoNext(i => i.MatchLdloc(9));
				c.Index++;

				// Emit all required values to stack
				c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_1); // Inventory array
				c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_3); // Slot number
				c.Emit(Mono.Cecil.Cil.OpCodes.Ldloc_S, (byte)7); // Texture value
				c.Emit(Mono.Cecil.Cil.OpCodes.Ldarg_2); // Context

				// Emit the delegate (the code)
				c.EmitDelegate<Func<Item[], int, Texture2D, int, Texture2D>>((inv, slot, originalTexture, context) => {

					if (Main.LocalPlayer.GetModPlayer<AbilityHandler>().Abilities.Any(x => x.Slot == slot))
					{
						if (context == 0 || context == 13 && slot != Main.LocalPlayer.selectedItem)
						{
							if (inv[slot].favorited)
							{
								return TextureAssets.InventoryBack19.Value;
							}

							return TextureAssets.InventoryBack2.Value;
						}

					}

					return originalTexture;
				});

				// Emit return value
				c.Emit(Mono.Cecil.Cil.OpCodes.Stloc_S, (byte)7);
			}
			catch (Exception e)
			{
				MonoModHooks.DumpIL(ModContent.GetInstance<TerrariaCells>(), il);
			}
		}

		//Prevent non-ability item pickups from going into ability slots
		private static bool On_Player_GetItem_FillEmptyInventorySlot(On_Player.orig_GetItem_FillEmptyInventorySlot orig, Player self, int plr, Item newItem, GetItemSettings settings, Item returnItem, int i)
		{
			if (Ability.AbilityList.ContainsKey(newItem.type) //Pickup item is an ability
				&& !self.GetModPlayer<AbilityHandler>().Abilities.Any(x => x.GetAbilityType(self).Equals(newItem.type))) //No abilities of the same type
			{
				return false;
			}
			else
			{
				return orig.Invoke(self, plr, newItem, settings, returnItem, i);
			}
		}

		//Prevent abilities on cooldown from being dropped
		private static void On_Player_DropSelectedItem(On_Player.orig_DropSelectedItem orig, Player self)
		{
			//If selected item isn't an ability, dropping it sooner > later
			Item selectedItem = self.inventory[self.selectedItem];
			if (!Ability.IsAbility(selectedItem.type))
				goto VanillaDropLogic;

			AbilityHandler modPlayer = self.GetModPlayer<AbilityHandler>();
			if (!AbilityHandler.TryGetSelectedAbility(self, out PlayerAbility ability))
				goto VanillaDropLogic; //If selected item not in ability slots
			if (!ability.IsOnCooldown)
				goto VanillaDropLogic; //If ability is NOT on cooldown, allow standard logic

			return; //Prevent drop logic

			//Label-goto used to avoid repeating orig invokation/staircase conditional
		VanillaDropLogic:
			orig.Invoke(self);
		}

		//Ensure DD2 sentries can be used despite clear flag
		private static bool On_Player_ItemCheck_CheckCanUse(On_Player.orig_ItemCheck_CheckCanUse orig, Player self, Item item)
		{
			//Pretend player has completed OOA to allow most existant useitem logic to run
			bool hasDoneOOA = self.downedDD2EventAnyDifficulty;
			self.downedDD2EventAnyDifficulty = true;
			bool result = orig.Invoke(self, item);
			//Change it back so it doesn't get saved or whatever
			self.downedDD2EventAnyDifficulty = hasDoneOOA;
			return result;
		}

		//Ensure sentries aren't removed when placing other sentries
		private static bool On_Projectile_TurretShouldPersist(On_Projectile.orig_TurretShouldPersist orig, Projectile self)
		{
			return true;
		}

		//Prevent abilities on cooldown from being displaced
		//Prevent non-ability items being placed into ability slots
		private static int ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item item)
		{
			if (context == 0)
			{
				AbilityHandler modPlayer = Main.LocalPlayer.GetModPlayer<AbilityHandler>();
				PlayerAbility? ability = modPlayer.Abilities.FirstOrDefault(x => x.Slot == slot);

				//Is not ability slot
				if (ability == default(PlayerAbility)) goto VanillaItemSlotLogic;
				//IS ability slot, but is on cooldown
				if (ability?.IsOnCooldown == true) return -1;

				if (!item.IsAir)
				{
					//Guaranteed slots is an ability slot not on cooldown
					if (!Ability.IsAbility(item.type)) return -1;

					//If held item exists as "equipped" ability already
					if (modPlayer.Abilities.Any(x => modPlayer.Player.inventory[x.Slot].type == item.type)) return -1;
				}
			}

			//Same rationale as drop selected item
		VanillaItemSlotLogic:
			return orig.Invoke(inv, context, slot, item);
		}

		public static bool TryGetWithFormat(Mod mod, Item item, out IEnumerable<TooltipLine> tooltips)
		{
			tooltips = default;
			if (!Ability.IsAbility(item.type))
				return false;
			Ability info = Ability.AbilityList[item.type];

			TooltipLine[] ttLines = new TooltipLine[]
			{
				new TooltipLine(mod, "SkillTitle", mod.GetLocalization("Tooltips.SkillStats").Value) { OverrideColor = Color.CadetBlue },
				new TooltipLine(mod, "SkillCooldown", mod.GetLocalization("Tooltips.Cooldown").Format(info.Cooldown / 60)) { OverrideColor = Color.LightCyan },
				new TooltipLine(mod, "SkillDuration", mod.GetLocalization("Tooltips.SummonTime").Format(info.Duration / 60)) { OverrideColor = Color.LightCyan },
				new TooltipLine(mod, "ShiftHint", mod.GetLocalization("Tooltips.ShiftHint").Value) { OverrideColor = Color.CadetBlue },
			};
			if (Main.keyState.PressingShift())
				tooltips = ttLines[..^1];
			else
				tooltips = ttLines[^1..];
			return true;
		}
	}
}