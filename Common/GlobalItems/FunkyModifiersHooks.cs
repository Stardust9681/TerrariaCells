using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using TerrariaCells.Common.GlobalProjectiles;
using TerrariaCells.Common.Items;

namespace TerrariaCells.Common.GlobalItems;

/// <summary>
/// Just adds a whole bunch of hooks into item functionality
/// feel free to add on to the switch cases for new modifiers, thats why theyre there.
///
/// This portion contains the logic of the modifier system, use this when adding new modifier types.
/// </summary>
public partial class FunkyModifierItemModifier : GlobalItem
{
    public override bool InstancePerEntity => true;

    internal FunkyModifier[] modifiers;

    // ranges given here are used inclusively, ie 0..1 is a range from 0..1, as opposed to exclusive ranges that Main.rand takes
    public static readonly (int, int)[] modifierQuantityRangesPerTier =
    [
        (0, 0), // tier 0 isnt possible?
        (0, 1),
        (0, 2),
        (1, 3),
        (2, 3),
        (3, 3),
    ];

    public static bool CanReceiveMods(int itemType)
    {
        //!weaponCategorizations.TryGetValue((short)itemType, out var categorizations)
        return weaponCategorizations.ContainsKey((short)itemType);
    }
    internal static FunkyModifier[] GetModPool(int itemType)
    {
        if (!weaponCategorizations.TryGetValue((short)itemType, out var categorizations))
        {
            ModContent.GetInstance<TerrariaCells>().Logger.Error($"Could not find modifier categorization for {itemType}");
            return System.Array.Empty<FunkyModifier>();
        }

        return modifierInitList
            .Where(checking =>
                ModifierCategorizations(checking.modifierType)
                    .Any(modifierFilter =>
                        modifierFilter == ModCategory.Generic
                        || categorizations.Contains(modifierFilter)
                    )
            )
            .ToArray();
    }
    internal static (int Min, int Max) GetModCount(int level)
    {
        return level switch
        {
            //If this ain't the most cursed switch statement you ever saw....
            <= 0 => (0, 0),
            1 => (0, 1),
            2 => (0, 2),
            3 => (1, 3),
            4 => (2, 3),
            >= 5 => (3, 3),
        };
    }
    internal static FunkyModifier[] PickMods(int itemType, int level)
    {
        (int min, int max) = GetModCount(level);
        int modifierCount = Main.rand.Next(min, max + 1); // offset by one to make inputs inclusive

        FunkyModifier[] pool = GetModPool(itemType);
        if (pool.Length < modifierCount) return pool;

        FunkyModifier[] mods = new FunkyModifier[modifierCount];
        for (int i = 0; i < modifierCount; i++)
        {
            FunkyModifier funkyModifier = Main.rand.Next(pool);
            if (mods.Contains(funkyModifier))
            {
                continue;
            }
            mods[i] = funkyModifier;
        }
        return mods;
    }
    public static void Reforge(Item item, int level)
    {
        //(int min, int max) = GetModCount(level);
        //int modifierCount = Main.rand.Next(min, max+1); // offset by one to make inputs inclusive

        FunkyModifierItemModifier funkyModifiers = item.GetGlobalItem<FunkyModifierItemModifier>();
        //funkyModifiers.modifiers = new FunkyModifier[modifierCount];

        funkyModifiers.modifiers = PickMods(item.type, level);
    }
    public static void Reforge(Item item)
    {
        if (!item.TryGetGlobalItem<TierSystemGlobalItem>(out var tierSystem))
        {
            return;
        }
        Reforge(item, tierSystem.itemLevel);
    }

    public override void SetDefaults(Item item)
    {
        if(CanReceiveMods(item.type))
            Reforge(item);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        FunkyModifier[] array = funkyModifiers.modifiers ?? [];
        for (int i = 0; i < array.Length; i++)
        {
            FunkyModifier modifier = array[i];
            int index = tooltips.FindIndex(x => x.Name == "PrefixDamage");
            if (index == -1)
            {
                tooltips.Add(
                    new TooltipLine(Mod, "FunkyModifier" + i.ToString(), modifier.ToString())
                    {
                        IsModifier = true,
                    }
                );
                continue;
            }
            tooltips.Insert(
                index,
                new TooltipLine(Mod, "FunkyModifier" + i.ToString(), modifier.ToString())
                {
                    IsModifier = true,
                }
            );
        }
    }

    public List<TooltipLine> GetTooltips(Item item)
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return [];
        }
        return (funkyModifiers.modifiers ?? [])
            .Select(
                (x, i) =>
                    new TooltipLine(Mod, "FunkyModifier" + i.ToString(), x.ToString())
                    {
                        IsModifier = true,
                    }
            )
            .ToList();
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            switch (modifier.modifierType)
            {
                case FunkyModifierType.Damage:
                {
                    damage *= modifier.modifier;
                    break;
                }
                case FunkyModifierType.ImbuedDamage:
                {
                    damage *= modifier.modifier;
                    break;
                }
                case FunkyModifierType.FrenzyFire:
                {
                    damage *= modifier.modifier;
                    break;
                }
            }
        }
    }

    public override void ModifyManaCost(Item item, Player player, ref float reduce, ref float mult)
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            switch (modifier.modifierType)
            {
                case FunkyModifierType.ManaCost:
                {
                    mult *= modifier.modifier;
                    break;
                }
                case FunkyModifierType.ImbuedDamage:
                {
                    mult *= modifier.secondaryModifier;
                    break;
                }
            }
        }
    }

    public override void ModifyItemScale(Item item, Player player, ref float scale)
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            switch (modifier.modifierType)
            {
                case FunkyModifierType.Size:
                {
                    scale *= modifier.modifier;
                    break;
                }
            }
        }
    }

    public override float UseSpeedMultiplier(Item item, Player player)
    {
        float scale = 1f;
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return scale;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            switch (modifier.modifierType)
            {
                case FunkyModifierType.AttackSpeed:
                {
                    scale *= modifier.modifier;
                    break;
                }
                case FunkyModifierType.FrenzyFire:
                {
                    scale *= modifier.secondaryModifier;
                    break;
                }
            }
        }
        return scale;
    }

    public override void ModifyHitNPC(
        Item item,
        Player player,
        NPC target,
        ref NPC.HitModifiers modifiers
    )
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            switch (modifier.modifierType)
            {
                case FunkyModifierType.DamageOnDebuff:
                {
                    if (target.HasBuff(modifier.id))
                    {
                        modifiers.SourceDamage *= modifier.modifier;
                    }
                    break;
                }
            }
        }
    }

    public override void ModifyShootStats(
        Item item,
        Player player,
        ref Vector2 position,
        ref Vector2 velocity,
        ref int type,
        ref int damage,
        ref float knockback
    )
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            switch (modifier.modifierType)
            {
                case FunkyModifierType.ProjectileVelocity:
                {
                    velocity *= modifier.modifier;
                    break;
                }
                case FunkyModifierType.CustomAmmoBullet:
                case FunkyModifierType.CustomAmmoArrow:
                case FunkyModifierType.CustomAmmoRocket:
                {
                    type = modifier.id;
                    break;
                }
            }
        }
    }

    public override void NetSend(Item item, BinaryWriter writer)
    {
        var modItem = item.GetGlobalItem(this);
        var modifiers = modItem.modifiers;
        writer.Write((byte)modifiers.Length);
        foreach (var modifier in modifiers)
        {
            writer.Write(modifier.id);
            writer.Write((ushort)modifier.modifierType);
            writer.Write(modifier.intModifier);
            writer.Write(modifier.modifier);
            writer.Write(modifier.secondaryModifier);
        }
    }
    public override void NetReceive(Item item, BinaryReader reader)
    {
        var modItem = item.GetGlobalItem(this);
        int count = reader.ReadByte();
        modItem.modifiers = new FunkyModifier[count];
        for (int i = 0; i < count; i++)
        {
            modItem.modifiers[i] = new FunkyModifier()
            {
                id = reader.ReadInt32(),
                modifierType = (FunkyModifierType)reader.ReadUInt16(),
                intModifier = reader.ReadInt32(),
                modifier = reader.ReadSingle(),
                secondaryModifier = reader.ReadSingle(),
            };
        }
    }
    public override void SaveData(Item item, TagCompound tag)
    {
        try
        {
            if (modifiers is not null)
            {
                tag.Add("modifiers.count", modifiers.Length);
                for (int i = 0; i < modifiers.Length; i++)
                {
                    tag.Add($"id{i}", modifiers[i].id);
                    tag.Add($"type{i}", (ushort)modifiers[i].modifierType);
                    tag.Add($"imod{i}", modifiers[i].intModifier);
                    tag.Add($"mod{i}", modifiers[i].modifier);
                    tag.Add($"smod{i}", modifiers[i].secondaryModifier);
                }
            }
        }
        catch (System.Exception x) { }
    }
    public override void LoadData(Item item, TagCompound tag)
    {
        try
        {
            modifiers = new FunkyModifier[tag.Get<int>("modifiers.count")];
            for (int i = 0; i < modifiers.Length; i++)
            {
                modifiers[i] = new FunkyModifier()
                {
                    id = tag.Get<int>($"id{i}"),
                    modifierType = (FunkyModifierType)tag.Get<ushort>($"type{i}"),
                    intModifier = tag.Get<int>($"imod{i}"),
                    modifier = tag.Get<float>($"mod{i}"),
                    secondaryModifier = tag.Get<float>($"smod{i}"),
                };
            }
        }
        catch (System.Exception x)
        {
            modifiers = Array.Empty<FunkyModifier>();
        }
    }
}

public partial class ProjectileFunker : GlobalProjectile
{
    public ProjectileFunker instance;

    public bool SetInstance(Projectile projectile)
    {
        if (!projectile.TryGetGlobalProjectile(out ProjectileFunker funker))
        {
            return false;
        }
        if (funker.modifiersOnSourceItem == null)
        {
            // Main.NewText("Could not find instance");
            return false;
        }
        instance = funker;
        return true;
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is not IEntitySource_WithStatsFromItem entitySource)
        {
            return;
        }

        bool tryGetModifier = entitySource.Item.TryGetGlobalItem<FunkyModifierItemModifier>(
            out var modifiers
        );
        if (!tryGetModifier)
        {
            return;
        }
        modifiersOnSourceItem = modifiers.modifiers;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        if (!SetInstance(projectile))
            return;

        foreach (FunkyModifier modifier in instance.modifiersOnSourceItem)
        {
            if (modifier.modifierType == FunkyModifierType.Size)
            {
                hitbox.Inflate((int)modifier.modifier, (int)modifier.modifier);
            }
        }
    }

    public override void ModifyHitNPC(
        Projectile projectile,
        NPC target,
        ref NPC.HitModifiers modifiers
    )
    {
        if (!SetInstance(projectile))
            return;

        foreach (FunkyModifier funkyModifier in instance.modifiersOnSourceItem)
        {
            switch (funkyModifier.modifierType)
            {
                case FunkyModifierType.DamageOnDebuff:
                {
                    if (target.HasBuff(funkyModifier.id))
                    {
                        modifiers.SourceDamage *= funkyModifier.modifier;
                    }
                    return;
                }
            }
        }
    }

    public override void OnHitNPC(
        Projectile projectile,
        NPC target,
        NPC.HitInfo hit,
        int damageDone
    )
    {
        if (!SetInstance(projectile))
            return;

        foreach (FunkyModifier funkyModifier in instance.modifiersOnSourceItem)
        {
            switch (funkyModifier.modifierType)
            {
                case FunkyModifierType.ApplyDebuff:
                {
                    //target.AddBuff(funkyModifier.id, (int)funkyModifier.modifier);
					GlobalNPCs.BuffNPC.AddBuff(target, funkyModifier.id, (int)funkyModifier.modifier, damageDone);
                    break;
                }
            }
        }
    }
}
