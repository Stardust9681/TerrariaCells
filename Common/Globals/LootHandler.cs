using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using static Terraria.ID.ItemID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;

namespace TerrariaCells.Common.Globals
{
	public class LootHandler : GlobalNPC
	{
		public static readonly int[] TIER_ONE_FOOD = new int[] //39 items
		{
			CookedMarshmallow, AppleJuice, BloodyMoscato, BunnyStew, CookedFish,
			BananaDaiquiri, FruitJuice, FruitSalad, GrilledSquirrel, Lemonade,
			PeachSangria, PinaColada, RoastedBird, SauteedFrogLegs, SmoothieofDarkness,
			TropicalSmoothie, Teacup, Apple, Apricot, Banana, BlackCurrant, BloodOrange,
			Cherry, Coconut, Elderberry, Grapefruit, Lemon, Mango, Peach, Pineapple,
			Plum, Pomegranate, Rambutan, SpicyPepper, MilkCarton, PotatoChips,
			ShuckedOyster, Marshmallow, JojaCola
		};
		public static readonly int[] TIER_TWO_FOOD = new int[] //27 items
		{
			BowlofSoup, CookedShrimp, Escargot, FroggleBunwich, GrubSoup, LobsterTail,
			MonsterLasagna, PrismaticPunch, RoastedDuck, SeafoodDinner, PumpkinPie,
			Sashimi, Dragonfruit, Starfruit, BananaSplit, ChickenNugget, ChocolateChipCookie,
			CoffeeCup, CreamSoda, FriedEgg, Fries, Grapes, IceCream, Nachos, ShrimpPoBoy,
			PadThai, Pho
		};
		public static readonly int[] TIER_THREE_FOOD = new int[] //13 items
		{
			GoldenDelight, GrapeJuice, Bacon, BBQRibs, Burger, Hotdog, Milkshake, Pizza,
			Spaghetti, Steak, ChristmasPudding, GingerbreadCookie, SugarCookie
		};

		public override void Load()
		{
			On_NPC.NPCLoot_DropCommonLifeAndMana += RemoveHealthManaDrops;
		}
		public override void Unload()
		{
			On_NPC.NPCLoot_DropCommonLifeAndMana -= RemoveHealthManaDrops;
		}

		private void RemoveHealthManaDrops(On_NPC.orig_NPCLoot_DropCommonLifeAndMana orig, NPC self, Player closestPlayer)
		{
			return;
		}

		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			globalLoot.RemoveWhere(x => true);
			globalLoot.Add(new FoodDropRule());
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			npcLoot.RemoveWhere(x => true);
		}
	}

	internal class FoodDropRule : IItemDropRule
	{
		private const float DROPRATE = 0.02f; //Base droprate for this rule
		private const float INV_DROPRATE = 1f / DROPRATE; //Used for logic, not necessary but I want to cache this
		private const float TIER_ONE_RATE = 0.8f; //Percent of drops to be Tier 1 foods
		private const float TIER_TWO_RATE = 0.195f; //Percent of drops to be Tier 2 foods
		private const float TIER_THREE_RATE = 0.005f; //Percent of drops to be Tier 3 foods

		public FoodDropRule() { }

		public List<IItemDropRuleChainAttempt> ChainedRules { get; set; }

		public bool CanDrop(DropAttemptInfo info)
		{
			return true;
		}

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
		{
			drops.Add(new DropRateInfo(Apple, 1, 1, DROPRATE * TIER_ONE_RATE));
			drops.Add(new DropRateInfo(PumpkinPie, 1, 1, DROPRATE * TIER_TWO_RATE));
			drops.Add(new DropRateInfo(Bacon, 1, 1, DROPRATE * TIER_THREE_RATE));
			Chains.ReportDroprates(ChainedRules, DROPRATE, drops, ratesInfo);
		}

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
		{
			float roll = info.rng.NextFloat(); //Chose not to use Player.RollLuck(..) here
			if (roll < DROPRATE)
			{
				roll *= INV_DROPRATE; //Expand this out to 0-1 (since we reduced range from 0-0.01)
				int itemToDrop;
				switch (roll) //Attempt to guarantee that food item is always dropped on success
				{
					case < TIER_THREE_RATE: //
						itemToDrop = info.rng.Next(LootHandler.TIER_THREE_FOOD);
						break;
					case < TIER_TWO_RATE + TIER_THREE_RATE: //
						itemToDrop = info.rng.Next(LootHandler.TIER_TWO_FOOD);
						break;
					case < TIER_ONE_RATE + TIER_TWO_RATE + TIER_THREE_RATE: //
						itemToDrop = info.rng.Next(LootHandler.TIER_ONE_FOOD);
						break;
					default:
						return new ItemDropAttemptResult() { State = ItemDropAttemptResultState.DidNotRunCode };
				}
				CommonCode.DropItem(info.npc.Center, info.npc.GetSource_Loot(), itemToDrop, 1);
				return new ItemDropAttemptResult() { State = ItemDropAttemptResultState.Success };
			}
			return new ItemDropAttemptResult() { State = ItemDropAttemptResultState.FailedRandomRoll };
		}
	}
}
