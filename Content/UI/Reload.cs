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
using Terraria.ID;

namespace TerrariaCells.Content.UI
{
    public class Reload : WindowState
    {
        public enum AmmoPositionMode
        {
            Cursor,
            Tween,
            TweenSag,
            Player,
            Resource,
        }

        const int Padding = 8;
        internal override string Name => "ReloadUI";
        internal override string InsertionIndex => "Vanilla: Resource Bars";
        GunAmmoIndicator? ammoDrawer;

        public override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void OnOpened()
        {
            if (!WeaponAnimations.Gun.TryGetGlobalItem(Main.LocalPlayer.HeldItem, out var gun))
            {
                Common.Systems.DeadCellsUISystem.ToggleActive<Reload>(false);
                return;
            }
            ammoDrawer = new GunAmmoIndicator(Main.LocalPlayer.HeldItem, gun);
            AddElement(ammoDrawer, Padding, Padding, 8, 8);
        }
        protected override void OnClosed()
        {
            Elements.Clear();
        }

        protected override bool PreUpdate(GameTime time)
        {
            if (!WeaponAnimations.Gun.TryGetGlobalItem(Main.LocalPlayer.HeldItem, out var gun)
                || !ammoDrawer.GunAmmo.Equals(gun)
                || Main.playerInventory)
            {
                Common.Systems.DeadCellsUISystem.ToggleActive<Reload>(false);
                return false;
            }

            _update(time);

            switch (Common.Configs.DevConfig.Instance.AmmoIndicatorType)
            {
                case AmmoPositionMode.Cursor:
                    float dir = (Main.MouseScreen.X < Main.screenWidth / 2) ? -1.125f : 0.125f;
                    WindowPosition = Main.MouseScreen + new Vector2(dir * WindowSize.X, -WindowSize.Y * 1.125f);
                    break;
                case AmmoPositionMode.Tween:
                    WindowPosition = (Main.MouseScreen * 0.5f) + (Main.ScreenSize.ToVector2() * 0.25f) - (WindowSize * 0.5f);
                    break;
                case AmmoPositionMode.TweenSag:
                    Vector2 centre = Main.ScreenSize.ToVector2() * 0.5f;
                    Vector2 mouse = Main.MouseScreen;
                    Vector2 targetPos = (centre + mouse) * 0.5f;
                    float lerpVal = ((mouse - centre) / centre).Length();
                    float offY = MathHelper.Lerp(Main.screenHeight/6, 0, lerpVal);
                    targetPos.Y += offY;
                    WindowPosition = targetPos - (WindowSize * 0.5f);
                    break;
                case AmmoPositionMode.Player:
                    WindowPosition = (Main.ScreenSize.ToVector2() * 0.5f) + new Vector2(0, 64) - (WindowSize * 0.5f);
                    break;
                case AmmoPositionMode.Resource:
                    WindowPosition = new Vector2(Main.screenWidth - (WindowSize.X * 1.125f), 84);
                    break;
            }

            Point oldSize = WindowSize.ToPoint() - new Point(2 * Padding, 2 * Padding);
            Recalculate();
            Point newSize = ammoDrawer.GetDimensions().ToRectangle().Size().ToPoint();
            if (newSize != oldSize)
            {
                WindowPosition -= new Vector2(newSize.X - oldSize.X, newSize.Y - oldSize.Y) * 0.5f;
                WindowSize = newSize.ToVector2();
                WindowSize += new Vector2(2 * Padding);
            }

            Recalculate();

            return false;
        }

        public override void Recalculate()
        {
            Rectangle bounds = Bounds;
            this.Left.Set(bounds.X, 0);
            this.Top.Set(bounds.Y, 0);
            this.Width.Set(bounds.Width, 0);
            this.Height.Set(bounds.Y, 0);
            base.Recalculate();
        }

        protected override bool PreDrawChildren(SpriteBatch spriteBatch)
        {
            switch (Common.Configs.DevConfig.Instance.AmmoIndicatorType)
            {
                case AmmoPositionMode.Tween:
                case AmmoPositionMode.TweenSag:
                    if (IsMouseOnWindow())
                        return false;
                    break;
            }
            return base.PreDraw(spriteBatch);
        }
        protected override bool PreDrawSelf(SpriteBatch spriteBatch)
        {
            switch (Common.Configs.DevConfig.Instance.AmmoIndicatorType)
            {
                case AmmoPositionMode.Tween:
                case AmmoPositionMode.TweenSag:
                    if (IsMouseOnWindow())
                        return false;
                    break;
            }
            UIHelper.PANEL.Draw(spriteBatch, Bounds, UIHelper.InventoryColour);
            return false;
        }

        public class GunAmmoIndicator : UIElement
        {
            internal readonly Item GunItem;
            internal readonly WeaponAnimations.Gun GunAmmo;
            //Ammo gets subtracted at the END of the use animation?
            //Flagged as important in comments, so I didn't touch it...
            //But the resulting check for how much ammo the player actually has is gross
            internal int CurrentAmmo => GunAmmo.GetActualAmmo(Main.LocalPlayer, GunItem);
            internal int MaxAmmo => Math.Max(GunAmmo.MaxAmmo, 1);
            public GunAmmoIndicator() : base()
            {
                GunItem = null;
                GunAmmo = null;
            }
            public GunAmmoIndicator(Item item, WeaponAnimations.Gun gun) : base()
            {
                GunItem = item;
                GunAmmo = gun;
            }
            public override void Recalculate()
            {
                if (GunAmmo is null) return;
                int maxAmmo = Math.Min(MaxAmmo, 15);
                int rowCount = (int)MathF.Ceiling((float)MaxAmmo / (float)maxAmmo);
                Point size = new Point();
                size.X = maxAmmo * Terraria.GameContent.TextureAssets.Item[ItemID.HighVelocityBullet].Width();
                if (MaxAmmo != maxAmmo)
                    size.X += Terraria.GameContent.TextureAssets.Item[ItemID.HighVelocityBullet].Width() / 2;
                size.Y = Terraria.GameContent.TextureAssets.Item[ItemID.HighVelocityBullet].Height() * rowCount - ((rowCount - 1) * 4);
                Width.Set(size.X, 0);
                Height.Set(size.Y, 0);
                base.Recalculate();
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (GunAmmo is null) return;
                Rectangle bounds = GetDimensions().ToRectangle();
                int maxAmmo = Math.Min(MaxAmmo, 15);
                int rowCount = (int)MathF.Ceiling((float)MaxAmmo / (float)maxAmmo);

                var tex = Terraria.GameContent.TextureAssets.Item[ItemID.HighVelocityBullet].Value;
                int width = tex.Width;
                int height = tex.Height;
                for (int i = 0; i < MaxAmmo; i++)
                {
                    bool isActive = (MaxAmmo - i) <= CurrentAmmo;
                    Color drawColour = isActive ? Color.White : (Color.SlateGray * 0.67f);
                    if((MaxAmmo - i) == GunAmmo.Ammo && !Main.LocalPlayer.GetModPlayer<Common.ModPlayers.WeaponPlayer>().reloading && !Main.LocalPlayer.ItemAnimationEndingOrEnded)
                        drawColour = Color.OrangeRed;
                    int y = i / maxAmmo;
                    int x = (i - (y * maxAmmo)) * width;
                    if (y % 2 != 0)
                        x += (width / 2);
                    y *= height;
                    y -= (i / maxAmmo) * 4;
                    spriteBatch.Draw(
                        tex,
                        new Vector2(bounds.Right - x - width, bounds.Y + y),
                        drawColour);
                }
            }
        }
    }
}