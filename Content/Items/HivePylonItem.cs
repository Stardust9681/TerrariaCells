using Terraria.Enums;
using Terraria.ModLoader;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.Items;

public class HivePylonItem : ModItem
{
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<HivePylonTile>());
    }
}