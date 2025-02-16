using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalProjectiles;

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

    public override void SetDefaults(Item item)
    {
        if (!weaponCategorizations.TryGetValue((short)item.netID, out var categorizations))
        {
            return;
        }

        int modifierCount = 1;
        FunkyModifierItemModifier funkyModifiers = item.GetGlobalItem<FunkyModifierItemModifier>();
        funkyModifiers.modifiers = new FunkyModifier[modifierCount];

        FunkyModifier[] modifierPool = modifierInitList
            .Where(checking =>
                ModifierCategorizations(checking.modifierType)
                    .Any(modifierFilter =>
                        modifierFilter == ModCategory.Generic
                        || categorizations.Contains(modifierFilter)
                    )
            )
            .ToArray();

        for (int i = 0; i < modifierCount; i++)
        {
            FunkyModifier funkyModifier = modifierPool[Main.rand.Next(modifierPool.Length)];
            if (funkyModifiers.modifiers.Contains(funkyModifier))
            {
                continue;
            }
            funkyModifiers.modifiers[i] = funkyModifier;
        }
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!item.TryGetGlobalItem(out FunkyModifierItemModifier funkyModifiers))
        {
            return;
        }
        foreach (FunkyModifier modifier in funkyModifiers.modifiers ?? [])
        {
            tooltips.Add(
                new TooltipLine(
                    Mod,
                    "FunkyModifier",
                    "Funky Modifier: "
                        + modifier.modifierType.ToString()
                        + $" [{modifier.modifier} {modifier.secondaryModifier} {modifier.id}]"
                )
            );
        }
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
                case FunkyModifierType.CustomAmmo:
                {
                    type = modifier.id;
                    break;
                }
            }
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
                    target.AddBuff(funkyModifier.id, (int)funkyModifier.modifier);
                    break;
                }
            }
        }
    }
}
