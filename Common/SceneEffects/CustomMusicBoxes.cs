using System;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;

namespace TerrariaCells.Common.SceneEffects;

public class DesertMusicEffect : ModSceneEffect {
    public override int Music => MusicLoader.GetMusicSlot("TerrariaCells/Pyramid 1.2(2)");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override bool IsSceneEffectActive(Player player)
    {
        return player.ZoneDesert;
    }
}

public class UndergroundDesertMusicEffect : ModSceneEffect {
    public override int Music => MusicLoader.GetMusicSlot("TerrariaCells/Pyramid ambience(1)");
    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
    public override bool IsSceneEffectActive(Player player)
    {
        return player.ZoneUndergroundDesert;
    }
}

public class MusicBoxSystem : ModSystem {

    public override void Load()
    {
        MusicLoader.AddMusic(Mod, "Pyramid ambience(1)");
        MusicLoader.AddMusic(Mod, "Pyramid 1.2(2)");
        // MusicLoader.AddMusic(Mod, "Beautiful Isolation");

        // MusicLoader.AddMusicBox(Mod, MusicLoader.GetMusicSlot("TerrariaCells/Pyramid ambience(1)"), ItemID.MusicBoxDesert, TileID.MusicBoxes); 
    }

    public override void SetStaticDefaults()
    {
    }
}
