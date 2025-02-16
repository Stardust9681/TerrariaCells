using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.GlobalProjectiles;

namespace TerrariaCells.Common.GlobalItems;

public enum FunkyModifierType
{
    None,
    Damage,
    ProjectileVelocity,
    AttackSpeed,
    Size,
    ManaCost,

    // Increased mana consumption, massively increased damage
    // Can be negated for the opposite effect
    ImbuedDamage,
    DamageOnDebuff,
    CustomAmmo,
    ApplyDebuff,
    CritsExplode,
};

/// <summary>
/// Used to filter out modifiers that can or can't be given to specific weapons
///
/// These correlate to mechanics
/// </summary>
public enum ModCategory
{
    // Can be applied to all weapons
    Generic,

    // Applies to weapons that are swung overhead to deal damage.
    Sword,

    // Applied on weapons that fire projectiles
    Projectile,

    // Applied on weapons that consume mana
    Mana,

    // Applies to weapons that are capable of applying buffs/debuffs
    Buff,

    // Applies to weapons that consume ammo from the inventory
    Ammo,

    // dunno if these'll be used
    Spear,
    Flail,
};

public class FunkyModifierItemModifier : GlobalItem
{
    public override bool InstancePerEntity => true;

    private static readonly FunkyModifier[] modifierInitList =
    [
        FunkyModifier.Damage(1.5f),
        FunkyModifier.ProjectileVelocity(1.5f),
        FunkyModifier.AttackSpeed(1.5f),
        FunkyModifier.ManaCost(1 / 1.5f),
        // FunkyModifier.Size(4f),
        // FunkyModifier.(1.5f),
    ];

    /// <summary>
    /// Used to determine which modifiers are associated with each category, in order to filter out which
    /// modifiers a weapon can get depending on the categorizations the weapon has.
    /// </summary>
    private static readonly Dictionary<FunkyModifierType, ModCategory[]> modifierCategorizations =
        new (FunkyModifierType, ModCategory[])[]
        {
            (FunkyModifierType.None, []),
            (FunkyModifierType.Damage, [ModCategory.Generic]),
            (FunkyModifierType.ProjectileVelocity, [ModCategory.Projectile]),
            (FunkyModifierType.AttackSpeed, [ModCategory.Generic]),
            (FunkyModifierType.Size, [ModCategory.Sword, ModCategory.Projectile]),
            (FunkyModifierType.ManaCost, [ModCategory.Mana]),
            (FunkyModifierType.ImbuedDamage, [ModCategory.Mana]),
            (FunkyModifierType.DamageOnDebuff, [ModCategory.Sword, ModCategory.Projectile]),
            (FunkyModifierType.CustomAmmo, [ModCategory.Ammo]),
            (FunkyModifierType.ApplyDebuff, [ModCategory.Sword, ModCategory.Projectile]),
            (FunkyModifierType.CritsExplode, [ModCategory.Sword, ModCategory.Projectile]),
        }.ToDictionary();

    private static ModCategory[] ModifierCategorizations(FunkyModifierType modifierType)
    {
        return modifierType switch
        {
            FunkyModifierType.None => [],
            FunkyModifierType.Damage => [ModCategory.Generic],
            FunkyModifierType.ProjectileVelocity => [ModCategory.Projectile],
            FunkyModifierType.AttackSpeed => [ModCategory.Generic],
            FunkyModifierType.Size => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.ManaCost => [ModCategory.Mana],
            FunkyModifierType.ImbuedDamage => [ModCategory.Mana],
            FunkyModifierType.DamageOnDebuff => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.CustomAmmo => [ModCategory.Ammo],
            FunkyModifierType.ApplyDebuff => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.CritsExplode => [ModCategory.Sword, ModCategory.Projectile],
            _ => throw new System.Exception("Could not find enum variant: " + modifierType),
        };
    }

    private static readonly Dictionary<short, ModCategory[]> weaponCategorizations = new (
        short,
        ModCategory[]
    )[]
    {
        (ItemID.PearlwoodSword, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.BreakerBlade, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.CopperShortsword, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.FieryGreatsword, [ModCategory.Sword]),
        (ItemID.VolcanoSmall, [ModCategory.Sword]),
        (ItemID.VolcanoLarge, [ModCategory.Sword]),
        (ItemID.Excalibur, [ModCategory.Sword]),
        (ItemID.NightsEdge, [ModCategory.Sword]),
        (ItemID.FetidBaghnakhs, [ModCategory.Sword]),
        (ItemID.Starfury, [ModCategory.Sword, ModCategory.Projectile]),
        (ItemID.TerraBlade, [ModCategory.Sword, ModCategory.Projectile]),
        (ItemID.ThunderSpear, [ModCategory.Projectile]),
        (ItemID.Sunfury, [ModCategory.Projectile]),
        (ItemID.GolemFist, [ModCategory.Projectile]),
        //
        (ItemID.PhoenixBlaster, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.SniperRifle, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.OnyxBlaster, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.Minishark, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.WoodenBow, [ModCategory.Projectile, ModCategory.Ammo]),
        (ItemID.IceBow, [ModCategory.Projectile]),
        (ItemID.PulseBow, [ModCategory.Projectile]),
        (ItemID.RocketLauncher, [ModCategory.Projectile]),
        (ItemID.GrenadeLauncher, [ModCategory.Projectile]),
        (ItemID.StarCannon, [ModCategory.Projectile]),
        //
        (ItemID.Toxikarp, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.DemonScythe, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.Flamelash, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.ShadowbeamStaff, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.VenomStaff, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.FlowerofFrost, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.InfernoFork, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.StaffofEarth, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.HeatRay, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.WaterBolt, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.EmeraldStaff, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.LaserRifle, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.SharpTears, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.BubbleGun, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.BookofSkulls, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.AleThrowingGlove, [ModCategory.Projectile, ModCategory.Mana]),
    }.ToDictionary();

    public FunkyModifier[] modifiers;

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
                    "Funky Modifier: " + modifier.modifierType.ToString() + $"[{modifier.modifier}]"
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

    public override bool Shoot(
        Item item,
        Player player,
        EntitySource_ItemUse_WithAmmo source,
        Vector2 position,
        Vector2 velocity,
        int type,
        int damage,
        float knockback
    )
    {
        return true;
    }
}

public class ProjectileFunker : GlobalProjectile
{
    public FunkyModifier[] modifiersOnSourceItem;

    public override bool InstancePerEntity => true;

    public override void SetDefaults(Projectile entity)
    {
        bool tryGetSource = entity.TryGetGlobalProjectile<SourceGlobalProjectile>(
            out var sourceGlobal
        );
        if (!tryGetSource)
        {
            return;
        }

        if (sourceGlobal.itemSource == null)
        {
            return;
        }

        bool tryGetModifier = sourceGlobal.itemSource.TryGetGlobalItem<FunkyModifierItemModifier>(
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
        if (!projectile.TryGetGlobalProjectile(out ProjectileFunker funker))
        {
            return;
        }

        if (funker.modifiersOnSourceItem == null)
        {
            return;
        }

        foreach (FunkyModifier modifier in funker.modifiersOnSourceItem)
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
    ) { }

    public override void OnHitNPC(
        Projectile projectile,
        NPC target,
        NPC.HitInfo hit,
        int damageDone
    ) { }
}

/// <summary>
/// Uses *incredibly* generic names for variables that serve different purposes depending on the modifier.
/// See the documentation for each field to get an idea of what each field is used for. (The lists will likely not be kept up to date)
/// Ideally, you can see the Modifier constructors to get an idea of what each field is used for
/// by the name of the parameters that the constructors take.
/// </summary>
public struct FunkyModifier(FunkyModifierType type, float modifier)
{
    public FunkyModifierType modifierType = type;

    /// <summary>
    /// used for Damage, ProjectileVelocity, AttackSpeed, Size, ManaCost, ImbuedDamage (damage), DamageOnDebuff (damage)
    /// </summary>
    public float modifier = modifier;

    public int intModifier = 0;

    /// <summary>
    /// Unlike modifier, this field has a default value of 0.0f.
    ///
    /// Used for ImbuedDamage (cost)
    /// </summary>
    public float secondaryModifier = 0f;

    /// <summary>
    /// used for DamageOnDebuff (BuffID), CustomAmmo (ItemID), Apply Debuff (BuffID).
    /// </summary>
    public int id = 0;

    public static FunkyModifier Damage(float damageMod) => new(FunkyModifierType.Damage, damageMod);

    public static FunkyModifier ProjectileVelocity(float speedModifier) =>
        new(FunkyModifierType.ProjectileVelocity, speedModifier);

    public static FunkyModifier AttackSpeed(float speedModifier) =>
        new(FunkyModifierType.AttackSpeed, speedModifier);

    /// <summary>
    /// Note for projectiles! Projectile hitboxes can only be flatly incrememnted by integers
    /// since their Rect's use int's, so keep that in mind when working with that.
    /// </summary>
    /// <param name="flatSizeModifier"></param>
    /// <returns></returns>
    public static FunkyModifier Size(float flatSizeModifier) =>
        new FunkyModifier(FunkyModifierType.Size, flatSizeModifier);

    public static FunkyModifier ManaCost(float costModifier) =>
        new(FunkyModifierType.ManaCost, costModifier);
}
