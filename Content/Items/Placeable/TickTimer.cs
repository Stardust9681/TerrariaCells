using Terraria.ModLoader;
using Terraria;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.Items.Placeable {
    internal class TickTimer : ModItem {
        public override void SetDefaults() {
            Item.DefaultToPlaceableTile(ModContent.GetInstance<TickTimerTile>().Type);
        }
    }
}
