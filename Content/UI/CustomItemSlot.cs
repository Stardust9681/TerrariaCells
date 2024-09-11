using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;

namespace TerrariaCells.Content.UI;

public class CustomItemSlot : UIElement
{
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        ItemSlot.Draw(
            spriteBatch,
            Main.LocalPlayer.inventory,
            ItemSlot.Context.HotbarItem,
            0,
            Vector2.One * 25
        );
    }

    // public override void RightClick(UIMouseEvent evt)
    // {
    //     ItemSlot.RightClick(Main.LocalPlayer.inventory, ItemSlot.Context.HotbarItem, 0);
    // }

    // public override void LeftClick(UIMouseEvent evt)
    // {
    //     ItemSlot.LeftClick(Main.LocalPlayer.inventory, ItemSlot.Context.HotbarItem, 0);
    //     throw new Exception("LeftClick");
    // }

    // void a() {
    //     ItemSlot.
    // }
}
