using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent;
using Terraria.ModLoader.UI;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using TerrariaCells.Content.TileEntities;

namespace TerrariaCells.Common.UI {
    internal class SoundPlayerUI : ModSystem {
        internal State state;
        UserInterface ui;

        public override void PostSetupContent() {
            ui = new();
            state = new();
            state.Activate();
        }

        public override void UpdateUI(GameTime gameTime) {
            if (state.tile is not null) {
                ui.SetState(state);
                ui.Update(gameTime);
            } else {
                ui.SetState(null);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1) {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Sound Emitter UI",
                    delegate {
                        if (state.tile is not null) {
                            ui.Draw(Main.spriteBatch, new());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }

        static void setRect(UIElement el, float x, float y, float w, float h) {
            el.Left.Set(x, 0);
            el.Top.Set(y, 0);
            el.Width.Set(w, 0);
            el.Height.Set(h, 0);
        }

        internal class State : UIState {
            internal SoundPlayerTileEntity? tile;
            public DraggableUIPanel panel;
            public UIText soundLabel;

            public override void OnInitialize() {
                base.OnInitialize();
                panel = new DraggableUIPanel();
                panel.SetPadding(0);
                setRect(panel, 100, 400, 300, 220);
                panel.BackgroundColor = new(73, 94, 171);
                Append(panel);

                var cycleLeft = new UIButton<char>('<');
                setRect(cycleLeft, 20, 20, 40, 40);
                cycleLeft.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        tile.prev();
                    }
                });
                panel.Append(cycleLeft);

                var cycleRight = new UIButton<char>('>');
                setRect(cycleRight, 240, 20, 40, 40);
                cycleRight.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        tile.next();
                    }
                });
                panel.Append(cycleRight);

                soundLabel = new("Wire Click");
                setRect(soundLabel, 80, 30, 140, 40);
                panel.Append(soundLabel);

                var shiftUp = new UIButton<char>('^');
                setRect(shiftUp, 60, 80, 40, 40);
                shiftUp.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        tile.y--;
                        if (tile.y < 0) {
                            tile.y += Main.tile.Height;
                        }
                    }
                });
                panel.Append(shiftUp);

                var shiftDown = new UIButton<char>('v');
                setRect(shiftDown, 60, 160, 40, 40);
                shiftDown.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        tile.y++;
                        tile.y %= Main.tile.Height;
                    }
                });
                panel.Append(shiftDown);

                var shiftLeft = new UIButton<char>('<');
                setRect(shiftLeft, 20, 120, 40, 40);
                shiftLeft.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        tile.x--;
                        if (tile.x < 0) {
                            tile.x += Main.tile.Width;
                        }
                    }
                });
                panel.Append(shiftLeft);

                var shiftRight = new UIButton<char>('>');
                setRect(shiftRight, 100, 120, 40, 40);
                shiftRight.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuTick);
                        tile.x++;
                        tile.x %= Main.tile.Width;
                    }
                });
                panel.Append(shiftRight);

                var close = new UIButton<char>('X');
                setRect(close, 240, 160, 40, 40);
                close.OnLeftClick += new MouseEvent((evt, el) => {
                    if (tile is not null) {
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    tile = null;
                });
                panel.Append(close);
                Recalculate();
            }

            public override void Update(GameTime gameTime) {
                base.Update(gameTime);
                soundLabel.SetText(tile?.label ?? "XXXXX");
            }

            protected override void DrawSelf(SpriteBatch spriteBatch) {
                base.DrawSelf(spriteBatch);
                DrawUtils.highlightTileRegion(spriteBatch, tile.Position.ToPoint(), Color.White);
                spriteBatch.Draw(
                    // should probably cache this, but it will always be loaded and this
                    // isn't a gameplay UI, so it's probably fine
                    ModContent.Request<Texture2D>(
                        $"{nameof(TerrariaCells)}/Common/Assets/SoundPlayer/Icon"
                    ).Value,
                    new Point(tile.x, tile.y).ToWorldCoordinates() - Main.screenPosition,
                    null,
                    Color.White * 0.6f,
                    0,
                    Vector2.One * 8,
                    2,
                    SpriteEffects.None,
                    0
                );
                //Utils.highlightTileRegion(spriteBatch, new(tile.x, tile.y), Color.Green);
            }
        }
    }

    public static class DrawUtils {
        static readonly Vector2 normaliseVector = new(1, 0.001f);

        public static void highlightTileRegion(
            SpriteBatch spriteBatch,
            Point start,
            Point end,
            Color colour
        ) {
            var r = new Rectangle(
                (int) MathF.Min(start.X, end.X),
                (int) MathF.Min(start.Y, end.Y),
                (int) MathF.Abs(start.X - end.X) + 1,
                (int) MathF.Abs(start.Y - end.Y) + 1
            );
            var pos = r.TopLeft() * 16 - Main.screenPosition;
            var size = r.Size() * 16;
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos,
                null,
                colour * 0.6f,
                0,
                Vector2.Zero,
                size * normaliseVector,
                SpriteEffects.None,
                0
            );
            // Draw borders
            var vScale = new Vector2(2, size.Y) * normaliseVector;
            var hScale = new Vector2(size.X, 2) * normaliseVector;
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos - Vector2.UnitX * 2,
                null,
                colour,
                0,
                Vector2.Zero,
                vScale,
                SpriteEffects.None,
                0
            );
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos - Vector2.UnitY * 2,
                null,
                colour,
                0,
                Vector2.Zero,
                hScale,
                SpriteEffects.None,
                0
            );
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos + Vector2.UnitX * size.X,
                null,
                colour,
                0,
                Vector2.Zero,
                vScale,
                SpriteEffects.None,
                0
            );
            spriteBatch.Draw(
                TextureAssets.MagicPixel.Value,
                pos + Vector2.UnitY * size.Y,
                null,
                colour,
                0,
                Vector2.Zero,
                hScale,
                SpriteEffects.None,
                0
            );
        }

        public static void highlightTileRegion(
            SpriteBatch spriteBatch,
            Point start,
            Color colour,
            int width = 0,
            int height = 0
        ) {
            highlightTileRegion(
                spriteBatch,
                start,
                start + new Point(width, height),
                colour
            );
        }
    }
}
