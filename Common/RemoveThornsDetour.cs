
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

public class RemoveThornsDetour : ModSystem
{
    
    public override void Load()
    {
        Terraria.On_WorldGen.GrowSpike += On_WorldGen_GrowSpike;
        On_WorldGen.PlaceTile += On_WorldGen_PlaceTile;
    }

    private bool On_WorldGen_PlaceTile(On_WorldGen.orig_PlaceTile orig, int i, int j, int Type, bool mute, bool forced, int plr, int style)
    {
        bool returnValue = orig(i, j, Type, mute, forced, plr, style);
        if (Main.tile[i, j].HasTile && (Main.tile[i, j].TileType == TileID.CorruptThorns || Main.tile[i, j].TileType == TileID.CrimsonThorns || Main.tile[i, j].TileType == TileID.JungleThorns))
        {
            Main.tile[i, j].ClearTile();
        }
        return returnValue;
    }

    //just do nothing because not calling orig disables the method
    private void On_WorldGen_GrowSpike(Terraria.On_WorldGen.orig_GrowSpike orig, int i, int j, ushort spikeType, ushort landType)
    {
        
    }
}