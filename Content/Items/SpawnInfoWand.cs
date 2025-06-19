using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI;
using Terraria.UI;
using TerrariaCells.Common.Items;
using TerrariaCells.Common.Systems;
using TerrariaCells.Common.UI;
using TerrariaCells.Common.Utilities;

namespace TerrariaCells.Content.Items;

public class SpawnInfoWand : ModItem, ITerraCellsCategorization
{
    public TerraCellsItemCategory Category { get => TerraCellsItemCategory.Skill; }

    public Level currentLevel;
    public LevelStructure currentStructure;
    public Point structurePosition = new (319, 150);
    public bool settingStructurePosition = false;
    public override void SetDefaults()
    {
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
    }

    public override bool? UseItem(Player player)
    {
        if (settingStructurePosition)
        {
            structurePosition = Main.MouseWorld.ToTileCoordinates();
            settingStructurePosition = false;
            SpawnInfoUI.State.Message = $"Anchor set to tile coordinates {structurePosition}";
            SpawnInfoUI.State.PositionMessage = $"{structurePosition}";
        }

        return base.UseItem(player);
    }
}

public class SpawnInfoRenderer : ModSystem
{
    public BasicWorldGenData WorldGenData => StaticFileAccess.Instance.WorldGenData;

    public override void PreUpdateDusts()
    {
        if (Main.LocalPlayer.HeldItem == null || WorldGenData == null)
        {
            return;
        }
        if (Main.LocalPlayer.HeldItem.ModItem is not SpawnInfoWand wand)
        {
            return;
        }

        if (wand.currentStructure == null)
        {
            return;
        }

        int height = StructureHelper.API.Generator.GetStructureData(wand.currentStructure.Path, Mod).height;

        foreach (StructureSpawnInfo spawnInfo in wand.currentStructure?.SpawnInfo ?? [])
        {
            Point pos = spawnInfo.Position;
            pos.Y = -pos.Y;
            pos.Y += height;
            var dust = Dust.NewDustPerfect(pos.ToWorldCoordinates() + wand.structurePosition.ToWorldCoordinates(), DustID.GemDiamond, Scale: 1f);
            dust.velocity = Vector2.Zero; dust.noGravity = true;
        }
    }
}

// totally stole from Sound Player UI, thanks for doing the hard bit ;)
internal class SpawnInfoUI : ModSystem
{
    internal State state;
    UserInterface ui;

    public override void PostSetupContent()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            ui = new();
            state = new();
            state.Activate();
        }
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (Main.LocalPlayer.HeldItem != null && Main.LocalPlayer.HeldItem.ModItem is SpawnInfoWand wand)
        {
            state.wand = wand;
        }
        else {
            state.wand = null;
        } 
        if (state.wand is not null)
        {
            ui.SetState(state);
            ui.Update(gameTime);
        }
        else
        {
            ui.SetState(null);
        }
    }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Spawn Info UI",
                delegate
                {
                    if (state.wand is not null)
                    {
                        ui.Draw(Main.spriteBatch, new());
                    }
                    return true;
                },
                InterfaceScaleType.UI
            ));
        }
    }

    static void setRect(UIElement el, float x, float y, float w, float h)
    {
        el.Left.Set(x, 0);
        el.Top.Set(y, 0);
        el.Width.Set(w, 0);
        el.Height.Set(h, 0);
    }

    internal class State : UIState
    {
        public List<Level> LevelData => BasicWorldGenData.LevelData;

        internal SpawnInfoWand wand;
        public DraggableUIPanel panel;
        string levelInput = "Forest";
        string structureInput = "forest_1";
        public static string StructureMessage = "";
        public static string PositionMessage = "{319, 150}";
        public static string Message = ":3";
        UIText message;

        public override void OnInitialize()
        {
            base.OnInitialize();
            panel = new DraggableUIPanel();
            panel.SetPadding(0);
            setRect(panel, 100, 600, 300, 220);
            panel.BackgroundColor = new(73, 94, 171);
            Append(panel);

            Mod mod = ModContent.GetInstance<TerrariaCells>();

            Terraria.Localization.LocalizedText emptyContentText = mod.GetLocalization("levelInputEmptyContextText", delegate { return "Forest"; });
            var levelNameInput = new UISearchBar(emptyContentText, 1f);
            levelNameInput.SetContents("");
            setRect(levelNameInput, 20, 20, 400, 80);
            levelNameInput.OnContentsChanged += textInput =>
            {
                levelInput = textInput;
            };
            levelNameInput.OnLeftClick += delegate
            {
                if (!levelNameInput.IsWritingText)
                {
                    levelNameInput.ToggleTakingText();
                }
            };
            panel.Append(levelNameInput);

            emptyContentText = mod.GetLocalization("structureInputEmptyContextText", delegate { return "forest_1"; });
            var structureNameInput = new UISearchBar(emptyContentText, 1f);
            structureNameInput.SetContents("");
            setRect(structureNameInput, 20, 60, 400, 80);
            structureNameInput.OnContentsChanged += textInput =>
            {
                structureInput = textInput;
            };
            structureNameInput.OnLeftClick += delegate
            {
                if (!structureNameInput.IsWritingText)
                {
                    structureNameInput.ToggleTakingText();
                }
            };
            panel.Append(structureNameInput);

            var setStructureButton = new UIButton<string>("Set structure");
            setRect(setStructureButton, 20, 120, 100, 30);
            setStructureButton.OnLeftClick += new MouseEvent((evt, el) =>
            {
                if (wand is not null)
                {
                    SoundEngine.PlaySound(SoundID.MenuTick);
                    wand.currentLevel = LevelData.Find(x => x.Name == levelInput);
                    if (wand.currentLevel == null)
                    {
                        Message = $"Could not find level: {levelInput}";
                        return;
                    }

                    wand.currentStructure = wand.currentLevel.Structures.Find(x => x.Name == this.structureInput);
                    if (wand.currentStructure == null)
                    {
                        Message = $"Could not find structure: {structureInput}";
                        return;
                    }

                    Message = $"Found {wand.currentStructure.SpawnInfo.Length} spawns";
                    StructureMessage = wand.currentStructure.Path;
                }
            });
            panel.Append(setStructureButton);

            var setStructureText = new UIText(StructureMessage);
            setRect(setStructureText, 130, 120, 0, 30);
            panel.Append(setStructureText);

            var setPositionButton = new UIButton<string>("Set position");
            setRect(setPositionButton, 20, 160, 100, 30);
            setPositionButton.OnLeftClick += new MouseEvent((evt, el) =>
            {
                if (wand is not null)
                {
                    wand.settingStructurePosition = true;

                    Message = "Click on a tile to set as anchor";
                }
            });
            panel.Append(setPositionButton);

            var setPositionText = new UIText(PositionMessage);
            setRect(setPositionText, 130, 160, 0, 30);
            panel.Append(setPositionText);

            message = new UIText(Message);
            setRect(message, 0, 200, 300, 40);
            panel.Append(message);

            Recalculate();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            message.SetText(Message);
            
        }
    }
}