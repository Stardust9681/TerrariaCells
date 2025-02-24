using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
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
    CustomAmmo,
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
    Ammo,

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
        FunkyModifier.ProjectileVelocity(1.50f),
        FunkyModifier.AttackSpeed(1.15f),
        FunkyModifier.AttackSpeed(1.25f),
        FunkyModifier.ManaCost(0.8f),
        FunkyModifier.ImbuedDamage(1.40f, 1.25f),
        FunkyModifier.FrenzyFire(1.40f, 0.75f),
        FunkyModifier.DamageOnDebuff(1.50f, BuffID.OnFire),
        FunkyModifier.DamageOnDebuff(1.50f, BuffID.Poisoned),
        FunkyModifier.CustomAmmo(ProjectileID.ExplosiveBullet),
        FunkyModifier.CustomAmmo(ProjectileID.BulletHighVelocity),
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
            FunkyModifierType.Size => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.ManaCost => [ModCategory.Mana],
            FunkyModifierType.ImbuedDamage => [ModCategory.Mana],
            FunkyModifierType.FrenzyFire => [ModCategory.Generic],
            FunkyModifierType.DamageOnDebuff => [ModCategory.Sword, ModCategory.Projectile],
            FunkyModifierType.CustomAmmo => [ModCategory.Ammo],
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

    public static FunkyModifier ProjectileVelocity(float speedMod) =>
        new(FunkyModifierType.ProjectileVelocity, speedMod);

    public static FunkyModifier AttackSpeed(float speedMod) =>
        new(FunkyModifierType.AttackSpeed, speedMod);

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

    public static FunkyModifier CustomAmmo(int ammoItemID) =>
        new FunkyModifier(FunkyModifierType.CustomAmmo, 0f) with
        {
            id = ammoItemID,
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
		string s = "";
        // s += $"[{modifierType}] ";
		float mod1 = (modifier - 1) * 100;
		string mod1Text = $"{mod1:+#.#;-#.#;0.0}";
		float mod2 = (secondaryModifier - 1) * 100;
		string mod2Text = $"{mod2:+#.#;-#.#;0.0}";
		switch (modifierType)
		{
			case FunkyModifierType.Damage:
				s += $"{mod1Text}% damage";
				break;
			case FunkyModifierType.ProjectileVelocity:
				s += $"{mod1Text}% projectile speed";
				break;
			case FunkyModifierType.AttackSpeed:
				s += $"{mod1Text}% attack speed";
				break;
			case FunkyModifierType.Size:
				s += $"{mod1Text}% weapon size";
				break;
			case FunkyModifierType.ManaCost:
				s += $"{mod1Text}% mana cost";
				break;
			case FunkyModifierType.ImbuedDamage:
				s += $"{mod1Text}% damage, {mod2Text}% mana cost";
				break;
			case FunkyModifierType.FrenzyFire:
				s += $"{mod1Text}% damage, {mod2Text}% attack speed";
				break;
			case FunkyModifierType.DamageOnDebuff:
				s += $"{mod1Text}% damage vs targets afflicted by {Terraria.Lang.GetBuffName(id)}";
				break;
			case FunkyModifierType.CustomAmmo:
                string localizedText = Terraria.Lang.GetProjectileName(id).Value;
                if (id == ProjectileID.ExplosiveBullet) {
                    localizedText = "Explosive Bullet";
                }
                if (id == ProjectileID.BulletHighVelocity) {
                    localizedText = "High Velocity Bullet";
                }
                s += $"Weapon fires {localizedText}s";
				break;
			case FunkyModifierType.ApplyDebuff:
				s += $"Inflict {Terraria.Lang.GetBuffName(id)} for {modifier/60:0.0} sec";
				break;
			case FunkyModifierType.CritsExplode:
				s += $"Critical strikes cause explosions";
				break;
			case FunkyModifierType.DropMoreMana:
				s += $"Enemies drop {mod1Text}% more Mana Stars when hit";
				break;
		}
		return s;
	}
}
