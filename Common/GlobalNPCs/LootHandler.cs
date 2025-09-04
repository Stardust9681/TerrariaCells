using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using static Terraria.ID.ItemID;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using System.Reflection;

namespace TerrariaCells.Common.GlobalNPCs
{
    public class LootHandler : GlobalNPC
    {
        public override void Load()
        {
            On_NPC.NPCLoot_DropHeals += ModifyDropHeals;
            On_NPC.DoDeathEvents_DropBossPotionsAndHearts += NPCDeathEvents;
            On_NPC.CountKillForBannersAndDropThem += NPCHandleBanners;
        }

        public override void Unload()
        {
            On_NPC.NPCLoot_DropHeals -= ModifyDropHeals;
            On_NPC.DoDeathEvents_DropBossPotionsAndHearts -= NPCDeathEvents;
            On_NPC.CountKillForBannersAndDropThem -= NPCHandleBanners;
        }

        private void NPCHandleBanners(On_NPC.orig_CountKillForBannersAndDropThem orig, NPC self)
        {
            int bannerID = Item.NPCtoBanner(self.BannerID());
            if (bannerID <= 0 || self.ExcludedFromDeathTally()) return;
            NPC.killCount[bannerID]++;
            //Not sure if this is necessary
            //if (Main.netMode == 2) NetMessage.SendData(MessageID.NPCKillCountDeathTally, -1, -1, null, bannerID);
        }

        private void ModifyDropHeals(On_NPC.orig_NPCLoot_DropHeals orig, NPC self, Player closestPlayer)
        {
            if (!NPCID.Sets.NeverDropsResourcePickups[self.type] && self.lifeMax > 5 && !self.friendly && self.CanBeChasedBy() && !NPCID.Sets.ProjectileNPC[self.type])
                DropFoodHeals.TryDroppingHeal(self, closestPlayer);
            return;
        }
        private const bool _DO_BADGERS_HAT = false;
        private void NPCDeathEvents(On_NPC.orig_DoDeathEvents_DropBossPotionsAndHearts orig, NPC self, ref string typeName)
        {
            //We don't care if it's not a boss (they don't have death events), or if it's a modded one
            if (!self.boss || self.type > NPCID.Count)
            {
                orig.Invoke(self, ref typeName);
                return;
            }

            //Vanilla logic for that hat thing, maintaining this for compatability
            if (_DO_BADGERS_HAT)
            {
                bool killedEyeOrWall = false; //Save on reflection calls
                if (self.type == NPCID.EyeofCthulhu)
                {
                    typeof(NPC).GetField("EoCKilledToday", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
                    killedEyeOrWall = true;
                }
                else if (self.type == NPCID.WallofFlesh)
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
        }

        public override void ModifyGlobalLoot(GlobalLoot globalLoot)
        {
            globalLoot.RemoveWhere(x => true);
        }

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            npcLoot.RemoveWhere(x => true);
            IItemDropRule commonLesserHealthPotion = ItemDropRule.NotScalingWithLuck(LesserHealingPotion, 1, 2, 2);
            IItemDropRule commonHealthPotion = ItemDropRule.NotScalingWithLuck(HealingPotion, 1, 2, 3);
            IItemDropRule commonGreaterHealthPotion = ItemDropRule.NotScalingWithLuck(GreaterHealingPotion, 1, 2, 3);
            switch (npc.type)
            {
                case NPCID.BrainofCthulhu:
                    npcLoot.Add(commonHealthPotion);
                    npcLoot.Add(new DropPerPlayerOnThePlayer(ItemID.CloudinaBottle, 1, 1, 1, new PowerDropRuleCondition(static mplayer => !mplayer.CloudJump)));
                    break;
                case NPCID.EyeofCthulhu:
                case NPCID.KingSlime:
                case NPCID.EaterofWorldsHead:
                case NPCID.EaterofWorldsBody:
                case NPCID.EaterofWorldsTail:
                case NPCID.SkeletronHead:
                case NPCID.QueenBee:
                case NPCID.Deerclops:
                    npcLoot.Add(commonHealthPotion);
                    break;
                case NPCID.WallofFlesh:
                    npcLoot.Add(commonGreaterHealthPotion);
                    break;
                default:
                    break;
            }
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

        public const float DROPRATE = 0.05f; //Base droprate for this rule
        public const float INV_DROPRATE = 1f / DROPRATE; //Used for logic, not necessary but I want to cache this
        public const float TIER_ONE_RATE = 0.8f; //Percent of drops to be Tier 1 foods
        public const float TIER_TWO_RATE = 0.195f; //Percent of drops to be Tier 2 foods
        public const float TIER_THREE_RATE = 0.005f; //Percent of drops to be Tier 3 foods

        /// <summary> </summary>
        /// <param name="self">NPC to drop from.</param>
        /// <param name="interactionPlayer">Player that is used to determine luck.</param>
        /// <returns>True on success. False on fail.</returns>
        public static bool TryDroppingHeal(NPC self, Player interactionPlayer)
        {
			bool attem = TryDropItem(self.GetSource_Death(), self.Center);
			if (!attem && interactionPlayer.statLife < interactionPlayer.statLifeMax2 * 0.05f)
				attem = TryDropItem(self.GetSource_Death(), self.Center);
			return attem;
        }

		public static bool TryDropItem(Terraria.DataStructures.IEntitySource source, Vector2 position)
		{
			float roll = Main.rand.NextFloat(); //Chose not to use Player.RollLuck(..) here
			if (roll < DROPRATE)
			{
				int itemToDrop = PickFoodItem();
				CommonCode.DropItem(position, source, itemToDrop, 1); //Drop item
				return true; //Succeeded
			}
			return false; //Failed RNG
		}

		public static int PickFoodItem()
		{
			switch (Main.rand.NextFloat())
			{
				case < TIER_THREE_RATE:
					return Main.rand.Next(TIER_THREE_FOOD);
				case < TIER_TWO_RATE + TIER_THREE_RATE:
					return Main.rand.Next(TIER_TWO_FOOD);
				case < TIER_ONE_RATE + TIER_TWO_RATE + TIER_THREE_RATE:
					return Main.rand.Next(TIER_ONE_FOOD);
				default:
					return -1; //Something went wrong
			}
		}
    }

    internal class PowerDropRuleCondition : IItemDropRuleCondition
    {
        private Func<Common.ModPlayers.MetaPlayer, bool> _check;
        public PowerDropRuleCondition(Func<ModPlayers.MetaPlayer, bool> condition)
        {
            _check = condition;
        }
        public bool CanDrop(DropAttemptInfo info)
        {
            return _check.Invoke(info.player.GetModPlayer<ModPlayers.MetaPlayer>());
        }

        public bool CanShowItemDropInUI()
        {
            return false;
        }
        public string GetConditionDescription()
        { //Will never be seen by players anyway. I hope
            return string.Empty;
        }
    }
}