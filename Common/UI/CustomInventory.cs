using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.UI.Chat;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.Graphics.Capture;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.UI.Gamepad;
using TerrariaCells.Common.Configs;
using TerrariaCells.Common.GlobalItems;
using TerrariaCells.Common.Items;

namespace TerrariaCells.Common.UI;

public class CustomAccessorySlot : ModAccessorySlot
{
    public override bool DrawDyeSlot => false;
    public override bool DrawVanitySlot => false;
}

public class CustomAccessorySlot2 : ModAccessorySlot
{
    public override bool DrawDyeSlot => false;
    public override bool DrawVanitySlot => false;
}

public class LimitedStorageUI : UIState
{
    static TerraCellsItemCategory currentSlotCategory = TerraCellsItemCategory.Default;

    static readonly (Vector2, int, TerraCellsItemCategory)[] inventorySlots =
    [
        (new Vector2(56f * 0, 0f), 0, TerraCellsItemCategory.Weapon),
        (new Vector2(56f * 1, 0f), 1, TerraCellsItemCategory.Weapon),
        (new Vector2(56f * 2, 0f), 2, TerraCellsItemCategory.Skill),
        (new Vector2(56f * 3, 0f), 3, TerraCellsItemCategory.Skill),
        (new Vector2(56f * 4, 0f), 4, TerraCellsItemCategory.Potion),
        (new Vector2(56f * 0, 60f), 10, TerraCellsItemCategory.Storage),
        (new Vector2(56f * 1, 60f), 11, TerraCellsItemCategory.Storage),
        (new Vector2(56f * 2, 60f), 12, TerraCellsItemCategory.Storage),
        (new Vector2(56f * 3, 60f), 13, TerraCellsItemCategory.Storage),
    ];

    public static readonly Color defaultSlotColor = new(12, 245, 103);
    public static readonly Color weaponSlotColor = new(150, 245, 150);
    public static readonly Color skillSlotColor = new(205, 150, 150);
    public static readonly Color potionSlotColor = new(215, 150, 243);
    public static readonly Color storageSlotColor = new(254, 255, 255);

    public static void CustomGUIHotbarDrawInner()
    {
        if (Main.playerInventory || Main.LocalPlayer.ghost)
            return;

        string text = Lang.inter[37].Value;
        if (
            Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].Name != null
            && Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].Name != ""
        )
            text =
                Main.LocalPlayer.inventory[Main.LocalPlayer.selectedItem].AffixName() ;

        DynamicSpriteFontExtensionMethods.DrawString(
            position: new Vector2(
                236f - (FontAssets.MouseText.Value.MeasureString(text) / 2f).X,
                0f
            ),
            spriteBatch: Main.spriteBatch,
            spriteFont: FontAssets.MouseText.Value,
            text: text,
            color: new Color(
                Main.mouseTextColor,
                Main.mouseTextColor,
                Main.mouseTextColor,
                Main.mouseTextColor
            ),
            rotation: 0f,
            origin: default,
            scale: 1f,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
        Main.inventoryScale = 1.15f;
        int positionX = 20;
        for (int i = 0; i < 5; i++)
        {

            switch (inventorySlots[i].Item3)
            {
                case TerraCellsItemCategory.Default:
                    Main.inventoryBack = defaultSlotColor;
                    break;
                case TerraCellsItemCategory.Weapon:
                    Main.inventoryBack = weaponSlotColor;
                    break;
                case TerraCellsItemCategory.Skill:
                    Main.inventoryBack = skillSlotColor;
                    break;
                case TerraCellsItemCategory.Potion:
                    Main.inventoryBack = potionSlotColor;
                    break;
                case TerraCellsItemCategory.Storage:
                    Main.inventoryBack = storageSlotColor;
                    break;
            }

            if (i == Main.LocalPlayer.selectedItem)
            {
                if (Main.hotbarScale[i] < 1f)
                    Main.hotbarScale[i] += 0.05f;
            }
            else if (Main.hotbarScale[i] > 0.75)
            {
                Main.hotbarScale[i] -= 0.05f;
            }

            float tempInventoryScale = Main.hotbarScale[i];
            int num3 = (int)(20f + 22f * (1f - tempInventoryScale));
            int a = (int)(75f + 150f * tempInventoryScale);
            Color lightColor = Main.inventoryBack with { A = (byte)a };
            if (
                !Main.LocalPlayer.hbLocked
                && !PlayerInput.IgnoreMouseInterface
                && Main.mouseX >= positionX
                && Main.mouseX
                    <= positionX + TextureAssets.InventoryBack.Width() * Main.hotbarScale[i]
                && Main.mouseY >= num3
                && Main.mouseY <= num3 + TextureAssets.InventoryBack.Height() * Main.hotbarScale[i]
                && !Main.LocalPlayer.channel
            )
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.LocalPlayer.cursorItemIconEnabled = false;
                if (Main.mouseLeft && !Main.LocalPlayer.hbLocked && !Main.blockMouse)
                    Main.LocalPlayer.changeItem = i;

                Main.hoverItemName = Main.LocalPlayer.inventory[i].AffixName();
                if (Main.LocalPlayer.inventory[i].stack > 1)
                    Main.hoverItemName =
                        Main.hoverItemName + " (" + Main.LocalPlayer.inventory[i].stack + ")";

                Main.rare = Main.LocalPlayer.inventory[i].rare;
            }

            float previousInventoryScale = Main.inventoryScale;
            Main.inventoryScale = tempInventoryScale;
            ItemSlot.Draw(
                Main.spriteBatch,
                Main.LocalPlayer.inventory,
                13,
                i,
                new Vector2(positionX, num3),
                lightColor
            );

            // Show cooldown ui on hotbar
            SkillSlotData skillSlotData = SkillModPlayer.GetSkillSlotData(i);

            if (skillSlotData != null && skillSlotData.cooldownTimer > 0)
            {
                // Cooldown item slot indicator
                Main.spriteBatch.Draw(
                    TextureAssets.InventoryBack.Value,
                    position: new Vector2(positionX, num3) + (Vector2.One * 20),
                    sourceRectangle: new Rectangle(
                        0,
                        0,
                        52,
                        (int)(
                            52 * ((float)skillSlotData.cooldownTimer / skillSlotData.cooldownTotal)
                        )
                    ),
                    color: new Color(15, 15, 15, 128),
                    rotation: 3.14159f,
                    origin: new Vector2(26, 26),
                    scale: new Vector2(Main.inventoryScale),
                    SpriteEffects.None,
                    layerDepth: 0f
                );

                // Cooldown countdown text display
                string currentCooldown = MathF.Ceiling(skillSlotData.cooldownTimer / 60).ToString();

                float width = FontAssets.DeathText.Value.MeasureString(currentCooldown).X;
                float textScale = Main.inventoryScale * 0.50f;

                if (TerrariaCellsConfig.Instance.ShowCooldown)
                {
                    ChatManager.DrawColorCodedStringWithShadow(
                        Main.spriteBatch,
                        FontAssets.DeathText.Value,
                        currentCooldown,
                        new Vector2(positionX, num3)
                            + (new Vector2(0f - width / 2f, 0f) * textScale)
                            + (Vector2.One * 20),
                        Color.White,
                        0,
                        Vector2.Zero,
                        new Vector2(textScale, textScale)
                    );
                }
            }

            Main.inventoryScale = previousInventoryScale;
            positionX += (int)(TextureAssets.InventoryBack.Width() * Main.hotbarScale[i]) + 4;
        }

        int selectedItem = Main.LocalPlayer.selectedItem;
        if (selectedItem >= 10 && (selectedItem != 58 || Main.mouseItem.type > ItemID.None))
        {
            float tempDrawScale = 1f;
            int positionY = (int)(20f + 22f * (1f - tempDrawScale));
            int alpha2 = (int)(75f + 150f * tempDrawScale);
            Color lightColor2 = new(255, 255, 255, alpha2);
            float tempPreviousDrawScale = Main.inventoryScale;
            Main.inventoryScale = tempDrawScale;
            ItemSlot.Draw(
                Main.spriteBatch,
                Main.LocalPlayer.inventory,
                13,
                selectedItem,
                new Vector2(positionX, positionY),
                lightColor2
            );
            Main.inventoryScale = tempPreviousDrawScale;
        }
    }

    public static void CustomDrawInterface_27_Inventory()
    {
        if (PlayerInput.SettingsForUI.ShowGamepadHints)
        {
            PlayerInput.ComposeInstructionsForGamepad();
            PlayerInput.AllowExecutionOfGamepadInstructions = false;
        }

        if (Main.playerInventory)
        {
            if (Main.player[Main.myPlayer].chest != -1)
                Main.CreativeMenu.CloseMenu();

            if (Main.ignoreErrors)
            {
                try
                {
                    DrawInventory();
                    return;
                }
                catch (Exception e)
                {
                    TimeLogger.DrawException(e);
                    return;
                }
            }

            DrawInventory();
        }
        else
        {
            Main.CreativeMenu.CloseMenu();
            Main.recFastScroll = true;
            Main.instance.SetMouseNPC(-1, -1);
            Main.EquipPage = 0;
        }
    }

    protected static void DrawInventory()
    {
        Recipe.GetThroughDelayedFindRecipes();
        // if (Main.ShouldPVPDraw)
        // Main.DrawPVPIcons();

        int num = 0;
        int num2 = 0;
        int num3 = Main.screenWidth;
        int num4 = 0;
        int num5 = Main.screenWidth;
        int num6 = 0;
        Vector2 vector = new Vector2(num, num2);
        Main.spriteBatch.DrawString(
            FontAssets.MouseText.Value,
            Lang.inter[4].Value,
            new Vector2(40f, 0f) + vector,
            new Color(
                Main.mouseTextColor,
                Main.mouseTextColor,
                Main.mouseTextColor,
                Main.mouseTextColor
            ),
            0f,
            default,
            1f,
            SpriteEffects.None,
            0f
        );
        Main.inventoryScale = 1.15f;
        if (
            Main.mouseX > 20
            && Main.mouseY > 20
            && !PlayerInput.IgnoreMouseInterface
            && Main.mouseX < (int)(20f + 56f * 4 * Main.inventoryScale)
            && Main.mouseY < (int)(20f + 60f * Main.inventoryScale)
            && !(
                Main.mouseX > (int)(20f + 56f * 3 * Main.inventoryScale)
                && Main.mouseY > (int)(20f + 60f * Main.inventoryScale)
            )
        )
            Main.player[Main.myPlayer].mouseInterface = true;

        // for (int i = 0; i < 10; i++)
        // {
        //     for (int j = 0; j < 5; j++)
        //     {
        foreach (var slot in inventorySlots)
        {
            int xPos = (int)(20f + slot.Item1.X * Main.inventoryScale) + num;
            int yPos = (int)(20f + slot.Item1.Y * Main.inventoryScale) + num2;
            int slotNumber = slot.Item2;
            currentSlotCategory = slot.Item3;
            switch (currentSlotCategory)
            {
                case TerraCellsItemCategory.Default:
                    Main.inventoryBack = new Color(12, 245, 103);
                    break;
                case TerraCellsItemCategory.Weapon:
                    Main.inventoryBack = new Color(150, 245, 150);
                    break;
                case TerraCellsItemCategory.Skill:
                    Main.inventoryBack = new Color(205, 150, 150);
                    break;
                case TerraCellsItemCategory.Potion:
                    Main.inventoryBack = new Color(215, 150, 243);
                    break;
                case TerraCellsItemCategory.Storage:
                    Main.inventoryBack = new Color(254, 255, 255);
                    break;
            }
            if (
                Main.mouseX >= xPos
                && Main.mouseX <= xPos + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                && Main.mouseY >= yPos
                && Main.mouseY <= yPos + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                && !PlayerInput.IgnoreMouseInterface
            )
            {
                Main.player[Main.myPlayer].mouseInterface = true;
                ItemSlot.OverrideHover(
                    Main.player[Main.myPlayer].inventory,
                    ItemSlot.Context.InventoryItem,
                    slotNumber
                );
                if (
                    Main.player[Main.myPlayer].inventoryChestStack[slotNumber]
                    && (
                        Main.player[Main.myPlayer].inventory[slotNumber].type == ItemID.None
                        || Main.player[Main.myPlayer].inventory[slotNumber].stack == 0
                    )
                )
                    Main.player[Main.myPlayer].inventoryChestStack[slotNumber] = false;

                if (!Main.player[Main.myPlayer].inventoryChestStack[slotNumber])
                {
                    ItemSlotLeftClick(Main.player[Main.myPlayer].inventory, 0, slotNumber);
                    ItemSlotRightClick(Main.player[Main.myPlayer].inventory, 0, slotNumber);
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                        Recipe.FindRecipes();
                }

                ItemSlot.MouseHover(Main.player[Main.myPlayer].inventory, 0, slotNumber);
            }

            ItemSlotDraw(
                Main.spriteBatch,
                Main.player[Main.myPlayer].inventory,
                0,
                slotNumber,
                new Vector2(xPos, yPos)
            );
        }
        // }

        /*
        GetBuilderAccsCountToShow(LocalPlayer, out var _, out var _, out var totalDrawnIcons);
        bool pushSideToolsUp = totalDrawnIcons >= 10;
        */
        int activeToggles = BuilderToggleLoader.ActiveBuilderToggles();
        bool pushSideToolsUp =
            activeToggles / 12 != BuilderToggleLoader.BuilderTogglePage || activeToggles % 12 >= 10;
        // if (!PlayerInput.UsingGamepad)
        //     Main.DrawHotbarLockIcon(num, num2, pushSideToolsUp);

        ItemSlot.DrawRadialDpad(
            Main.spriteBatch,
            new Vector2(20f)
                + new Vector2(56f * Main.inventoryScale * 10f, 56f * Main.inventoryScale * 5f)
                + new Vector2(26f, 70f)
                + vector
        );

        if (Main.mapEnabled)
        {
            bool flag = false;
            int num12 = num3 - 440;
            int num13 = 40 + num4;
            if (Main.screenWidth < 940)
                flag = true;

            if (flag)
            {
                num12 = num5 - 40;
                num13 = num6 - 200;
            }

            int num14 = 0;
            for (int k = 0; k < 4; k++)
            {
                int num15 = 255;
                int num16 = num12 + k * 32 - num14;
                int num17 = num13;
                if (flag)
                {
                    num16 = num12;
                    num17 = num13 + k * 32 - num14;
                }

                int num18 = k;
                num15 = 120;
                if (k > 0 && Main.mapStyle == k - 1)
                    num15 = 200;

                if (
                    Main.mouseX >= num16
                    && Main.mouseX <= num16 + 32
                    && Main.mouseY >= num17
                    && Main.mouseY <= num17 + 30
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    num15 = 255;
                    num18 += 4;
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (k == 0)
                        {
                            Main.playerInventory = false;
                            Main.player[Main.myPlayer].SetTalkNPC(-1);
                            Main.npcChatCornerItem = 0;
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Open"));
                            Main.mapFullscreenScale = 2.5f;
                            Main.mapFullscreen = true;
                            Main.resetMapFull = true;
                        }

                        if (k == 1)
                        {
                            Main.mapStyle = 0;
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        }

                        if (k == 2)
                        {
                            Main.mapStyle = 1;
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        }

                        if (k == 3)
                        {
                            Main.mapStyle = 2;
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        }
                    }
                }

                Main.spriteBatch.Draw(
                    TextureAssets.MapIcon[num18].Value,
                    new Vector2(num16, num17),
                    new Rectangle(
                        0,
                        0,
                        TextureAssets.MapIcon[num18].Width(),
                        TextureAssets.MapIcon[num18].Height()
                    ),
                    new Color(num15, num15, num15, num15),
                    0f,
                    default(Vector2),
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        if (Main.armorHide)
        {
            Main.armorAlpha -= 0.1f;
            if (Main.armorAlpha < 0f)
                Main.armorAlpha = 0f;
        }
        else
        {
            Main.armorAlpha += 0.025f;
            if (Main.armorAlpha > 1f)
                Main.armorAlpha = 1f;
        }

        Main.armorHide = false;

        int mH = 0;
        if (!Main.mapFullscreen && Main.mapStyle == 1)
            mH = 256;
        if (mH + Main.instance.RecommendedEquipmentAreaPushUp > Main.screenHeight)
        {
            mH = Main.screenHeight - Main.instance.RecommendedEquipmentAreaPushUp;
        }

        int num19 = 8 + Main.player[Main.myPlayer].GetAmountOfExtraAccessorySlotsToShow();
        int num20 = 174 + mH;
        int num21 = 950;
        // Main._cannotDrawAccessoriesHorizontally = false;
        if (Main.screenHeight < num21 && num19 >= 10)
        {
            num20 -= (int)(56f * Main.inventoryScale * (num19 - 9));
            // Main._cannotDrawAccessoriesHorizontally = true;
        }

        Main.EquipPage = 0;
        // int num22 = DrawPageIcons(num20 - 32);
        // if (num22 > -1)
        // {
        //     Main.HoverItem = new Item();
        //     switch (num22)
        //     {
        //         case 1:
        //             Main.hoverItemName = Lang.inter[80].Value;
        //             break;
        //         case 2:
        //             Main.hoverItemName = Lang.inter[79].Value;
        //             break;
        //         case 3:
        //             Main.hoverItemName = (
        //                 Main.CaptureModeDisabled ? Lang.inter[115].Value : Lang.inter[81].Value
        //             );
        //             break;
        //     }
        // }


        if (Main.EquipPage == 2)
        {
            Point mousePos = new Point(Main.mouseX, Main.mouseY);
            Rectangle inventoryBackTexture = new Rectangle(
                0,
                0,
                (int)(TextureAssets.InventoryBack.Width() * Main.inventoryScale),
                (int)(TextureAssets.InventoryBack.Height() * Main.inventoryScale)
            );
            Item[] inv = Main.player[Main.myPlayer].miscEquips;
            int screenWidthMinus92 = Main.screenWidth - 92;
            int mapHeightPlus174 = mH + 174;
            for (int l = 0; l < 2; l++)
            {
                switch (l)
                {
                    case 0:
                        inv = Main.player[Main.myPlayer].miscEquips;
                        break;
                    case 1:
                        inv = Main.player[Main.myPlayer].miscDyes;
                        break;
                }

                inventoryBackTexture.X = screenWidthMinus92 + l * -47;
                for (int m = 0; m < 5; m++)
                {
                    int context = 0;
                    int num25 = -1;
                    bool unlockedSuperCart = false;
                    switch (m)
                    {
                        case 0:
                            context = 19;
                            num25 = 0;
                            break;
                        case 1:
                            context = 20;
                            num25 = 1;
                            break;
                        case 2:
                            context = 18;
                            unlockedSuperCart = Main.player[Main.myPlayer].unlockedSuperCart;
                            break;
                        case 3:
                            context = 17;
                            break;
                        case 4:
                            context = 16;
                            break;
                    }

                    if (l == 1)
                    {
                        context = 33;
                        num25 = -1;
                        unlockedSuperCart = false;
                    }

                    inventoryBackTexture.Y = mapHeightPlus174 + m * 47;
                    bool flag3 = false;
                    Texture2D value2 = TextureAssets.InventoryTickOn.Value;
                    Rectangle r2 = new Rectangle(
                        inventoryBackTexture.Left + 34,
                        inventoryBackTexture.Top - 2,
                        value2.Width,
                        value2.Height
                    );
                    int num26 = 0;
                    if (num25 != -1)
                    {
                        if (Main.player[Main.myPlayer].hideMisc[num25])
                            value2 = TextureAssets.InventoryTickOff.Value;

                        if (r2.Contains(mousePos) && !PlayerInput.IgnoreMouseInterface)
                        {
                            Main.player[Main.myPlayer].mouseInterface = true;
                            flag3 = true;
                            if (Main.mouseLeft && Main.mouseLeftRelease)
                            {
                                if (num25 == 0)
                                    Main.player[Main.myPlayer].TogglePet();

                                if (num25 == 1)
                                    Main.player[Main.myPlayer].ToggleLight();

                                Main.mouseLeftRelease = false;
                                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));

                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    NetMessage.SendData(
                                        MessageID.SyncPlayer,
                                        -1,
                                        -1,
                                        null,
                                        Main.myPlayer
                                    );
                            }

                            num26 = ((!Main.player[Main.myPlayer].hideMisc[num25]) ? 1 : 2);
                        }
                    }

                    if (unlockedSuperCart)
                    {
                        value2 = TextureAssets.Extra[255].Value;
                        if (!Main.player[Main.myPlayer].enabledSuperCart)
                            value2 = TextureAssets.Extra[256].Value;

                        r2 = new Rectangle(
                            r2.X + r2.Width / 2,
                            r2.Y + r2.Height / 2,
                            r2.Width,
                            r2.Height
                        );
                        r2.Offset(-r2.Width / 2, -r2.Height / 2);
                        if (r2.Contains(mousePos) && !PlayerInput.IgnoreMouseInterface)
                        {
                            Main.player[Main.myPlayer].mouseInterface = true;
                            flag3 = true;
                            if (Main.mouseLeft && Main.mouseLeftRelease)
                            {
                                Main.player[Main.myPlayer].enabledSuperCart = !Main.player[
                                    Main.myPlayer
                                ].enabledSuperCart;
                                Main.mouseLeftRelease = false;
                                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                                if (Main.netMode == NetmodeID.MultiplayerClient)
                                    NetMessage.SendData(
                                        MessageID.SyncPlayer,
                                        -1,
                                        -1,
                                        null,
                                        Main.myPlayer
                                    );
                            }

                            num26 = ((!Main.player[Main.myPlayer].enabledSuperCart) ? 1 : 2);
                        }
                    }

                    if (
                        inventoryBackTexture.Contains(mousePos)
                        && !flag3
                        && !PlayerInput.IgnoreMouseInterface
                    )
                    {
                        Main.player[Main.myPlayer].mouseInterface = true;
                        Main.armorHide = true;
                        ItemSlot.Handle(inv, context, m);
                    }

                    ItemSlot.Draw(
                        Main.spriteBatch,
                        inv,
                        context,
                        m,
                        inventoryBackTexture.TopLeft()
                    );
                    if (num25 != -1)
                    {
                        Main.spriteBatch.Draw(value2, r2.TopLeft(), Color.White * 0.7f);
                        if (num26 > 0)
                        {
                            Main.HoverItem = new Item();
                            Main.hoverItemName = Lang.inter[58 + num26].Value;
                        }
                    }

                    if (unlockedSuperCart)
                    {
                        Main.spriteBatch.Draw(value2, r2.TopLeft(), Color.White);
                        if (num26 > 0)
                        {
                            Main.HoverItem = new Item();
                            Main.hoverItemName = Language.GetTextValue(
                                (num26 == 1)
                                    ? "GameUI.SuperCartDisabled"
                                    : "GameUI.SuperCartEnabled"
                            );
                        }
                    }
                }
            }

            mapHeightPlus174 += 247;
            screenWidthMinus92 += 8;
            int num27 = -1;
            int num28 = 0;
            int num29 = 3;
            int num30 = 260;
            if (Main.screenHeight > 630 + num30 * (Main.mapStyle == 1).ToInt())
                num29++;

            if (Main.screenHeight > 680 + num30 * (Main.mapStyle == 1).ToInt())
                num29++;

            if (Main.screenHeight > 730 + num30 * (Main.mapStyle == 1).ToInt())
                num29++;

            int num31 = 46;
            for (int n = 0; n < Player.MaxBuffs; n++)
            {
                if (Main.player[Main.myPlayer].buffType[n] != 0)
                {
                    int num32 = num28 / num29;
                    int num33 = num28 % num29;
                    Point point = new Point(
                        screenWidthMinus92 + num32 * -num31,
                        mapHeightPlus174 + num33 * num31
                    );
                    num27 = Main.DrawBuffIcon(num27, n, point.X, point.Y);
                    UILinkPointNavigator.SetPosition(
                        9000 + num28,
                        new Vector2(point.X + 30, point.Y + 30)
                    );
                    num28++;
                    if (Main.buffAlpha[n] < 0.65f)
                        Main.buffAlpha[n] = 0.65f;
                }
            }

            UILinkPointNavigator.Shortcuts.BUFFS_DRAWN = num28;
            UILinkPointNavigator.Shortcuts.BUFFS_PER_COLUMN = num29;
            if (num27 >= 0)
            {
                int num34 = Main.player[Main.myPlayer].buffType[num27];
                if (num34 > 0)
                {
                    string buffName = Lang.GetBuffName(num34);
                    string buffTooltip = Main.GetBuffTooltip(Main.player[Main.myPlayer], num34);
                    if (num34 == 147)
                        Main.bannerMouseOver = true;

                    /*
                    if (meleeBuff[num34])
                        MouseTextHackZoom(buffName, -10, 0, buffTooltip);
                    else
                        MouseTextHackZoom(buffName, buffTooltip);
                    */
                    int rare = 0;
                    if (Main.meleeBuff[num34])
                        rare = -10;

                    BuffLoader.ModifyBuffText(num34, ref buffName, ref buffTooltip, ref rare);
                    Main.instance.MouseTextHackZoom(buffName, rare, diff: 0, buffTooltip);
                }
            }
        }
        else if (Main.EquipPage == 1)
        {
            // Main.DrawNPCHousesInUI();
        }
        else if (Main.EquipPage == 0)
        { // Added 'if (EquipPage == 0)' to add custom equip pages
            int num35 = 4;
            if (
                Main.mouseX > Main.screenWidth - 64 - 28
                && Main.mouseX < (int)(Main.screenWidth - 64 - 28 + 56f * Main.inventoryScale)
                && Main.mouseY > num20
                && Main.mouseY < (int)(num20 + 448f * Main.inventoryScale)
                && !PlayerInput.IgnoreMouseInterface
            )
                Main.player[Main.myPlayer].mouseInterface = true;

            float num36 = Main.inventoryScale;
            bool flag4 = false;
            int num37 = num19 - 1;
            bool flag5 = Main.LocalPlayer.CanDemonHeartAccessoryBeShown();
            bool flag6 = Main.LocalPlayer.CanMasterModeAccessoryBeShown();

            var _settingsButtonIsPushedToSide =
                false
                && Main.player[Main.myPlayer].GetAmountOfExtraAccessorySlotsToShow() >= 1
                && (Main.screenHeight < num3 || (Main.screenHeight < num4 && Main.mapStyle == 1));

            if (_settingsButtonIsPushedToSide)
                num37--;

            int num38 = num37 - 1;
            Color color = Main.inventoryBack;
            Color color2 = new Color(80, 80, 80, 80);
            int num39 = -1;

            // ARMOR interface logic and draw call ================================================================================================
            // ====================================================================================================================================
            for (int armorSlot = 0; armorSlot < 5; armorSlot++)
            {
                Main.inventoryBack = new Color(244, 227, 150, 220);
                if (armorSlot > 2)
                {
                    Main.inventoryBack = new Color(229, 99, 145, 220);
                }

                if ((armorSlot == 8 && !flag5) || (armorSlot == 9 && !flag6))
                    continue;

                num39++;
                bool isSlotUnlocked = Main.LocalPlayer.IsItemSlotUnlockedAndUsable(armorSlot);
                if (!isSlotUnlocked)
                    flag4 = true;

                int num41 = Main.screenWidth - 64 - 28;
                int num42 = (int)(num20 + num39 * 56 * Main.inventoryScale);
                int num43 = Main.screenWidth - 58;
                int num44 = (int)(num20 - 2 + num39 * 56 * Main.inventoryScale);
                int context2 = 8;
                if (armorSlot > 2)
                {
                    num42 += num35;
                    num44 += num35;
                    context2 = 10;
                }

                Texture2D value3 = TextureAssets.InventoryTickOn.Value;
                if (Main.player[Main.myPlayer].hideVisibleAccessory[armorSlot])
                    value3 = TextureAssets.InventoryTickOff.Value;

                Rectangle rectangle = new Rectangle(num43, num44, value3.Width, value3.Height);
                int num45 = 0;
                if (
                    armorSlot > 2
                    && rectangle.Contains(new Point(Main.mouseX, Main.mouseY))
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        Main.player[Main.myPlayer].hideVisibleAccessory[armorSlot] = !Main.player[
                            Main.myPlayer
                        ].hideVisibleAccessory[armorSlot];
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, Main.myPlayer);
                    }

                    num45 = (!Main.player[Main.myPlayer].hideVisibleAccessory[armorSlot]) ? 1 : 2;
                }
                else if (
                    Main.mouseX >= num41
                    && Main.mouseX
                        <= num41 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                    && Main.mouseY >= num42
                    && Main.mouseY
                        <= num42 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.armorHide = true;
                    Main.player[Main.myPlayer].mouseInterface = true;
                    ItemSlot.OverrideHover(Main.player[Main.myPlayer].armor, context2, armorSlot);
                    if (isSlotUnlocked || Main.mouseItem.IsAir)
                        ItemSlotLeftClick(Main.player[Main.myPlayer].armor, context2, armorSlot);

                    ItemSlot.MouseHover(Main.player[Main.myPlayer].armor, context2, armorSlot);
                }

                if (flag4)
                    Main.inventoryBack = color2;

                ItemSlotDraw(
                    Main.spriteBatch,
                    Main.player[Main.myPlayer].armor,
                    context2,
                    armorSlot,
                    new Vector2(num41, num42)
                );
                if (armorSlot > 2)
                {
                    Main.spriteBatch.Draw(value3, new Vector2(num43, num44), Color.White * 0.7f);
                    if (num45 > 0)
                    {
                        Main.HoverItem = new Item();
                        Main.hoverItemName = Lang.inter[58 + num45].Value;
                    }
                }
            }

            Main.inventoryBack = color;
            if (
                Main.mouseX > Main.screenWidth - 64 - 28 - 47
                && Main.mouseX < (int)(Main.screenWidth - 64 - 20 - 47 + 56f * Main.inventoryScale)
                && Main.mouseY > num20
                && Main.mouseY < (int)(num20 + 168f * Main.inventoryScale)
                && !PlayerInput.IgnoreMouseInterface
            )
                Main.player[Main.myPlayer].mouseInterface = true;

            num39 = -1;

            /*
            vanity slots
            for (int num46 = 10; num46 < 13; num46++)
            {
                if ((num46 == 18 && !flag5) || (num46 == 19 && !flag6))
                    continue;

                num39++;
                bool num47 = Main.LocalPlayer.IsItemSlotUnlockedAndUsable(num46);
                flag4 = !num47;
                bool flag8 = !num47 && !Main.mouseItem.IsAir;
                int num48 = Main.screenWidth - 64 - 28 - 47;
                int num49 = (int)(num20 + num39 * 56 * Main.inventoryScale);
                // _ = new Color(100, 100, 100, 100);
                if (num46 > 12)
                    num49 += num35;

                int context3 = 9;
                if (num46 > 12)
                    context3 = 11;

                if (
                    Main.mouseX >= num48
                    && Main.mouseX
                        <= num48 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                    && Main.mouseY >= num49
                    && Main.mouseY
                        <= num49 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    Main.armorHide = true;
                    ItemSlot.OverrideHover(Main.player[Main.myPlayer].armor, context3, num46);
                    if (!flag8)
                    {
                        ItemSlot.LeftClick(Main.player[Main.myPlayer].armor, context3, num46);
                        ItemSlot.RightClick(Main.player[Main.myPlayer].armor, context3, num46);
                    }

                    ItemSlot.MouseHover(Main.player[Main.myPlayer].armor, context3, num46);
                }

                if (flag4)
                    Main.inventoryBack = color2;

                ItemSlot.Draw(
                    Main.spriteBatch,
                    Main.player[Main.myPlayer].armor,
                    context3,
                    num46,
                    new Vector2(num48, num49)
                );
            }
            */

            Main.inventoryBack = color;
            if (
                Main.mouseX > Main.screenWidth - 64 - 28 - 47
                && Main.mouseX < (int)(Main.screenWidth - 64 - 20 - 47 + 56f * Main.inventoryScale)
                && Main.mouseY > num20
                && Main.mouseY < (int)(num20 + 168f * Main.inventoryScale)
                && !PlayerInput.IgnoreMouseInterface
            )
                Main.player[Main.myPlayer].mouseInterface = true;

            num39 = -1;

            // Vanilla acc (not armor) slot drawing moved to AccessorySlotLoader.DrawAccSlots
            /*
            for (int num50 = 0; num50 < 10; num50++) {
            
            dyes
            for (int num50 = 0; num50 < 3; num50++)
            {
                if ((num50 == 8 && !flag5) || (num50 == 9 && !flag6))
                    continue;

                num39++;
                bool num51 = Main.LocalPlayer.IsItemSlotUnlockedAndUsable(num50);
                flag4 = !num51;
                bool flag9 = !num51 && !Main.mouseItem.IsAir;
                int num52 = Main.screenWidth - 64 - 28 - 47 - 47;
                int num53 = (int)(num20 + num39 * 56 * Main.inventoryScale);
                _ = new Color(100, 100, 100, 100);
                if (num50 > 2)
                    num53 += num35;

                if (
                    Main.mouseX >= num52
                    && Main.mouseX
                        <= num52 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                    && Main.mouseY >= num53
                    && Main.mouseY
                        <= num53 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    Main.armorHide = true;
                    ItemSlot.OverrideHover(Main.player[Main.myPlayer].dye, 12, num50);
                    if (!flag9)
                    {
                        if (Main.mouseRightRelease && Main.mouseRight)
                            ItemSlot.RightClick(Main.player[Main.myPlayer].dye, 12, num50);

                        ItemSlot.LeftClick(Main.player[Main.myPlayer].dye, 12, num50);
                    }

                    ItemSlot.MouseHover(Main.player[Main.myPlayer].dye, 12, num50);
                }

                if (flag4)
                    Main.inventoryBack = color2;

                ItemSlot.Draw(
                    Main.spriteBatch,
                    Main.player[Main.myPlayer].dye,
                    12,
                    num50,
                    new Vector2(num52, num53)
                );
            }
            */

            Main.inventoryBack = color;

            // Moved from above [[
            var defPos = AccessorySlotLoader.DefenseIconPosition;
            defPos += new Vector2(360, -180);
            typeof(Main)
                .GetMethod("DrawDefenseCounter", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, [(int)defPos.X, (int)defPos.Y]);

            Main.inventoryBack = color;
            // ]]

            Main.inventoryScale = num36;
        }

        // LoaderManager.Get<AccessorySlotLoader>().DrawAccSlots(num20);

        int num54 = (Main.screenHeight - 600) / 2;
        int num55 = (int)(Main.screenHeight / 600f * 250f);
        if (Main.screenHeight < 700)
        {
            num54 = (Main.screenHeight - 508) / 2;
            num55 = (int)(Main.screenHeight / 600f * 200f);
        }
        else if (Main.screenHeight < 850)
        {
            num55 = (int)(Main.screenHeight / 600f * 225f);
        }

        if (Main.craftingHide)
        {
            Main.craftingAlpha -= 0.1f;
            if (Main.craftingAlpha < 0f)
                Main.craftingAlpha = 0f;
        }
        else
        {
            Main.craftingAlpha += 0.025f;
            if (Main.craftingAlpha > 1f)
                Main.craftingAlpha = 1f;
        }

        Color color3 = new Color(
            (byte)(Main.mouseTextColor * Main.craftingAlpha),
            (byte)(Main.mouseTextColor * Main.craftingAlpha),
            (byte)(Main.mouseTextColor * Main.craftingAlpha),
            (byte)(Main.mouseTextColor * Main.craftingAlpha)
        );
        Main.craftingHide = false;

        if (Main.InReforgeMenu)
        {
            // if (Main.mouseReforge)
            // {
            //     if (Main.reforgeScale < 1f)
            //         Main.reforgeScale += 0.02f;
            // }
            // else if (Main.reforgeScale > 1f)
            // {
            //     Main.reforgeScale -= 0.02f;
            // }

            if (
                Main.LocalPlayer.chest != -1
                || Main.npcShop != 0
                || Main.LocalPlayer.talkNPC == -1
                || Main.InGuideCraftMenu
            )
            {
                Main.InReforgeMenu = false;
                Main.LocalPlayer.dropItemCheck();
                Recipe.FindRecipes();
            }
            else
            {
                int slotX = 50;
                int slotY = 270;
                string text = Lang.inter[46].Value + ": ";
                if (Main.reforgeItem.type > ItemID.None)
                {
                    int reforgeCost = Main.reforgeItem.value;
                    reforgeCost *= Main.reforgeItem.stack; // #StackablePrefixWeapons: scale with current stack size

                    bool canApplyDiscount = true;
                    if (!ItemLoader.ReforgePrice(Main.reforgeItem, ref reforgeCost, ref canApplyDiscount))
                        goto skipVanillaPricing;

                    /*
                    if (Main.LocalPlayer.discountAvailable)
                    */
                    if (canApplyDiscount && Main.LocalPlayer.discountAvailable)
                        reforgeCost = (int)((double)reforgeCost * 0.8);

                    reforgeCost = (int)(
                        (double)reforgeCost * Main.LocalPlayer.currentShoppingSettings.PriceAdjustment
                    );
                    reforgeCost /= 3;
                    skipVanillaPricing:

                    string costText = "";
                    int platinumCost = 0;
                    int goldCost = 0;
                    int silverCost = 0;
                    int copperCost = 0;
                    int finalCost = reforgeCost;
                    if (finalCost < 1)
                        finalCost = 1;

                    if (finalCost >= 1000000)
                    {
                        platinumCost = finalCost / 1000000;
                        finalCost -= platinumCost * 1000000;
                    }

                    if (finalCost >= 10000)
                    {
                        goldCost = finalCost / 10000;
                        finalCost -= goldCost * 10000;
                    }

                    if (finalCost >= 100)
                    {
                        silverCost = finalCost / 100;
                        finalCost -= silverCost * 100;
                    }

                    if (finalCost >= 1)
                        copperCost = finalCost;

                    if (platinumCost > 0)
                        costText =
                            costText
                            + "[c/"
                            + Colors.AlphaDarken(Colors.CoinPlatinum).Hex3()
                            + ":"
                            + platinumCost
                            + " "
                            + Lang.inter[15].Value
                            + "] ";

                    if (goldCost > 0)
                        costText =
                            costText
                            + "[c/"
                            + Colors.AlphaDarken(Colors.CoinGold).Hex3()
                            + ":"
                            + goldCost
                            + " "
                            + Lang.inter[16].Value
                            + "] ";

                    if (silverCost > 0)
                        costText =
                            costText
                            + "[c/"
                            + Colors.AlphaDarken(Colors.CoinSilver).Hex3()
                            + ":"
                            + silverCost
                            + " "
                            + Lang.inter[17].Value
                            + "] ";

                    if (copperCost > 0)
                        costText =
                            costText
                            + "[c/"
                            + Colors.AlphaDarken(Colors.CoinCopper).Hex3()
                            + ":"
                            + copperCost
                            + " "
                            + Lang.inter[18].Value
                            + "] ";

                    // ItemSlot.DrawSavings(
                    //     Main.spriteBatch,
                    //     num56 + 130,
                    //     Main.instance.invBottom,
                    //     horizontal: true
                    // );
                    ChatManager.DrawColorCodedStringWithShadow(
                        Main.spriteBatch,
                        FontAssets.MouseText.Value,
                        costText,
                        new Vector2(
                            (float)(slotX + 50 + 25) + FontAssets.MouseText.Value.MeasureString(text).X,
                            slotY
                        ),
                        Microsoft.Xna.Framework.Color.White,
                        0f,
                        Vector2.Zero,
                        Vector2.One
                    );
                    int reforgeButtonX = slotX + 70 + 25; // moving elements over by 25px to fix overlap
                    int reforgeButtonY = slotY + 40;
                    bool hoveringOverReforgeButton =
                        Main.mouseX > reforgeButtonX - 15 
                        && Main.mouseX < reforgeButtonX + 15
                        && Main.mouseY > reforgeButtonY - 15
                        && Main.mouseY < reforgeButtonY + 15
                        && !PlayerInput.IgnoreMouseInterface;
                    Texture2D hoverTexture = TextureAssets.Reforge[0].Value;
                    if (hoveringOverReforgeButton)
                        hoverTexture = TextureAssets.Reforge[1].Value;

                    Main.spriteBatch.Draw(
                        hoverTexture,
                        new Vector2(reforgeButtonX, reforgeButtonY),
                        null,
                        Microsoft.Xna.Framework.Color.White,
                        0f,
                        hoverTexture.Size() / 2f,
                        Main.reforgeScale,
                        SpriteEffects.None,
                        0f
                    );
                    UILinkPointNavigator.SetPosition(
                        304,
                        new Vector2(reforgeButtonX, reforgeButtonY) + hoverTexture.Size() / 4f
                    );
                    if (hoveringOverReforgeButton)
                    {
                        Main.hoverItemName = Lang.inter[19].Value;
                        if (!Main.mouseReforge)
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));

                        Main.mouseReforge = true;
                        Main.LocalPlayer.mouseInterface = true;

                        /*
                        if (mouseLeftRelease && mouseLeft && Main.LocalPlayer.BuyItem(num58)) {
                        */
                        if (
                            Main.mouseLeftRelease
                            && Main.mouseLeft
                            && Main.LocalPlayer.CanAfford(reforgeCost)
                            && ItemLoader.CanReforge(Main.reforgeItem)
                        )
                        {
                            if (Main.reforgeItem.TryGetGlobalItem(out FunkyModifierItemModifier modifier)) {

                                Main.LocalPlayer.BuyItem(reforgeCost);
                                // Disabled reforge system, using FunkyModifier system now >:3
                                // ItemLoader.PreReforge(Main.reforgeItem); // After BuyItem just in case

                                FunkyModifierItemModifier.Reforge(Main.reforgeItem);
                                // Main.reforgeItem.ResetPrefix();
                                // Main.reforgeItem.Prefix(-2);
                                Main.reforgeItem.position.X =
                                    Main.LocalPlayer.position.X
                                    + (float)(Main.LocalPlayer.width / 2)
                                    - (float)(Main.reforgeItem.width / 2);
                                Main.reforgeItem.position.Y =
                                    Main.LocalPlayer.position.Y
                                    + (float)(Main.LocalPlayer.height / 2)
                                    - (float)(Main.reforgeItem.height / 2);

                                // ItemLoader.PostReforge(Main.reforgeItem);

                                foreach (var mod in modifier.modifiers)
                                {
                                    var item = Main.reforgeItem.Clone();
                                    item.SetNameOverride(mod.ToString());
                                    PopupText.NewText(
                                        PopupTextContext.ItemReforge,
                                        item,
                                        Main.reforgeItem.stack,
                                        noStack: true,
                                        longText: true
                                    );
                                }
                            } else {
                                var item = Main.reforgeItem.Clone();
                                item.SetNameOverride("Could not reforge!");
                                PopupText.NewText(
                                    PopupTextContext.ItemReforge,
                                    item,
                                    Main.reforgeItem.stack,
                                    noStack: true,
                                    longText: true
                                );
                            }
                            SoundEngine.PlaySound(SoundID.Item37);
                        }
                    }
                    else
                    {
                        Main.mouseReforge = false;
                    }
                }
                else
                {
                    text = Lang.inter[20].Value;
                }

                ChatManager.DrawColorCodedStringWithShadow(
                    Main.spriteBatch,
                    FontAssets.MouseText.Value,
                    text,
                    new Vector2(slotX + 50 + 25, slotY),
                    new Microsoft.Xna.Framework.Color(
                        Main.mouseTextColor,
                        Main.mouseTextColor,
                        Main.mouseTextColor,
                        Main.mouseTextColor
                    ),
                    0f,
                    Vector2.Zero,
                    Vector2.One
                );
                if (
                    Main.mouseX >= slotX
                    && (float)Main.mouseX
                        <= (float)slotX
                            + (float)TextureAssets.InventoryBack.Width() * Main.inventoryScale
                    && Main.mouseY >= slotY
                    && (float)Main.mouseY
                        <= (float)slotY
                            + (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.LocalPlayer.mouseInterface = true;
                    Main.craftingHide = true;
                    ItemSlot.LeftClick(ref Main.reforgeItem, 5);
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                        Recipe.FindRecipes();

                    ItemSlot.RightClick(ref Main.reforgeItem, 5);
                    ItemSlot.MouseHover(ref Main.reforgeItem, 5);
                }

                ItemSlot.Draw(Main.spriteBatch, ref Main.reforgeItem, 5, new Vector2(slotX, slotY));
            }
        }
        // else if (InGuideCraftMenu) {

        Main.CreativeMenu.Draw(Main.spriteBatch);
        bool flag10 = Main.CreativeMenu.Enabled && !Main.CreativeMenu.Blocked;
        flag10 |= Main.hidePlayerCraftingMenu;

        if (!Main.InReforgeMenu && !Main.LocalPlayer.tileEntityAnchor.InUse && !flag10)
        {
            UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeBig = -1;
            UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeSmall = -1;
            if (Main.numAvailableRecipes > 0)
                Main.spriteBatch.DrawString(
                    FontAssets.MouseText.Value,
                    Lang.inter[25].Value,
                    new Vector2(76f, 414 + num54),
                    color3,
                    0f,
                    default,
                    1f,
                    SpriteEffects.None,
                    0f
                );

            for (int num67 = 0; num67 < Recipe.maxRecipes; num67++)
            {
                Main.inventoryScale = 100f / (Math.Abs(Main.availableRecipeY[num67]) + 100f);
                if (Main.inventoryScale < 0.75)
                    Main.inventoryScale = 0.75f;

                if (Main.recFastScroll)
                    Main.inventoryScale = 0.75f;

                if (Main.availableRecipeY[num67] < (num67 - Main.focusRecipe) * 65)
                {
                    if (Main.availableRecipeY[num67] == 0f && !Main.recFastScroll)
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));

                    Main.availableRecipeY[num67] += 6.5f;
                    if (Main.recFastScroll)
                        Main.availableRecipeY[num67] += 130000f;

                    if (Main.availableRecipeY[num67] > (num67 - Main.focusRecipe) * 65)
                        Main.availableRecipeY[num67] = (num67 - Main.focusRecipe) * 65;
                }
                else if (Main.availableRecipeY[num67] > (num67 - Main.focusRecipe) * 65)
                {
                    if (Main.availableRecipeY[num67] == 0f && !Main.recFastScroll)
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));

                    Main.availableRecipeY[num67] -= 6.5f;
                    if (Main.recFastScroll)
                        Main.availableRecipeY[num67] -= 130000f;

                    if (Main.availableRecipeY[num67] < (num67 - Main.focusRecipe) * 65)
                        Main.availableRecipeY[num67] = (num67 - Main.focusRecipe) * 65;
                }
                else
                {
                    Main.recFastScroll = false;
                }

                if (
                    num67 >= Main.numAvailableRecipes
                    || Math.Abs(Main.availableRecipeY[num67]) > num55
                )
                    continue;

                int num68 = (int)(46f - 26f * Main.inventoryScale);
                int num69 = (int)(
                    410f
                    + Main.availableRecipeY[num67] * Main.inventoryScale
                    - 30f * Main.inventoryScale
                    + num54
                );
                double num70 = Main.inventoryBack.A + 50;
                double num71 = 255.0;
                if (Math.Abs(Main.availableRecipeY[num67]) > num55 - 100f)
                {
                    num70 =
                        (double)(
                            150f
                            * (100f - (Math.Abs(Main.availableRecipeY[num67]) - (num55 - 100f)))
                        ) * 0.01;
                    num71 =
                        (double)(
                            255f
                            * (100f - (Math.Abs(Main.availableRecipeY[num67]) - (num55 - 100f)))
                        ) * 0.01;
                }

                new Color((byte)num70, (byte)num70, (byte)num70, (byte)num70);
                Color lightColor = new Color((byte)num71, (byte)num71, (byte)num71, (byte)num71);
                if (
                    !Main.LocalPlayer.creativeInterface
                    && Main.mouseX >= num68
                    && Main.mouseX
                        <= num68 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                    && Main.mouseY >= num69
                    && Main.mouseY
                        <= num69 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    var reflectedMain = typeof(Main);
                    reflectedMain
                        .GetMethod(
                            "HoverOverCraftingItemButton",
                            BindingFlags.NonPublic | BindingFlags.Static
                        )
                        .Invoke(null, [num67]);
                }

                if (Main.numAvailableRecipes <= 0)
                    continue;

                num70 -= 50.0;
                if (num70 < 0.0)
                    num70 = 0.0;

                if (num67 == Main.focusRecipe)
                {
                    UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeSmall = 0;
                    if (PlayerInput.SettingsForUI.HighlightThingsForMouse)
                        ItemSlot.DrawGoldBGForCraftingMaterial = true;
                }
                else
                {
                    UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeSmall = -1;
                }

                Color color4 = Main.inventoryBack;
                Main.inventoryBack = new Color((byte)num70, (byte)num70, (byte)num70, (byte)num70);
                ItemSlot.Draw(
                    Main.spriteBatch,
                    ref Main.recipe[Main.availableRecipe[num67]].createItem,
                    22,
                    new Vector2(num68, num69),
                    lightColor
                );
                Main.inventoryBack = color4;
            }

            if (Main.numAvailableRecipes > 0)
            {
                UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeBig = -1;
                UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeSmall = -1;
                for (
                    int num72 = 0;
                    num72 < Main.recipe[Main.availableRecipe[Main.focusRecipe]].requiredItem.Count;
                    num72++
                )
                {
                    if (
                        Main.recipe[Main.availableRecipe[Main.focusRecipe]].requiredItem[num72].type
                        == ItemID.None
                    )
                    {
                        UILinkPointNavigator.Shortcuts.CRAFT_CurrentIngredientsCount = num72 + 1;
                        break;
                    }

                    int num73 = 80 + num72 * 40;
                    int num74 = 380 + num54;
                    double num75 = Main.inventoryBack.A + 50;
                    double num76 = 255.0;
                    Color white = Color.White;
                    Color white2 = Color.White;
                    num75 =
                        Main.inventoryBack.A
                        + 50
                        - Math.Abs(Main.availableRecipeY[Main.focusRecipe]) * 2f;
                    num76 = 255f - Math.Abs(Main.availableRecipeY[Main.focusRecipe]) * 2f;
                    if (num75 < 0.0)
                        num75 = 0.0;

                    if (num76 < 0.0)
                        num76 = 0.0;

                    white.R = (byte)num75;
                    white.G = (byte)num75;
                    white.B = (byte)num75;
                    white.A = (byte)num75;
                    white2.R = (byte)num76;
                    white2.G = (byte)num76;
                    white2.B = (byte)num76;
                    white2.A = (byte)num76;
                    Main.inventoryScale = 0.6f;
                    if (num75 == 0.0)
                        break;

                    if (
                        Main.mouseX >= num73
                        && Main.mouseX
                            <= num73 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                        && Main.mouseY >= num74
                        && Main.mouseY
                            <= num74 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                        && !PlayerInput.IgnoreMouseInterface
                    )
                    {
                        Main.craftingHide = true;
                        Main.player[Main.myPlayer].mouseInterface = true;
                        // SetRecipeMaterialDisplayName(num72);
                    }

                    num75 -= 50.0;
                    if (num75 < 0.0)
                        num75 = 0.0;

                    UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeSmall = 1 + num72;
                    Color color5 = Main.inventoryBack;
                    Main.inventoryBack = new Color(
                        (byte)num75,
                        (byte)num75,
                        (byte)num75,
                        (byte)num75
                    );

                    //TML: Ref a local copy variable instead of the data in the array.
                    /*
                    ItemSlot.Draw(spriteBatch, ref recipe[availableRecipe[focusRecipe]].requiredItem[num72], 22, new Vector2(num73, num74));
                    */
                    Item tempItem = Main.recipe[
                        Main.availableRecipe[Main.focusRecipe]
                    ].requiredItem[num72];
                    ItemSlot.Draw(Main.spriteBatch, ref tempItem, 22, new Vector2(num73, num74));
                    Main.inventoryBack = color5;
                }
            }

            if (Main.numAvailableRecipes == 0)
            {
                Main.recBigList = false;
            }
            else
            {
                int num77 = 94;
                int num78 = 450 + num54;
                if (Main.InGuideCraftMenu)
                    num78 -= 150;

                bool flag11 =
                    Main.mouseX > num77 - 15
                    && Main.mouseX < num77 + 15
                    && Main.mouseY > num78 - 15
                    && Main.mouseY < num78 + 15
                    && !PlayerInput.IgnoreMouseInterface;
                int num79 = Main.recBigList.ToInt() * 2 + flag11.ToInt();
                Main.spriteBatch.Draw(
                    TextureAssets.CraftToggle[num79].Value,
                    new Vector2(num77, num78),
                    null,
                    Color.White,
                    0f,
                    TextureAssets.CraftToggle[num79].Value.Size() / 2f,
                    1f,
                    SpriteEffects.None,
                    0f
                );
                if (flag11)
                {
                    Main.instance.MouseText(Language.GetTextValue("GameUI.CraftingWindow"), 0, 0);
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (!Main.recBigList)
                        {
                            Main.recBigList = true;
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        }
                        else
                        {
                            Main.recBigList = false;
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        }
                    }
                }
            }
        }

        // Added by TML
        Main.hidePlayerCraftingMenu = false;

        if (Main.recBigList && !flag10)
        {
            UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeBig = -1;
            UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeSmall = -1;
            int num80 = 42;
            if (Main.inventoryScale < 0.75)
                Main.inventoryScale = 0.75f;

            int num81 = 340;
            int num82 = 310;
            int num83 = (Main.screenWidth - num82 - 280) / num80;
            int num84 = (Main.screenHeight - num81 - 20) / num80;
            UILinkPointNavigator.Shortcuts.CRAFT_IconsPerRow = num83;
            UILinkPointNavigator.Shortcuts.CRAFT_IconsPerColumn = num84;
            int num85 = 0;
            int num86 = 0;
            int num87 = num82;
            int num88 = num81;
            int num89 = num82 - 20;
            int num90 = num81 + 2;
            if (Main.recStart > Main.numAvailableRecipes - num83 * num84)
            {
                Main.recStart = Main.numAvailableRecipes - num83 * num84;
                if (Main.recStart < 0)
                    Main.recStart = 0;
            }

            if (Main.recStart > 0)
            {
                if (
                    Main.mouseX >= num89
                    && Main.mouseX <= num89 + TextureAssets.CraftUpButton.Width()
                    && Main.mouseY >= num90
                    && Main.mouseY <= num90 + TextureAssets.CraftUpButton.Height()
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        Main.recStart -= num83;
                        if (Main.recStart < 0)
                            Main.recStart = 0;

                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        Main.mouseLeftRelease = false;
                    }
                }

                Main.spriteBatch.Draw(
                    TextureAssets.CraftUpButton.Value,
                    new Vector2(num89, num90),
                    new Rectangle(
                        0,
                        0,
                        TextureAssets.CraftUpButton.Width(),
                        TextureAssets.CraftUpButton.Height()
                    ),
                    new Color(200, 200, 200, 200),
                    0f,
                    default(Vector2),
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            if (Main.recStart < Main.numAvailableRecipes - num83 * num84)
            {
                num90 += 20;
                if (
                    Main.mouseX >= num89
                    && Main.mouseX <= num89 + TextureAssets.CraftUpButton.Width()
                    && Main.mouseY >= num90
                    && Main.mouseY <= num90 + TextureAssets.CraftUpButton.Height()
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        Main.recStart += num83;
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        if (Main.recStart > Main.numAvailableRecipes - num83)
                            Main.recStart = Main.numAvailableRecipes - num83;

                        Main.mouseLeftRelease = false;
                    }
                }

                Main.spriteBatch.Draw(
                    TextureAssets.CraftDownButton.Value,
                    new Vector2(num89, num90),
                    new Rectangle(
                        0,
                        0,
                        TextureAssets.CraftUpButton.Width(),
                        TextureAssets.CraftUpButton.Height()
                    ),
                    new Color(200, 200, 200, 200),
                    0f,
                    default(Vector2),
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            for (
                int num91 = Main.recStart;
                num91 < Recipe.maxRecipes && num91 < Main.numAvailableRecipes;
                num91++
            )
            {
                int num92 = num87;
                int num93 = num88;
                double num94 = Main.inventoryBack.A + 50;
                double num95 = 255.0;
                new Color((byte)num94, (byte)num94, (byte)num94, (byte)num94);
                new Color((byte)num95, (byte)num95, (byte)num95, (byte)num95);
                if (
                    Main.mouseX >= num92
                    && Main.mouseX
                        <= num92 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                    && Main.mouseY >= num93
                    && Main.mouseY
                        <= num93 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                    && !PlayerInput.IgnoreMouseInterface
                )
                {
                    Main.player[Main.myPlayer].mouseInterface = true;
                    if (Main.mouseLeftRelease && Main.mouseLeft)
                    {
                        Main.focusRecipe = num91;
                        Main.recFastScroll = true;
                        Main.recBigList = false;
                        // SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        Main.mouseLeftRelease = false;
                        if (PlayerInput.UsingGamepadUI)
                        {
                            UILinkPointNavigator.ChangePage(9);
                            Main.LockCraftingForThisCraftClickDuration();
                        }
                    }

                    Main.craftingHide = true;
                    Main.HoverItem = Main.recipe[Main.availableRecipe[num91]].createItem.Clone();
                    ItemSlot.MouseHover(22);
                    Main.hoverItemName = Main.recipe[Main.availableRecipe[num91]].createItem.Name;
                    if (Main.recipe[Main.availableRecipe[num91]].createItem.stack > 1)
                        Main.hoverItemName =
                            Main.hoverItemName
                            + " ("
                            + Main.recipe[Main.availableRecipe[num91]].createItem.stack
                            + ")";
                }

                if (Main.numAvailableRecipes > 0)
                {
                    num94 -= 50.0;
                    if (num94 < 0.0)
                        num94 = 0.0;

                    UILinkPointNavigator.Shortcuts.CRAFT_CurrentRecipeBig = num91 - Main.recStart;
                    Color color6 = Main.inventoryBack;
                    Main.inventoryBack = new Color(
                        (byte)num94,
                        (byte)num94,
                        (byte)num94,
                        (byte)num94
                    );
                    ItemSlot.Draw(
                        Main.spriteBatch,
                        ref Main.recipe[Main.availableRecipe[num91]].createItem,
                        22,
                        new Vector2(num92, num93)
                    );
                    Main.inventoryBack = color6;
                }

                num87 += num80;
                num85++;
                if (num85 >= num83)
                {
                    num87 = num82;
                    num88 += num80;
                    num85 = 0;
                    num86++;
                    if (num86 >= num84)
                        break;
                }
            }
        }

        // Vector2 vector2 = FontAssets.MouseText.Value.MeasureString("Coins");
        // Vector2 vector3 = FontAssets.MouseText.Value.MeasureString(Lang.inter[26].Value);
        // float num96 = vector2.X / vector3.X;
        // Main.spriteBatch.DrawString(
        //     FontAssets.MouseText.Value,
        //     Lang.inter[26].Value,
        //     new Vector2(496f, 84f + (vector2.Y - vector2.Y * num96) / 2f),
        //     new Color(
        //         Main.mouseTextColor,
        //         Main.mouseTextColor,
        //         Main.mouseTextColor,
        //         Main.mouseTextColor
        //     ),
        //     0f,
        //     default(Vector2),
        //     0.75f * num96,
        //     SpriteEffects.None,
        //     0f
        // );

        // COIN SLOTS
        Main.inventoryScale = 0.6f;
        for (int coinSlotIndex = 0; coinSlotIndex < 4; coinSlotIndex++)
        {
            int slotPositionX = 350;
            int slotPositionY = (int)(20f + coinSlotIndex * 56 * Main.inventoryScale + 20f);
            int inventoryCoinSlotIndex = coinSlotIndex + 50;
            new Color(100, 100, 100, 100);
            if (
                Main.mouseX >= slotPositionX
                && Main.mouseX
                    <= slotPositionX + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                && Main.mouseY >= slotPositionY
                && Main.mouseY
                    <= slotPositionY + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                && !PlayerInput.IgnoreMouseInterface
            )
            {
                Main.player[Main.myPlayer].mouseInterface = true;
                ItemSlot.OverrideHover(
                    Main.player[Main.myPlayer].inventory,
                    1,
                    inventoryCoinSlotIndex
                );
                ItemSlot.LeftClick(Main.player[Main.myPlayer].inventory, 1, inventoryCoinSlotIndex);
                ItemSlot.RightClick(
                    Main.player[Main.myPlayer].inventory,
                    1,
                    inventoryCoinSlotIndex
                );
                if (Main.mouseLeftRelease && Main.mouseLeft)
                    Recipe.FindRecipes();

                ItemSlot.MouseHover(
                    Main.player[Main.myPlayer].inventory,
                    1,
                    inventoryCoinSlotIndex
                );
            }

            ItemSlot.Draw(
                Main.spriteBatch,
                Main.player[Main.myPlayer].inventory,
                1,
                inventoryCoinSlotIndex,
                new Vector2(slotPositionX, slotPositionY)
            );
        }

        Main.inventoryScale = 0.6f;
        // for (int num101 = 0; num101 < 4; num101++)
        // {
        //     int num102 = 534;
        //     int num103 = (int)(85f + num101 * 56 * Main.inventoryScale + 20f);
        //     int slot2 = 54 + num101;
        //     new Color(100, 100, 100, 100);
        //     if (
        //         Main.mouseX >= num102
        //         && Main.mouseX <= num102 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
        //         && Main.mouseY >= num103
        //         && Main.mouseY
        //             <= num103 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
        //         && !PlayerInput.IgnoreMouseInterface
        //     )
        //     {
        //         Main.player[Main.myPlayer].mouseInterface = true;
        //         ItemSlot.OverrideHover(Main.player[Main.myPlayer].inventory, 2, slot2);
        //         ItemSlot.LeftClick(Main.player[Main.myPlayer].inventory, 2, slot2);
        //         ItemSlot.RightClick(Main.player[Main.myPlayer].inventory, 2, slot2);
        //         if (Main.mouseLeftRelease && Main.mouseLeft)
        //             Recipe.FindRecipes();

        //         ItemSlot.MouseHover(Main.player[Main.myPlayer].inventory, 2, slot2);
        //     }

        //     ItemSlot.Draw(
        //         Main.spriteBatch,
        //         Main.player[Main.myPlayer].inventory,
        //         2,
        //         slot2,
        //         new Vector2(num102, num103)
        //     );
        // }

        if (Main.npcShop > 0 && (!Main.playerInventory || Main.player[Main.myPlayer].talkNPC == -1))
            Main.SetNPCShopIndex(0);

        if (Main.npcShop > 0 && !Main.recBigList)
        {
            Utils.DrawBorderStringFourWay(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                Lang.inter[28].Value,
                504f,
                Main.instance.invBottom,
                Color.White * (Main.mouseTextColor / 255f),
                Color.Black,
                Vector2.Zero
            );
            ItemSlot.DrawSavings(Main.spriteBatch, 504f, Main.instance.invBottom);
            Main.inventoryScale = 0.755f;
            if (
                Main.mouseX > 73
                && Main.mouseX < (int)(73f + 560f * Main.inventoryScale)
                && Main.mouseY > Main.instance.invBottom
                && Main.mouseY < (int)(Main.instance.invBottom + 224f * Main.inventoryScale)
                && !PlayerInput.IgnoreMouseInterface
            )
                Main.player[Main.myPlayer].mouseInterface = true;

            for (int num104 = 0; num104 < 10; num104++)
            {
                for (int num105 = 0; num105 < 4; num105++)
                {
                    int num106 = (int)(73f + num104 * 56 * Main.inventoryScale);
                    int num107 = (int)(Main.instance.invBottom + num105 * 56 * Main.inventoryScale);
                    int slot3 = num104 + num105 * 10;
                    new Color(100, 100, 100, 100);
                    if (
                        Main.mouseX >= num106
                        && Main.mouseX
                            <= num106 + TextureAssets.InventoryBack.Width() * Main.inventoryScale
                        && Main.mouseY >= num107
                        && Main.mouseY
                            <= num107 + TextureAssets.InventoryBack.Height() * Main.inventoryScale
                        && !PlayerInput.IgnoreMouseInterface
                    )
                    {
                        ItemSlot.OverrideHover(Main.instance.shop[Main.npcShop].item, 15, slot3);
                        Main.player[Main.myPlayer].mouseInterface = true;
                        ItemSlot.LeftClick(Main.instance.shop[Main.npcShop].item, 15, slot3);
                        ItemSlot.RightClick(Main.instance.shop[Main.npcShop].item, 15, slot3);
                        ItemSlot.MouseHover(Main.instance.shop[Main.npcShop].item, 15, slot3);
                    }

                    ItemSlot.Draw(
                        Main.spriteBatch,
                        Main.instance.shop[Main.npcShop].item,
                        15,
                        slot3,
                        new Vector2(num106, num107)
                    );
                }
            }
        }

        // var tilemapReflection = typeof(Tilemap);
        // if (
        //     Main.player[Main.myPlayer].chest > -1
        //     && !Main.tileContainer[
        //         (
        //             tilemapReflection
        //                 .GetField("type")
        //                 .GetValue(
        //                     Main.tile[
        //                         Main.player[Main.myPlayer].chestX,
        //                         Main.player[Main.myPlayer].chestY
        //                     ]
        //                 ) as int?
        //         ).Value
        //     ]
        // )
        // {
        //     Main.player[Main.myPlayer].chest = -1;
        //     Recipe.FindRecipes();
        // }

        int offsetDown = 0;
        UIVirtualKeyboard.ShouldHideText = !PlayerInput.SettingsForUI.ShowGamepadHints;
        if (!PlayerInput.UsingGamepad)
            offsetDown = 9999;

        UIVirtualKeyboard.OffsetDown = offsetDown;
        ChestUI.Draw(Main.spriteBatch);
        Main.LocalPlayer.tileEntityAnchor.GetTileEntity()
            ?.OnInventoryDraw(Main.LocalPlayer, Main.spriteBatch);
        if (Main.player[Main.myPlayer].chest == -1 && Main.npcShop == 0 && false)
        {
            int num108 = 0;
            int num109 = 498;
            int num110 = 244;
            int num111 = TextureAssets.ChestStack[num108].Width();
            int num112 = TextureAssets.ChestStack[num108].Height();
            UILinkPointNavigator.SetPosition(
                301,
                new Vector2(num109 + num111 * 0.75f, num110 + num112 * 0.75f)
            );
            if (
                Main.mouseX >= num109
                && Main.mouseX <= num109 + num111
                && Main.mouseY >= num110
                && Main.mouseY <= num110 + num112
                && !PlayerInput.IgnoreMouseInterface
            )
            {
                num108 = 1;
                if (!Main.allChestStackHover)
                {
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                    Main.allChestStackHover = true;
                }

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.mouseLeftRelease = false;
                    Main.player[Main.myPlayer].QuickStackAllChests();
                    Recipe.FindRecipes();
                }

                Main.player[Main.myPlayer].mouseInterface = true;
            }
            else if (Main.allChestStackHover)
            {
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                Main.allChestStackHover = false;
            }

            Main.spriteBatch.Draw(
                TextureAssets.ChestStack[num108].Value,
                new Vector2(num109, num110),
                new Rectangle(
                    0,
                    0,
                    TextureAssets.ChestStack[num108].Width(),
                    TextureAssets.ChestStack[num108].Height()
                ),
                Color.White,
                0f,
                default(Vector2),
                1f,
                SpriteEffects.None,
                0f
            );
            if (!Main.mouseText && num108 == 1)
                Main.instance.MouseText(Language.GetTextValue("GameUI.QuickStackToNearby"), 0, 0);
        }

        if (Main.player[Main.myPlayer].chest != -1 || Main.npcShop != 0)
            return;

        // deleted sort inventory
    }

    private static int DrawPageIcons(int yPos)
    {
        int num = -1;
        Vector2 vector = new(Main.screenWidth - 162, yPos);
        vector.X += 82f;
        Texture2D value = TextureAssets.EquipPage[(Main.EquipPage == 2) ? 3 : 2].Value;
        if (
            Collision.CheckAABBvAABBCollision(
                vector,
                value.Size(),
                new Vector2(Main.mouseX, Main.mouseY),
                Vector2.One
            ) && (Main.mouseItem.stack < 1 || Main.mouseItem.dye > 0)
        )
            num = 2;

        if (num == 2)
            Main.spriteBatch.Draw(
                TextureAssets.EquipPage[6].Value,
                vector,
                null,
                Main.OurFavoriteColor,
                0f,
                new Vector2(2f),
                0.9f,
                SpriteEffects.None,
                0f
            );

        Main.spriteBatch.Draw(
            value,
            vector,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            0.9f,
            SpriteEffects.None,
            0f
        );
        UILinkPointNavigator.SetPosition(305, vector + value.Size() * 0.75f);
        vector.X -= 48f;
        value = TextureAssets.EquipPage[(Main.EquipPage == 1) ? 5 : 4].Value;
        if (
            Collision.CheckAABBvAABBCollision(
                vector,
                value.Size(),
                new Vector2(Main.mouseX, Main.mouseY),
                Vector2.One
            )
            && Main.mouseItem.stack < 1
        )
            num = 1;

        if (num == 1)
            Main.spriteBatch.Draw(
                TextureAssets.EquipPage[7].Value,
                vector,
                null,
                Main.OurFavoriteColor,
                0f,
                new Vector2(2f),
                0.9f,
                SpriteEffects.None,
                0f
            );

        Main.spriteBatch.Draw(
            value,
            vector,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            0.9f,
            SpriteEffects.None,
            0f
        );
        UILinkPointNavigator.SetPosition(306, vector + value.Size() * 0.75f);
        vector.X -= 48f;
        value = TextureAssets.EquipPage[(Main.EquipPage == 3) ? 10 : 8].Value;
        if (
            Collision.CheckAABBvAABBCollision(
                vector,
                value.Size(),
                new Vector2(Main.mouseX, Main.mouseY),
                Vector2.One
            )
            && Main.mouseItem.stack < 1
        )
            num = 3;

        if (num == 3 && !Main.CaptureModeDisabled)
            Main.spriteBatch.Draw(
                TextureAssets.EquipPage[9].Value,
                vector,
                null,
                Main.OurFavoriteColor,
                0f,
                Vector2.Zero,
                0.9f,
                SpriteEffects.None,
                0f
            );

        Main.spriteBatch.Draw(
            value,
            vector,
            null,
            Main.CaptureModeDisabled ? Color.Red : Color.White,
            0f,
            Vector2.Zero,
            0.9f,
            SpriteEffects.None,
            0f
        );
        UILinkPointNavigator.SetPosition(307, vector + value.Size() * 0.75f);
        if (num != -1)
        {
            Main.player[Main.myPlayer].mouseInterface = true;
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                // bool flag = true;
                if (num == 3)
                {
                    if (Main.CaptureModeDisabled)
                    {
                        // flag = false;
                    }
                    else if (PlayerInput.UsingGamepad)
                    {
                        CaptureInterface.QuickScreenshot();
                    }
                    else
                    {
                        CaptureManager.Instance.Active = true;
                        Main.blockMouse = true;
                    }
                }
                else if (Main.EquipPageSelected != num)
                {
                    Main.EquipPageSelected = num;
                }
                else
                {
                    Main.EquipPageSelected = 0;
                }

                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Open"));
            }
        }

        ItemSlot.SelectEquipPage(Main.mouseItem);
        if (Main.EquipPage == -1)
            Main.EquipPage = Main.EquipPageSelected;

        return num;
    }

    /// a customized version of ItemSlot.Draw
    public static void ItemSlotDraw(
        SpriteBatch spriteBatch,
        Item[] inv,
        int context,
        int slot,
        Vector2 position,
        Color lightColor = default(Color)
    )
    {
        Type itemSlotReflection = typeof(ItemSlot);

        Player player = Main.player[Main.myPlayer];
        Item item = inv[slot];
        float inventoryScale = Main.inventoryScale;
        Color color = Color.White;
        if (lightColor != Color.Transparent)
            color = lightColor;

        bool flag = false;
        int num = 0;
        int gamepadPointForSlot = (
            itemSlotReflection
                .GetMethod("GetGamepadPointForSlot", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, [inv, context, slot]) as int?
        ).Value;
        if (PlayerInput.UsingGamepadUI)
        {
            flag = UILinkPointNavigator.CurrentPoint == gamepadPointForSlot;
            if (PlayerInput.SettingsForUI.PreventHighlightsForGamepad)
                flag = false;

            if (context == 0)
            {
                num = player.DpadRadial.GetDrawMode(slot);
                if (num > 0 && !PlayerInput.CurrentProfile.UsingDpadHotbar())
                    num = 0;
            }
        }

        Texture2D value = TextureAssets.InventoryBack.Value;
        Color color2 = Main.inventoryBack;
        bool flag2 = false;
        bool highlightThingsForMouse = PlayerInput.SettingsForUI.HighlightThingsForMouse;
        if (
            item.type > ItemID.None
            && item.stack > 0
            && item.favorited
            && context != 13
            && context != 21
            && context != 22
            && context != 14
        )
        {
            value = TextureAssets.InventoryBack10.Value;
            if (context == 32)
                value = TextureAssets.InventoryBack19.Value;
        }
        else if (
            item.type > ItemID.None
            && item.stack > 0
            && ItemSlot.Options.HighlightNewItems
            && item.newAndShiny
            && context != 13
            && context != 21
            && context != 14
            && context != 22
        )
        {
            value = TextureAssets.InventoryBack15.Value;
            float num2 = (float)(int)Main.mouseTextColor / 255f;
            num2 = num2 * 0.2f + 0.8f;
            color2 = color2.MultiplyRGBA(new Color(num2, num2, num2));
        }
        else if (
            !highlightThingsForMouse
            && item.type > ItemID.None
            && item.stack > 0
            && num != 0
            && context != 13
            && context != 21
            && context != 22
        )
        {
            value = TextureAssets.InventoryBack15.Value;
            float num3 = (float)(int)Main.mouseTextColor / 255f;
            num3 = num3 * 0.2f + 0.8f;
            color2 = (
                (num != 1)
                    ? color2.MultiplyRGBA(new Color(num3 / 2f, num3, num3 / 2f))
                    : color2.MultiplyRGBA(new Color(num3, num3 / 2f, num3 / 2f))
            );
        }
        else if (context == 0 && slot < 10)
        {
            value = TextureAssets.InventoryBack9.Value;
        }
        else
        {
            switch (context)
            {
                case 28:
                    value = TextureAssets.InventoryBack7.Value;
                    color2 = Color.White;
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    value = TextureAssets.InventoryBack3.Value;
                    break;
                case 8:
                case 10:
                    value = TextureAssets.InventoryBack13.Value;
                    color2 = ItemSlot.GetColorByLoadout(slot, context);
                    break;
                case 23:
                case 24:
                case 26:
                    value = TextureAssets.InventoryBack8.Value;
                    break;
                case 9:
                case 11:
                    value = TextureAssets.InventoryBack13.Value;
                    color2 = ItemSlot.GetColorByLoadout(slot, context);
                    break;
                case 25:
                case 27:
                case 33:
                    value = TextureAssets.InventoryBack12.Value;
                    break;
                case 12:
                    value = TextureAssets.InventoryBack13.Value;
                    color2 = ItemSlot.GetColorByLoadout(slot, context);
                    break;
                case 3:
                    value = TextureAssets.InventoryBack5.Value;
                    break;
                case 4:
                case 32:
                    value = TextureAssets.InventoryBack2.Value;
                    break;
                case 5:
                case 7:
                    value = TextureAssets.InventoryBack4.Value;
                    break;
                case 6:
                    value = TextureAssets.InventoryBack7.Value;
                    break;
                case 13:
                {
                    byte b = 200;
                    if (slot == Main.player[Main.myPlayer].selectedItem)
                    {
                        value = TextureAssets.InventoryBack14.Value;
                        b = byte.MaxValue;
                    }

                    color2 = new Color(b, b, b, b);
                    break;
                }
                case 14:
                case 21:
                    flag2 = true;
                    break;
                case 15:
                    value = TextureAssets.InventoryBack6.Value;
                    break;
                case 29:
                    color2 = new Color(53, 69, 127, 255);
                    value = TextureAssets.InventoryBack18.Value;
                    break;
                case 30:
                    flag2 = !flag;
                    break;
                case 22:
                    value = TextureAssets.InventoryBack4.Value;
                    if (ItemSlot.DrawGoldBGForCraftingMaterial)
                    {
                        ItemSlot.DrawGoldBGForCraftingMaterial = false;
                        value = TextureAssets.InventoryBack14.Value;
                        float num4 = (float)(int)color2.A / 255f;
                        num4 = (
                            (!(num4 < 0.7f))
                                ? 1f
                                : Utils.GetLerpValue(0f, 0.7f, num4, clamped: true)
                        );
                        color2 = Color.White * num4;
                    }
                    break;
            }
        }

        int[] inventoryGlowTime =
            itemSlotReflection
                .GetField("inventoryGlowTime", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null) as int[];
        float[] inventoryGlowHue =
            itemSlotReflection
                .GetField("inventoryGlowHue", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null) as float[];
        int[] inventoryGlowTimeChest =
            itemSlotReflection
                .GetField("inventoryGlowTimeChest", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null) as int[];
        float[] inventoryGlowHueChest =
            itemSlotReflection
                .GetField("inventoryGlowHueChest", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null) as float[];

        if (
            (context == 0 || context == 2)
            && inventoryGlowTime[slot] > 0
            && !inv[slot].favorited
            && !inv[slot].IsAir
        )
        {
            float num5 = Main.invAlpha / 255f;
            Color value2 = new Color(63, 65, 151, 255) * num5;
            Color value3 = Main.hslToRgb(inventoryGlowHue[slot], 1f, 0.5f) * num5;
            float num6 = (float)inventoryGlowTime[slot] / 300f;
            num6 *= num6;
            color2 = Color.Lerp(value2, value3, num6 / 2f);
            value = TextureAssets.InventoryBack13.Value;
        }

        if (
            (context == 4 || context == 32 || context == 3)
            && inventoryGlowTimeChest[slot] > 0
            && !inv[slot].favorited
            && !inv[slot].IsAir
        )
        {
            float num7 = Main.invAlpha / 255f;
            Color value4 = new Color(130, 62, 102, 255) * num7;
            if (context == 3)
                value4 = new Color(104, 52, 52, 255) * num7;

            Color value5 = Main.hslToRgb(inventoryGlowHueChest[slot], 1f, 0.5f) * num7;
            float num8 = (float)inventoryGlowTimeChest[slot] / 300f;
            num8 *= num8;
            color2 = Color.Lerp(value4, value5, num8 / 2f);
            value = TextureAssets.InventoryBack13.Value;
        }

        if (flag)
        {
            value = TextureAssets.InventoryBack14.Value;
            color2 = Color.White;
            if (item.favorited)
                value = TextureAssets.InventoryBack17.Value;
        }

        if (
            context == 28
            && Main.MouseScreen.Between(position, position + value.Size() * inventoryScale)
            && !player.mouseInterface
        )
        {
            value = TextureAssets.InventoryBack14.Value;
            color2 = Color.White;
        }

        if (!flag2)
            spriteBatch.Draw(
                value,
                position,
                null,
                color2,
                0f,
                default(Vector2),
                inventoryScale,
                SpriteEffects.None,
                0f
            );

        int num9 = -1;
        switch (context)
        {
            case 8:
            case 23:
                if (slot == 0)
                    num9 = 0;
                if (slot == 1)
                    num9 = 6;
                if (slot == 2)
                    num9 = 12;
                break;
            case 26:
                num9 = 0;
                break;
            case 9:
                if (slot == 10)
                    num9 = 3;
                if (slot == 11)
                    num9 = 9;
                if (slot == 12)
                    num9 = 15;
                break;
            case 10:
            case 24:
                num9 = 11;
                break;
            case 11:
                num9 = 2;
                break;
            case 12:
            case 25:
            case 27:
            case 33:
                num9 = 1;
                break;
            case 16:
                num9 = 4;
                break;
            case 17:
                num9 = 13;
                break;
            case 19:
                num9 = 10;
                break;
            case 18:
                num9 = 7;
                break;
            case 20:
                num9 = 17;
                break;
        }

        if ((item.type <= ItemID.None || item.stack <= 0) && num9 != -1)
        {
            Texture2D value6 = TextureAssets.Extra[54].Value;
            Rectangle rectangle = value6.Frame(3, 6, num9 % 3, num9 / 3);
            rectangle.Width -= 2;
            rectangle.Height -= 2;
            spriteBatch.Draw(
                value6,
                position + value.Size() / 2f * inventoryScale,
                rectangle,
                Color.White * 0.35f,
                0f,
                rectangle.Size() / 2f,
                inventoryScale,
                SpriteEffects.None,
                0f
            );
        }

        Vector2 vector = value.Size() * inventoryScale;
        if (item.type > ItemID.None && item.stack > 0)
        {
            float scale = ItemSlot.DrawItemIcon(
                item,
                context,
                spriteBatch,
                position + vector / 2f,
                inventoryScale,
                32f,
                color
            );
            if (ItemID.Sets.TrapSigned[item.type])
                spriteBatch.Draw(
                    TextureAssets.Wire.Value,
                    position + new Vector2(40f, 40f) * inventoryScale,
                    new Rectangle(4, 58, 8, 8),
                    color,
                    0f,
                    new Vector2(4f),
                    1f,
                    SpriteEffects.None,
                    0f
                );

            if (ItemID.Sets.DrawUnsafeIndicator[item.type])
            {
                Vector2 vector2 = new Vector2(-4f, -4f) * inventoryScale;
                Texture2D value7 = TextureAssets.Extra[258].Value;
                Rectangle rectangle2 = value7.Frame();
                spriteBatch.Draw(
                    value7,
                    position + vector2 + new Vector2(40f, 40f) * inventoryScale,
                    rectangle2,
                    color,
                    0f,
                    rectangle2.Size() / 2f,
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            if (item.type == 5324 || item.type == 5329 || item.type == 5330)
            {
                Vector2 vector3 = new Vector2(2f, -6f) * inventoryScale;
                switch (item.type)
                {
                    case 5324:
                    {
                        Texture2D value10 = TextureAssets.Extra[257].Value;
                        Rectangle rectangle5 = value10.Frame(3, 1, 2);
                        spriteBatch.Draw(
                            value10,
                            position + vector3 + new Vector2(40f, 40f) * inventoryScale,
                            rectangle5,
                            color,
                            0f,
                            rectangle5.Size() / 2f,
                            1f,
                            SpriteEffects.None,
                            0f
                        );
                        break;
                    }
                    case 5329:
                    {
                        Texture2D value9 = TextureAssets.Extra[257].Value;
                        Rectangle rectangle4 = value9.Frame(3, 1, 1);
                        spriteBatch.Draw(
                            value9,
                            position + vector3 + new Vector2(40f, 40f) * inventoryScale,
                            rectangle4,
                            color,
                            0f,
                            rectangle4.Size() / 2f,
                            1f,
                            SpriteEffects.None,
                            0f
                        );
                        break;
                    }
                    case 5330:
                    {
                        Texture2D value8 = TextureAssets.Extra[257].Value;
                        Rectangle rectangle3 = value8.Frame(3);
                        spriteBatch.Draw(
                            value8,
                            position + vector3 + new Vector2(40f, 40f) * inventoryScale,
                            rectangle3,
                            color,
                            0f,
                            rectangle3.Size() / 2f,
                            1f,
                            SpriteEffects.None,
                            0f
                        );
                        break;
                    }
                }
            }

            if (item.stack > 1)
                ChatManager.DrawColorCodedStringWithShadow(
                    spriteBatch,
                    FontAssets.ItemStack.Value,
                    item.stack.ToString(),
                    position + new Vector2(10f, 26f) * inventoryScale,
                    color,
                    0f,
                    Vector2.Zero,
                    new Vector2(inventoryScale),
                    -1f,
                    inventoryScale
                );

            int num10 = -1;
            if (context == 13)
            {
                if (item.DD2Summon)
                {
                    for (int i = 0; i < 58; i++)
                    {
                        if (inv[i].type == ItemID.DD2EnergyCrystal)
                            num10 += inv[i].stack;
                    }

                    if (num10 >= 0)
                        num10++;
                }

                if (item.useAmmo > 0)
                {
                    int useAmmo = item.useAmmo;
                    num10 = 0;
                    for (int j = 0; j < 58; j++)
                    {
                        if (inv[j].ammo == useAmmo)
                            num10 += inv[j].stack;
                    }
                }

                if (item.fishingPole > 0)
                {
                    num10 = 0;
                    for (int k = 0; k < 58; k++)
                    {
                        if (inv[k].bait > 0)
                            num10 += inv[k].stack;
                    }
                }

                if (item.tileWand > 0)
                {
                    int tileWand = item.tileWand;
                    num10 = 0;
                    for (int l = 0; l < 58; l++)
                    {
                        if (inv[l].type == tileWand)
                            num10 += inv[l].stack;
                    }
                }

                if (
                    item.type == ItemID.Wrench
                    || item.type == ItemID.GreenWrench
                    || item.type == ItemID.BlueWrench
                    || item.type == ItemID.YellowWrench
                    || item.type == ItemID.MulticolorWrench
                    || item.type == ItemID.WireKite
                )
                {
                    num10 = 0;
                    for (int m = 0; m < 58; m++)
                    {
                        if (inv[m].type == ItemID.Wire)
                            num10 += inv[m].stack;
                    }
                }
            }

            if (num10 != -1)
                ChatManager.DrawColorCodedStringWithShadow(
                    spriteBatch,
                    FontAssets.ItemStack.Value,
                    num10.ToString(),
                    position + new Vector2(8f, 30f) * inventoryScale,
                    color,
                    0f,
                    Vector2.Zero,
                    new Vector2(inventoryScale * 0.8f),
                    -1f,
                    inventoryScale
                );

            if (context == 13)
            {
                string text = string.Concat(slot + 1);
                if (text == "10")
                    text = "0";

                ChatManager.DrawColorCodedStringWithShadow(
                    spriteBatch,
                    FontAssets.ItemStack.Value,
                    text,
                    position + new Vector2(8f, 4f) * inventoryScale,
                    color,
                    0f,
                    Vector2.Zero,
                    new Vector2(inventoryScale),
                    -1f,
                    inventoryScale
                );
            }

            if (context == 13 && item.potion)
            {
                Vector2 position2 =
                    position
                    + value.Size() * inventoryScale / 2f
                    - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                Color color3 =
                    item.GetAlpha(color)
                    * ((float)player.potionDelay / (float)player.potionDelayTime);
                spriteBatch.Draw(
                    TextureAssets.Cd.Value,
                    position2,
                    null,
                    color3,
                    0f,
                    default(Vector2),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }

            if ((context == 10 || context == 18) && item.expertOnly && !Main.expertMode)
            {
                Vector2 position3 =
                    position
                    + value.Size() * inventoryScale / 2f
                    - TextureAssets.Cd.Value.Size() * inventoryScale / 2f;
                Color white = Color.White;
                spriteBatch.Draw(
                    TextureAssets.Cd.Value,
                    position3,
                    null,
                    white,
                    0f,
                    default(Vector2),
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
        else if (context == 6)
        {
            Texture2D value11 = TextureAssets.Trash.Value;
            Vector2 position4 =
                position
                + value.Size() * inventoryScale / 2f
                - value11.Size() * inventoryScale / 2f;
            spriteBatch.Draw(
                value11,
                position4,
                null,
                new Color(100, 100, 100, 100),
                0f,
                default(Vector2),
                inventoryScale,
                SpriteEffects.None,
                0f
            );
        }

        if (context == 0 && slot < 10)
        {
            float num11 = inventoryScale;
            string text2 = string.Concat(slot + 1);
            if (text2 == "10")
                text2 = "0";

            Color baseColor = Main.inventoryBack;
            int num12 = 0;
            if (Main.player[Main.myPlayer].selectedItem == slot)
            {
                baseColor = Color.White;
                baseColor.A = 200;
                num12 -= 2;
                num11 *= 1.4f;
            }

            ChatManager.DrawColorCodedStringWithShadow(
                spriteBatch,
                FontAssets.ItemStack.Value,
                text2,
                position + new Vector2(6f, 4 + num12) * inventoryScale,
                baseColor,
                0f,
                Vector2.Zero,
                new Vector2(inventoryScale),
                -1f,
                inventoryScale
            );
        }

        if (gamepadPointForSlot != -1)
            UILinkPointNavigator.SetPosition(gamepadPointForSlot, position + vector * 0.75f);
    }

    // a customized version of ItemSlot.LeftClick
    public static void ItemSlotLeftClick(Item[] inv, int context = 0, int slot = 0)
    {
        if (
            !Main.mouseItem.IsAir
            && currentSlotCategory != TerraCellsItemCategory.Storage
            && InventoryManager.GetItemCategorization(Main.LocalPlayer.inventory[58])
                != currentSlotCategory
        )
        {
            return;
        }
        Player player = Main.player[Main.myPlayer];
        bool isMouseLeftRelease = Main.mouseLeftRelease && Main.mouseLeft;
        if (isMouseLeftRelease)
        {
            if (ItemSlotOverrideLeftClick(inv, context, slot))
                return;

            inv[slot].newAndShiny = false;
            if (
                ItemSlotOverrideLeftClick(inv, context, slot)
                || player.itemAnimation != 0
                || player.itemTime != 0
            )
                return;
        }

        int num = ItemSlot.PickItemMovementAction(inv, context, slot, Main.mouseItem);
        if (num != 3 && !isMouseLeftRelease)
            return;

        switch (num)
        {
            case 0:
                if (context == 6 && Main.mouseItem.type != ItemID.None)
                    inv[slot].SetDefaults();

                if (
                    (
                        typeof(ItemSlot)
                            .GetMethod(
                                "IsAccessoryContext",
                                BindingFlags.NonPublic | BindingFlags.Static
                            )
                            .Invoke(null, [context]) as bool?
                    ).Value && !ItemLoader.CanEquipAccessory(inv[slot], slot, context < 0)
                )
                    break;

                if (context == 11 && !inv[slot].FitsAccessoryVanitySlot)
                    break;

                if (
                    context < 0
                    && !LoaderManager
                        .Get<AccessorySlotLoader>()
                        .CanAcceptItem(slot, inv[slot], context)
                )
                    break;

                // #OnStackHook: Prevent swapping items if they will be stacked together.
                if (
                    Main.mouseItem.maxStack > 1
                    && inv[slot].type == Main.mouseItem.type
                    && inv[slot].stack != inv[slot].maxStack
                    && Main.mouseItem.stack != Main.mouseItem.maxStack
                )
                    goto SkipSwap;

                Utils.Swap(ref inv[slot], ref Main.mouseItem);
                SkipSwap:

                if (inv[slot].stack > 0)
                    ItemSlot.AnnounceTransfer(
                        new ItemSlot.ItemTransferInfo(inv[slot], 21, context, inv[slot].stack)
                    );
                else
                    ItemSlot.AnnounceTransfer(
                        new ItemSlot.ItemTransferInfo(
                            Main.mouseItem,
                            context,
                            21,
                            Main.mouseItem.stack
                        )
                    );
                if (inv[slot].stack > 0)
                {
                    switch (Math.Abs(context))
                    {
                        case 0:
                            AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                            break;
                        case 8:
                        case 9:
                        case 10:
                        case 11:
                        case 12:
                        case 16:
                        case 17:
                        case 25:
                        case 27:
                        case 33:
                            AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                            break;
                    }
                }
                if (inv[slot].type == ItemID.None || inv[slot].stack < 1)
                    inv[slot] = new Item();
                if (Main.mouseItem.type == inv[slot].type)
                {
                    // #OnStackHook: Gameplay impact: The favorited item will now always be the item that has it's stack being increased instead of swapping favorites for consistency with the OnStack() hook.
                    /*
                    Utils.Swap(ref inv[slot].favorited, ref Main.mouseItem.favorited);
                    */

                    if (
                        inv[slot].stack != inv[slot].maxStack
                        && Main.mouseItem.stack != Main.mouseItem.maxStack
                    )
                    {
                        if (
                            ItemLoader.TryStackItems(
                                inv[slot],
                                Main.mouseItem,
                                out int numTransfered
                            )
                        )
                            ItemSlot.AnnounceTransfer(
                                new ItemSlot.ItemTransferInfo(inv[slot], 21, context, numTransfered)
                            );

                        /* #OnStackHook
                        if (Main.mouseItem.stack + inv[slot].stack <= Main.mouseItem.maxStack) {
                            inv[slot].stack += Main.mouseItem.stack;
                            Main.mouseItem.stack = 0;
                            AnnounceTransfer(new ItemTransferInfo(inv[slot], 21, context, inv[slot].stack));
                        }
                        else {
                            int num2 = Main.mouseItem.maxStack - inv[slot].stack;
                            inv[slot].stack += num2;
                            Main.mouseItem.stack -= num2;
                            AnnounceTransfer(new ItemTransferInfo(inv[slot], 21, context, num2));
                        }
                        */
                    }
                }
                if (Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1)
                    Main.mouseItem = new Item();
                if (Main.mouseItem.type > ItemID.None || inv[slot].type > ItemID.None)
                {
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                }
                if (context == 3 && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, slot);
                break;
            case 1:
                if (
                    Main.mouseItem.stack == 1
                    && Main.mouseItem.type > ItemID.None
                    && inv[slot].type > ItemID.None
                    && inv[slot].type != Main.mouseItem.type
                    && (context != 11 || Main.mouseItem.FitsAccessoryVanitySlot)
                )
                {
                    if (
                        Math.Abs(context)
                            is ItemSlot.Context.EquipAccessory
                                or ItemSlot.Context.EquipAccessoryVanity
                        && !ItemLoader.CanEquipAccessory(Main.mouseItem, slot, context < 0)
                    )
                        break;

                    if (
                        Math.Abs(context) == ItemSlot.Context.EquipAccessoryVanity
                        && !Main.mouseItem.FitsAccessoryVanitySlot
                    )
                        break;

                    if (
                        context < 0
                        && !LoaderManager
                            .Get<AccessorySlotLoader>()
                            .CanAcceptItem(slot, Main.mouseItem, context)
                    )
                        break;

                    Utils.Swap(ref inv[slot], ref Main.mouseItem);
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                    if (inv[slot].stack > 0)
                    {
                        switch (Math.Abs(context))
                        {
                            case 0:
                                AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                break;
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 16:
                            case 17:
                            case 25:
                            case 27:
                            case 33:
                                AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                break;
                        }
                    }
                }
                else if (Main.mouseItem.type == ItemID.None && inv[slot].type > ItemID.None)
                {
                    Utils.Swap(ref inv[slot], ref Main.mouseItem);
                    if (inv[slot].type == ItemID.None || inv[slot].stack < 1)
                        inv[slot] = new Item();

                    if (Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1)
                        Main.mouseItem = new Item();

                    if (Main.mouseItem.type > ItemID.None || inv[slot].type > ItemID.None)
                    {
                        Recipe.FindRecipes();
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                    }
                }
                else if (
                    Main.mouseItem.type > ItemID.None
                    && inv[slot].type == ItemID.None
                    && (context != 11 || Main.mouseItem.FitsAccessoryVanitySlot)
                )
                {
                    if (
                        Math.Abs(context)
                            is ItemSlot.Context.EquipAccessory
                                or ItemSlot.Context.EquipAccessoryVanity
                        && !ItemLoader.CanEquipAccessory(Main.mouseItem, slot, context < 0)
                    )
                        break;

                    if (
                        Math.Abs(context) == ItemSlot.Context.EquipAccessoryVanity
                        && !Main.mouseItem.FitsAccessoryVanitySlot
                    )
                        break;

                    if (
                        context < 0
                        && !LoaderManager
                            .Get<AccessorySlotLoader>()
                            .CanAcceptItem(slot, Main.mouseItem, context)
                    )
                        break;
                    /*
                    if (Main.mouseItem.stack == 1) {
                        Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        if (inv[slot].type == 0 || inv[slot].stack < 1)
                            inv[slot] = new Item();

                        if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
                            Main.mouseItem = new Item();

                        if (Main.mouseItem.type > 0 || inv[slot].type > 0) {
                            Recipe.FindRecipes();
                            SoundEngine.PlaySound(7);
                        }
                    }
                    else {
                        Main.mouseItem.stack--;
                        inv[slot].SetDefaults(Main.mouseItem.type);
                        Recipe.FindRecipes();
                        SoundEngine.PlaySound(7);
                    }
                    */
                    inv[slot] = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));

                    if (inv[slot].stack > 0)
                    {
                        switch (Math.Abs(context))
                        {
                            case 0:
                                AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                break;
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 16:
                            case 17:
                            case 25:
                            case 27:
                            case 33:
                                AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                break;
                        }
                    }
                }
                if ((context == 23 || context == 24) && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(
                        MessageID.TEDisplayDollItemSync,
                        -1,
                        -1,
                        null,
                        Main.myPlayer,
                        player.tileEntityAnchor.interactEntityID,
                        slot
                    );
                if (context == 26 && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(
                        MessageID.TEHatRackItemSync,
                        -1,
                        -1,
                        null,
                        Main.myPlayer,
                        player.tileEntityAnchor.interactEntityID,
                        slot
                    );
                break;
            case 2:
                if (
                    Main.mouseItem.stack == 1
                    && Main.mouseItem.dye > 0
                    && inv[slot].type > ItemID.None
                    && inv[slot].type != Main.mouseItem.type
                )
                {
                    Utils.Swap(ref inv[slot], ref Main.mouseItem);
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                    if (inv[slot].stack > 0)
                    {
                        switch (Math.Abs(context))
                        {
                            case 0:
                                AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                break;
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 16:
                            case 17:
                            case 25:
                            case 27:
                            case 33:
                                AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                break;
                        }
                    }
                }
                else if (Main.mouseItem.type == ItemID.None && inv[slot].type > ItemID.None)
                {
                    Utils.Swap(ref inv[slot], ref Main.mouseItem);
                    if (inv[slot].type == ItemID.None || inv[slot].stack < 1)
                        inv[slot] = new Item();

                    if (Main.mouseItem.type == ItemID.None || Main.mouseItem.stack < 1)
                        Main.mouseItem = new Item();

                    if (Main.mouseItem.type > ItemID.None || inv[slot].type > ItemID.None)
                    {
                        Recipe.FindRecipes();
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                    }
                }
                else if (Main.mouseItem.dye > 0 && inv[slot].type == ItemID.None)
                {
                    /*
                    if (Main.mouseItem.stack == 1) {
                        Utils.Swap(ref inv[slot], ref Main.mouseItem);
                        if (inv[slot].type == 0 || inv[slot].stack < 1)
                            inv[slot] = new Item();

                        if (Main.mouseItem.type == 0 || Main.mouseItem.stack < 1)
                            Main.mouseItem = new Item();

                        if (Main.mouseItem.type > 0 || inv[slot].type > 0) {
                            Recipe.FindRecipes();
                            SoundEngine.PlaySound(7);
                        }
                    }
                    else {
                        Main.mouseItem.stack--;
                        inv[slot].SetDefaults(Main.mouseItem.type);
                        Recipe.FindRecipes();
                        SoundEngine.PlaySound(7);
                    }
                    */
                    inv[slot] = ItemLoader.TransferWithLimit(Main.mouseItem, 1);
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));

                    if (inv[slot].stack > 0)
                    {
                        switch (Math.Abs(context))
                        {
                            case 0:
                                AchievementsHelper.NotifyItemPickup(player, inv[slot]);
                                break;
                            case 8:
                            case 9:
                            case 10:
                            case 11:
                            case 12:
                            case 16:
                            case 17:
                            case 25:
                            case 27:
                            case 33:
                                AchievementsHelper.HandleOnEquip(player, inv[slot], context);
                                break;
                        }
                    }
                }
                if (context == 25 && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(
                        MessageID.TEDisplayDollItemSync,
                        -1,
                        -1,
                        null,
                        Main.myPlayer,
                        player.tileEntityAnchor.interactEntityID,
                        slot,
                        1f
                    );
                if (context == 27 && Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(
                        MessageID.TEHatRackItemSync,
                        -1,
                        -1,
                        null,
                        Main.myPlayer,
                        player.tileEntityAnchor.interactEntityID,
                        slot,
                        1f
                    );
                break;
            case 3:
                typeof(ItemSlot)
                    .GetMethod("HandleShopSlot", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, [inv, slot, false, true]);
                break;
            case 4:
            {
                if (!PlayerLoader.CanSellItem(player, player.TalkNPC, inv, Main.mouseItem))
                    break;

                Chest chest = Main.instance.shop[Main.npcShop];
                if (player.SellItem(Main.mouseItem))
                {
                    int soldItemIndex = chest.AddItemToShop(Main.mouseItem);
                    Main.mouseItem.SetDefaults();
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Coins"));
                    ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 21, 15));

                    PlayerLoader.PostSellItem(
                        player,
                        player.TalkNPC,
                        chest.item,
                        chest.item[soldItemIndex]
                    );
                }
                else if (Main.mouseItem.value == 0)
                {
                    int soldItemIndex = chest.AddItemToShop(Main.mouseItem);
                    Main.mouseItem.SetDefaults();
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                    ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 21, 15));

                    PlayerLoader.PostSellItem(
                        player,
                        player.TalkNPC,
                        chest.item,
                        chest.item[soldItemIndex]
                    );
                }

                Recipe.FindRecipes();
                Main.stackSplit = 9999;
                break;
            }
            case 5:
                if (Main.mouseItem.IsAir)
                {
                    SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                    //Main.mouseItem.SetDefaults(inv[slot].netID);
                    Main.mouseItem = inv[slot].Clone();
                    Main.mouseItem.stack = Main.mouseItem.maxStack;
                    Main.mouseItem.OnCreated(new JourneyDuplicationItemCreationContext());
                    ItemSlot.AnnounceTransfer(new ItemSlot.ItemTransferInfo(inv[slot], 29, 21));
                }
                break;
        }

        if ((uint)context > 2u && context != 5 && context != 32)
            inv[slot].favorited = false;
    }

    // a customized version of ItemSlot.RightClick
    public static void ItemSlotRightClick(Item[] inv, int context = 0, int slot = 0)
    {
        Player player = Main.player[Main.myPlayer];
        inv[slot].newAndShiny = false;
        if (player.itemAnimation > 0)
            return;

        if (context == 15)
        {
            typeof(ItemSlot)
                .GetMethod("HandleShopSlot", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, [inv, slot, true, false]);
        }
        else
        {
            if (!Main.mouseRight)
                return;

            if (context == 0 && Main.mouseRightRelease) // toggleable inventory items like the encumbering stone or book of critter friendship
                typeof(ItemSlot)
                    .GetMethod("TryItemSwap", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, [inv[slot]]);

            /*
            if (context == 0 && ItemID.Sets.OpenableBag[inv[slot].type]) {
                if (Main.mouseRightRelease)
                    TryOpenContainer(inv[slot], player);
            */
            if (context == 0 && ItemLoader.CanRightClick(inv[slot]))
            {
                if (Main.mouseRightRelease)
                {
                    if (Main.ItemDropsDB.GetRulesForItemID(inv[slot].type).Any())
                        typeof(ItemSlot)
                            .GetMethod(
                                "TryOpenContainer",
                                BindingFlags.NonPublic | BindingFlags.Static
                            )
                            .Invoke(null, [inv[slot], player]);
                    else
                        ItemLoader.RightClick(inv[slot], player);
                }

                return;
            }

            switch (Math.Abs(context))
            {
                case 9:
                case 11:
                    // if (Main.mouseRightRelease)
                    // SwapVanityEquip(inv, context, slot, player);
                    break;
                case 12:
                case 25:
                case 27:
                case 33:
                    // if (Main.mouseRightRelease)
                    // TryPickupDyeToCursor(context, inv, slot, player);
                    break;
                case 0:
                case 3:
                case 4:
                case 32:
                    if (inv[slot].maxStack == 1)
                    {
                        if (Main.mouseRightRelease)
                            ItemSlotSwapEquip(inv, context, slot);

                        break;
                    }
                    goto default;
                default:
                {
                    if (Main.stackSplit > 1)
                        break;

                    bool flag = true;
                    bool flag2 = inv[slot].maxStack <= 1 && inv[slot].stack <= 1;
                    if (context == 0 && flag2)
                        flag = false;

                    if (context == 3 && flag2)
                        flag = false;

                    if (context == 4 && flag2)
                        flag = false;

                    if (context == 32 && flag2)
                        flag = false;

                    if (!flag)
                        break;

                    int num = Main.superFastStack + 1;
                    for (int i = 0; i < num; i++)
                    {
                        /*
                        if ((Main.mouseItem.IsTheSameAs(inv[slot]) || Main.mouseItem.type == 0) && (Main.mouseItem.stack < Main.mouseItem.maxStack || Main.mouseItem.type == 0)) {
                        */

                        if (
                            (
                                Main.mouseItem == inv[slot]
                                    && ItemLoader.CanStack(Main.mouseItem, inv[slot])
                                || Main.mouseItem.type == ItemID.None
                            )
                            && (
                                Main.mouseItem.stack < Main.mouseItem.maxStack
                                || Main.mouseItem.type == ItemID.None
                            )
                        )
                        {
                            ItemSlot.PickupItemIntoMouse(inv, context, slot, player);
                            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                            ItemSlot.RefreshStackSplitCooldown();
                        }
                    }

                    break;
                }
            }
        }
    }

    // a customized version of ItemSlot.SwapEquip
    public static void ItemSlotSwapEquip(Item[] inv, int context, int slot)
    {
        Player player = Main.player[Main.myPlayer];
        if (ItemSlot.isEquipLocked(inv[slot].type) || inv[slot].IsAir)
            return;

        bool success;
        if (inv[slot].dye > 0)
        {
            // inv[slot] = ItemSlot.DyeSwap(inv[slot], out success);
            // if (success)
            // {
            // Main.EquipPageSelected = 0;
            // AchievementsHelper.HandleOnEquip(player, inv[slot], 12);
            // }
        }
        else if (Main.projHook[inv[slot].shoot])
        {
            // inv[slot] = EquipSwap(inv[slot], player.miscEquips, 4, out success);
            // if (success)
            // {
            // Main.EquipPageSelected = 2;
            // AchievementsHelper.HandleOnEquip(player, inv[slot], 16);
            // }
        }
        else if (inv[slot].mountType != -1 && !MountID.Sets.Cart[inv[slot].mountType])
        {
            // inv[slot] = EquipSwap(inv[slot], player.miscEquips, 3, out success);
            // if (success)
            // {
            // Main.EquipPageSelected = 2;
            // AchievementsHelper.HandleOnEquip(player, inv[slot], 17);
            // }
        }
        else if (inv[slot].mountType != -1 && MountID.Sets.Cart[inv[slot].mountType])
        {
            // inv[slot] = EquipSwap(inv[slot], player.miscEquips, 2, out success);
            // if (success)
            // Main.EquipPageSelected = 2;
        }
        else if (inv[slot].buffType > 0 && Main.lightPet[inv[slot].buffType])
        {
            // inv[slot] = EquipSwap(inv[slot], player.miscEquips, 1, out success);
            // if (success)
            // Main.EquipPageSelected = 2;
        }
        else if (inv[slot].buffType > 0 && Main.vanityPet[inv[slot].buffType])
        {
            // inv[slot] = EquipSwap(inv[slot], player.miscEquips, 0, out success);
            // if (success)
            // Main.EquipPageSelected = 2;
        }
        else
        {
            Item item = inv[slot];
            inv[slot] = ItemSlotArmorSwap(inv[slot], out success);
            if (success)
            {
                Main.EquipPageSelected = 0;
                AchievementsHelper.HandleOnEquip(
                    player,
                    item,
                    (item.accessory ? 10 : 8) * Math.Sign(context)
                );
            }
        }

        Recipe.FindRecipes();
        if (context == 3 && Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, slot);
    }

    public static Item ItemSlotArmorSwap(Item item, out bool success)
    {
        success = false;
        if (item.stack < 1)
            return item;

        if (item.headSlot == -1 && item.bodySlot == -1 && item.legSlot == -1 && !item.accessory)
            return item;

        Player player = Main.player[Main.myPlayer];
        int num = ((item.vanity && !item.accessory) ? 10 : 0);
        item.favorited = false;
        Item result = item;
        if (item.headSlot != -1)
        {
            result = player.armor[num].Clone();
            player.armor[num] = item.Clone();
        }
        else if (item.bodySlot != -1)
        {
            result = player.armor[num + 1].Clone();
            player.armor[num + 1] = item.Clone();
        }
        else if (item.legSlot != -1)
        {
            result = player.armor[num + 2].Clone();
            player.armor[num + 2] = item.Clone();
        }
        else if (item.accessory)
        {
            if (!ItemSlotAccessorySwap(player, item, ref result))
                return result;
        }

        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
        Recipe.FindRecipes();
        success = true;
        return result;
    }

    private static bool ItemSlotAccessorySwap(Player player, Item item, ref Item result)
    {
        //TML: Rewrote ArmorSwap for accessories under the PR #1299 so it was actually readable. No vanilla functionality lost in transition
        var accSlotToSwapTo = -1;

        //TML: Check if there is an empty slot available in functional slots, and if not, track the last available slot
        for (int i = 3; i < 5; i++)
        {
            if (player.IsItemSlotUnlockedAndUsable(i))
            {
                if (ItemLoader.CanEquipAccessory(item, i, false))
                {
                    accSlotToSwapTo = i;
                    break;
                }
            }
        }

        //TML: Check if there is an existing copy of the item in any slot (including vanity)
        // Will also replace wings with wings
        for (int j = 3; j < 5; j++)
        {
            if (item.type == player.armor[j].type && ItemLoader.CanEquipAccessory(item, j, false))
                accSlotToSwapTo = j;

            // if (
            //     j < 10
            //     && (
            //         item.wingSlot > 0 && player.armor[j].wingSlot > 0
            //         || !ItemLoader.CanAccessoryBeEquippedWith(player.armor[j], item)
            //     )
            //     && ItemLoader.CanEquipAccessory(item, j, false)
            // )
            //     accSlotToSwapTo = j - 3;
        }

        // No slot found, and it can't go in slot zero, than return
        if (accSlotToSwapTo == -1 && !ItemLoader.CanEquipAccessory(item, 0, false))
            return false;

        // accSlotToSwapTo = Math.Max(accSlotToSwapTo, 0);

        if (ItemSlot.isEquipLocked(player.armor[accSlotToSwapTo].type))
        {
            result = item;
            return false;
        }

        result = player.armor[accSlotToSwapTo].Clone();
        player.armor[accSlotToSwapTo] = item.Clone();

        return true;
    }

    private static bool ItemSlotOverrideLeftClick(Item[] inv, int context = 0, int slot = 0)
    {
        if (Math.Abs(context) == 10 && ItemSlot.isEquipLocked(inv[slot].type))
            return true;

        if (
            Main.LocalPlayer.tileEntityAnchor.IsInValidUseTileEntity()
            && Main.LocalPlayer.tileEntityAnchor.GetTileEntity()
                .OverrideItemSlotLeftClick(inv, context, slot)
        )
            return true;

        Item item = inv[slot];

        // TODO: Make this more generalized.
        if (
            ItemSlot.ShiftInUse
            && PlayerLoader.ShiftClickSlot(Main.player[Main.myPlayer], inv, context, slot)
        )
        {
            return true;
        }

        if (Main.cursorOverride == 2)
        {
            if (
                ChatManager.AddChatText(
                    FontAssets.MouseText.Value,
                    ItemTagHandler.GenerateTag(item),
                    Vector2.One
                )
            )
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));

            return true;
        }

        if (Main.cursorOverride == 3)
        {
            if (
                !(typeof(ItemSlot).GetField("canFavoriteAt").GetValue(null) as bool[])[
                    Math.Abs(context)
                ]
            )
                return false;

            item.favorited = !item.favorited;
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
            return true;
        }

        if (Main.cursorOverride == 7)
        {
            if (context == 29)
            {
                /*
                Item item2 = new Item();
                item2.SetDefaults(inv[slot].netID);
                */
                Item item2 = inv[slot].Clone();
                item2.stack = item2.maxStack;
                item2.OnCreated(new JourneyDuplicationItemCreationContext());
                for (int i = 10; i < 14; i++)
                {
                    if (Main.LocalPlayer.inventory[i].IsAir)
                    {
                        Utils.Swap(ref item2, ref Main.LocalPlayer.inventory[i]);
                        SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
                        break;
                    }
                }
                // item2 = Main.player[Main.myPlayer]
                //     .GetItem(
                //         Main.myPlayer,
                //         item2,
                //         GetItemSettings.InventoryEntityToPlayerInventorySettings
                //     );

                return true;
            }

            inv[slot] = Main.player[Main.myPlayer]
                .GetItem(
                    Main.myPlayer,
                    inv[slot],
                    GetItemSettings.InventoryEntityToPlayerInventorySettings
                );
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Menu_Tick"));
            return true;
        }

        if (Main.cursorOverride == 8)
        {
            inv[slot] = Main.player[Main.myPlayer]
                .GetItem(
                    Main.myPlayer,
                    inv[slot],
                    GetItemSettings.InventoryEntityToPlayerInventorySettings
                );
            if (Main.player[Main.myPlayer].chest > -1)
                NetMessage.SendData(
                    MessageID.SyncChestItem,
                    -1,
                    -1,
                    null,
                    Main.player[Main.myPlayer].chest,
                    slot
                );

            return true;
        }

        if (Main.cursorOverride == 9)
        {
            if (Main.CreativeMenu.IsShowingResearchMenu())
            {
                Main.CreativeMenu.SwapItem(ref inv[slot]);
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
                Main.CreativeMenu.SacrificeItemInSacrificeSlot();
            }
            else if (Main.InReforgeMenu)
            {
                Utils.Swap(ref inv[slot], ref Main.reforgeItem);
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
            }
            else if (Main.InGuideCraftMenu)
            {
                Utils.Swap(ref inv[slot], ref Main.guideItem);
                Recipe.FindRecipes();
                SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Grab"));
            }
            else
            {
                ChestUI.TryPlacingInChest(inv[slot], justCheck: false, context);
            }

            return true;
        }

        return false;
    }
}
