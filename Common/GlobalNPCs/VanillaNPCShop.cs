using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalNPCs
{
    class VanillaNPCShop : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.ArmsDealer)
            {
                // Remove all existing entries from the shop
                foreach (var entry in shop.Entries.ToList())
                {
                    entry.Disable();
                }

                // Add the specified vanilla items to the shop with custom price
                shop.Add(new Item(ItemID.PhoenixBlaster) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.SniperRifle) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.OnyxBlaster) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.PulseBow) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.IceBow) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.Toxikarp) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.Minishark) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.GrenadeLauncher) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.FieryGreatsword) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.AleThrowingGlove) {
                    shopCustomPrice = 10
                });
            }

            if (shop.NpcType == NPCID.Merchant) 
            {
                foreach (var entry in shop.Entries.ToList())
                {
                    entry.Disable();
                }

                shop.Add(new Item(ItemID.CelestialMagnet) { 
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.NaturesGift) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.ArcaneFlower) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.ManaRegenerationBand) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.MagicCuffs) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.StalkersQuiver) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.AmmoBox) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.ChlorophyteDye) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.BallOfFuseWire) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.ReconScope) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.BerserkerGlove) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.Nazar) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.FeralClaws) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.ThePlan) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.ObsidianShield) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.FrozenTurtleShell) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.BandofRegeneration) {
                    shopCustomPrice = 10
                });
                shop.Add(new Item(ItemID.FastClock) {
                    shopCustomPrice = 10
                });
            }
        }
    }
}
