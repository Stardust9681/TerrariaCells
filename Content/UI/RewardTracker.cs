using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TerrariaCells.Common.UI;
using TerrariaCells.Common.UI.Components.Windows;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.Utilities;
using ReLogic.Graphics;
using Terraria.GameContent.UI.Elements;

namespace TerrariaCells.Content.UI
{
    public class RewardTracker : DraggableWindow
    {
        internal override string Name => "LevelTimer";
        public override Rectangle GrabBox => this.Bounds;

        internal const int Padding = 8;

        private TimerPanel _timer;
        private DeathsPanel _deaths;
        protected override void Init()
        {
            Vector2 timerSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString("00:00.00");
            timerSize += new Vector2(TimerPanel.Padding * 2, TimerPanel.Padding);
            timerSize += new Vector2(timerSize.Y, 0);

            Vector2 deathsSize = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString("000") * DeathsPanel.Scaling;
            deathsSize += new Vector2(DeathsPanel.Padding, DeathsPanel.Padding);
            deathsSize += new Vector2(deathsSize.Y, 0);
            Vector2 deathsPos = new Vector2((timerSize.X - deathsSize.X) * 0.5f, timerSize.Y);

            _deaths = new DeathsPanel();
            AddElement(_deaths, (int)deathsPos.X, (int)deathsPos.Y, (int)deathsSize.X, (int)deathsSize.Y);
            _timer = new TimerPanel();
            AddElement(_timer, 0, 0, (int)timerSize.X, (int)timerSize.Y);

            //Expand to fill each child
            WindowSize = new Vector2(timerSize.X, timerSize.Y + deathsSize.Y);
            this.WindowPosition = new Vector2(Main.screenWidth - NoDragZone, 128);
        }

        protected override void UpdateChildPositions(Vector2 newPosition)
        {
            _timer.Left.Set(newPosition.X, 0);
            _timer.Top.Set(newPosition.Y, 0);
            _deaths.Left.Set(newPosition.X + ((_timer.Width.Pixels - _deaths.Width.Pixels) * 0.5f), 0);
            _deaths.Top.Set(newPosition.Y + _timer.Height.Pixels, 0);
        }

        protected override bool PreDrawSelf(SpriteBatch spriteBatch)
        {
            return false;
        }
    }
    public class TimerPanel : UIElement
    {
        internal const int Padding = RewardTracker.Padding;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetDimensions().ToRectangle();
            Vector2 panelPos = bounds.TopLeft();
            Vector2 panelSize = bounds.Size();

            UIHelper.PANEL.Draw(spriteBatch, bounds with { X = (int)(panelPos.X + (panelSize.Y * 0.5f)), Width = (int)(panelSize.X - (panelSize.Y * 0.5f)) }, UIHelper.InventoryColour);

            TimeSpan currentTime = Main.LocalPlayer.GetModPlayer<Common.ModPlayers.RewardPlayer>().LevelTime;
            string drawString = $"{currentTime.Minutes:00}:{currentTime.Seconds:00}";
            Vector2 size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(drawString);
            Vector2 drawPos = new Vector2(panelPos.X + panelSize.Y + Padding, panelPos.Y + Padding);
            spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            drawPos += new Vector2(size.X, 0);
            drawString = $".{(currentTime.Milliseconds / 10):00}";
            spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.SlateGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);

            var watchSprite = Terraria.GameContent.TextureAssets.Item[Terraria.ID.ItemID.GoldWatch];
            drawPos = panelPos;
            spriteBatch.Draw(watchSprite.Value, new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)panelSize.Y, (int)panelSize.Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
    public class DeathsPanel : UIElement
    {
        internal const int Padding = (int)(RewardTracker.Padding * Scaling);
        internal const float Scaling = 0.85f;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetDimensions().ToRectangle();
            Vector2 panelPos = bounds.TopLeft();
            Vector2 panelSize = bounds.Size();

            UIHelper.PANEL.Draw(spriteBatch, bounds, UIHelper.InventoryColour);

            byte killCount = Main.LocalPlayer.GetModPlayer<Common.ModPlayers.RewardPlayer>().KillCount;
            string drawString = $"{killCount:D3}";
            int zeros = drawString.Count(x => x.Equals('0'));
            Vector2 size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString("000") * Scaling;
            Vector2 drawPos = new Vector2(panelPos.X + panelSize.X - Padding - size.X, panelPos.Y + Padding);
            if (zeros > 0)
            {
                spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString[..zeros], drawPos, Color.SlateGray, 0f, Vector2.Zero, Scaling, SpriteEffects.None, 0);
                drawPos.X += Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(drawString[..zeros]).X * Scaling;
            }
            spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString[zeros..], drawPos, Color.White, 0f, Vector2.Zero, Scaling, SpriteEffects.None, 0);

            var skullSprite = Terraria.GameContent.TextureAssets.Extra[Terraria.ID.ExtrasID.UnsafeIndicator];
            drawPos = panelPos + new Vector2(Padding);
            size = new Vector2(panelSize.Y - (2 * Padding));
            spriteBatch.Draw(skullSprite.Value, new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)size.X, (int)size.Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}