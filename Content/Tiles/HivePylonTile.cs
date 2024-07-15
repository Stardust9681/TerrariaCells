using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;
using TerrariaCells.Content.Items;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Content.Tiles;

public class HivePylonTile : ModPylon
{
    private const int CrystalVerticalFrameCount = 8;

    private Asset<Texture2D> crystalHighlightTexture;
    private Asset<Texture2D> crystalTexture;
    private Asset<Texture2D> mapIcon;

    public override void Load()
    {
        crystalTexture = ModContent.Request<Texture2D>("TerrariaCells/Content/Tiles/HivePylonTile_Crystal");
        crystalHighlightTexture = ModContent.Request<Texture2D>("TerrariaCells/Content/Tiles/PylonHighlight");
        mapIcon = ModContent.Request<Texture2D>("TerrariaCells/Content/Tiles/HivePylonTile_MapIcon");
    }

    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        
        TEModdedPylon moddedPylon = ModContent.GetInstance<PylonTileEntity.SimplePylonTileEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.CountsAsPylon);

        LocalizedText pylonName = CreateMapEntryName();
        AddMapEntry(Color.White, pylonName);
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<HivePylonItem>();
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        ModContent.GetInstance<PylonTileEntity.SimplePylonTileEntity>().Kill(i, j);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) 
    {
        DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.DarkOrange * 0.1f, Color.Transparent, 200, CrystalVerticalFrameCount);
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (Main.rand.NextBool(40) && drawData.tileFrameY < 28 && drawData.tileFrameY > 2 && drawData.tileFrameX < 23)
        {
            Dust.NewDustPerfect(new Vector2(i * 16 + 16 + Main.rand.Next(8), j * 16), DustID.Honey2, new Vector2(0, 0), 1);
        }
        base.DrawEffects(i, j, spriteBatch, ref drawData);
    }

    public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
    {
        var mouseOver = DefaultDrawMapIcon(ref context, mapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
        DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<HivePylonItem>().DisplayName.Key, ref mouseOverText);
    }
}