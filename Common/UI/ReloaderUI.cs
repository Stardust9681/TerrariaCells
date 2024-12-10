using Ionic.Zip;
using Microsoft.Xna.Framework.Graphics;
using rail;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using TerrariaCells.Common.GlobalItems;

namespace TerrariaCells.Common.UI
{
    class ReloaderUI : UIState
    {
        public UIImage Bar;
        public override void OnInitialize()
        {

            Bar = new UIImage(ModContent.Request<Texture2D>("TerrariaCells/Common/UI/ReloadBar"));
            Bar.Top.Set(510, 0);
            Bar.Left.Set(838, 0);
            Bar.Width.Set(60, 0);
            Bar.Height.Set(16, 0);

            Append(Bar);
            //UIImage succ = new UIImage(ModContent.Request<Texture2D>("TerrariaCells/Common/UI/SuccessRange"));
            //UIImage ind = new UIImage(ModContent.Request<Texture2D>("TerrariaCells/Common/UI/Indicator"));
            //UIImage ammoBG = new UIImage(ModContent.Request<Texture2D>("TerrariaCells/Common/UI/AmmoBarBG"));
            //UIImage ammo = new UIImage(ModContent.Request<Texture2D>("TerrariaCells/Common/UI/AmmoBar"));
            //bar.Append(succ);
            //bar.Append(ind);
            //bar.Append(ammoBG);
            //bar.Append(ammo);
            Append(Bar);
        }
        public override void Update(GameTime gameTime)
        {
            Left.Set(0, 0);
            Top.Set(0, 0);
            scale = ModContent.GetInstance<WeaponUIConfig>().Scale;
            if (dragging)
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (dragging && ModContent.GetInstance<WeaponUIConfig>().DragUI)
            {
                Bar.Left.Set(Main.mouseX - offset.X, 0f); // Main.MouseScreen.X and Main.mouseX are the same
                Bar.Top.Set(Main.mouseY - offset.Y, 0f);
                Recalculate();
            }

            //var parentSpace = GetDimensions().ToRectangle();
            //if (!Bar.GetDimensions().ToRectangle().Intersects(parentSpace))
            //{
            //    Bar.Left.Pixels = Utils.Clamp(Left.Pixels, 0, parentSpace.Right - Width.Pixels);
            //    Bar.Top.Pixels = Utils.Clamp(Top.Pixels, 0, parentSpace.Bottom - Height.Pixels);
            //    // Recalculate forces the UI system to do the positioning math again.
            //    Recalculate();

            //}
        }
        public Vector2 offset;
        public float scale = 1.2f;
        public bool dragging;
        //dragging ui stuff stolen from example mod
        public override void LeftMouseDown(UIMouseEvent evt)
        {

            base.LeftMouseDown(evt);
            Rectangle barRect = new Rectangle((int)Bar.Left.Pixels, (int)Bar.Top.Pixels, (int)(Bar.Width.Pixels * scale), (int)(Bar.Height.Pixels * scale));
            if (barRect.Contains(Main.mouseX, Main.mouseY))
            {
                Main.NewText(dragging);

                DragStart(evt);
            }
        }
        public override void LeftMouseUp(UIMouseEvent evt)
        {
            base.LeftMouseUp(evt);
            DragEnd(evt);
        }
        private void DragStart(UIMouseEvent evt)
        {
            // The offset variable helps to remember the position of the panel relative to the mouse position
            // So no matter where you start dragging the panel, it will move smoothly
            offset = new Vector2(evt.MousePosition.X - Bar.Left.Pixels, evt.MousePosition.Y - Bar.Top.Pixels);
            dragging = true;
        }

        private void DragEnd(UIMouseEvent evt)
        {
            Vector2 endMousePosition = evt.MousePosition;
            if (dragging)
            {
                dragging = false;


                Bar.Left.Set(endMousePosition.X - offset.X, 0f);
                Bar.Top.Set(endMousePosition.Y - offset.Y, 0f);
            }
            Recalculate();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            Player player = Main.LocalPlayer;
            if (player == null) return;
            if (!player.HeldItem.active)
            {
                return;
            }
            WeaponHoldoutify weapon = player.HeldItem?.GetGlobalItem<WeaponHoldoutify>();
            if (weapon == null) return;
            Asset<Texture2D> bar = ModContent.Request<Texture2D>("TerrariaCells/Common/UI/ReloadBar");
            Asset<Texture2D> succ = ModContent.Request<Texture2D>("TerrariaCells/Common/UI/SuccessRange");
            Asset<Texture2D> ind = ModContent.Request<Texture2D>("TerrariaCells/Common/UI/Indicator");
            Asset<Texture2D> bullet = TextureAssets.Item[ItemID.HighVelocityBullet];
            Main.instance.LoadItem(ItemID.HighVelocityBullet);
            float opacity = 1;
            if (weapon.SkillTimer > weapon.ReloadTime && ModContent.GetInstance<WeaponUIConfig>().FadeOut)
            {
                opacity = 1 - (weapon.SkillTimer - weapon.ReloadTime) / 20f;
            }

            Vector2 startOfBar = new Vector2(Bar.Left.Pixels + 2 * scale, Bar.Top.Pixels);

            spriteBatch.Draw(bar.Value, new Vector2(Bar.Left.Pixels, Bar.Top.Pixels), null, Color.White * opacity, 0, new Vector2(0, 0), scale, SpriteEffects.None, 0f);

            spriteBatch.Draw(succ.Value, new Vector2(Bar.Left.Pixels, Bar.Top.Pixels + 2 * scale) + new Vector2(MathHelper.Lerp(0, (Bar.Width.Pixels) * scale, weapon.ReloadSuccessLocation), 0), null, Color.White * opacity, 0, new Vector2(succ.Width() / 2, 0), new Vector2(2.8f * scale * (10 * weapon.ReloadSuccessRange), scale), SpriteEffects.None, 0f);

            float amount = Math.Clamp(weapon.SkillTimer, 0, weapon.ReloadTime) / weapon.ReloadTime;

            if (amount == 1f)
            {
                amount = (float)weapon.Ammo / weapon.MaxAmmo;
            }

            float barPosition = MathHelper.Lerp(value1: 0, (Bar.Width.Pixels - 4) * scale, amount);
            Vector2 barReloadingOffset = new Vector2(barPosition, 8 * scale);
            spriteBatch.Draw(ind.Value, startOfBar + barReloadingOffset, null, Color.White * opacity, 0, ind.Size() / 2, scale, SpriteEffects.None, 0f);

            Vector2 ammoLoc = new Vector2(Bar.Left.Pixels + 26 * scale, Bar.Top.Pixels + 25 * scale);
            spriteBatch.Draw(bullet.Value, ammoLoc, null, Color.White, 0, bullet.Size() / 2, scale, SpriteEffects.None, 0);

            Utils.DrawBorderString(spriteBatch, weapon.Ammo.ToString(), ammoLoc + new Vector2(8, -9) * scale, Color.White, scale: scale);
        }
    }
}
