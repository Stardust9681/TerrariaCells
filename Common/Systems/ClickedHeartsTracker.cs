using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;

namespace TerrariaCells.Common.Systems;

public class ClickedHeartsTracker : ModSystem
{
    internal List<(int, int)> collectedHearts = [];
    private List<(int, int)> resettingHearts = [];

    public override void OnWorldLoad()
    {
        Reset();
    }

    public override void PostUpdateWorld()
    {
        foreach (var (i, j) in resettingHearts)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileType != TileID.Heart)
            {
                continue;
            }
            tile.IsActuated = false;
            tile = Main.tile[i + 1, j];
            tile.IsActuated = false;
            tile = Main.tile[i, j + 1];
            tile.IsActuated = false;
            tile = Main.tile[i + 1, j + 1];
            tile.IsActuated = false;
        }
        resettingHearts.Clear();
    }

    public override void OnWorldUnload()
    {
        Reset();
    }

    public void Reset()
    {
        foreach ((int i, int j) in collectedHearts)
        {
            // the hearts are not reset here and now, since they cannot be unactuated at some times
            resettingHearts.Add((i, j));
        }
        collectedHearts.Clear();
        Main.LocalPlayer.GetModPlayer<LifeModPlayer>().extraHealth = 0;
    }

    public void ClickedHeart(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if (tile.TileFrameX == 18)
        {
            i -= 1;
        }
        if (tile.TileFrameY == 18)
        {
            j -= 1;
        }
        tile = Main.tile[i, j];

        (int, int) coords = (i, j);
        if (!collectedHearts.Contains(coords))
        {
            collectedHearts.Add(coords);
            Main.player[Main.myPlayer].GetModPlayer<LifeModPlayer>().extraHealth += 20;
            Main.LocalPlayer.Heal(20);
            tile = Main.tile[i, j];
            tile.IsActuated = true;
            tile = Main.tile[i + 1, j];
            tile.IsActuated = true;
            tile = Main.tile[i, j + 1];
            tile.IsActuated = true;
            tile = Main.tile[i + 1, j + 1];
            tile.IsActuated = true;

            // SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Item_4"));
            SoundEngine.PlaySound(new SoundStyle("Terraria/Sounds/Shatter"));
        }
    }

    public override void PreUpdateWorld()
    {
        base.PreUpdateWorld();
    }
}
