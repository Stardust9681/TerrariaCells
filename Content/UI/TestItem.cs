using Terraria;
using Terraria.ModLoader;

namespace TerrariaCells.Content.UI;

[Autoload(Side = ModSide.Both)]
public class TestItem : Item, ITerraCellsCategorization
{
    public TestItem()
    {
        expert = true;
        damage = 10;
        axe = 100;
    }

    public TerraCellsItemCategory Category => TerraCellsItemCategory.Weapon;
}
