using Terraria.ModLoader;

namespace TerrariaCells.Content.Items;

public class TeleportDoorItem : ModItem
{
    public override void SetDefaults() {
        Item.consumable = true;
        Item.useTime = 10;
        Item.useAnimation = 10;
        Item.useStyle = 1;
        Item.maxStack = 9999;
        Item.autoReuse = true;
        Item.createTile = ModContent.TileType<Tiles.TeleportDoorTile>();
    }
}