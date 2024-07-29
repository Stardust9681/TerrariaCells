using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems;

public class GlobalGoldArmor : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type is ItemID.GoldHelmet or ItemID.GoldChainmail or ItemID.GoldGreaves;
    }

    public override void SetDefaults(Item entity)
    {
        switch (entity.type)
        {
            case ItemID.GoldHelmet:
                entity.defense = 2;
                break;
            case ItemID.GoldChainmail:
                entity.defense = 4;
                break;
            case ItemID.GoldGreaves:
                entity.defense = 2;
                break;
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "GoldSetPerk", "Striking an enemy causes them drop 5 Silver"));
        
        switch (item.type)
        {
            case ItemID.GoldHelmet:
                tooltips.Add(new TooltipLine(Mod, "GoldHelmet", "10% increased damage"));
                break;
            case ItemID.GoldGreaves:
                tooltips.Add(new TooltipLine(Mod, "GoldGreaves", "15% increased movement speed"));
                break;
        }

        // UpdateArmorSet does not work for reasons unknown
        if (Main.LocalPlayer.armor[0].type == ItemID.GoldHelmet &&
            Main.LocalPlayer.armor[1].type == ItemID.GoldChainmail &&
            Main.LocalPlayer.armor[2].type == ItemID.GoldGreaves)
        {
            tooltips.Remove(tooltips.Find(x => x.Text.StartsWith("Set bonus")));
            tooltips.Add(new TooltipLine(Mod, "GoldArmorSet", "Set bonus: 1% increased damage for every gold coin in inventory"));
            tooltips.Add(new TooltipLine(Mod, "GoldArmorSet", "100% increased damage for every platinum coin in inventory"));
        }
            
    }

    public override void UpdateEquip(Item item, Player player)
    {
        switch (item.type)
        {
            case ItemID.GoldHelmet:
                player.GetDamage(DamageClass.Generic) += 0.1f;
                break;
            case ItemID.GoldGreaves:
                player.moveSpeed += 0.15f;
                break;
        }

        // UpdateArmorSet does not work for reasons unknown
        if (Main.LocalPlayer.armor[0].type == ItemID.GoldHelmet && 
            Main.LocalPlayer.armor[1].type == ItemID.GoldChainmail && 
            Main.LocalPlayer.armor[2].type == ItemID.GoldGreaves)
        {
            // Counteract vanilla set bonus (3 defense)
            player.statDefense -= 1;
            
            int goldCoins = 0;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].type == ItemID.GoldCoin)
                {
                    goldCoins += player.inventory[i].stack;
                }
            }

            player.GetDamage(DamageClass.Generic) += goldCoins * 0.01f;

            int platinumCoins = 0;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                if (player.inventory[i].type == ItemID.PlatinumCoin)
                {
                    platinumCoins += player.inventory[i].stack;
                }
            }

            player.GetDamage(DamageClass.Generic) += platinumCoins;
        }
    }
}