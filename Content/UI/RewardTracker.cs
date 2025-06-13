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
        internal override string Name => "RewardTrackerUI";
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
            this.WindowPosition = new Vector2((Main.screenWidth - WindowSize.X) * 0.5f, NoDragZone);
            UpdateChildPositions(WindowPosition);
        }

        protected override void OnOpened()
        {
            this.WindowPosition = new Vector2((Main.screenWidth - WindowSize.X) * 0.5f, NoDragZone);
            UpdateChildPositions(WindowPosition);
        }

        protected override void UpdateChildPositions(Vector2 newPosition)
        {
            /*_timer.Left.Set(newPosition.X, 0);
            _timer.Top.Set(newPosition.Y, 0);
            _deaths.Left.Set(newPosition.X + ((_timer.Width.Pixels - _deaths.Width.Pixels) * 0.5f), 0);
            _deaths.Top.Set(newPosition.Y + _timer.Height.Pixels, 0);*/
            _timer.Left.Set(0, 0);
            _timer.Top.Set(0, 0);
            _deaths.Left.Set((_timer.Width.Pixels - _deaths.Width.Pixels) * 0.5f, 0);
            _deaths.Top.Set(_timer.Height.Pixels, 0);
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

            bounds.X += (int)(panelSize.Y * 0.5f);
            bounds.Width -= (int)(panelSize.Y * 0.5f);
            UIHelper.PANEL.Draw(spriteBatch, bounds, UIHelper.InventoryColour);

            TimeSpan currentTime = Main.LocalPlayer.GetModPlayer<Common.ModPlayers.RewardPlayer>().LevelTime;
            string drawString = $"{currentTime.Minutes:00}:{currentTime.Seconds:00}";
            Vector2 size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(drawString);
            Vector2 drawPos = new Vector2(panelPos.X + panelSize.Y + Padding, panelPos.Y + Padding);
            spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            /*Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch, Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.White, 0f, Vector2.Zero, Vector2.One
            );*/
            drawPos += new Vector2(size.X, 0);
            drawString = $".{(currentTime.Milliseconds / 10):00}";
            spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.SlateGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
            /*Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch, Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.SlateGray, 0f, Vector2.Zero, Vector2.One
            );*/

            TimeSpan targetTime = Main.LocalPlayer.GetModPlayer<Common.ModPlayers.RewardPlayer>().targetTime;
            int watchType = (currentTime.TotalSeconds / (float)targetTime.TotalSeconds) switch
            {
                < 0.4 => Terraria.ID.ItemID.PlatinumWatch,
                < 0.7 => Terraria.ID.ItemID.GoldWatch,
                < 1 => Terraria.ID.ItemID.SilverWatch,
                _ => Terraria.ID.ItemID.CopperWatch,
            };
            var watchSprite = Terraria.GameContent.TextureAssets.Item[watchType];
            drawPos = panelPos;
            spriteBatch.Draw(watchSprite.Value, new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)panelSize.Y, (int)panelSize.Y), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
    public class DeathsPanel : UIElement
    {
        public DeathsPanel()
        {
            JourneyBunny = Main.Assets.Request<Texture2D>("Images/UI/WorldCreation/IconDifficultyCreative");
            ClassicBunny = Main.Assets.Request<Texture2D>("Images/UI/WorldCreation/IconDifficultyNormal");
            ExpertBunny = Main.Assets.Request<Texture2D>("Images/UI/WorldCreation/IconDifficultyExpert");
            MasterBunny = Main.Assets.Request<Texture2D>("Images/UI/WorldCreation/IconDifficultyMaster");
        }
        internal ReLogic.Content.Asset<Texture2D> JourneyBunny;
        internal ReLogic.Content.Asset<Texture2D> ClassicBunny;
        internal ReLogic.Content.Asset<Texture2D> ExpertBunny;
        internal ReLogic.Content.Asset<Texture2D> MasterBunny;

        internal const int Padding = (int)(RewardTracker.Padding * Scaling);
        internal const float Scaling = 0.85f;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            Rectangle bounds = GetDimensions().ToRectangle();
            Vector2 panelPos = bounds.TopLeft();
            Vector2 panelSize = bounds.Size();

            UIHelper.PANEL.Draw(spriteBatch, bounds, UIHelper.InventoryColour);

            byte killCount = Main.LocalPlayer.GetModPlayer<Common.ModPlayers.RewardPlayer>().KillCount;
            //Draw leading zeroes:
            /*
            string drawString = $"{killCount:D3}";
            int zeros = drawString.TakeWhile(c => c.Equals('0')).Count();
            Vector2 size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString("000") * Scaling;
            Vector2 drawPos = new Vector2(panelPos.X + panelSize.X - Padding - size.X, panelPos.Y + Padding);
            if (zeros > 0)
            {
                spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString[..zeros], drawPos, Color.SlateGray, 0f, Vector2.Zero, Scaling, SpriteEffects.None, 0);
                drawPos.X += Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(drawString[..zeros]).X * Scaling;
            }
            spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString[zeros..], drawPos, Color.White, 0f, Vector2.Zero, Scaling, SpriteEffects.None, 0);
            */

            //Don't draw leading zeroes:
            string drawString = killCount.ToString();
            Vector2 size = Terraria.GameContent.FontAssets.MouseText.Value.MeasureString(drawString) * Scaling;
            Vector2 drawPos = new Vector2(panelPos.X + panelSize.X - Padding - size.X, panelPos.Y + Padding);
            if (killCount > 0)
            {
                spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.White, 0f, Vector2.Zero, Scaling, SpriteEffects.None, 0);
                /*Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
                    spriteBatch, Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.White, 0f, Vector2.Zero, Vector2.One * Scaling
                );*/
            }
            else
            {
                spriteBatch.DrawString(Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.SlateGray, 0f, Vector2.Zero, Scaling, SpriteEffects.None, 0);
                /*Terraria.UI.Chat.ChatManager.DrawColorCodedStringWithShadow(
                    spriteBatch, Terraria.GameContent.FontAssets.MouseText.Value, drawString, drawPos, Color.SlateGray, 0f, Vector2.Zero, Vector2.One * Scaling
                );*/
            }

            byte targetKillCount = Main.LocalPlayer.GetModPlayer<Common.ModPlayers.RewardPlayer>().targetKillCount;
            if (targetKillCount == 0)
                targetKillCount = 1;
            float allKills = (float)killCount / (float)targetKillCount;
            ReLogic.Content.Asset<Texture2D> drawSprite = allKills switch
            {
                < 0.3f => JourneyBunny,
                < 0.6f => ClassicBunny,
                < .9f => ExpertBunny,
                _ => MasterBunny
            };
            drawPos = panelPos + new Vector2(Padding);
            size = new Vector2(panelSize.Y - (2 * Padding));
            drawPos.X += size.X * 0.125f;
            spriteBatch.Draw(drawSprite.Value, new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)(size.X * 0.75f), (int)size.Y), new Rectangle(7, 4, 18, 24), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}