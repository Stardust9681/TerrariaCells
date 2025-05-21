using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using Terraria.IO;
using Terraria.Map;
using Terraria.DataStructures;
using Terraria.UI;
using Terraria.ObjectData;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Content.Tiles
{
    public abstract class CustomPylon : ModPylon
    {
        public virtual string CrystalTexture => string.Empty;
        public virtual string OutlineTexture => string.Empty;
        public virtual string MapIconTexture => string.Empty;
        private Asset<Texture2D> _crystal;
        private Asset<Texture2D> _outline;
        private Asset<Texture2D> _mapIcon;

        public virtual void SafeLoad() { }
        public sealed override void Load()
        {
            SafeLoad();
            _crystal = (CrystalTexture.Equals(string.Empty)) ? TextureAssets.Extra[181] : ModContent.Request<Texture2D>(CrystalTexture);
            _outline = (OutlineTexture.Equals(string.Empty)) ? TextureAssets.Extra[181] : ModContent.Request<Texture2D>(OutlineTexture);
            _mapIcon = (MapIconTexture.Equals(string.Empty)) ? TextureAssets.Extra[182] : ModContent.Request<Texture2D>(MapIconTexture);
        }

        public virtual void SafeSetStaticDefaults() { }
        public sealed override void SetStaticDefaults()
        {
            SafeSetStaticDefaults();

            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;

            TEModdedPylon moddedPylon = ModContent.GetInstance<SimplePylonTileEntity>();
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

            TileObjectData.addTile(Type);

            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;
            TileID.Sets.AvoidedByMeteorLanding[Type] = true;

            AddToArray(ref TileID.Sets.CountsAsPylon);
        }

        protected void DrawCrystal(int i, int j, SpriteBatch spriteBatch, int frameCount = 8, Color dustColour = default)
        {
            Point16 p = new Point16(i, j);
            bool foundPylon = Common.Systems.WorldPylonSystem.PylonFound(p);

            Tile tile = Framing.GetTileSafely(i, j);
            TileObjectData tileData = TileObjectData.GetTileData(tile);

            int frameY;
            if (foundPylon) frameY = Main.tileFrameCounter[597] / frameCount;
            else frameY = 0;// (Main.tileFrameCounter[597] + p.X + p.Y) % 64 / 8;

            Texture2D crystalTextureValue;
            Texture2D outlineTextureValue;
            if (foundPylon)
            {
                crystalTextureValue = _crystal.Value;
                outlineTextureValue = _outline.Value;
            }
            else
            {
                crystalTextureValue = TextureAssets.Extra[181].Value;
                outlineTextureValue = TextureAssets.Extra[181].Value;
            }

            Rectangle crystalFrame;
            if (!foundPylon || CrystalTexture.Equals(string.Empty)) crystalFrame = crystalTextureValue.Frame(12, 8, 1, frameY);
            else crystalFrame = crystalTextureValue.Frame(1, frameCount, 0, frameY);
            Rectangle selectOutlineFrame;
            if (!foundPylon || OutlineTexture.Equals(string.Empty)) selectOutlineFrame = outlineTextureValue.Frame(12, 8, 2, frameY % 8);
            else selectOutlineFrame = outlineTextureValue.Frame(1, frameCount, 0, frameY);
            //Something about frame bleeding
            crystalFrame.Height -= 1;
            selectOutlineFrame.Height -= 1;

            Vector2 offscreenVector = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen) offscreenVector = Vector2.Zero;

            Vector2 origin = crystalFrame.Size() * 0.5f;
            Vector2 tileOrigin = new Vector2(tileData.CoordinateFullWidth * 0.5f, tileData.CoordinateFullHeight * 0.5f);
            Vector2 crystalPosition = p.ToWorldCoordinates(tileOrigin.X - 2f, tileOrigin.Y) + new Vector2(0, -12);
            float num5 = MathF.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi / 5f);
            Vector2 drawingPosition = crystalPosition + offscreenVector + new Vector2(0f, num5 * 4f);

            if (!Main.gamePaused && Main.instance.IsActive && (!Lighting.UpdateEveryFrame || Main.rand.NextBool(4)) && Main.rand.NextBool(10))
            {
                Rectangle dustBox = Utils.CenteredRectangle(crystalPosition, crystalFrame.Size());
                if (!foundPylon) TeleportPylonsSystem.SpawnInWorldDust(1, dustBox);
                else
                {
                    Dust dust = Dust.NewDustDirect(dustBox.TopLeft(), dustBox.Width, dustBox.Height, DustID.TintableDustLighted, 0f, 0f, 254, dustColour with { A = byte.MaxValue }, 0.5f);
                    dust.velocity *= 0.1f;
                    dust.velocity.Y -= 0.2f;
                }
            }

            Color color = Lighting.GetColor(p.X, p.Y);
            color = Color.Lerp(color, Color.White, 0.8f);
            if (!foundPylon) color *= 0.3f;
            spriteBatch.Draw(crystalTextureValue, drawingPosition - Main.screenPosition, crystalFrame, color * 0.7f, 0f, origin, 1f, SpriteEffects.None, 0f);

            float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * ((float)Math.PI * 2f) / 1f) * 0.2f + 0.8f;
            Color shadowColor = new Color(255, 255, 255, 1) * 0.1f * scale;
            for (float shadowPos = 0f; shadowPos < 1f; shadowPos += 1f / 6f)
            {
                spriteBatch.Draw(crystalTextureValue, drawingPosition - Main.screenPosition + (MathHelper.TwoPi * shadowPos).ToRotationVector2() * (6f + num5 * 2f), crystalFrame, shadowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
            }

            int selectionLevel = 0;
            if (Main.InSmartCursorHighlightArea(p.X, p.Y, out bool actuallySelected))
            {
                if (!actuallySelected)
                    selectionLevel = 1;
                else
                    selectionLevel = 2;
            }
            else return;

            int averageBrightness = (color.R + color.G + color.B) / 3;
            if (averageBrightness <= 10) return;

            Color selectionGlowColor = Colors.GetSelectionGlowColor(selectionLevel == 2, averageBrightness);
            spriteBatch.Draw(outlineTextureValue, drawingPosition - Main.screenPosition, selectOutlineFrame, selectionGlowColor, 0f, origin, 1f, SpriteEffects.None, 0f);
        }
        public virtual void PreDrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, ref Color drawColor, ref float deselectedScale, ref float selectedScale) { }
        public sealed override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
        {
            PreDrawMapIcon(ref context, ref mouseOverText, pylonInfo, isNearPylon, ref drawColor, ref deselectedScale, ref selectedScale);

            SpriteFrame frame;
            if (!MapIconTexture.Equals(string.Empty)) frame = new SpriteFrame(1, 1, 0, 0);
            else frame = new SpriteFrame(9, 1, 3, 0);
            //Using context.Draw instead of DefaultDraw(..) to be able to specify frame
            bool mouseOver = context.Draw(_mapIcon.Value, pylonInfo.PositionInTiles.ToVector2(), drawColor, frame, deselectedScale, selectedScale, Alignment.Center).IsMouseOver;
            DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<Items.Placeable.HivePylon>().DisplayName.Key, ref mouseOverText);
        }

        public virtual void SafeMouseOver(int i, int j) { }
        public sealed override void MouseOver(int i, int j)
        {
            SafeMouseOver(i, j);
            Tile tile = Framing.GetTileSafely(i, j);
            int primaryItemType = TileLoader.GetItemDropFromTypeAndStyle(tile.TileType, 0);
            if (primaryItemType > 0)
            {
                Main.LocalPlayer.cursorItemIconID = primaryItemType;
                Main.LocalPlayer.cursorItemIconEnabled = true;
            }
        }
    }
}
