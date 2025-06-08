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
	///<see cref="AbilitySlot"/> -- structure responsible for tracking ability cooldowns per Player Ability Slot
	///<see cref="AbilityConditions"/> -- Similar to <see cref="Terraria.WorldBuilding.Shapes"/>, contains ability conditions
	///<see cref="AbilityHandler"/> -- ModPlayer responsible for ability handling and updates.
	///<see cref="AbilityEdits"/> -- Detours/IL edits. Also item stats and tooltips.
	///Useful: <see cref="AbilityEdits.SetDefaults(Item)"/>

	//If you want to refactor this, yourself, go for it. Want to split this up into multiple files, redo code, etc, go nuts.
	//Please be sure to use provided player instance in methods when applicable (over 'Main.LocalPlayer')
	//Please be sure to check use of static accessors

	//Static/Template Ability type, holds static information
	public class Ability
	{
		public static void LoadAbilities()
		{
			RegisterAbility(ItemID.ClingerStaff, new Ability(NumberHelpers.SecToFrames(45), 10.SecToFrames(), new LineOfSight(), new InSolidTile().Invert()));
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
			RegisterAbility(ItemID.WrathPotion, new Ability(NumberHelpers.SecToFrames(60), 20.SecToFrames()));
			RegisterAbility(ItemID.MagicPowerPotion, new Ability(NumberHelpers.SecToFrames(60), 5.SecToFrames()));
			RegisterAbility(ItemID.SwiftnessPotion, new Ability(NumberHelpers.SecToFrames(60), 20.SecToFrames()));
		}

		/// <summary>
		/// <para>Keys = <c>Item Type</c></para>
		/// <para>Values = <c>Ability</c></para>
		/// </summary>
		public static IReadOnlyDictionary<int, Ability> AbilityList => _AbilityRegistry;
		private static Dictionary<int, Ability> _AbilityRegistry = new Dictionary<int, Ability>();
		///<summary> Used to register different abilities during loading </summary>
		///<returns> This instance registered in ability registry </returns>
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
		///<returns> Whether or not a given item type represents an ability item </returns>
		public static bool IsAbility(int itemID) => _AbilityRegistry.ContainsKey(itemID);


		
		///<summary>The base cooldown of this ability</summary>
		public readonly int Cooldown;
		///<summary>The base duration of this ability</summary>
		///<remarks>If set to -1, no ability duration is applied, <see cref="StopAbility(Player, Item)"/> and <see cref="OnStopAbility"/> will not be called.</remarks>
		public readonly int Duration;
		public IReadOnlyList<AbilityCondition> Conditions;

		/// <param name="self"> Information about this ability (max cooldown, duration, conditions) </param>
		/// <param name="player"> Player that used the ability </param>
		/// <param name="sItem"> Item that started the ability </param>
		public delegate void On_StopAbility(Ability self, Player player, Item sItem);
		/// <summary> Event called when ability duration expires </summary>
		public event On_StopAbility OnStopAbility;

		internal Ability(int cooldown, int duration = -1, params AbilityCondition[] conditions)
		{
			this.Cooldown = cooldown;
			this.Duration = duration;
			this.Conditions = conditions.ToList();
		}
		//Ensure no memory leak from failing to unsubscribe methods ;w;
		~Ability()
		{
			OnStopAbility = default;
		}

		/// <summary>
		/// Whether this ability meets all of its criteria to be used.
		/// Set this with <c>params AbilityCondition[]</c> parameter in <see cref="Ability.Ability(int, int, AbilityCondition[])"/>
		/// </summary>
		public bool MeetsConditions(Player player)
		{
			if (!Conditions.Any()) return true;
			return Conditions.All(x => x.ConditionsMet(player));
		}
		/// <summary>
		/// Called when stopping an ability. Invokes any methods subscribed to <see cref="OnStopAbility"/>
		/// </summary>
		/// <param name="player"></param>
		/// <param name="sItem"> Item that started this ability </param>
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
	
	//Ability Slot on the player, handles cooldown and duration timers
	public class AbilitySlot
	{
		public AbilitySlot(int slot, ModKeybind? keybind = null)
		{
			this.Slot = slot;
			this.Keybind = keybind;
			cooldownTimer = 0;
			durationTimer = 0;
		}

		//Not sure if we want these to be a part of the ABILITY or PLAYER ABILITY
		//I figure it directly changes things here, so this is appropriate
		public delegate void AbilityCooldown(Player player, ref int cooldown);
		public delegate void AbilityDuration(Player player, ref int duration);
		public delegate void AbilityTimer(Player player, ref int timer);
		///<summary> Called when cooldown value is set </summary>
		public static event AbilityCooldown OnApplyCooldown;
		///<summary> Called when cooldown timer is updated </summary>
		public static event AbilityTimer OnUpdateCooldown;
		///<summary> Called when duration value is set </summary>
		public static event AbilityDuration OnApplyDuration;
		///<summary> Called when duration timer is updated </summary>
		public static event AbilityTimer OnUpdateDuration;

		public int cooldownTimer;
		///<remarks> Shorthand for <c>CooldownTimer > 0</c> </remarks>
		public bool IsOnCooldown => cooldownTimer > 0;

		public int durationTimer;
		///<remarks> Shorthand for <c>DurationTimer > 0</c> </remarks>
		public bool IsAbilityActive => durationTimer > 0;

		///<summary> Inventory slot that this ability slot occupies </summary>
		public readonly int Slot = -1;
		public bool ValidSlotIndex => Slot > -1;
		///<summary> Keybind associated with this ability slot </summary>
		public readonly ModKeybind? Keybind;
		public bool ValidKeybind => Keybind != null;

		/// <summary> Tries to get the associated Ability with this slot </summary>
		public bool TryGetAbility(Player player, out Ability ability)
		{
			ability = default(Ability);
			if (!ValidSlotIndex) return false;
			int itemID = GetAbilityType(player);
			if (!Ability.IsAbility(itemID)) return false;
			return Ability.AbilityList.TryGetValue(itemID, out ability);
		}
		///<summary> Gets the total cooldown for the ability in this slot </summary>
		public int GetCooldown(Player player)
		{
			if (!TryGetAbility(player, out Ability ability)) return -1;
			int cooldown = ability.Cooldown;
			OnApplyCooldown?.Invoke(player, ref cooldown);
			return cooldown;
		}
		///<summary> Gets the total duration for the ability in this slot </summary>
		public int GetDuration(Player player)
		{
			if (!TryGetAbility(player, out Ability ability)) return -1;
			int duration = ability.Duration;
			OnApplyDuration?.Invoke(player, ref duration);
			return duration;
		}
		///<summary>Gets the item type of this slot </summary>
		public int GetAbilityType(Player player)
		{
			Item item = player.inventory[Slot];
			if (item.IsAir) return -1;
			return item.type;
		}
		///<returns> Whether or not the player can currently use this ability </returns>
		public bool CanUseAbility(Player player)
		{
			return !IsOnCooldown && Ability.IsAbility(GetAbilityType(player));
		}
		///<summary> Sets information for this slot on use </summary>
		public void UseAbility(Player player)
		{
			cooldownTimer = GetCooldown(player);
			durationTimer = GetDuration(player);
		}
		///<summary> Update method invoked every frame to update cooldown and duration timers </summary>
		public void UpdateTimers(Player player)
		{
			if (IsOnCooldown)
			{
				int newCD = cooldownTimer-1;
				OnUpdateCooldown?.Invoke(player, ref newCD);
				cooldownTimer = newCD;
			}
			if (IsAbilityActive)
			{
				int newDur = durationTimer-1;
				OnUpdateDuration?.Invoke(player, ref newDur);
				durationTimer = newDur;
			}
		}
	}

	//Collection of conditional types for abilities
	public static class AbilityConditions
	{
		public abstract class AbilityCondition
		{
			//I'd just use delegates instead
			//EXCEPT I want some way to specify that the condition is to be negated
			//Without just. Having a duplicate function.
			public abstract bool CheckCondition(Player player);
			public bool Inverted;
			/// <summary> Inverts the conditional </summary>
			/// <returns> This instance for chaining purposes </returns>
			public AbilityCondition Invert()
			{
				Inverted = !Inverted;
				return this;
			}
			/// <returns> Whether or not this condition is met </returns>
			public bool ConditionsMet(Player player)
			{
				return Inverted ^ CheckCondition(player);
			}
		}

		//Some examples of Ability Conditions (LoS and InSolidTile are used)
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

	//ModPlayer that handles abilities
	public class AbilityHandler : ModPlayer
    {
		internal List<AbilitySlot> Abilities = new List<AbilitySlot>();

		private int prevSelectedItem = -1;

		private static ModKeybind Ability1, Ability2;
		public override void Load()
		{
			Ability.LoadAbilities();

			//Keybinds loaded here (not Initialize) because then they'd be created many times over
			Ability1 = KeybindLoader.RegisterKeybind(Mod, "First Skill", "Q");
			Ability2 = KeybindLoader.RegisterKeybind(Mod, "Second Skill", "E");
		}
		public override void Initialize()
		{
			//List initialised here because it wasn't for some reason anywhere else
			Abilities = new List<AbilitySlot>()
			{
				new AbilitySlot(2, Ability1),
				new AbilitySlot(3, Ability2),
			};
		}
		public override void Unload()
		{
			Abilities = null;
		}

		//Check for keybind presses, use appropriate ability when able
		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			foreach (AbilitySlot ability in Abilities)
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

		//Change selected item back to previously-held item if it was hot-swapped off
		public override void PostUpdate()
		{
			if (prevSelectedItem != -1 && !Player.ItemAnimationEndingOrEnded)
			{
				Player.selectedItem = prevSelectedItem;
				prevSelectedItem = -1;
			}

			foreach (AbilitySlot ability in Abilities)
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


		//Try to get the selected ability with 'Player.selectedItem'
		public static bool TryGetSelectedAbility(Player player, out AbilitySlot ability)
		{
			return player.GetModPlayer<AbilityHandler>().TryGetSelectedAbility(out ability);
		}
		public bool TryGetSelectedAbility(out AbilitySlot ability)
		{
			ability = default(AbilitySlot);
			if (!Abilities.Any(x => x.Slot == Player.selectedItem))
				return false;
			ability = Abilities.First(x => x.Slot == Player.selectedItem);
			return true;
		}
	}

	//Detours and IL Edits
	internal class AbilityEdits : GlobalItem
	{
		#region GlobalItem Impl
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
				case ItemID.ManaPotion:
					item.healMana = 200;
					break;
			}
			if (item.buffType != 0 && item.buffTime != 0)
			{
				item.buffTime = Ability.AbilityList[item.type].Duration;
			}
            item.useTime = 8;
            item.reuseDelay = 0;
            item.autoReuse = false;
            item.useAnimation = item.useTime;

			item.consumable = false;
			item.maxStack = 1;
		}

		//Remove mana cost from abilities
		public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
		{
			mult = 0;
		}

		//Check that ability is off cooldown for use
		public override bool CanUseItem(Item item, Player player)
		{
			AbilityHandler modPlayer = player.GetModPlayer<AbilityHandler>();
			foreach (AbilitySlot slot in modPlayer.Abilities)
			{
				if (player.inventory[slot.Slot].Equals(item))
					return slot.CanUseAbility(player);
			}
			return false;
		}

		//Set timers for ability use
		public override bool? UseItem(Item item, Player player)
		{
			AbilityHandler modPlayer = player.GetModPlayer<AbilityHandler>();
			foreach (AbilitySlot slot in modPlayer.Abilities)
			{
				if (player.inventory[slot.Slot].Equals(item))
				{
					slot.UseAbility(player);
					break;
				}
			}
			return null;
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
            tooltips.Find(x => x.Name.Equals("EtherianManaWarning"))?.Hide();
            tooltips.Find(x => x.Name.Equals("BuffTime"))?.Hide();
		}
		#endregion

		public override void Load()
		{
			//IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;

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
			//IL_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color -= IL_ItemSlot_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color;

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

		#region Detours/IL
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
					try {
						if (Main.gameMenu)
							return originalTexture;
						if (!Configs.DevConfig.Instance.EnableInventoryChanges)
							return originalTexture;

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
                    } catch (IndexOutOfRangeException) {
						// prevent main engine crash on player select
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
			if (!Configs.DevConfig.Instance.EnableInventoryChanges)
				goto VanillaPickupLogic;

			AbilityHandler modPlayer = self.GetModPlayer<AbilityHandler>();
			if (!modPlayer.Abilities.Any(x => x.Slot == i))
				goto VanillaPickupLogic;

			if (!Ability.IsAbility(newItem.type))
				return false;

			if (!modPlayer.Abilities.Any(x => x.GetAbilityType(self) == newItem.type))
				goto VanillaPickupLogic;

			return false;

		VanillaPickupLogic:
			return orig.Invoke(self, plr, newItem, settings, returnItem, i);
		}

		//Prevent abilities on cooldown (and active abilities) from being dropped
		private static void On_Player_DropSelectedItem(On_Player.orig_DropSelectedItem orig, Player self)
		{
			//If selected item isn't an ability, dropping it sooner > later
			Item selectedItem = self.inventory[self.selectedItem];
			if (!Ability.IsAbility(selectedItem.type))
				goto VanillaDropLogic;

			AbilityHandler modPlayer = self.GetModPlayer<AbilityHandler>();
			if (!AbilityHandler.TryGetSelectedAbility(self, out AbilitySlot ability))
				goto VanillaDropLogic; //If selected item not in ability slots
			if (!ability.IsOnCooldown && !ability.IsAbilityActive)
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

		//Prevent abilities on cooldown (and active abilities) from being displaced
		//Prevent non-ability items being placed into ability slots
		private static int ItemSlot_PickItemMovementAction(On_ItemSlot.orig_PickItemMovementAction orig, Item[] inv, int context, int slot, Item item)
		{
			if (!Configs.DevConfig.Instance.EnableInventoryChanges)
				goto VanillaItemSlotLogic;

			if (context == 0)
			{
				AbilityHandler modPlayer = Main.LocalPlayer.GetModPlayer<AbilityHandler>();
				AbilitySlot? ability = modPlayer.Abilities.FirstOrDefault(x => x.Slot == slot);

				//Is not ability slot
				if (ability == default(AbilitySlot)) goto VanillaItemSlotLogic;
				//IS ability slot, but is on cooldown
				if (ability?.IsOnCooldown == true) return -1;
				if (ability?.IsAbilityActive == true) return -1;

				if (!item.IsAir)
				{
					//Guaranteed slots is an ability slot not on cooldown
					if (!Ability.IsAbility(item.type)) return -1;

					//If held item exists as "equipped" ability already (excepting the slot the player is clicking at)
					if (modPlayer.Abilities.Any(x => x.Slot != slot && modPlayer.Player.inventory[x.Slot].type == item.type)) return -1;
				}
			}

			//Same rationale as drop selected item
		VanillaItemSlotLogic:
			return orig.Invoke(inv, context, slot, item);
		}
		#endregion

		//Get tooltips for item
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