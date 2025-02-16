// TODO: Add namespaces so that it can be built
// I can't access the tModLoader package for building (since im away from my pc), so I cant test for it myself right now

namespace TerrariaCells.Common.GlobalItems;

public class FunkyModifiers : GlobalItem {
    public static Modifier[] modifiers = [
        ModifierType.IncreaseDamage.Flat(20f),
        ModifierType.IncreaseDamage.Flat(40f, BuffID.OnFire)
    ];
    public override void SetDefaults(Item item) {
        int modifiers = (int)(Main.rand.Next() * 4);
        
        foreach (var modifier in modifiers) {

        }
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {

    }
}

public enum ModifierType {
    /// <summary>
    /// if buff is given, will only give damage increase when attacking target with the buff
    /// </summary>
    Damage,
    ProjectileVelocity,
    AttackSpeed,
    Size,
    ManaCost,
    /// <summary>
    /// Increased mana consumption, massively increased damage
    /// Can be negated for the opposite effect
    /// </summary> 
    ImbuedDamage,
    DamageOnDebuff,
    ExplosiveBullets,
    HighVelocityBullets,
    ApplyDebuff,
    CritsExplode,
};

/// <summary>
/// Used to filter out modifiers that can or can't be given to specific weapons
///
/// These don't directly co
/// </summary>
public enum ModifierCategorization {
    Swords,
    Mana,

};

public static class Extensions {
    public static ModifierType minPassing = Grades.D;
    public static Modifier Modifier(float amount, int buff = 0)
    {
        return new Modifier() with {
            buff = buff,
            modifier = amount, 
            ModifierType = modifier,
        };
    }
    
    public static Modifier ApplyBuff(int buff)
    {
        return new Modifier() with {
            buff = buff,
            flat_bonus = amount, 
            ModifierType = modifier,
        };

    }
};

public struct Modifier {
    public ModifierType ModifierType;
    public int buff;
    public float modifier = 1.0;
    public float flat_bonus = 0.0;

    public static Modifier ApplyBuff(int buff)
    {
        return Modifier() with
        {
            buff = buff

        };
    }

    public static Modifier Modifier(float ) {

    }
    public static Modifier Flat() {

    } 
}