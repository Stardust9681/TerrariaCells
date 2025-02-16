
namespace TerrariaCells.Common.GlobalItems;

public class FunkyModifiers : GlobalItem {
    public static Modifier[] modifiers = [
        new Modifier(ModifierType.IncreaseDamage, 15f)
    ]; 
    public override void SetDefaults(Item item) {
        int modifiers = (int)(Main.rand.Next() * 4);
        
        switch (item.type) {
        }
    }
}

public enum ModifierType {
    IncreaseDamage,
    ApplyDebuff,
    DamageModOnBuff
};

public static class Extensions {
    public static ModifierType minPassing = Grades.D;
    public static Modifier Modifier(this ModifierType modifier)
    {
    }
    public static Modifier Flat(this ModifierType modifier)
    {
    }
    public static Modifier ApplyBuff(this ModifierType modifier)
    {
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