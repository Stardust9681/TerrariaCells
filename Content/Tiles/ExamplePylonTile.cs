using System.Linq;
using Microsoft.Xna.Framework;
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
using TerrariaCells.Common.Items;
using TerrariaCells.Content.Items.Placeable;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Content.Tiles;

public class TeleportTracker : ModSystem
{
    public int teleports = 0;

    public override void OnModLoad()
    {

        base.OnModLoad();
    }

    public void Reset()
    {
        teleports = 0;
    }

    public override void OnWorldLoad()
    {
        teleports = 0;


        base.OnWorldLoad();
    }

    public void Teleport()
    {
        teleports += 1;
        switch (teleports)
        {
            case 2: Main.LocalPlayer.Teleport(new Vector2(32461f, 7814f)); return; //desert
            case 4: Main.LocalPlayer.Teleport(new Vector2(47403f, 7158f)); return; //hive
            case 6: Main.LocalPlayer.Teleport(new Vector2(56771f, 6790f)); return; //ice
            case 8: Main.LocalPlayer.Teleport(new Vector2(8771f, 6102f)); teleports = 0; return; //forest
        }
        Main.LocalPlayer.Teleport(new Vector2(19623f, 10326f)); //inn
    }
}

/// <summary>
/// An example for creating a Pylon, identical to how they function in Vanilla. Shows off <seealso cref="ModPylon"/>, an abstract
/// extension of <seealso cref="ModTile"/> that has additional functionality for Pylon specific tiles.
/// <br>
/// If you are going to make multiple pylons that all act the same (like in Vanilla), it is recommended you make a base class
/// with override functionality in order to prevent writing boilerplate. (For example, making a "CrystalTexture" property that you can
/// override in order to streamline that process.)
/// </br>
/// </summary>
public class ForestExitPylon : ModPylon, ITerraCellsCategorization
{
    public const int CrystalVerticalFrameCount = 8;

    public Asset<Texture2D> crystalTexture;
    public Asset<Texture2D> crystalHighlightTexture;
    public Asset<Texture2D> mapIcon;

    public TerraCellsItemCategory Category => TerraCellsItemCategory.Storage;

    public override string Texture => (GetType().Namespace + "." + "ExamplePylonTile").Replace('.', '/');

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
        TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonTileEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;

        // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
        // AddToArray(ref TileID.Sets.CountsAsPylon);

        // LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
        // AddMapEntry(Color.White, pylonName);
    }

    public override NPCShop.Entry GetNPCShopEntry()
    {
        return null;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconText = "Leave";
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
    }

    public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
    {
        return true;
    }

    public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
    {
        return false;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.02f;
        g = b = 0.75f;

    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, Color.White, 1, CrystalVerticalFrameCount);
    }

    public override bool RightClick(int i, int j)
    {
        Mod.GetContent<TeleportTracker>().First().Teleport();
        // return base.RightClick(i, j);
        return true;
    }
}

public class DesertExitPylon : ModPylon, ITerraCellsCategorization
{
    public const int CrystalVerticalFrameCount = 8;

    public Asset<Texture2D> crystalTexture;
    public Asset<Texture2D> crystalHighlightTexture;
    public Asset<Texture2D> mapIcon;

    public TerraCellsItemCategory Category => TerraCellsItemCategory.Storage;
    public override string Texture => (GetType().Namespace + "." + "ExamplePylonTile").Replace('.', '/');

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
        TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonTileEntity>();
        // bad return set to -1 to disable the pylon placement limit
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, -1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);



        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;

        // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
        // AddToArray(ref TileID.Sets.CountsAsPylon);

        // LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
        // AddMapEntry(Color.White, pylonName);
    }

    public override NPCShop.Entry GetNPCShopEntry()
    {
        return null;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconText = "Leave";
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
    }

    public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
    {
        return true;
    }

    public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
    {
        return false;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.02f;
        g = b = 0.75f;

    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, Color.White, 1, CrystalVerticalFrameCount);
    }

    public override bool RightClick(int i, int j)
    {
        Mod.GetContent<TeleportTracker>().First().Teleport();
        // return base.RightClick(i, j);
        return true;
    }
}

public class HiveExitPylon : ModPylon, ITerraCellsCategorization
{
    public const int CrystalVerticalFrameCount = 8;

    public Asset<Texture2D> crystalTexture;
    public Asset<Texture2D> crystalHighlightTexture;
    public Asset<Texture2D> mapIcon;

    public TerraCellsItemCategory Category => TerraCellsItemCategory.Storage;
    public override string Texture => (GetType().Namespace + "." + "ExamplePylonTile").Replace('.', '/');

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
        TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonTileEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;

        // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
        // AddToArray(ref TileID.Sets.CountsAsPylon);

        // LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
        // AddMapEntry(Color.White, pylonName);
    }

    public override NPCShop.Entry GetNPCShopEntry()
    {
        return null;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconText = "Leave";
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
    }

    public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
    {
        return true;
    }

    public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
    {
        return false;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.02f;
        g = b = 0.75f;

    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, Color.White, 1, CrystalVerticalFrameCount);
    }

    public override bool RightClick(int i, int j)
    {
        Mod.GetContent<TeleportTracker>().First().Teleport();
        // return base.RightClick(i, j);
        return true;
    }
}

public class SnowExitPylon : ModPylon, ITerraCellsCategorization
{
    public const int CrystalVerticalFrameCount = 8;

    public Asset<Texture2D> crystalTexture;
    public Asset<Texture2D> crystalHighlightTexture;
    public Asset<Texture2D> mapIcon;

    public TerraCellsItemCategory Category => TerraCellsItemCategory.Storage;
    public override string Texture => (GetType().Namespace + "." + "ExamplePylonTile").Replace('.', '/');

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
        TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonTileEntity>();
        TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

        TileObjectData.addTile(Type);

        TileID.Sets.InteractibleByNPCs[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        TileID.Sets.AvoidedByMeteorLanding[Type] = true;

        // Adds functionality for proximity of pylons; if this is true, then being near this tile will count as being near a pylon for the teleportation process.
        // AddToArray(ref TileID.Sets.CountsAsPylon);

        // LocalizedText pylonName = CreateMapEntryName(); //Name is in the localization file
        // AddMapEntry(Color.White, pylonName);
    }

    public override NPCShop.Entry GetNPCShopEntry()
    {
        return null;
    }

    public override void MouseOver(int i, int j)
    {
        Main.LocalPlayer.cursorItemIconEnabled = true;
        Main.LocalPlayer.cursorItemIconText = "Leave";
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
    }

    public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
    {
        return true;
    }

    public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
    {
        return false;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.02f;
        g = b = 0.75f;

    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, Color.White, 1, CrystalVerticalFrameCount);
    }

    public override bool RightClick(int i, int j)
    {
        Mod.GetContent<TeleportTracker>().First().Teleport();
        // return base.RightClick(i, j);
        return true;
    }
}