using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using TerrariaCells.Common.ModPlayers;
using TerrariaCells.Common.Systems;

namespace TerrariaCells.Common.GlobalTiles;

public class ClickcableHeartsGlobalTile : GlobalTile
{
    public override void RightClick(int i, int j, int type)
    {
        if (type == TileID.Heart)
        {
            Mod.GetContent<ClickedHeartsTracker>().First().ClickedHeart(i, j);
        }
        else
        {
            base.RightClick(i, j, type);
        }
    }

    public override bool AutoSelect(int i, int j, int type, Item item)
    {
        if (type == TileID.Heart)
        {
            return true;
        }
        else
        {
            return base.AutoSelect(i, j, type, item);
        }
    }

    public override bool CanDrop(int i, int j, int type)
    {
        if (type == TileID.Heart)
        {
            return false;
        }
        else
        {
            return base.CanDrop(i, j, type);
        }
    }
}
