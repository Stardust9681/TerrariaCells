using Terraria;
using Terraria.ModLoader;
using TerrariaCells.Utils;

namespace TerrariaCells.Content.Items
{
	public class BuilderAccessory : ModItem
	{
		public override void SetDefaults()
		{
			Item.accessory = true;
		}
		public override void UpdateEquip(Player player)
		{
			GlobalPlayer.isBuilder = true;
			player.noBuilding = false;
		}
	}
}
