using Terraria.ModLoader;
using Terraria;
using TerrariaCells.Content.Tiles;

namespace TerrariaCells.Content.Items {
    internal class SoundPlayer : ModItem {
        public override string Texture => $"{nameof(TerrariaCells)}/Common/Assets/SoundPlayer/Icon";

        public override void SetDefaults() {
            Item.DefaultToPlaceableTile(ModContent.GetInstance<SoundPlayerTile>().Type);
        }
    }
}
