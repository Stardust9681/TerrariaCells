using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

using TerrariaCells.Common.Utilities;
using static TerrariaCells.Common.Utilities.NumberHelpers;
using static TerrariaCells.Common.GlobalItems.AbilityConditions;
using Terraria.GameInput;

namespace TerrariaCells.Common.GlobalItems
{
	public class Ability
	{
		public static void LoadAbilities()
		{
			RegisterAbility(ItemID.ClingerStaff, new Ability(NumberHelpers.SecToFrames(4) + 30, 10.SecToFrames(), new LineOfSight(), new InSolidTile().Invert()));
			RegisterAbility(ItemID.ToxicFlask, new Ability(NumberHelpers.SecToFrames(30)));
			RegisterAbility(ItemID.StaffoftheFrostHydra, new Ability(NumberHelpers.SecToFrames(30), 10.SecToFrames()));
			RegisterAbility(ItemID.DD2ExplosiveTrapT1Popper, new Ability(NumberHelpers.SecToFrames(30), 30.SecToFrames(), new LineOfSight(), new InSolidTile().Invert()));
			RegisterAbility(ItemID.MolotovCocktail, new Ability(NumberHelpers.SecToFrames(15)));
			RegisterAbility(ItemID.BouncyDynamite, new Ability(NumberHelpers.SecToFrames(60)));
			//Sgt. United Shield. 6 sec cooldown? For some reason?
			RegisterAbility(ItemID.BouncingShield, new Ability(NumberHelpers.SecToFrames(6)));
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
		internal static void RegisterAbility(int itemID, Ability ability)
		{
			if (_AbilityRegistry.ContainsKey(itemID))
			{
				ModContent.GetInstance<TerrariaCells>().Logger.Warn($"Key 'itemID' ({itemID}) already has an associated ability");
				return;
			}
			_AbilityRegistry.Add(itemID, ability);
		}
		public static bool IsAbility(int itemID) => _AbilityRegistry.ContainsKey(itemID);



		public readonly int Cooldown;
		public readonly int Duration;
		public IReadOnlyList<AbilityCondition> Conditions;

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
	}

	public class PlayerAbility
	{
		public PlayerAbility(int slot, ModKeybind? keybind = null)
		{
			this.Slot = slot;
			this.Keybind = keybind;
		}

		public int cooldownTimer;

		public readonly int Slot = -1;
		public bool ValidSlotIndex => Slot > -1;
		public readonly ModKeybind? Keybind;
		public bool ValidKeybind => Keybind != null;

		public int GetCooldown(Player player)
		{
			if (!ValidSlotIndex) return -1;
			int itemID = GetAbilityType(player);
			if (Ability.IsAbility(itemID)) return -1;
			if (!Ability.AbilityList.TryGetValue(itemID, out Ability ability)) return -1;
			return ability.Cooldown;
		}
		public int GetAbilityType(Player player)
		{
			Item item = player.inventory[Slot];
			if (item.IsAir) return -1;
			return item.type;
		}
		public bool IsOnCooldown => cooldownTimer > 0;

		public bool CanUseAbility(Player player)
		{
			return !IsOnCooldown && Ability.IsAbility(GetAbilityType(player));
		}
		public void UseAbility(Player player)
		{
			cooldownTimer = GetCooldown(player);
		}
		public bool TryUseAbility(Player player)
		{
			if (!CanUseAbility(player))
				return false;
			UseAbility(player);
			return true;
		}
	}

	public class AbilitySystem : ModPlayer
	{
		public List<PlayerAbility> Abilities;

		public override void Load()
		{
			Abilities = new List<PlayerAbility>()
			{
				new PlayerAbility(2, KeybindLoader.RegisterKeybind(Mod, "First Skill", "Q")),
				new PlayerAbility(3, KeybindLoader.RegisterKeybind(Mod, "Second Skill", "E")),
			};
		}
		public override void SetStaticDefaults()
		{
			Ability.LoadAbilities();
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
					//Use ability here
				}
			}
		}
	}

	//Going for a sort of ECS for conditions
	//Rather than a bunch of hardcoded bools, so if we need something else
	//It just gets thrown here
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
}
