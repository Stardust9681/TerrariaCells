using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers;

public class PowerupEffects : ModPlayer
{
    private static LocalizedText UnlockCloud;
    public override void Load()
    {
        UnlockCloud = Mod.GetLocalization("unlocks.cloud", () => "Unlocked double jump!");
    }
    public override bool OnPickup(Item item)
    {
        switch (item.type) {
            case ItemID.CloudinaBottle:
                MetaPlayer meta = Player.GetModPlayer<MetaPlayer>();
                meta.CloudJump = true;
                meta.DoUnlockText(UnlockCloud, Color.CornflowerBlue);
                return false;
            default:
                return true;
        }
    }
}
