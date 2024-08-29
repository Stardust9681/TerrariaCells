using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using static Terraria.ID.ItemID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using System.Reflection;

namespace TerrariaCells.Common.Globals
{
	public class LootHandler : GlobalNPC
	{
		public override void Load()
		{
			On_NPC.NPCLoot_DropHeals += ModifyDropHeals;

			//I have beef with this event
			//Literally I thought I removed ALL of the NPC Loot
			//But it turns out, HEALING POTIONS from BOSSES are handled COMPLETELY DIFFERENTLY
			On_NPC.DoDeathEvents_DropBossPotionsAndHearts += NPCDeathEvents;
		}

		public override void Unload()
		{
			On_NPC.NPCLoot_DropHeals -= ModifyDropHeals;
			On_NPC.DoDeathEvents_DropBossPotionsAndHearts -= NPCDeathEvents;
		}

		private void ModifyDropHeals(On_NPC.orig_NPCLoot_DropHeals orig, NPC self, Player closestPlayer)
		{
			_ = DropFoodHeals.TryDroppingHeal(self, closestPlayer);
			return;
		}
		private void NPCDeathEvents(On_NPC.orig_DoDeathEvents_DropBossPotionsAndHearts orig, NPC self, ref string typeName)
		{
			if (!self.boss || self.type > Terraria.ID.NPCID.Count) //We don't care if it's not a boss, or if it's a modded one
			{
				orig.Invoke(self, ref typeName);
				return;
			}

			//Vanilla logic for that hat thing, maintaining this for compatability
			bool killedEyeOrWall = false; //Save on reflection calls
			if (self.type == Terraria.ID.NPCID.EyeofCthulhu)
			{
				typeof(NPC).GetField("EoCKilledToday", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
				killedEyeOrWall = true;
			}
			else if (self.type == Terraria.ID.NPCID.WallofFlesh)
			{
				typeof(NPC).GetField("WoFKilledToday", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
				killedEyeOrWall = true;
			}

			if (killedEyeOrWall
				&& typeof(NPC).GetField("EoCKilledToday", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null).Equals(true)
				&& typeof(NPC).GetField("WoFKilledToday", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null).Equals(true))
			{
				NPC.ResetBadgerHatTime();
				Item.NewItem(self.GetSource_Loot(), self.position, self.width, self.height, BadgersHat);
			}
		}

		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			globalLoot.RemoveWhere(x => true);
			//globalLoot.Add(new FoodDropRule());
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			npcLoot.RemoveWhere(x => true);
		}
	}

	internal class DropFoodHeals
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

		private const float DROPRATE = 0.05f; //Base droprate for this rule
		private const float INV_DROPRATE = 1f / DROPRATE; //Used for logic, not necessary but I want to cache this
		private const float TIER_ONE_RATE = 0.8f; //Percent of drops to be Tier 1 foods
		private const float TIER_TWO_RATE = 0.195f; //Percent of drops to be Tier 2 foods
		private const float TIER_THREE_RATE = 0.005f; //Percent of drops to be Tier 3 foods

		public static bool TryDroppingHeal(NPC self, Player interactionPlayer)
		{
			float roll = Main.rand.NextFloat(); //Chose not to use Player.RollLuck(..) here
			if (roll > DROPRATE)
			{
				//Just.. give players another chance at success if they're low on health
				if (interactionPlayer.statLife < 15)
				{
					roll = Main.rand.NextFloat();
				}
			}
			if (roll < DROPRATE)
			{
				roll *= INV_DROPRATE; //Expand this out to 0-1 (since we reduced range from 0-0.01)
				int itemToDrop;
				switch (roll) //Attempt to guarantee that food item is always dropped on success
				{
					case < TIER_THREE_RATE:
						itemToDrop = Main.rand.Next(TIER_THREE_FOOD);
						break;
					case < TIER_TWO_RATE + TIER_THREE_RATE:
						itemToDrop = Main.rand.Next(TIER_TWO_FOOD);
						break;
					case < TIER_ONE_RATE + TIER_TWO_RATE + TIER_THREE_RATE:
						itemToDrop = Main.rand.Next(TIER_ONE_FOOD);
						break;
					default:
						return false; //Something went wrong
				}
				CommonCode.DropItem(self.Center, self.GetSource_Loot(), itemToDrop, 1); //Drop item
				return true; //Succeeded
			}
			return false; //Failed RNG
		}
	}
}
