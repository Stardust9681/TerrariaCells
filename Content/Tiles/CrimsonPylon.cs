using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.ModLoader.Default;
using TerrariaCells.Content.TileEntities;
using Terraria.GameContent;
using Terraria.Map;
using TerrariaCells.Content.Items.Placeable;

namespace TerrariaCells.Content.Tiles
{
    public class CrimsonPylon : CustomPylon
    {
        public const int CrystalVerticalFrameCount = 8;
        public override string CrystalTexture => $"{Texture}_Crystal";
        public override string OutlineTexture => Texture.Replace("CrimsonPylon", "PylonHighlight");
        public override string MapIconTexture => $"{Texture}_MapIcon";
        public override void SafeSetStaticDefaults()
        {
            AddMapEntry(Color.IndianRed, ModContent.GetInstance<Content.Items.Placeable.CrimsonPylon>().DisplayName);
        }

        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
        {
            //DefaultDrawPylonCrystal(spriteBatch, i, j, crystalTexture, crystalHighlightTexture, new Vector2(0f, -12f), Color.YellowGreen * 0.1f, Color.YellowGreen, 4, CrystalVerticalFrameCount);
            DrawCrystal(i, j, spriteBatch, CrystalVerticalFrameCount, Color.IndianRed);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<SimplePylonTileEntity>().Kill(i, j);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Vector3 lightColour = Color.IndianRed.ToVector3() * 0.8f;
            r = lightColour.X;
            g = lightColour.Y;
            b = lightColour.Z;
        }
    }
}
