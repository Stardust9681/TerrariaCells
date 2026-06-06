using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaCells.Common.GlobalItems;

// if you're looking to add a new weapon, see FunkyModifierItemModifier.weaponCategorizations
// if you're looking to add a variant of an existing modifier, see FunkyModifierItemModifier.modifierInitList
// to add a modifier:
// add the enum variant here
//      (create a new category for your functionality in ModCategory if you want)
// categorize the modifier in FunkyModifierItemModifier.ModifierCategorizations
// create construction method(s) in FunkyModifier
// use those methods in FunkyModifierItemModifier.modifierInitList
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
    FrenzyFire,
    DamageOnDebuff,
    ExtraAmmo,
    CustomAmmoBullet,
    CustomAmmoArrow,
    CustomAmmoRocket,
    ApplyDebuff,
    CritsExplode,
    DropMoreMana,
};

/// <summary>
/// Used to filter out modifiers that can or can't be given to specific weapons
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
    AmmoBullet,
    AmmoArrow,
    AmmoRocket,

    // dunno if these'll be used
    Spear,
    Flail,
};

/// <summary>
/// Just adds a whole bunch of hooks into item functionality
/// feel free to add on to the switch cases for new modifiers, thats why theyre there
///
/// This portion contains
/// </summary>
public partial class FunkyModifierItemModifier : GlobalItem
{
    // public override bool InstancePerEntity => true;

    private static readonly FunkyModifier[] modifierInitList =
    [
        FunkyModifier.Damage(1.15f),
        FunkyModifier.Damage(1.30f),
        FunkyModifier.Size(1.20f),
        FunkyModifier.ProjectileVelocity(1.50f),
        FunkyModifier.AttackSpeed(1.15f),
        FunkyModifier.AttackSpeed(1.25f),
        FunkyModifier.ManaCost(0.8f),
        FunkyModifier.ImbuedDamage(1.40f, 1.25f),
        // FunkyModifier.FrenzyFire(1.40f, 0.75f),
        FunkyModifier.DamageOnDebuff(1.50f, BuffID.OnFire),
        FunkyModifier.DamageOnDebuff(1.50f, BuffID.Poisoned),
        FunkyModifier.ExtraAmmo(1.50f),
        FunkyModifier.CustomBulletAmmo(ProjectileID.ExplosiveBullet, ItemID.ExplodingBullet),
        FunkyModifier.CustomBulletAmmo(ProjectileID.BulletHighVelocity, ItemID.HighVelocityBullet),
        FunkyModifier.ApplyDebuff(BuffID.Poisoned, 30f),
        FunkyModifier.ApplyDebuff(BuffID.OnFire),
        FunkyModifier.DropMoreMana(2),
        // FunkyModifier.CritsExplode(),
    ];

    /// <summary>
    /// Used to determine which modifiers are associated with each category, in order to filter out which
    /// modifiers a weapon can get depending on the categorizations the weapon has.
    ///
    /// If you need a new category, don't hesistate to add one.
    /// </summary>
    private static ModCategory[] ModifierCategorizations(FunkyModifierType modifierType)
    {
        return modifierType switch
        {
            FunkyModifierType.None => [],
            FunkyModifierType.Damage => [ModCategory.Generic],
            FunkyModifierType.ProjectileVelocity => [ModCategory.Projectile],
            FunkyModifierType.AttackSpeed => [ModCategory.Generic],
            FunkyModifierType.Size => [ModCategory.Sword],
            FunkyModifierType.ManaCost => [ModCategory.Mana],
            FunkyModifierType.ImbuedDamage => [ModCategory.Mana],
            FunkyModifierType.FrenzyFire => [ModCategory.Generic],
            FunkyModifierType.DamageOnDebuff => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.ExtraAmmo => [ModCategory.AmmoBullet, ModCategory.AmmoRocket],
            FunkyModifierType.CustomAmmoBullet => [ModCategory.AmmoBullet],
            FunkyModifierType.CustomAmmoArrow => [ModCategory.AmmoArrow],
            FunkyModifierType.CustomAmmoRocket => [ModCategory.AmmoRocket],
            FunkyModifierType.ApplyDebuff => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.CritsExplode => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.DropMoreMana => [ModCategory.Mana],
            _ => throw new Exception("Could not find enum variant: " + modifierType),
        };
    }

    private static readonly Dictionary<short, ModCategory[]> weaponCategorizations = new (
        short,
        ModCategory[]
    )[]
    {
		(ItemID.PlatinumBroadsword, [ModCategory.Sword]),
		//Removed:
		//(ItemID.PearlwoodSword, [ModCategory.Sword]),
        (ItemID.BreakerBlade, [ModCategory.Sword]),
        (ItemID.CopperShortsword, [ModCategory.Sword]),
        (ItemID.FieryGreatsword, [ModCategory.Sword]),
        (ItemID.VolcanoSmall, [ModCategory.Sword]),
        (ItemID.VolcanoLarge, [ModCategory.Sword]),
        (ItemID.Excalibur, [ModCategory.Sword]),
        (ItemID.NightsEdge, [ModCategory.Sword]),
        (ItemID.FetidBaghnakhs, [ModCategory.Sword]),
        (ItemID.Gladius, [ModCategory.Sword]),
        (ItemID.SawtoothShark, [ModCategory.Sword]),
        (ItemID.Katana, [ModCategory.Sword]),
        (ItemID.Starfury, [ModCategory.Sword, ModCategory.Projectile]),
        (ItemID.TerraBlade, [ModCategory.Sword, ModCategory.Projectile]),
        (ItemID.ThunderSpear, [ModCategory.Projectile]),
        (ItemID.Sunfury, [ModCategory.Projectile]),
        (ItemID.GolemFist, [ModCategory.Projectile]),
        //
        (ItemID.PhoenixBlaster, [ModCategory.Projectile, ModCategory.AmmoBullet]),
        (ItemID.SniperRifle, [ModCategory.Projectile, ModCategory.AmmoBullet]),
        (ItemID.OnyxBlaster, [ModCategory.Projectile, ModCategory.AmmoBullet]),
        (ItemID.Minishark, [ModCategory.Projectile, ModCategory.AmmoBullet]),
        (ItemID.WoodenBow, [ModCategory.Projectile, ModCategory.AmmoArrow]),
        (ItemID.PlatinumBow, [ModCategory.Projectile, ModCategory.AmmoArrow]),
        (ItemID.IceBow, [ModCategory.Projectile, ModCategory.AmmoArrow]),
        (ItemID.PulseBow, [ModCategory.Projectile, ModCategory.AmmoArrow]),
        (ItemID.RocketLauncher, [ModCategory.Projectile, ModCategory.AmmoRocket]),
        (ItemID.GrenadeLauncher, [ModCategory.Projectile, ModCategory.AmmoRocket]),
        (ItemID.StarCannon, [ModCategory.Projectile]),
        (ItemID.AleThrowingGlove, [ModCategory.Projectile]),
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
        (ItemID.AmberStaff, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.RubyStaff, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.LaserRifle, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.SharpTears, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.BubbleGun, [ModCategory.Projectile, ModCategory.Mana]),
        (ItemID.BookofSkulls, [ModCategory.Projectile, ModCategory.Mana]),
    }.ToDictionary();
}

public partial class ProjectileFunker : GlobalProjectile
{
    internal FunkyModifier[] modifiersOnSourceItem;

    public override bool InstancePerEntity => true;
}

/// <summary>
/// Uses *incredibly* generic names for variables that serve different purposes depending on the modifier.
/// See the documentation for each field to get an idea of what each field is used for. (The lists will likely not be kept up to date)
/// Ideally, you can see the Modifier constructors to get an idea of what each field is used for
/// by the name of the parameters that the constructors take.
/// </summary>
public struct FunkyModifier(FunkyModifierType type, float modifier)
{
    private static Dictionary<FunkyModifierType, LocalizedText> _localization;
    internal static void LoadLocalization(Mod mod)
    {
        _localization = new Dictionary<FunkyModifierType, LocalizedText>();
        foreach(FunkyModifierType type in Enum.GetValues<FunkyModifierType>())
        {
            //mod1Text = {0}
            //mod2Text = {1}
            //id-Localized = {2}
            //modifier = {3}
            string @default = type switch
            {
                FunkyModifierType.Damage => "{0}% damage",
                FunkyModifierType.ProjectileVelocity => "{0}% projectile speed",
                FunkyModifierType.AttackSpeed => "{0}% attack speed",
                FunkyModifierType.Size => "{0}% weapon size",
                FunkyModifierType.ManaCost => "{0}% mana cost",
                FunkyModifierType.ImbuedDamage => "{0}% damage, {1}% mana cost",
                FunkyModifierType.FrenzyFire => "{0}% damage, {0}% attack speed",
                FunkyModifierType.DamageOnDebuff => "{0}% damage vs targets afflicted by {2}",// {Terraria.Lang.GetBuffName(id)}";
                FunkyModifierType.ExtraAmmo => "{0}% more ammo",
                FunkyModifierType.CustomAmmoBullet => "Weapon fires {2}",
                    // string localizedText = Terraria.Lang.GetProjectileName(id).Value;
                    // if (id == ProjectileID.ExplosiveBullet) {
                    //     localizedText = "Explosive Bullet";
                    // }
                    // if (id == ProjectileID.BulletHighVelocity) {
                    //     localizedText = "High Velocity Bullet";
                    // }
                FunkyModifierType.ApplyDebuff => "Inflicts {2} for {3} seconds",
                    //"Inflict {Terraria.Lang.GetBuffName(id)} for {modifier/60:0.0} sec";
                FunkyModifierType.CritsExplode => "Critical strikes are explosive",
                FunkyModifierType.DropMoreMana => "Enemies drop {0}% more Mana Stars when hit",
                _ => string.Empty,
            };
            if (string.IsNullOrEmpty(@default)) continue;
            _localization.Add(type, Language.GetOrRegister(mod.GetLocalizationKey($"Tooltips.Modifiers.{type.ToString()}"), () => @default));
        }
    }
    
    public FunkyModifierType modifierType = type;

    /// <summary>
    /// used for Damage, ProjectileVelocity, AttackSpeed, Size, ManaCost, ExtraAmmo, ImbuedDamage (damage), DamageOnDebuff (damage)
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

    public static FunkyModifier ProjectileVelocity(float speedMod) =>
        new(FunkyModifierType.ProjectileVelocity, speedMod);

    public static FunkyModifier AttackSpeed(float speedMod) =>
        new(FunkyModifierType.AttackSpeed, speedMod);
    
    public static FunkyModifier ExtraAmmo(float ammoMod) =>
        new(FunkyModifierType.ExtraAmmo, ammoMod);

    /// <summary>
    /// Note for projectiles! Projectile hitboxes can only be flatly incrememnted by integers
    /// since their Rect's use int's, so keep that in mind when working with that.
    /// </summary>
    /// <param name="sizeMod"></param>
    /// <returns></returns>
    public static FunkyModifier Size(float sizeMod) => new(FunkyModifierType.Size, sizeMod);

    public static FunkyModifier ManaCost(float manaCostMod) =>
        new(FunkyModifierType.ManaCost, manaCostMod);

    public static FunkyModifier ImbuedDamage(float damageMod, float manaMod) =>
        new FunkyModifier(FunkyModifierType.ImbuedDamage, damageMod) with
        {
            secondaryModifier = manaMod,
        };

    public static FunkyModifier FrenzyFire(float speedMod, float damageMod) =>
        new FunkyModifier(FunkyModifierType.FrenzyFire, damageMod) with
        {
            secondaryModifier = speedMod,
        };

    public static FunkyModifier DamageOnDebuff(float damageMod, int buffID) =>
        new FunkyModifier(FunkyModifierType.DamageOnDebuff, damageMod) with
        {
            id = buffID,
        };

    public static FunkyModifier CustomBulletAmmo(int projID, int ammoID) =>
        new FunkyModifier(FunkyModifierType.CustomAmmoBullet, 0f) with
        {
            id = ammoID,
            intModifier = projID,
        };

    public static FunkyModifier ApplyDebuff(int buffID, float timeSeconds = 10f) =>
        new FunkyModifier(FunkyModifierType.ApplyDebuff, timeSeconds * 60) with
        {
            id = buffID,
        };

    public static FunkyModifier CritsExplode() => new(FunkyModifierType.CritsExplode, 0f);

    public static FunkyModifier DropMoreMana(int manaDropMultiplier) =>
        new(FunkyModifierType.DropMoreMana, manaDropMultiplier);

	public override string ToString()
	{
		float mod1 = (modifier - 1) * 100;
		string mod1Text = $"{mod1:+#.#;-#.#;0.0}";
		float mod2 = (secondaryModifier - 1) * 100;
        string mod2Text = $"{mod2:+#.#;-#.#;0.0}";
        string idText = modifierType switch
        {
            FunkyModifierType.ApplyDebuff => Terraria.Lang.GetBuffName(id),
            FunkyModifierType.DamageOnDebuff => Terraria.Lang.GetBuffName(id),

            FunkyModifierType.CustomAmmoBullet => Terraria.Lang.GetItemNameValue(id),
            FunkyModifierType.CustomAmmoArrow => Terraria.Lang.GetItemNameValue(id),
            FunkyModifierType.CustomAmmoRocket => Terraria.Lang.GetItemNameValue(id),

            _ => string.Empty,
        };
        string modifierText = modifierType switch
        {
            FunkyModifierType.ApplyDebuff => $"{modifier / 60:0.0}",

            _ => string.Empty,
        };
        return _localization.GetValueOrDefault(modifierType, LocalizedText.Empty).Format(mod1Text, mod2Text, idText, modifierText);
	}
}