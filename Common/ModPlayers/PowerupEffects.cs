using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TerrariaCells.Common.ModPlayers;

public class PowerupEffects : ModPlayer
{
    public override bool OnPickup(Item item)
    {
        switch (item.type) {
            case ItemID.CloudinaBottle:
                //Utils.Swap(ref Player.armor[5], ref item);
                ChatHelper.DisplayMessageOnClient(
                    NetworkText.FromLiteral("Unlocked double jump!"),
                    Color.CornflowerBlue,
                    Player.whoAmI
                );
                Player.chatOverhead.NewMessage("Unlocked double jump!", 360);
                Player.GetModPlayer<MetaPlayer>().CloudJump = true;
                return false;
            default:
                return true;
        }
    }
}
