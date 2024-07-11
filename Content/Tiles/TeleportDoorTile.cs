using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TerrariaCells.Content.Tiles;

public class TeleportDoorTile : ModTile
{
    public Vector2 destination = Vector2.Zero;
    public bool open;
    
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = false;
        Main.tileFrameImportant[Type] = true;
        
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.addTile(Type);
        
        AnimationFrameHeight = 72;
    }

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (Main.LocalPlayer.Hitbox.Intersects(new Rectangle(i * 16 - 32, j * 16, 48, 16)) &&
            Main.LocalPlayer.velocity.Y == 0)
        {
            open = true;

            if (Keybinds.doorInteract.JustPressed)
            {
                // Placeholder destination
                if (destination == Vector2.Zero)
                    destination = new Vector2(i * 16 + 16 * 16, j * 16);

                Main.LocalPlayer.Teleport(destination);
            }
        }
        else
            open = false;
    }

    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        // TODO: Animate door open and close when sprites are made
        if (open)
            frame = 1;
        else
            frame = 0;
    }
}