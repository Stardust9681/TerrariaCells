using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Common.Systems;

public class TooltipReorganization : ModSystem
{
    public List<string> tooltipOrganization =
    [ // this is initialized with the default list, do NOT modify the list here! modify the list in Load()
        "ItemName",
        "Favorite",
        "FavoriteDesc",
        "NoTransfer",
        "Social",
        "SocialDesc",
        "Damage",
        "CritChance",
        "Speed",
        "NoSpeedScaling",
        "SpecialSpeedScaling",
        "Knockback",
        "FishingPower",
        "NeedsBait",
        "BaitPower",
        "Equipable",
        "WandConsumes",
        "Quest",
        "Vanity",
        "Defense",
        "PickPower",
        "AxePower",
        "HammerPower",
        "TileBoost",
        "HealLife",
        "HealMana",
        "UseMana",
        "Placeable",
        "Ammo",
        "Consumable",
        "Material",
        "Tooltip#",
        "EtherianManaWarning",
        "WellFedExpert",
        "BuffTime",
        "OneDropLogo",
        "PrefixDamage",
        "PrefixSpeed",
        "PrefixCritChance",
        "PrefixUseMana",
        "PrefixSize",
        "PrefixShootSpeed",
        "PrefixKnockback",
        "PrefixAccDefense",
        "PrefixAccMaxMana",
        "PrefixAccCritChance",
        "PrefixAccDamage",
        "PrefixAccMoveSpeed",
        "PrefixAccMeleeSpeed",
        "SetBonus",
        "Expert",
        "Master",
        "JourneyResearch",
        "ModifiedByMods",
        "BestiaryNotes",
        "SpecialPrice",
        "Price",
    ];

    /// <summary>
    /// Tooltips created outside of this system that will still be allowed to be shown.
    /// </summary>
    internal List<string> tooltipWhitelist =
    [
        "ItemName",
        "Damage",
        // "CritChance",
        // "Speed",
        // "NoSpeedScaling",
        // "SpecialSpeedScaling",
        // "Knockback",
        // "Equipable",
        "Defense",
        "HealLife",
        "HealMana",
        "UseMana",
        "Consumable",
        // "Material", // maybe add ??
        "BuffTime",
        "SpecialPrice",
        "Price",
    ];

    public override void Load()
    {
        RegisterTooltip("ItemCategorization", "ItemName");
        RegisterTooltip("SkillCooldown", "Knockback");
        RegisterTooltip("Tooltip0", "Material");
        RegisterTooltip("Tooltip1", "Tooltip0");
        RegisterTooltip("Tooltip2", "Tooltip1");
        RegisterTooltip("FunkyModifier0", "OneDropLogo");
        RegisterTooltip("FunkyModifier1", "FunkyModifier0");
        RegisterTooltip("FunkyModifier2", "FunkyModifier1");
        RegisterTooltip("FunkyModifier3", "FunkyModifier2");
        RegisterTooltip("FunkyModifier4", "FunkyModifier3");
        RegisterTooltip("FunkyModifier5", "FunkyModifier4");
    }

    /// <summary>
    /// Adds a tooltip to the registration so that other tooltips can use it as an anchor point.
    ///
    /// The tooltip will be inserted into the registration right after the anchor if found.
    /// If nothing is found for anchor, an exception will be thrown.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="anchor"></param>
    public void RegisterTooltip(string name, string anchor = "Damage")
    {
        // this all is reliant on the fact that the ItemName tooltip exists for every item.
        // if an exception is found, this system may need to be reworked to accomodate for that edge case.
        int index = tooltipOrganization.FindIndex(x => x == anchor);
        if (index == -1)
        {
            // this could potentially get replaced by just inserted the item to the beginning of the list
            // which would have the added benefit of actively passing in null to put the registration in front.
            throw new Exception("Could not find the anchor " + anchor);
        }

        tooltipOrganization.Insert(index + 1, name);
    }

    public void InsertTooltip(TooltipLine tooltip, List<TooltipLine> tooltips)
    {
        // multiple linear time complexity operations may not be great,
        // but it's more optimal for ensuring that the list doesn't need to be constantly resorted.

        // actually its almost kind of n^2 time complexity? it runs tooltips.len * tooltipOrganization.len worst case
        int anchor = tooltipOrganization.FindIndex(x => x == tooltip.Name);
        while (anchor > -1)
        {
            int index = tooltips.FindIndex(x => x.Name == tooltipOrganization[anchor]);
            if (index == -1)
            {
                anchor -= 1;
                continue;
            }
            tooltips.Insert(index, tooltip);
            return;
        }
        tooltips.Add(tooltip);
    }
}

public class TooltipManager : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        TooltipReorganization reorganization = Mod.GetContent<TooltipReorganization>().First();

        foreach (TooltipLine oldTooltip in tooltips)
        {
            if (!reorganization.tooltipWhitelist.Contains(oldTooltip.Name))
            {
                oldTooltip.Hide();
            }
        }
        foreach (
            TooltipLine tooltip in Mod.GetContent<FunkyModifierItemModifier>()
                .First()
                .GetTooltips(item)
        )
        {
            reorganization.InsertTooltip(tooltip, tooltips);
        }
        foreach (
            TooltipLine tooltip in Mod.GetContent<VanillaReworksGlobalItem>()
                .First()
                .GetTooltips(item)
        )
        {
            reorganization.InsertTooltip(tooltip, tooltips);
        }
        foreach (
            TooltipLine tooltip in Mod.GetContent<AccessoryEffects>().First().GetTooltips(item)
        )
        {
            reorganization.InsertTooltip(tooltip, tooltips);
        }
        foreach (
            TooltipLine tooltip in Mod.GetContent<SkillSystemGlobalItem>().First().GetTooltips(item)
        )
        {
            reorganization.InsertTooltip(tooltip, tooltips);
        }

        tooltips.Sort(
            comparison: (x, y) =>
                reorganization.tooltipOrganization.FindIndex(a => a == x.Name)
                - reorganization.tooltipOrganization.FindIndex(a => a == y.Name)
        );
    }
}
