using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems;

public class GlobalNinjaArmor : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type is ItemID.NinjaHood or ItemID.NinjaShirt or ItemID.NinjaPants;
    }
    
    public override void SetDefaults(Item entity)
    {
        switch (entity.type)
        {
            case ItemID.NinjaHood:
                entity.defense = 2;
                break;
            case ItemID.NinjaShirt:
                entity.defense = 4;
                break;
            case ItemID.NinjaPants:
                entity.defense = 2;
                break;
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        switch (item.type)
        {
            case ItemID.NinjaHood:
                tooltips.Add(new TooltipLine(Mod, "NinjaHood", "10% increased damage"));
                tooltips.Add(new TooltipLine(Mod, "NinjaHood", "Grants danger sense effect"));
                break;
            case ItemID.NinjaShirt:
                if (Main.LocalPlayer.armor[0].type != ItemID.NinjaHood ||
                    Main.LocalPlayer.armor[2].type != ItemID.NinjaPants ||
                    Main.LocalPlayer.armor[1].type != ItemID.NinjaShirt)
                    tooltips.Add(new TooltipLine(Mod, "NinjaShirt", "10% chance to dodge damage"));
                break;
            case ItemID.NinjaPants:
                tooltips.Add(new TooltipLine(Mod, "NinjaPants", "25% increased movement speed"));
                tooltips.Add(new TooltipLine(Mod, "NinjaShirt", "Allows the wearer to double jump"));
                break;
        }
        
        if (Main.LocalPlayer.armor[0].type == ItemID.NinjaHood &&
            Main.LocalPlayer.armor[1].type == ItemID.NinjaShirt &&
            Main.LocalPlayer.armor[2].type == ItemID.NinjaPants)
        {
            tooltips.Remove(tooltips.Find(x => x.Text.StartsWith("Set bonus")));
            tooltips.Add(new TooltipLine(Mod, "NinjaArmorSet", "set bonus: 25% chance to dodge damage"));
        }
    }

    public override void UpdateEquip(Item item, Player player)
    {
        // counteract vanilla behavior
        player.GetCritChance(DamageClass.Generic) -= 0.03f;
        
        switch (item.type)
        {
            case ItemID.NinjaHood:
                player.GetDamage(DamageClass.Generic) += 0.1f;
                player.dangerSense = true;
                break;
            case ItemID.NinjaShirt:
                player.dashType = 1;
                break;
            case ItemID.NinjaPants:
                player.moveSpeed += 0.25f;
                player.GetJumpState(ExtraJump.CloudInABottle).Enable();
                break;
        }
        
        if (Main.LocalPlayer.armor[0].type == ItemID.NinjaHood &&
            Main.LocalPlayer.armor[1].type == ItemID.NinjaShirt &&
            Main.LocalPlayer.armor[2].type == ItemID.NinjaPants)
        {
            ///<see cref="NinjaEvadeChancePlayer"/>
            player.moveSpeed -= 0.2f;
        }
    }
}