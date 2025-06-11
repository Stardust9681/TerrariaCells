using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

using Size = Microsoft.Xna.Framework.Point;

namespace TerrariaCells.Common.Utilities
{
    public class NinePatch
    {
        public NinePatch(int patchSizeX, int patchSizeY, int overlapX = 0, int overlapY = 0)
        {
            PatchSize = new Size(patchSizeX, patchSizeY);
            BufferSize = new Size(overlapX, overlapY);
            SourceSize = new Size((patchSizeX * 3) - (overlapX * 2), (patchSizeY * 3) - (overlapY * 2));
        }
        public Size PatchSize { get; private set; }
        public Size BufferSize { get; private set; }
        public Size SourceSize { get; private set; }

        public Rectangle TopLeft => new Rectangle(0, 0, PatchSize.X, PatchSize.Y);
        public Rectangle Top => new Rectangle(PatchSize.X - BufferSize.X, 0, PatchSize.X, PatchSize.Y);
        public Rectangle TopRight => new Rectangle((PatchSize.X * 2) - (BufferSize.X * 2), 0, PatchSize.X, PatchSize.Y);
        public Rectangle Left => new Rectangle(0, PatchSize.Y - BufferSize.Y, PatchSize.X, PatchSize.Y);
        public Rectangle Middle => new Rectangle(PatchSize.X - BufferSize.X, PatchSize.Y - BufferSize.Y, PatchSize.X, PatchSize.Y);
        public Rectangle Right => new Rectangle((PatchSize.X * 2) - (BufferSize.X * 2), PatchSize.Y - BufferSize.Y, PatchSize.X, PatchSize.Y);
        public Rectangle BottomLeft => new Rectangle(0, (PatchSize.Y * 2) - (BufferSize.Y * 2), PatchSize.X, PatchSize.Y);
        public Rectangle Bottom => new Rectangle(PatchSize.X - BufferSize.X, (PatchSize.Y * 2) - (BufferSize.Y * 2), PatchSize.X, PatchSize.Y);
        public Rectangle BottomRight => new Rectangle((PatchSize.X * 2) - (BufferSize.X * 2), (PatchSize.Y * 2) - (BufferSize.Y * 2), PatchSize.X, PatchSize.Y);
    }
    public class AssetDrawer : IDisposable
    {
        public AssetDrawer() : this(UIHelper.GetPathToAsset("Panel"), 8, 8, 0, 0) { }
        public AssetDrawer(string path, int patchWidth, int patchHeight) : this(path, patchWidth, patchHeight, 0, 0) { }
        public AssetDrawer(string path, int patchWidth, int patchHeight, int overlapWidth, int overlapHeight)
        {
            AssetPath = path;
            Asset = ModContent.Request<Texture2D>(AssetPath);
            Patch = new NinePatch(patchWidth, patchHeight, overlapWidth, overlapHeight);
        }
        public readonly string AssetPath;
        private readonly Asset<Texture2D> Asset;
        public readonly NinePatch Patch;
        public void Draw(SpriteBatch batch, Rectangle area, Color colour)
        {
            if (area.Width < Patch.SourceSize.X || area.Height < Patch.SourceSize.Y)
            {
                return;
            }

            if (!Asset.IsLoaded)
            {
                return;
            }
            Texture2D tex = Asset.Value;

            if (Patch.SourceSize.Equals(area.Size().ToPoint()))
            {
                batch.Draw(tex, area, null, colour);
                return;
            }

            Size scaledSize = Patch.PatchSize;

            //Top Left
            batch.Draw(tex, new Rectangle(area.Left, area.Top, scaledSize.X, scaledSize.Y), Patch.TopLeft, colour);
            //Top Right
            batch.Draw(tex, new Rectangle(area.Right - scaledSize.X, area.Top, scaledSize.X, scaledSize.Y), Patch.TopRight, colour);
            //Bottom Left
            batch.Draw(tex, new Rectangle(area.Left, area.Bottom - scaledSize.Y, scaledSize.X, scaledSize.Y), Patch.BottomLeft, colour);
            //Bottom Right
            batch.Draw(tex, new Rectangle(area.Right - scaledSize.X, area.Bottom - scaledSize.Y, scaledSize.X, scaledSize.Y), Patch.BottomRight, colour);

            bool flagWidth = false;
            bool flagHeight = false;

            if (area.Width - (scaledSize.X * 2) != 0)
            {
                //Top
                batch.Draw(tex, new Rectangle(area.Left + scaledSize.X, area.Top, area.Width - (scaledSize.X * 2), scaledSize.Y), Patch.Top, colour);
                //Bottom
                batch.Draw(tex, new Rectangle(area.Left + scaledSize.X, area.Bottom - scaledSize.Y, area.Width - (scaledSize.X * 2), scaledSize.Y), Patch.Bottom, colour);

                flagWidth = true;
            }

            if (area.Height - (scaledSize.Y * 2) != 0)
            {
                //Left
                batch.Draw(tex, new Rectangle(area.Left, area.Top + scaledSize.Y, scaledSize.X, area.Height - (scaledSize.Y * 2)), Patch.Left, colour);
                //Right
                batch.Draw(tex, new Rectangle(area.Right - scaledSize.X, area.Top + scaledSize.Y, scaledSize.X, area.Height - (scaledSize.Y * 2)), Patch.Right, colour);

                flagHeight = true;
            }

            if (flagWidth && flagHeight)
            {
                //Middle
                batch.Draw(tex, new Rectangle(area.Left + scaledSize.X, area.Top + scaledSize.Y, area.Width - (scaledSize.X * 2), area.Height - (scaledSize.Y * 2)), Patch.Middle, colour);
            }
        }
        public void Dispose()
        {
            Asset.Dispose();
        }
    }
    public class UIHelper
    {
        public static readonly AssetDrawer PANEL = new AssetDrawer(GetPathToAsset("Panel"), 8, 8);

        public static readonly Color InventoryColour = new Color(63, 65, 151);
        internal static string GetPathToAsset(string assetName) => $"TerrariaCells/Common/Assets/UI/{assetName}";

        ~UIHelper()
        {
            PANEL.Dispose();
        }
    }
}