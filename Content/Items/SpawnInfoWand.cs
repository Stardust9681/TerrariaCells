using System;
using System.Collections.Generic;
using System.Linq;
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
    public Point structurePosition = new(319, 150);
    public bool settingStructurePosition = false;
    public StructureSpawnInfo selected = null;
    public override void SetDefaults()
    {
        Item.useAnimation = 30;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
    }

    public override bool? UseItem(Player player)
    {
        if (currentStructure == null)
        {
            SpawnInfoUI.State.Message = "Structure not set";
            return base.UseItem(player);
        }

        if (player.altFunctionUse == 2)
        {
            if (settingStructurePosition)
            {
                SpawnInfoUI.State.Message = $"Cancelled position set";
                settingStructurePosition = false;
                return base.UseItem(player);
            }
            StructureSpawnInfo hoveringPoint = ModContent.GetInstance<SpawnInfoRenderer>().hoveringPoint;
            if (hoveringPoint != null)
            {
                currentStructure.SpawnInfo.Remove(hoveringPoint);
                SpawnInfoUI.State.Message = $"Deleted spawn info at {structurePosition}";
            }
            selected = null;
            return base.UseItem(player);
        }

        if (settingStructurePosition)
        {
            structurePosition = Main.MouseWorld.ToTileCoordinates();
            settingStructurePosition = false;
            SpawnInfoUI.State.Message = $"Anchor set to tile coordinates {structurePosition}";
            SpawnInfoUI.State.PositionMessage = $"{structurePosition}";
            return base.UseItem(player);
        }

        SpawnInfoUI.State UIstate = ModContent.GetInstance<SpawnInfoUI>().state;
        string text = "none";
        StructureSpawnInfo spawnInfo;
        Point dest = Main.MouseWorld.ToTileCoordinates() - structurePosition;
        switch (UIstate.spawnInputMode)
        {
            case 0:
                text = UIstate.idInput.ToString();
                spawnInfo = new(UIstate.idInput, dest.X, dest.Y);
                break;
            case 1:
                text = UIstate.nameInput;
                spawnInfo = new(UIstate.nameInput, dest.X, dest.Y);
                break;
            case 2:
                text = SpawnInfoUI.State.GetIntListDisplay(UIstate.idPoolInput);
                spawnInfo = new(UIstate.idPoolInput, Main.rand, dest.X, dest.Y);
                break;
            case 3:
                text = $"ids:{UIstate.widPoolInput.Select(x => x.GetID())}\n weights:{UIstate.widPoolInput.Select(x => x.Weight)}";
                spawnInfo = new(UIstate.widPoolInput, Main.rand, dest.X, dest.Y);
                break;
            default:
                return base.UseItem(player);
        }

        currentStructure.SpawnInfo.Add(spawnInfo);
        SpawnInfoUI.State.Message = $"Placed new SpawnInfo [{text}] @ {structurePosition}";

        return base.UseItem(player);
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }
}

public class SpawnInfoRenderer : ModSystem
{
    public BasicWorldGenData WorldGenData => StaticFileAccess.Instance.WorldGenData;
    public StructureSpawnInfo hoveringPoint = null;

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

        Point mousePosition = Main.MouseWorld.ToTileCoordinates();

        foreach (StructureSpawnInfo spawnInfo in wand.currentStructure?.SpawnInfo ?? [])
        {
            Point pos = spawnInfo.Position;
            pos.Y = -pos.Y;
            pos.Y += height;
            pos += wand.structurePosition;

            if (pos == mousePosition)
            {
                Dust dust = Dust.NewDustPerfect(pos.ToWorldCoordinates(), DustID.GemRuby, Scale: 1f);
                dust.velocity = Vector2.Zero;
                dust.noGravity = true;
                hoveringPoint = spawnInfo;
            }
            else
            {
                Dust dust = Dust.NewDustPerfect(pos.ToWorldCoordinates(), DustID.GemDiamond, Scale: 1f);
                dust.velocity = Vector2.Zero;
                dust.noGravity = true;
            }
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
        else
        {
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
        public DraggableUIPanel panel2;
        string levelInput = "Forest";
        string structureInput = "forest_1";
        public static string StructureMessage = "";
        public static string PositionMessage = "{319, 150}";
        public static string Message = ":3";
        UIText message;
        UIText setStructureText;
        UIText setPositionText;

        public int spawnInputMode = 0;
        public DraggableUIPanel[] panels = new DraggableUIPanel[4];
        internal int idInput = 0;
        internal string nameInput = "";
        internal int[] idPoolInput = new int[1];
        internal WeightedID[] widPoolInput = [new WeightedID(0, 1f)];

        public override void OnInitialize()
        {
            base.OnInitialize();
            Mod mod = ModContent.GetInstance<TerrariaCells>();
            Terraria.Localization.LocalizedText emptyContentText;

            for (int i = 0; i < panels.Length; i++)
            {
                panels[i] = new DraggableUIPanel();
                panels[i].SetPadding(0);
                panels[i].BackgroundColor = new(73, 94, 171);
                Append(panels[i]);
                panel2 = panels[i];
                spawnInputMode = i;
                AddUItoSecondPanel();
                panel2.Deactivate();
            }

            panel = new DraggableUIPanel();
            panel.SetPadding(0);
            setRect(panel, 100, 600, 300, 260);
            panel.BackgroundColor = new(73, 94, 171);
            Append(panel);

            var modeTextInput = TextInput();
            setRect(modeTextInput, 200, 20, 30, 30);
            modeTextInput.OnContentsChanged += textInput =>
            {
                foreach (var panell in panels)
                {
                    panell.Remove();
                }

                if (int.TryParse(textInput, out int result) && result >= 0 && result < panels.Length)
                {
                    spawnInputMode = result;
                    if (!Children.Contains(panels[result]))
                    {
                        Append(panels[result]);
                        Message = "Changed view panel";
                    }
                }
                else
                {
                    Message = "";
                }
            };
            panel.Append(modeTextInput);

            emptyContentText = mod.GetLocalization("levelInputEmptyContextText", delegate { return "Forest"; });
            var levelNameInput = new UISearchBar(emptyContentText, 1f);
            levelNameInput.SetContents("");
            setRect(levelNameInput, 20, 20, 200, 20);
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
            setRect(structureNameInput, 20, 50, 200, 20);
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
            setRect(setStructureButton, 20, 80, 100, 30);
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

                    if (wand.currentStructure.SpawnInfo == null)
                    {
                        Message = "SpawnInfo failed to load. Check client.log before using this structure file";
                        mod.Logger.Error($"{wand.currentStructure.SpawnInfoPath} failed to deserialize. Fix the JSON configuration and rebuild the mod before you potentially override the data.");
                    }
                    else
                    {
                        Message = $"Found {wand.currentStructure.SpawnInfo.Count} spawns";
                    }
                    StructureMessage = wand.currentStructure.Path;
                }
            });
            panel.Append(setStructureButton);

            setStructureText = new UIText(StructureMessage);
            setRect(setStructureText, 130, 90, 0, 30);
            panel.Append(setStructureText);

            var setPositionButton = new UIButton<string>("Set position");
            setRect(setPositionButton, 20, 120, 100, 30);
            setPositionButton.OnLeftClick += new MouseEvent((evt, el) =>
            {
                if (wand is not null)
                {
                    wand.settingStructurePosition = true;

                    Message = "Click on a tile to set as anchor";
                }
            });
            panel.Append(setPositionButton);

            setPositionText = new UIText(PositionMessage);
            setRect(setPositionText, 130, 130, 0, 30);
            panel.Append(setPositionText);

            var resetSpawnsButton = new UIButton<string>("Respawn world NPCs");
            setRect(resetSpawnsButton, 20, 160, 200, 30);
            resetSpawnsButton.OnLeftClick += new MouseEvent((evt, el) =>
            {
                NPCRoomSpawner.ResetSpawns();
                Message = "Reset all spawned NPCs";
            });
            panel.Append(resetSpawnsButton);

            var resetSpawnsButton2 = new UIButton<string>("Respawn structure NPCs");
            setRect(resetSpawnsButton2, 20, 200, 200, 30);
            resetSpawnsButton2.OnLeftClick += new MouseEvent((evt, el) =>
            {
                if (wand.currentStructure == null)
                {
                    Message = $"No structure is set";
                    return;
                }
                NPCRoomSpawner.ResetSpawnsForStructure(wand.currentStructure, wand.structurePosition);
                Message = $"Respawned NPCs for {wand.currentStructure.Name}";
            });
            panel.Append(resetSpawnsButton2);

            message = new UIText(Message);
            setRect(message, 0, 240, 300, 40);
            panel.Append(message);

            Recalculate();
        }

        private void AddUItoSecondPanel()
        {
            switch (spawnInputMode)
            {
                case 0: // id
                    setRect(panel2, 400, 600, 300, 50);

                    UISearchBar idTextInput = TextInput();
                    idTextInput.OnContentsChanged += textInput =>
                    {
                        if (int.TryParse(textInput, out int intInput))
                        {
                            idInput = intInput;
                            Message = $"Set ID {intInput}";
                        }
                        else
                        {
                            Message = $"Invalid ID";
                        }
                    };
                    panel2.Append(idTextInput);
                    break;
                case 1: // name
                    setRect(panel2, 400, 600, 300, 50);

                    UISearchBar nameTextInput = TextInput();

                    nameTextInput.OnContentsChanged += textInput =>
                    {
                        if (NPCID.Search.TryGetId(textInput, out int result))
                        {
                            nameInput = textInput;
                            Message = $"Set NPC name {textInput} (ID {result})";
                        }
                        else
                        {
                            Message = $"Invalid Name ID";
                        }
                    };
                    panel2.Append(nameTextInput);
                    break;
                case 2: // id pool
                    setRect(panel2, 400, 600, 400, 50);

                    for (int i = 0; i < idPoolInput.Length; i++)
                    {
                        UISearchBar idPoolTextInput = TextInput();
                        idPoolTextInput.OnContentsChanged += textInput =>
                        {
                            if (int.TryParse(textInput, out int result))
                            {
                                idPoolInput[i] = result;
                                string text = GetIntListDisplay(idPoolInput);
                                Message = $"Set ID Pool {text}";
                            }
                            else
                            {
                                Message = $"Invalid ID set index [{i}]";
                            }
                        };
                        setRect(idPoolTextInput, (40 * i) + 10, 10, 50, 30);
                        panel2.Append(idPoolTextInput);
                    }
                    break;
                case 3:
                    setRect(panel2, 400, 600, 400, 100);

                    for (int i = 0; i < idPoolInput.Length; i++)
                    {
                        UISearchBar widPoolIdTextInput = TextInput();
                        widPoolIdTextInput.OnContentsChanged += textInput =>
                        {
                            if (int.TryParse(textInput, out int result))
                            {
                                widPoolInput[i] = new WeightedID(result, widPoolInput[i].Weight);
                                string text = GetIntListDisplay(widPoolInput.Select(x => x.GetID()).ToArray());
                                Message = $"Set ID in Weighted Pool {text}[{i}]";
                            }
                            else if (NPCID.Search.TryGetId(textInput, out int result2))
                            {
                                nameInput = textInput;
                                Message = $"Set NPC name {textInput} (ID {result2})[{i}]";
                            }
                            else
                            {
                                Message = $"Invalid ID set index [{i}]";
                            }
                        };
                        setRect(widPoolIdTextInput, (40 * i) + 10, 10, 50, 30);
                        panel2.Append(widPoolIdTextInput);

                        UISearchBar widPoolWeightTextInput = TextInput();
                        widPoolWeightTextInput.OnContentsChanged += textInput =>
                        {
                            if (float.TryParse(textInput, out float result))
                            {
                                widPoolInput[i].Weight = result;
                                string text = GetFloatListDisplay(widPoolInput.Select(x => x.Weight));
                                Message = $"Set weight in Weighted Pool {text}[{i}]";
                            }
                            else
                            {
                                Message = $"Invalid weight set index [{i}]";
                            }
                        };
                        setRect(widPoolWeightTextInput, (40 * i) + 10, 10, 50, 30);
                        panel2.Append(widPoolWeightTextInput);
                    }
                    break;

            }
        }

        public static string GetFloatListDisplay(IEnumerable<float> floatList)
        {
            string text = "[";
            foreach (float id in floatList)
            {
                text += id.ToString() + ",";
            }
            text += "\b]";
            return text;
        }

        public static string GetIntListDisplay(int[] idPoolInput)
        {
            string text = "[";
            foreach (int id in idPoolInput)
            {
                text += id.ToString() + ",";
            }
            text += "\b]";
            return text;
        }

        private static UISearchBar TextInput()
        {
            Mod mod = ModContent.GetInstance<TerrariaCells>();
            Terraria.Localization.LocalizedText emptyContentText = mod.GetLocalization("blankInputEmptyContextText", delegate { return "_"; });
            var textInput = new UISearchBar(emptyContentText, 1f);
            textInput.SetContents("");
            setRect(textInput, 20, 00, 100, 20);
            textInput.OnLeftClick += delegate
            {
                if (!textInput.IsWritingText)
                {
                    textInput.ToggleTakingText();
                }
            };
            return textInput;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            message.SetText(Message);
            setStructureText.SetText(StructureMessage);
            setPositionText.SetText(PositionMessage);
            
        }
    }
}