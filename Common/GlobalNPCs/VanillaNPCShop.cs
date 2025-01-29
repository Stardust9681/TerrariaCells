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

                // Add the specified vanilla items to the shop
                shop.Add(ItemID.PhoenixBlaster);
                shop.Add(ItemID.SniperRifle);
                shop.Add(ItemID.OnyxBlaster);
                shop.Add(ItemID.PulseBow);
                shop.Add(ItemID.IceBow);
                shop.Add(ItemID.Toxikarp);
                shop.Add(ItemID.Minishark);
                shop.Add(ItemID.GrenadeLauncher);
                shop.Add(ItemID.FieryGreatsword);
                shop.Add(ItemID.AleThrowingGlove);
            }

            if (shop.NpcType == NPCID.Merchant) 
            {
                foreach (var entry in shop.Entries.ToList())
                {
                    entry.Disable();
                }

                shop.Add(ItemID.CelestialMagnet);
                shop.Add(ItemID.NaturesGift);
                shop.Add(ItemID.ArcaneFlower);
                shop.Add(ItemID.ManaRegenerationBand);
                shop.Add(ItemID.MagicCuffs);
                shop.Add(ItemID.StalkersQuiver);
                shop.Add(ItemID.AmmoBox);
                shop.Add(ItemID.ChlorophyteDye);
                shop.Add(ItemID.BallOfFuseWire);
                shop.Add(ItemID.ReconScope);
                shop.Add(ItemID.BerserkerGlove);
                shop.Add(ItemID.Nazar);
                shop.Add(ItemID.FeralClaws);
                shop.Add(ItemID.ThePlan);
                shop.Add(ItemID.ObsidianShield);
                shop.Add(ItemID.FrozenTurtleShell);
                shop.Add(ItemID.BandofRegeneration);
                shop.Add(ItemID.FastClock);
            }
        }
    }
}
