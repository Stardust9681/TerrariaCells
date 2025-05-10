using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;
//using TerrariaCells.Common.UI; // only needed if using the debug drawing
using TerrariaCells.Content.Items.Placeable;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Content.Tiles.LevelExitPylon;

/// <summary>
/// An example for creating a Pylon, identical to how they function in Vanilla. Shows off <seealso cref="ModPylon"/>, an abstract
/// extension of <seealso cref="ModTile"/> that has additional functionality for Pylon specific tiles.
/// <br>
/// If you are going to make multiple pylons that all act the same (like in Vanilla), it is recommended you make a base class
/// with override functionality in order to prevent writing boilerplate. (For example, making a "CrystalTexture" property that you can
/// override in order to streamline that process.)
/// </br>
/// </summary>
public class ForestExitPylon : ModTile, ITerraCellsCategorization
{
    public Asset<Texture2D> crystalTexture;
    public Asset<Texture2D> crystalHighlightTexture;
    public Asset<Texture2D> mapIcon;

    public TerraCellsItemCategory Category => TerraCellsItemCategory.Storage;


    public override string Texture =>
        (GetType().Namespace + "." + "ExamplePylonTile").Replace('.', '/');

    public override void Load()
    {
        // We'll need these textures for later, it's best practice to cache them on load instead of continually requesting every draw call.
        crystalTexture = ModContent.Request<Texture2D>(Texture + "_Crystal");
        crystalHighlightTexture = ModContent.Request<Texture2D>(Texture + "_CrystalHighlight");
        mapIcon = ModContent.Request<Texture2D>(Texture + "_MapIcon");
    }

    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleHorizontal = true;
        // These definitions allow for vanilla's pylon TileEntities to be placed.
        // tModLoader has a built in Tile Entity specifically for modded pylons, which we must extend (see SimplePylonTileEntity)
        ForestExitPylonTileEntity moddedPylon = ModContent.GetInstance<ForestExitPylonTileEntity>();
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(
            moddedPylon.Hook_AfterPlacement,
            -1,
            0,
            false
        );

        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(
            ModContent.GetInstance<ForestExitPylonTileEntity>().Hook_AfterPlacement,
            -1,
            0,
            true
        );
        TileObjectData.newTile.UsesCustomCanPlace = true;

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;
        TileID.Sets.AddCorruptionTile(Type);
        // LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
        // AddMapEntry(Color.White, pylonName);
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;

        Point16 origin = GetTopLeftTileInMultitile(i, j);

        ForestExitPylonTileEntity entity;

        // TileEntity.ByPosition is a Dictionary<Point16, TileEntity> which contains all placed TileEntity instances in the world
        // TryGetValue is used to both check if the dictionary has the key, origin, and get the value from that key if it's there
        if (
            TileEntity.ByPosition.TryGetValue(origin, out TileEntity existing)
            && existing is ForestExitPylonTileEntity existingAsT
        )
        {
            entity = existingAsT;
        }
        else
        {
            return;
        }

        if (entity.Destination.Equals("Inn", System.StringComparison.CurrentCultureIgnoreCase))
        {
            Main.instance.MouseText(
                "Continue to the " + Mod.GetContent<TeleportTracker>().First().NextLevel
            );
            Main.mouseText = true;
            // Main.LocalPlayer.cursorItemIconText =
            //     "Continue to the " + Mod.GetContent<TeleportTracker>().First().NextLevel;
        }
        else
        {
            Main.instance.MouseText("Leave for the " + entity.Destination);
            Main.mouseText = true;
            // Main.LocalPlayer.cursorItemIconText = "Leave for the " + entity.Destination;
        }
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
    }

    public override void PlaceInWorld(int _i, int _j, Item item)
    {
        for (int i = 0; i < Sign.maxSigns; i++)
        {
            if (Main.sign[i] != null)
            {
                continue;
            }

            Main.InputTextSign();

            break;
        }
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;

        // Should your tile entity bring up a UI, this line is useful to prevent item slots from misbehaving
        Main.mouseRightRelease = false;

        // The following four (4) if-blocks are recommended to be used if your multitile opens a UI when right clicked:
        // if (player.sign > -1)
        // {
        //     SoundEngine.PlaySound(SoundID.MenuClose);
        //     player.sign = -1;
        //     Main.editSign = false;
        //     Main.npcChatText = string.Empty;
        // }
        if (Main.editChest)
        {
            SoundEngine.PlaySound(SoundID.MenuTick);
            Main.editChest = false;
            Main.npcChatText = string.Empty;
        }
        if (player.editedChestName)
        {
            NetMessage.SendData(
                MessageID.SyncPlayerChest,
                -1,
                -1,
                NetworkText.FromLiteral(Main.chest[player.chest].name),
                player.chest,
                1f
            );
            player.editedChestName = false;
        }
        if (player.talkNPC > -1)
        {
            player.SetTalkNPC(-1);
            Main.npcChatCornerItem = 0;
            Main.npcChatText = string.Empty;
        }

        Point16 origin = GetTopLeftTileInMultitile(i, j);

        ForestExitPylonTileEntity entity;

        // TileEntity.ByPosition is a Dictionary<Point16, TileEntity> which contains all placed TileEntity instances in the world
        // TryGetValue is used to both check if the dictionary has the key, origin, and get the value from that key if it's there
        if (
            TileEntity.ByPosition.TryGetValue(origin, out TileEntity existing)
            && existing is ForestExitPylonTileEntity existingAsT
        )
        {
            entity = existingAsT;
        }
        else
        {
            return false;
        }

        if (
            Main.LocalPlayer.HeldItem.type == ModContent.GetContent<ExamplePylonItem>().First().Type
        )
        {
            Main.sign[0] = new Sign { x = i, y = j };
            Main.LocalPlayer.sign = 0;
            Main.editSign = true;
            entity.editing = true;
            Main.npcChatText = entity.Destination;
        }
        else
        {
            Mod.GetContent<TeleportTracker>().First().Teleport(entity.Destination);
        }

        return true;
    }

    public static Point16 GetTopLeftTileInMultitile(int x, int y)
    {
        Tile tile = Main.tile[x, y];

        int frameX = 0;
        int frameY = 0;

        if (tile.HasTile)
        {
            int style = 0,
                alt = 0;
            TileObjectData.GetTileInfo(tile, ref style, ref alt);
            TileObjectData data = TileObjectData.GetTileData(tile.TileType, style, alt);

            if (data != null)
            {
                int size = 16 + data.CoordinatePadding;

                frameX = tile.TileFrameX % (size * data.Width) / size;
                frameY = tile.TileFrameY % (size * data.Height) / size;
            }
        }

        return new Point16(x - frameX, y - frameY);
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
        //DrawUtils.highlightTileRegion(spriteBatch, new(i + 12, j + 12), Color.Red);
        //DrawUtils.highlightTileRegion(spriteBatch, new(i + 12, j + 12 + drawData.tileTop), Color.Green);
        if (TileObjectData.IsTopLeft(i, j)) {
            //Main.NewText($"{i} {j} {Main.tile[i, j].TileType} {ModContent.TileType<ForestExitPylon>()}");
            Main.instance.TilesRenderer.AddSpecialLegacyPoint(i, j);
        }
    }

    List<DrawData> voidLensData = [];
    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
        // *why* is it off by 12???
        //DrawUtils.highlightTileRegion(spriteBatch, new(i + 12, j + 12), Color.White);
        var gateHelper = new PotionOfReturnGateHelper(
            PotionOfReturnGateHelper.GateType.EntryPoint,
            new((i + 12) * 16 + 24, (j + 12) * 16 + 32),
            1f
        );
        voidLensData.Clear();
        gateHelper.DrawToDrawData(voidLensData, 0);
        foreach (var datum in voidLensData) {
            datum.Draw(spriteBatch);
        }
    }

    // TODO: draw Extra_175 as a map icon
}
